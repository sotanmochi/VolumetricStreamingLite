// This code is licensed under the MIT License.
// Copyright (c) 2020 Soichiro Sugimoto

using System;
using System.Runtime.InteropServices;

namespace DepthStreamCompression.NativePlugin
{
    class Plugin
    {
        [DllImport("DepthStreamCompression")]
        public static extern int EncodeRVL(short[] input, byte[] output, int numPixels);
        [DllImport("DepthStreamCompression")]
        public static extern void DecodeRVL(byte[] input, short[] output, int numPixels);
        [DllImport("DepthStreamCompression")]
        public static extern IntPtr CreateTemporalRVLEncoder(int frameSize, short changeThreshold, int invalidThreshold);
        [DllImport("DepthStreamCompression")]
        public static extern IntPtr CreateTemporalRVLDecoder(int frameSize);
        [DllImport("DepthStreamCompression")]
        public static extern void DeleteTemporalRVLEncoder(IntPtr encoderPtr);
        [DllImport("DepthStreamCompression")]
        public static extern void DeleteTemporalRVLDecoder(IntPtr decoderPtr);
        [DllImport("DepthStreamCompression")]
        public static extern int EncodeTemporalRVL(IntPtr encoderPtr, short[] depthBuffer, byte[] output, bool keyFrame);
        [DllImport("DepthStreamCompression")]
        public static extern void DecodeTemporalRVL(IntPtr decoderPtr, byte[] trvlFrame, short[] output, bool keyFrame);
    }

    public class RVL
    {
        public static int EncodeRVL(ref short[] input, ref byte[] output, int numPixels)
        {
            Array.Resize(ref output, numPixels);
            int numBytes = Plugin.EncodeRVL(input, output, numPixels);
            Array.Resize(ref output, numBytes);
            return numBytes;
        }

        public static void DecodeRVL(ref byte[] input, ref short[] output, int numPixels)
        {
            Plugin.DecodeRVL(input, output, numPixels);
        }
    }

    public class TemporalRVLEncoder
    {
        private IntPtr _ptr;
        private int _frameSize;

        public TemporalRVLEncoder(int frameSize, short changeThreshold, int invalidThreshold)
        {
            _ptr = Plugin.CreateTemporalRVLEncoder(frameSize, changeThreshold, invalidThreshold);
            _frameSize = frameSize;
        }

        ~TemporalRVLEncoder()
        {
            Plugin.DeleteTemporalRVLEncoder(_ptr);
        }

        public int Encode(ref short[] depthBuffer, ref byte[] output, bool keyFrame)
        {
            Array.Resize(ref output, _frameSize);
            int numBytes = Plugin.EncodeTemporalRVL(_ptr, depthBuffer, output, keyFrame);
            Array.Resize(ref output, numBytes);
            return numBytes;
        }
    }

    public class TemporalRVLDecoder
    {
        private IntPtr _ptr;

        public TemporalRVLDecoder(int frameSize)
        {
            _ptr = Plugin.CreateTemporalRVLDecoder(frameSize);
        }

        ~TemporalRVLDecoder()
        {
            Plugin.DeleteTemporalRVLDecoder(_ptr);
        }

        public void Decode(ref byte[] trvlFrame, ref short[] output, bool keyFrame)
        {
            Plugin.DecodeTemporalRVL(_ptr, trvlFrame, output, keyFrame);
        }
    }
}
