// Copyright (c) 2020 Soichiro Sugimoto.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEngine;
using AzureKinect4Unity;
using DepthStreamCompression;
using Microsoft.Azure.Kinect.Sensor;

namespace VolumetricStreamingLite.Client
{
    public class StreamingService : MonoBehaviour
    {
        [SerializeField] AzureKinectManager _AzureKinectManager;
        [SerializeField] int _DeviceNumber = 0;
        [SerializeField] StreamingClient _StreamingClient;

        public int ClientId { get { return _StreamingClient.ClientId ;} }

        Texture2D _DepthImageTexture;
        public Texture2D DepthImageTexture { get { return _DepthImageTexture; } }
        Texture2D _DecodedDepthImageTexture;
        public Texture2D DecodedDepthImageTexture { get { return _DecodedDepthImageTexture; } }
        Texture2D _DiffImageTexture;
        public Texture2D DiffImageTexture { get { return _DiffImageTexture; } }
        Texture2D _ColorImageTexture;
        public Texture2D ColorImageTexture { get { return _ColorImageTexture; } }

        float _CompressionRatio;
        public float CompressionRatio { get { return _CompressionRatio; } }
        int _CompressedDepthDataSize;
        public int CompressedDepthDataSize { get { return _CompressedDepthDataSize; } }
        int _OriginalDepthDataSize;
        public int OriginalDepthDataSize { get { return _OriginalDepthDataSize; } }
        int _CompressedColorDataSize;
        public int CompressedColorDataSize { get { return _CompressedColorDataSize; } }

        bool _Initialized = false;
        bool _Streaming = false;
        float _Timer = 0.0f;

        AzureKinectSensor _KinectSensor;
        K4A.Calibration _Calibration;
        K4A.CalibrationType _CalibrationType;

        TemporalRVLEncoder _TrvlEncoder;
        TemporalRVLDecoder _TrvlDecoder;

        int _DepthImageSize;
        byte[] _DepthRawData;
        byte[] _EncodedDepthData;
        short[] _DecodedDepthData;
        short[] _Diff;
        byte[] _EncodedColorImageData;

        CompressionMethod _DepthCompressionMethod;
        float _IntervalTimeSeconds = 0.01f;
        int _KeyFrameInterval = 0;
        int _FrameCount = 0;
        bool _KeyFrame = false;

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

        public void Initialize()
        {
            var kinectSensors = _AzureKinectManager.SensorList;
            if (_DeviceNumber < kinectSensors.Count)
            {
                _KinectSensor = kinectSensors[_DeviceNumber];
                if (_KinectSensor != null)
                {
                    Debug.Log("ColorResolution: " + _KinectSensor.ColorImageWidth + "x" + _KinectSensor.ColorImageHeight);
                    Debug.Log("DepthResolution: " + _KinectSensor.DepthImageWidth + "x" + _KinectSensor.DepthImageHeight);

                    _DepthImageSize = _KinectSensor.DepthImageWidth * _KinectSensor.DepthImageHeight;
                    _DepthRawData = new byte[_DepthImageSize * sizeof(short)];
                    _Diff = new short[_DepthImageSize];
                    _EncodedColorImageData = new byte[_DepthImageSize];

                    _DepthImageTexture = new Texture2D(_KinectSensor.DepthImageWidth, _KinectSensor.DepthImageHeight, TextureFormat.R16, false);
                    _DecodedDepthImageTexture = new Texture2D(_KinectSensor.DepthImageWidth, _KinectSensor.DepthImageHeight, TextureFormat.R16, false);
                    _DiffImageTexture = new Texture2D(_KinectSensor.DepthImageWidth, _KinectSensor.DepthImageHeight, TextureFormat.R16, false);
                    _ColorImageTexture = new Texture2D(_KinectSensor.DepthImageWidth, _KinectSensor.DepthImageHeight, TextureFormat.BGRA32, false);

                    _TrvlEncoder = new TemporalRVLEncoder(_DepthImageSize, 10, 2);
                    _TrvlDecoder = new TemporalRVLDecoder(_DepthImageSize);

                    CameraCalibration deviceDepthCameraCalibration = _KinectSensor.DeviceCalibration.DepthCameraCalibration;
                    CameraCalibration deviceColorCameraCalibration = _KinectSensor.DeviceCalibration.ColorCameraCalibration;

                    _Calibration = new K4A.Calibration();
                    _Calibration.DepthCameraCalibration = CreateCalibrationCamera(deviceDepthCameraCalibration, _KinectSensor.DepthImageWidth, _KinectSensor.DepthImageHeight);
                    _Calibration.ColorCameraCalibration = CreateCalibrationCamera(deviceColorCameraCalibration, _KinectSensor.ColorImageWidth, _KinectSensor.ColorImageHeight);

                    _CalibrationType = K4A.CalibrationType.Depth; // Color to depth

                    _Initialized = true;
                }
                else
                {
                    Debug.LogError("KinectSensor is null!");
                }
            }
        }

