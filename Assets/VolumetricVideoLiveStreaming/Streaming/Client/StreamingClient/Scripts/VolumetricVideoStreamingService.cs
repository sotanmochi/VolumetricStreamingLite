using System;
using UnityEngine;
using AzureKinect4Unity;
using TemporalRVL;
using RVL;

namespace VolumetricVideoStreaming.Client
{
    public class VolumetricVideoStreamingService : MonoBehaviour
    {
        [SerializeField] AzureKinectManager _AzureKinectManager;

        Texture2D _DepthImageTexture;
        public Texture2D DepthImageTexture { get { return _DepthImageTexture; } }
        Texture2D _DecodedDepthImageTexture;
        public Texture2D DecodedDepthImageTexture { get { return _DecodedDepthImageTexture; } }
        Texture2D _DiffImageTexture;
        public Texture2D DiffImageTexture { get { return _DiffImageTexture; } }
        Texture2D _ColorImageTexture;
        public Texture2D ColorImageTexture { get { return _ColorImageTexture; } }

        float _CompressionRatio;
        public float CompressionRation { get { return _CompressionRatio; } }
        int _CompressedDepthDataSize;
        public int CompressedDepthDataSize { get { return _CompressedDepthDataSize; } }
        int _OriginalDepthDataSize;
        public int OriginalDepthDataSize { get { return _OriginalDepthDataSize; } }
        int _CompressedColorDataSize;
        public int CompressedColorDataSize { get { return _CompressedColorDataSize; } }

        AzureKinectSensor _KinectSensor;
        byte[] _DepthRawData;
        byte[] _EncodedDepthData;
        short[] _DecodedDepthData;
        short[] _Diff;
        byte[] _EncodedColorImageData;

        TemporalRVLDepthStreamEncoder _Encoder;
        TemporalRVLDepthStreamDecoder _Decoder;

        ITextureStreamingClient _TextureStreamingClient;
        float _IntervalTimeSeconds = 100;
        int _KeyFrameInterval = 0;
        int _FrameCount = 0;
        float _Timer = 0.0f;
        bool _Initialized = false;
        bool _Streaming = false;

        public void Initialize(ITextureStreamingClient textureStreamingClient)
        {
            _KinectSensor = _AzureKinectManager.Sensor;
            if (_KinectSensor != null)
            {
                Debug.Log("ColorResolution: " + _KinectSensor.ColorImageWidth + "x" + _KinectSensor.ColorImageHeight);
                Debug.Log("DepthResolution: " + _KinectSensor.DepthImageWidth + "x" + _KinectSensor.DepthImageHeight);

                int depthImageSize = _KinectSensor.DepthImageWidth * _KinectSensor.DepthImageHeight;
                _DepthRawData = new byte[depthImageSize * sizeof(ushort)];
                _EncodedDepthData = new byte[depthImageSize];
                _DecodedDepthData = new short[depthImageSize];
                _Diff = new short[depthImageSize];
                _EncodedColorImageData = new byte[depthImageSize];

                _DepthImageTexture = new Texture2D(_KinectSensor.DepthImageWidth, _KinectSensor.DepthImageHeight, TextureFormat.R16, false);
                _DecodedDepthImageTexture = new Texture2D(_KinectSensor.DepthImageWidth, _KinectSensor.DepthImageHeight, TextureFormat.R16, false);
                _DiffImageTexture = new Texture2D(_KinectSensor.DepthImageWidth, _KinectSensor.DepthImageHeight, TextureFormat.R16, false);
                _ColorImageTexture = new Texture2D(_KinectSensor.DepthImageWidth, _KinectSensor.DepthImageHeight, TextureFormat.BGRA32, false);

                // int colorImageSize = _KinectSensor.ColorImageWidth * _KinectSensor.ColorImageHeight;
                // _EncodedColorImageData = new byte[depthImageSize];
                // _ColorImageTexture = new Texture2D(_KinectSensor.ColorImageWidth, _KinectSensor.ColorImageHeight, TextureFormat.BGRA32, false);

                _Encoder = new TemporalRVLDepthStreamEncoder(depthImageSize, 10, 2);
                _Decoder = new TemporalRVLDepthStreamDecoder(depthImageSize);

                _TextureStreamingClient = textureStreamingClient;
                _Initialized = true;
            }
        }

