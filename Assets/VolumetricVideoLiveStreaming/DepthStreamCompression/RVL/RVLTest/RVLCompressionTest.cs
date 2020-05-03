using System;
using UnityEngine;

namespace RVL.Test
{
    public class RVLCompressionTest : MonoBehaviour
    {
        void Start()
        {
            EncodingAndDecodingTest();
            // ProcessingTimeTest();
        }

        void EncodingAndDecodingTest()
        {
            int size = 100*100;

            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

            short[] input = new short[size];
            byte[] encoded = new byte[size];
            short[] decoded = new short[size];

            for (int i = 0; i < input.Length; i++)
            {
                input[i] = (short)(0 + i);
            }
            int inputDataBytes = (input.Length * sizeof(short));

            Debug.Log("********************");
            Debug.Log(" Input data size: " + inputDataBytes + " [bytes]");
            Debug.Log("********************");

            stopwatch.Start();

            int encodedDataBytes = RVLDepthImageCompressor.CompressRVL(input, encoded);
            Array.Resize(ref encoded, encodedDataBytes);

            stopwatch.Stop();
            Debug.Log("********************");
            Debug.Log(" Encoding time: " + stopwatch.ElapsedMilliseconds + " [ms]");
            Debug.Log(" Encoded data size: " + encodedDataBytes + " [bytes]");
            Debug.Log(" Compression ratio: " + ((float) inputDataBytes / encodedDataBytes));
            Debug.Log("********************");
            // for (int i = 0; i < encoded.Length; i++)
            // {
            //     Debug.Log(" Encoded[" + i + "]: " + encoded[i]);
            // }

            stopwatch.Reset();
            stopwatch.Start();

            RVLDepthImageCompressor.DecompressRVL(encoded, decoded);

            stopwatch.Stop();
            Debug.Log("********************");
            Debug.Log(" Decoding time: " + stopwatch.ElapsedMilliseconds + " [ms]");
            Debug.Log("********************");
            for (int i = 0; i < input.Length; i++)
            {
                Debug.Log("Input  [" + i + "]: " + input[i]);
                Debug.Log("Decoded[" + i + "]: " + decoded[i]);
            }
        }

        void ProcessingTimeTest()
        {
            int times = 10;
            int size = 512*512;

            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

            short[] input = new short[size];
            byte[] encoded = new byte[size];
            short[] decoded = new short[size];

            for (int i = 0; i < input.Length; i++)
            {
                input[i] = (short)(0 + i);
            }
            int inputDataBytes = (input.Length * sizeof(short));

            Debug.Log("********************");
            Debug.Log(" Input data size: " + inputDataBytes + " [bytes]");
            Debug.Log("********************");

            for (int t = 0; t < times; t++)
            {
                stopwatch.Start();

                int encodedDataBytes = RVLDepthImageCompressor.CompressRVL(input, encoded);
                Array.Resize(ref encoded, encodedDataBytes);

                stopwatch.Stop();
                Debug.Log("********************");
                Debug.Log(" Encoding time: " + stopwatch.ElapsedMilliseconds + " [ms]");
                Debug.Log(" Compression ratio: " + ((float) inputDataBytes / encodedDataBytes));
                Debug.Log("********************");

                stopwatch.Reset();
                stopwatch.Start();

                RVLDepthImageCompressor.DecompressRVL(encoded, decoded);

                stopwatch.Stop();
                Debug.Log("********************");
                Debug.Log(" Decoding time: " + stopwatch.ElapsedMilliseconds + " [ms]");
                Debug.Log("********************");
            }
        }
    }
}