        public void Connect(string address, int port)
        {
            _StreamingClient.StartClient(address, port);
        }

        public void Disconnect()
        {
            _StreamingClient.StopClient();
        }

        public void StartStreaming(int frameRate, CompressionMethod compressionMethod)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            if (_Initialized)
            {
                _StreamingClient.SendCalibration(_CalibrationType, _Calibration);

                _IntervalTimeSeconds = 1.0f / frameRate;
                _KeyFrameInterval = (int) Math.Ceiling(1.0f / _IntervalTimeSeconds);

                _DepthCompressionMethod = compressionMethod;

                _EncodedDepthData = new byte[_DepthImageSize];
                _DecodedDepthData = new short[_DepthImageSize];

                _Streaming = true;

                Debug.Log("***** Start streaming *****");
                Debug.Log(" Interval time: " + _IntervalTimeSeconds + " [sec]");
                Debug.Log(" Key frame interval: " + _KeyFrameInterval + " [frames]");
            }
            else
            {
                Debug.LogError("Volumetric streaming service has not been initialized.");
            }
        }

        public void StopStreaming()
        {
            _Streaming = false;
            Debug.Log("***** Stop Streaming *****");
        }

        void UpdateStreaming()
        {
            if (_KinectSensor.RawDepthImage != null)
            {
                // Original depth image
                short[] depthImage = _KinectSensor.RawDepthImage;
                Buffer.BlockCopy(depthImage, 0, _DepthRawData, 0, _DepthRawData.Length * sizeof(byte));
                _DepthImageTexture.LoadRawTextureData(_DepthRawData);
                _DepthImageTexture.Apply();

                _KeyFrame = (_FrameCount++ % _KeyFrameInterval == 0);

                if (_DepthCompressionMethod == CompressionMethod.TemporalRVL)
                {
                    // Temporal RVL compression
                    _EncodedDepthData = _TrvlEncoder.Encode(depthImage, _KeyFrame);
                    _CompressedDepthDataSize = _EncodedDepthData.Length;                    

                    // Temporal RVL decompression
                    _DecodedDepthData = _TrvlDecoder.Decode(_EncodedDepthData, _KeyFrame);
                }
                else if (_DepthCompressionMethod == CompressionMethod.RVL)
                {
                    // RVL compression
                    _CompressedDepthDataSize = RVL.CompressRVL(depthImage, _EncodedDepthData);                    

                    // RVL decompression
                    RVL.DecompressRVL(_EncodedDepthData, _DecodedDepthData);
                }

                _OriginalDepthDataSize = depthImage.Length * sizeof(ushort);
                _CompressionRatio = ((float) _OriginalDepthDataSize / _CompressedDepthDataSize);

                // Decoded depth image
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

            _StreamingClient.SendDepthAndColorData(_DepthCompressionMethod, _EncodedDepthData, 
                                                   _KinectSensor.DepthImageWidth, _KinectSensor.DepthImageHeight, _KeyFrame,
                                                   _EncodedColorImageData, _ColorImageTexture.width, _ColorImageTexture.height, _FrameCount);
        }

        K4A.CalibrationCamera CreateCalibrationCamera(CameraCalibration cameraCalibration, int width, int height)
        {
            K4A.CalibrationCamera calibrationCamera = new K4A.CalibrationCamera();

            float[] intrinsicsParameters = cameraCalibration.Intrinsics.Parameters;
            Extrinsics extrinsics = cameraCalibration.Extrinsics;

            calibrationCamera.resolutionWidth = width;
            calibrationCamera.resolutionHeight = height;
            calibrationCamera.metricRadius = cameraCalibration.MetricRadius;

            calibrationCamera.intrinsics.cx = intrinsicsParameters[0];
            calibrationCamera.intrinsics.cy = intrinsicsParameters[1];
            calibrationCamera.intrinsics.fx = intrinsicsParameters[2];
            calibrationCamera.intrinsics.fy = intrinsicsParameters[3];
            calibrationCamera.intrinsics.k1 = intrinsicsParameters[4];
            calibrationCamera.intrinsics.k2 = intrinsicsParameters[5];
            calibrationCamera.intrinsics.k3 = intrinsicsParameters[6];
            calibrationCamera.intrinsics.k4 = intrinsicsParameters[7];
            calibrationCamera.intrinsics.k5 = intrinsicsParameters[8];
            calibrationCamera.intrinsics.k6 = intrinsicsParameters[9];
            calibrationCamera.intrinsics.codx = intrinsicsParameters[10];
            calibrationCamera.intrinsics.cody = intrinsicsParameters[11];
            calibrationCamera.intrinsics.p2 = intrinsicsParameters[12]; // p2: tangential distortion coefficient y
            calibrationCamera.intrinsics.p1 = intrinsicsParameters[13]; // p1: tangential distortion coefficient x
            calibrationCamera.intrinsics.metricRadius = intrinsicsParameters[14];

            calibrationCamera.extrinsics.rotation[0][0] = extrinsics.Rotation[0];
            calibrationCamera.extrinsics.rotation[0][1] = extrinsics.Rotation[1];
            calibrationCamera.extrinsics.rotation[0][2] = extrinsics.Rotation[2];
            calibrationCamera.extrinsics.rotation[1][0] = extrinsics.Rotation[3];
            calibrationCamera.extrinsics.rotation[1][1] = extrinsics.Rotation[4];
            calibrationCamera.extrinsics.rotation[1][2] = extrinsics.Rotation[5];
            calibrationCamera.extrinsics.rotation[2][0] = extrinsics.Rotation[6];
            calibrationCamera.extrinsics.rotation[2][1] = extrinsics.Rotation[7];
            calibrationCamera.extrinsics.rotation[2][2] = extrinsics.Rotation[8];
            calibrationCamera.extrinsics.translation[0] = extrinsics.Translation[0];
            calibrationCamera.extrinsics.translation[1] = extrinsics.Translation[1];
            calibrationCamera.extrinsics.translation[2] = extrinsics.Translation[2];

            // Debug.Log("***** Camera parameters *****");
            // Debug.Log(" Intrinsics.cx: " + calibrationCamera.intrinsics.cx);
            // Debug.Log(" Intrinsics.cy: " + calibrationCamera.intrinsics.cy);
            // Debug.Log(" Intrinsics.fx: " + calibrationCamera.intrinsics.fx);
            // Debug.Log(" Intrinsics.fy: " + calibrationCamera.intrinsics.fy);
            // Debug.Log(" Intrinsics.k1: " + calibrationCamera.intrinsics.k1);
            // Debug.Log(" Intrinsics.k2: " + calibrationCamera.intrinsics.k2);
            // Debug.Log(" Intrinsics.k3: " + calibrationCamera.intrinsics.k3);
            // Debug.Log(" Intrinsics.k4: " + calibrationCamera.intrinsics.k4);
            // Debug.Log(" Intrinsics.k5: " + calibrationCamera.intrinsics.k5);
            // Debug.Log(" Intrinsics.k6: " + calibrationCamera.intrinsics.k6);
            // Debug.Log(" Intrinsics.codx: " + calibrationCamera.intrinsics.codx);
            // Debug.Log(" Intrinsics.cody: " + calibrationCamera.intrinsics.cody);
            // Debug.Log(" Intrinsics.p2: " + calibrationCamera.intrinsics.p2);
            // Debug.Log(" Intrinsics.p1: " + calibrationCamera.intrinsics.p1);
            // Debug.Log(" Intrinsics.metricRadius: " + calibrationCamera.intrinsics.metricRadius);
            // Debug.Log(" MetricRadius: " + calibrationCamera.metricRadius);
            // Debug.Log("*****************************");

            // for (int i = 0; i < extrinsics.Rotation.Length; i++)
            // {
            //     Debug.Log(" Extrinsics.R[" + i + "]: " + extrinsics.Rotation[i]);
            // }
            // for (int i = 0; i < extrinsics.Translation.Length; i++)
            // {
            //     Debug.Log(" Extrinsics.T[" + i + "]: " + extrinsics.Translation[i]);
            // }

            // extrinsics = deviceDepthCameraCalibration.Extrinsics;
            // for (int i = 0; i < extrinsics.Rotation.Length; i++)
            // {
            //     Debug.Log(" Extrinsics.R[" + i + "]: " + extrinsics.Rotation[i]);
            // }
            // for (int i = 0; i < extrinsics.Translation.Length; i++)
            // {
            //     Debug.Log(" Extrinsics.T[" + i + "]: " + extrinsics.Translation[i]);
            // }

            return calibrationCamera;
        }
    }
}