        public void StartStreaming(int frameRate)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            if (_Initialized)
            {
                _IntervalTimeSeconds = 1.0f / frameRate;
                _KeyFrameInterval = (int)Math.Ceiling(1.0f / _IntervalTimeSeconds);
                _Streaming = true;
                Debug.Log("***** Start streaming *****");
                Debug.Log(" Interval time: " + _IntervalTimeSeconds + "[sec]");
                Debug.Log(" Key frame interval: " + _KeyFrameInterval + "[frames]");
            }
            else
            {
                Debug.LogError("Volumetric video streaming service has not been initialized.");
            }
        }

        public void StopStreaming()
        {
            _Streaming = false;
            Debug.Log("***** Stop Streaming *****");
        }

        void Update()
        {
            if (_Streaming)
            {
                _Timer += Time.deltaTime;
                if(_Timer >= _IntervalTimeSeconds)
                {
                    UpdateStreaming();
                    _Timer = _Timer - _IntervalTimeSeconds;
                }
            }
        }

        void UpdateStreaming()
        {
            if (_KinectSensor.RawDepthImage != null)
            {
                // Visualize original depth image
                short[] depthImage = _KinectSensor.RawDepthImage;
                Buffer.BlockCopy(depthImage, 0, _DepthRawData, 0, _DepthRawData.Length * sizeof(byte));
                _DepthImageTexture.LoadRawTextureData(_DepthRawData);
                _DepthImageTexture.Apply();

                bool keyFrame = (_FrameCount++ % _KeyFrameInterval == 0);

                // Temporal RVL compression
                _EncodedDepthData = _Encoder.Encode(depthImage, keyFrame);
                _CompressedDepthDataSize = _EncodedDepthData.Length;
                // RVL compression
                // _CompressedDepthDataSize = RVLDepthImageCompressor.CompressRVL(depthImage, _EncodedDepthData);

                _OriginalDepthDataSize = depthImage.Length * sizeof(ushort);
                _CompressionRatio = ((float) _OriginalDepthDataSize / _CompressedDepthDataSize);

                // Temporal RVL decompression
                _DecodedDepthData = _Decoder.Decode(_EncodedDepthData, keyFrame);
                // RVL decompression
                // RVLDepthImageCompressor.DecompressRVL(_EncodedDepthData, _DecodedDepthData);

                // Visualize decoded depth image
                Buffer.BlockCopy(_DecodedDepthData, 0, _DepthRawData, 0, _DepthRawData.Length * sizeof(byte));
                _DecodedDepthImageTexture.LoadRawTextureData(_DepthRawData);
                _DecodedDepthImageTexture.Apply();

                // Difference of original and decoded image
                for (int i = 0; i < depthImage.Length; i++)
                {
                    _Diff[i] = (short)Math.Abs(depthImage[i] - _DecodedDepthData[i]);
                }

                // Visualize diff image
                Buffer.BlockCopy(_Diff, 0, _DepthRawData, 0, _DepthRawData.Length * sizeof(byte));
                _DiffImageTexture.LoadRawTextureData(_DepthRawData);
                _DiffImageTexture.Apply();
            }

            if (_KinectSensor.TransformedColorImage != null)
            {
                _ColorImageTexture.LoadRawTextureData(_KinectSensor.TransformedColorImage);
                _ColorImageTexture.Apply();
                _EncodedColorImageData = ImageConversion.EncodeToJPG(_ColorImageTexture);
                _CompressedColorDataSize = _EncodedColorImageData.Length;
            }
            // if (_KinectSensor.RawColorImage != null)
            // {
            //     _ColorImageTexture.LoadRawTextureData(_KinectSensor.RawColorImage);
            //     _ColorImageTexture.Apply();
            //     _EncodedColorImageData = ImageConversion.EncodeToJPG(_ColorImageTexture);
            //     _CompressedColorDataSize = _EncodedColorImageData.Length;
            // }
        }
    }
}
