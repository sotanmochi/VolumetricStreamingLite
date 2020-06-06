// Copyright (c) 2020 Soichiro Sugimoto.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using UnityEngine;
using AzureKinect4Unity;
using DepthStreamCompression.NativePlugin;
using Microsoft.Azure.Kinect.Sensor;

namespace VolumetricStreamingLite.Client
{
    public class MultipleKinectStreamingService : MonoBehaviour
    {
        [SerializeField] AzureKinectManager _AzureKinectManager;
        [SerializeField] StreamingClient _StreamingClient;

        public int DeviceCount = 1;
        public int ClientId => _StreamingClient.ClientId;

        List<Texture2D> _ColorImageTexture;
        public List<Texture2D> ColorImageTexture => _ColorImageTexture;

        int _DepthImageWidth;
        public int DepthImageWidth => _DepthImageWidth;
        int _DepthImageHeight;
        public int DepthImageHeight => _DepthImageHeight;
        int _DepthImageSize;
        public int DepthImageSize => _DepthImageSize;

        short[] _DepthImageData;
        public short[] DepthImageData => _DepthImageData;

        List<short[]> _DepthDataList = new List<short[]>();
        public List<short[]> DepthDataList => _DepthDataList;

        // float _CompressionRatio;
        // public float CompressionRatio { get { return _CompressionRatio; } }
        // int _CompressedDepthDataSize;
        // public int CompressedDepthDataSize { get { return _CompressedDepthDataSize; } }
        // int _OriginalDepthDataSize;
        // public int OriginalDepthDataSize { get { return _OriginalDepthDataSize; } }
        // int _CompressedColorDataSize;
        // public int CompressedColorDataSize { get { return _CompressedColorDataSize; } }

        bool _Initialized = false;
        bool _Streaming = false;
        float _Timer = 0.0f;

        K4A.Calibration _Calibration;
        public K4A.Calibration Calibration => _Calibration;
        K4A.CalibrationType _CalibrationType;
        public K4A.CalibrationType CalibrationType => _CalibrationType;

        AzureKinectSensor _KinectSensor;

        List<TemporalRVLEncoder> _TrvlEncoders = new List<TemporalRVLEncoder>();

        byte[] _EncodedDepthData;
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
            _KinectSensor = _AzureKinectManager.SensorList[0];
            if (_KinectSensor != null)
            {
                Debug.Log("ColorResolution: " + _KinectSensor.ColorImageWidth + "x" + _KinectSensor.ColorImageHeight);
                Debug.Log("DepthResolution: " + _KinectSensor.DepthImageWidth + "x" + _KinectSensor.DepthImageHeight);

                _DepthImageWidth = _KinectSensor.DepthImageWidth;
                _DepthImageHeight = _KinectSensor.DepthImageHeight;
                _DepthImageSize = _KinectSensor.DepthImageWidth * _KinectSensor.DepthImageHeight;

                CameraCalibration deviceDepthCameraCalibration = _KinectSensor.DeviceCalibration.DepthCameraCalibration;
                CameraCalibration deviceColorCameraCalibration = _KinectSensor.DeviceCalibration.ColorCameraCalibration;

                _Calibration = new K4A.Calibration();
                _Calibration.DepthCameraCalibration = CreateCalibrationCamera(deviceDepthCameraCalibration, _KinectSensor.DepthImageWidth, _KinectSensor.DepthImageHeight);
                _Calibration.ColorCameraCalibration = CreateCalibrationCamera(deviceColorCameraCalibration, _KinectSensor.ColorImageWidth, _KinectSensor.ColorImageHeight);

                _CalibrationType = K4A.CalibrationType.Depth; // Color to depth

                var kinectSensors = _AzureKinectManager.SensorList;

                for (int i = 0; i < kinectSensors.Count; i++)
                {
                    _TrvlEncoders.Add(new TemporalRVLEncoder(_DepthImageSize, 10, 2));
                }

                for (int i = 0; i < kinectSensors.Count; i++)
                {
                    _DepthDataList.Add(new short[_DepthImageSize]);
                }
                _DepthImageData = new short[_DepthImageSize];
                _EncodedColorImageData = new byte[_DepthImageSize];

                _ColorImageTexture = new List<Texture2D>();
                for (int i = 0; i < kinectSensors.Count; i++)
                {
                    _ColorImageTexture.Add(new Texture2D(_KinectSensor.DepthImageWidth, _KinectSensor.DepthImageHeight, TextureFormat.BGRA32, false));
                }

                _Initialized = true;
            }
            else
            {
                Debug.LogError("KinectSensor is null!");
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
                int deviceCount = Mathf.Min(DeviceCount, _AzureKinectManager.SensorList.Count);
                _StreamingClient.SendCalibration(deviceCount, _CalibrationType, _Calibration);

                _IntervalTimeSeconds = 1.0f / frameRate;
                _KeyFrameInterval = (int) Math.Ceiling(1.0f / _IntervalTimeSeconds);

                _DepthCompressionMethod = compressionMethod;

                _EncodedDepthData = new byte[_DepthImageSize];

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
            _KeyFrame = (_FrameCount++ % _KeyFrameInterval == 0);

            var kinectSensors = _AzureKinectManager.SensorList;
            for (int deviceNumber = 0; (deviceNumber < kinectSensors.Count) && (deviceNumber < DeviceCount); deviceNumber++)
            {
                var KinectSensor = kinectSensors[deviceNumber];
                if (KinectSensor.RawDepthImage != null)
                {
                    // Original depth image
                    _DepthImageData = KinectSensor.RawDepthImage;

                    if (_DepthCompressionMethod == CompressionMethod.TemporalRVL)
                    {
                        // Temporal RVL compression
                        _TrvlEncoders[deviceNumber].Encode(ref _DepthImageData, ref _EncodedDepthData, _KeyFrame);
                    }
                    else if (_DepthCompressionMethod == CompressionMethod.RVL)
                    {
                        // RVL compression
                        RVL.EncodeRVL(ref _DepthImageData, ref _EncodedDepthData, _DepthImageData.Length);
                    }

                    Buffer.BlockCopy(_DepthImageData, 0, _DepthDataList[deviceNumber], 0, _DepthImageData.Length * sizeof(short));
                }

                if (KinectSensor.TransformedColorImage != null)
                {
                    _ColorImageTexture[deviceNumber].LoadRawTextureData(KinectSensor.TransformedColorImage);
                    _ColorImageTexture[deviceNumber].Apply();
                    _EncodedColorImageData = ImageConversion.EncodeToJPG(_ColorImageTexture[deviceNumber]);
                }

                _StreamingClient.SendDepthAndColorData(deviceNumber, _DepthCompressionMethod, _EncodedDepthData, 
                                                       KinectSensor.DepthImageWidth, KinectSensor.DepthImageHeight, _KeyFrame,
                                                       _EncodedColorImageData, _ColorImageTexture[deviceNumber].width, _ColorImageTexture[deviceNumber].height, _FrameCount);
            }
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
