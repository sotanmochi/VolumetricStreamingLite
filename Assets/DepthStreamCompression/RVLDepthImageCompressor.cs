//
// This is a modified version of the RVL source code from 
// Fast Lossless Depth Image Compression (A. D. Wilson, 2017).
//
// This code is licensed under the MIT License.
// Copyright (c) 2017 Andrew D. Wilson
// Copyright (c) 2020 Soichiro Sugimoto
//
// The original RVL source code is available in Wilson's paper.
//
// A. D. Wilson. (2017). Fast Lossless Depth Image Compression. 
// https://dl.acm.org/doi/pdf/10.1145/3132272.3134144
// https://www.microsoft.com/en-us/research/uploads/prod/2018/09/p100-wilson.pdf
//
// -----
//
// MIT License
//
// Copyright (c) 2017 Andrew D. Wilson
// Copyright (c) 2020 Soichiro Sugimoto
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

using System;

namespace DepthStreamCompression
{
    public class RVLDepthImageCompressor
    {
        public static int CompressRVL(short[] input, byte[] output)
        {
            int bufferLength = output.Length / sizeof(int);
            int[] _buffer = new int[bufferLength];
            int _bufferCounter = 0;

            int _word = 0;
            int _nibblesWritten = 0;
            short previous = 0;

            int index = 0;
            while (index < input.Length)
            {
                int zeros = 0, nonzeros = 0;
                for (; (index < input.Length) && (input[index] == 0); index++, zeros++);
                EncodeVLE(zeros, _buffer, ref _bufferCounter, ref _word, ref _nibblesWritten); // number of zeros
                for (int p = index; (p < input.Length) && (input[p++] != 0); nonzeros++);
                EncodeVLE(nonzeros, _buffer, ref _bufferCounter, ref _word, ref _nibblesWritten); // number of nonzeros
                for (int i = 0; i < nonzeros; i++)
                {
                    short current = input[index++];
                    int delta = current - previous;
                    int positive = (delta << 1) ^ (delta >> 31);
                    EncodeVLE(positive, _buffer, ref _bufferCounter, ref _word, ref _nibblesWritten); // nonzero value
                    previous = current;
                }
            }
            if (_nibblesWritten != 0) // last few values
            {
                _buffer[_bufferCounter++] = _word << 4 * (8 - _nibblesWritten);
            }

            Buffer.BlockCopy(_buffer, 0, output, 0, _buffer.Length * sizeof(int));
            int numBytes = _bufferCounter * sizeof(int);
            return numBytes;
        }

        public static void DecompressRVL(byte[] input, short[] output)
        {
            int bufferLength = input.Length / sizeof(int);
            int[] _buffer = new int[bufferLength];

            Buffer.BlockCopy(input, 0, _buffer, 0, input.Length * sizeof(byte));
            int _bufferCounter = 0;

            int _word = 0;
            int _nibblesWritten = 0;
            short current, previous = 0;

            int index = 0;
            int numPixelsToDecode = output.Length; // num pixels
            while (numPixelsToDecode != 0)
            {
                int zeros = DecodeVLE(_buffer, ref _bufferCounter, ref _word, ref _nibblesWritten); // number of zeros
                numPixelsToDecode -= zeros;
                for (; (zeros != 0); zeros--)
                {
                    output[index++] = 0;
                }
                int nonzeros = DecodeVLE(_buffer, ref _bufferCounter, ref _word, ref _nibblesWritten); // number of nonzeros
                numPixelsToDecode -= nonzeros;
                for (; (nonzeros != 0); nonzeros--)
                {
                    int positive = DecodeVLE(_buffer, ref _bufferCounter, ref _word, ref _nibblesWritten); // nonzero value
                    int delta = (positive >> 1) ^ -(positive & 1);
                    current = (short)(previous + delta);
                    output[index++] = current;
                    previous = current;
                }
            }
        }

        private static void EncodeVLE(int value, int[] _buffer, ref int _bufferCounter, ref int _word, ref int _nibblesWritten)
        {
            do
            {
                int nibble = value & 0x7; // lower 3 bits
                if ((value >>= 3) != 0) nibble |= 0x8; // more to come
                _word <<= 4;
                _word |= nibble;
                if (++_nibblesWritten == 8) // output word
                {
                    _buffer[_bufferCounter++] = _word;
                    _nibblesWritten = 0;
                    _word = 0;
                }
            } while (value != 0);
        }

        private static int DecodeVLE(int[] _buffer, ref int _bufferCounter, ref int _word, ref int _nibblesWritten)
        {
            uint nibble;
            int value = 0, bits = 29;
            do
            {
                if (_nibblesWritten == 0)
                {
                    _word = _buffer[_bufferCounter++]; // load word
                    _nibblesWritten = 8;
                }
                nibble = (uint)(_word & 0xf0000000);
                value |= (int)((nibble << 1) >> bits);
                _word <<= 4;
                _nibblesWritten--;
                bits -= 3;
            } while ((nibble & 0x80000000) != 0);
            return value;
        }
    }
}
