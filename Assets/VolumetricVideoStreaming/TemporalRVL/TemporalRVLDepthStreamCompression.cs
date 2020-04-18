//
// This code is licensed under the MIT License.
//
// This algorithm is from 
// Temporal RVL: A Depth Stream Compression Method (H. Jun and J. Bailenson., 2020).
//
// The original Temporal RVL source code is available on GitHub.
// https://github.com/hanseuljun/temporal-rvl/blob/master/cpp/src/trvl.h
//
// H. Jun and J. Bailenson. (2020). Temporal RVL: A Depth Stream Compression Method. 
// https://vhil.stanford.edu/mm/2020/02/jun-vr-temporal.pdf
//

using System;
using RVL;

namespace TemporalRVL
{
    struct Pixel
    {
        public ushort value;
        public ushort invalidCound;
    };

    public class TemporalRVLDepthStreamEncoder
    {
        Pixel[] _Pixels;
        ushort _ChangeThreshold;
        int _InvalidThreshold;
        byte[] _EncodedFrame;

        public TemporalRVLDepthStreamEncoder(int frameSize, ushort changeThreshold, int invalidThreshold)
        {
            _Pixels = new Pixel[frameSize];
            _ChangeThreshold = changeThreshold;
            _InvalidThreshold = invalidThreshold;
            _EncodedFrame = new byte[frameSize];
        }

        public byte[] Encode(ushort[] depthBuffer, bool keyFrame)
        {
            int frameSize = _Pixels.Length;

            byte[] encodedFrame = new byte[frameSize];
            int encodedDataBytes = 0;

            if (keyFrame)
            {
                for (int i = 0; i < _Pixels.Length; i++)
                {
                    _Pixels[i].value = depthBuffer[i];
                    _Pixels[i].invalidCound = (ushort)((depthBuffer[i] == 0) ? 1 : 0);
                }

                encodedDataBytes = RVLDepthImageCompressor.CompressRVL(depthBuffer, encodedFrame);
                Array.Resize(ref encodedFrame, encodedDataBytes);
                return encodedFrame;
            }

            ushort[] pixelDiffs = new ushort[frameSize];
            for (int i = 0; i < pixelDiffs.Length; i++)
            {
                pixelDiffs[i] = _Pixels[i].value;
                UpdatePixel(ref _Pixels[i], depthBuffer[i], _ChangeThreshold, _InvalidThreshold);
                pixelDiffs[i] = (ushort)(_Pixels[i].value - pixelDiffs[i]);
            }

            encodedDataBytes = RVLDepthImageCompressor.CompressRVL(pixelDiffs, encodedFrame);
            Array.Resize(ref encodedFrame, encodedDataBytes);
            return encodedFrame;
        }

        private void UpdatePixel(ref Pixel pixel, ushort rawValue, ushort changeThreshold, int invalidationThreshold)
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

        private ushort AbsDiff(ushort x, ushort y)
        {
            if (x > y) { return (ushort)(x - y); }
            else { return (ushort)(y - x); }
        }
    }


    public class TemporalRVLDepthStreamDecoder
    {
        ushort[] _PreviousPixelValues;
        ushort[] _PixelDiffs;

        public TemporalRVLDepthStreamDecoder(int frameSize)
        {
            _PreviousPixelValues = new ushort[frameSize];
            _PixelDiffs = new ushort[frameSize];
        }

        public ushort[] Decode(byte[] trvlEncodedFrame, bool keyFrame)
        {
            int frameSize = _PreviousPixelValues.Length;
            if (keyFrame)
            {
                RVLDepthImageCompressor.DecompressRVL(trvlEncodedFrame, _PreviousPixelValues);
                return _PreviousPixelValues;
            }

            RVLDepthImageCompressor.DecompressRVL(trvlEncodedFrame, _PixelDiffs);
            for (int i = 0; i < frameSize; i++)
            {
                _PreviousPixelValues[i] += _PixelDiffs[i];
            }

            return _PreviousPixelValues;
        }
    }
}
