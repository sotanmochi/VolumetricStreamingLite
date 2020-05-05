//
// This is a modified version of the Temporal RVL source code from 
// Temporal RVL: A Depth Stream Compression Method (H. Jun and J. Bailenson., 2020).
//
// This code is licensed under the MIT License.
// Copyright (c) 2020 Hanseul Jun
// Copyright (c) 2020 Soichiro Sugimoto
//
// The original Temporal RVL source code is available on GitHub.
// https://github.com/hanseuljun/temporal-rvl/blob/master/cpp/src/trvl.h
//
// H. Jun and J. Bailenson. (2020). Temporal RVL: A Depth Stream Compression Method. 
// https://vhil.stanford.edu/mm/2020/02/jun-vr-temporal.pdf
//
// -----
//
// MIT License
//
// Copyright (c) 2020 Hanseul Jun
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
    struct Pixel
    {
        public short value;
        public short invalidCound;
    };

    public class TemporalRVLDepthStreamEncoder
    {
        Pixel[] _pixels;
        short _changeThreshold;
        int _invalidThreshold;
        byte[] _encodedFrame;

        public TemporalRVLDepthStreamEncoder(int frameSize, short changeThreshold, int invalidThreshold)
        {
            _pixels = new Pixel[frameSize];
            _changeThreshold = changeThreshold;
            _invalidThreshold = invalidThreshold;
            _encodedFrame = new byte[frameSize];
        }

        public byte[] Encode(short[] depthBuffer, bool keyFrame)
        {
            int frameSize = _pixels.Length;

            byte[] encodedFrame = new byte[frameSize];
            int encodedDataBytes = 0;

            if (keyFrame)
            {
                for (int i = 0; i < _pixels.Length; i++)
                {
                    _pixels[i].value = depthBuffer[i];
                    _pixels[i].invalidCound = (short)((depthBuffer[i] == 0) ? 1 : 0);
                }

                encodedDataBytes = RVLDepthImageCompressor.CompressRVL(depthBuffer, encodedFrame);
                Array.Resize(ref encodedFrame, encodedDataBytes);
                return encodedFrame;
            }

            short[] pixelDiffs = new short[frameSize];
            for (int i = 0; i < pixelDiffs.Length; i++)
            {
                pixelDiffs[i] = _pixels[i].value;
                UpdatePixel(ref _pixels[i], depthBuffer[i], _changeThreshold, _invalidThreshold);
                pixelDiffs[i] = (short)(_pixels[i].value - pixelDiffs[i]);
            }

            encodedDataBytes = RVLDepthImageCompressor.CompressRVL(pixelDiffs, encodedFrame);
            Array.Resize(ref encodedFrame, encodedDataBytes);
            return encodedFrame;
        }

        private void UpdatePixel(ref Pixel pixel, short rawValue, short changeThreshold, int invalidationThreshold)
        {
            if (pixel.value == 0)
            {
                if (rawValue > 0)
                {
                    pixel.value = rawValue;
                }
                return;
            }

            // Reset the pixel if the depth value indicates the input was invalid two times in a row.
            if (rawValue == 0)
            {
                ++pixel.invalidCound;
                if (pixel.invalidCound >= invalidationThreshold)
                {
                    pixel.value = 0;
                    pixel.invalidCound = 0;
                }
                return;
            }
            pixel.invalidCound = 0;

            // Update pixel value when change is detected.
            if (AbsDiff(pixel.value, rawValue) > changeThreshold)
            {
                pixel.value = rawValue;
            }
        }

        private short AbsDiff(short x, short y)
        {
            if (x > y) { return (short)(x - y); }
            else { return (short)(y - x); }
        }
    }

    public class TemporalRVLDepthStreamDecoder
    {
        short[] _previousPixelValues;
        short[] _pixelDiffs;

        public TemporalRVLDepthStreamDecoder(int frameSize)
        {
            _previousPixelValues = new short[frameSize];
            _pixelDiffs = new short[frameSize];
        }

        public short[] Decode(byte[] trvlEncodedFrame, bool keyFrame)
        {
            int frameSize = _previousPixelValues.Length;
            if (keyFrame)
            {
                RVLDepthImageCompressor.DecompressRVL(trvlEncodedFrame, _previousPixelValues);
                return _previousPixelValues;
            }

            RVLDepthImageCompressor.DecompressRVL(trvlEncodedFrame, _pixelDiffs);
            for (int i = 0; i < frameSize; i++)
            {
                _previousPixelValues[i] += _pixelDiffs[i];
            }

            return _previousPixelValues;
        }
    }
}
