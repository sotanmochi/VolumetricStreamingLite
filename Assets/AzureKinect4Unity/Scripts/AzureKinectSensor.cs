// Copyright (c) Soichiro Sugimoto.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using Microsoft.Azure.Kinect.Sensor;

namespace AzureKinect4Unity
{
    public class AzureKinectSensor : MonoBehaviour
    {
        public ColorCameraMode ColorCameraMode = ColorCameraMode._1920_x_1080_30fps;
        public DepthCameraMode DepthCameraMode = DepthCameraMode._512_x_512_30fps;

        private Device _KinectSensor;
        public Device Device { get { return _KinectSensor; } }

        private Calibration _DeviceCalibration;
        private Transformation _Transformation;
        private bool _IsCameraStarted = false;

        private int _ColorImageWidth;
        public int ColorImageWidth { get { return _ColorImageWidth; } }
        private int _ColorImageHeight;
        public int ColorImageHeight { get { return _ColorImageHeight; } }
        private int _DepthImageWidth;
        public int DepthImageWidth { get { return _DepthImageWidth; } }
        private int _DepthImageHeight;
        public int DepthImageHeight { get { return _DepthImageHeight; } }

        private byte[] _RawColorImage = null;
        public byte[] RawColorImage { get { return _RawColorImage; } }
        private byte[] _TransformedColorImage = null;
        public byte[] TransformedColorImage { get { return _TransformedColorImage; } }
        private short[] _RawDepthImage = null;
        public short[] RawDepthImage { get { return _RawDepthImage; } }
        private short[] _TransformedDepthImage = null;
        public short[] TransformedDepthImage { get { return _TransformedDepthImage; } }

        public void OpenSensor(int deviceIndex = 0)
        {
            _KinectSensor = Device.Open(deviceIndex);
            if (_KinectSensor == null)
            {
                Debug.LogError("AzureKinect cannot be opened.");
                return;
            }

            DeviceConfiguration kinectConfig = new DeviceConfiguration();
            kinectConfig.ColorFormat = ImageFormat.ColorBGRA32;
            kinectConfig.ColorResolution = (ColorResolution) ColorCameraMode;
            kinectConfig.DepthMode = (DepthMode) DepthCameraMode;

            if (ColorCameraMode != ColorCameraMode._4096_x_3072_15fps
             && DepthCameraMode != DepthCameraMode._1024x1024_15fps)
            {
                kinectConfig.CameraFPS = FPS.FPS30;
            }
            else
            {
                kinectConfig.CameraFPS = FPS.FPS15;
            }

            _KinectSensor.StartCameras(kinectConfig);
            _IsCameraStarted = true;

            _DeviceCalibration = _KinectSensor.GetCalibration();
            _Transformation = _DeviceCalibration.CreateTransformation();

            CameraCalibration colorCamera = _DeviceCalibration.ColorCameraCalibration;
            _ColorImageWidth = colorCamera.ResolutionWidth;
            _ColorImageHeight = colorCamera.ResolutionHeight;

            CameraCalibration depthCamera = _DeviceCalibration.DepthCameraCalibration;
            _DepthImageWidth = depthCamera.ResolutionWidth;
            _DepthImageHeight = depthCamera.ResolutionHeight;

            _RawColorImage = new byte[_ColorImageWidth * _ColorImageHeight * 4];
            _TransformedColorImage = new byte[_DepthImageWidth * _DepthImageHeight * 4];
            _RawDepthImage = new short[_DepthImageWidth * _DepthImageHeight];
            _TransformedDepthImage = new short[_ColorImageWidth * _ColorImageHeight];
        }

        public void CloseSensor()
        {
            if (_KinectSensor != null)
            {
                _IsCameraStarted = false;
                _KinectSensor.StopCameras();

                _KinectSensor.Dispose();
                _KinectSensor = null;
            }
        }

        public void ProcessCameraFrame()
        {
            if (_IsCameraStarted)
            {
                Capture capture = _KinectSensor.GetCapture();

                if (capture.Color != null)
                {
                    _RawColorImage = capture.Color.Memory.ToArray();
                    _TransformedColorImage = _Transformation.ColorImageToDepthCamera(capture).Memory.ToArray();
                }

                if (capture.Depth != null)
                {
                    _RawDepthImage = capture.Depth.GetPixels<short>().ToArray();
                    _TransformedDepthImage = _Transformation.DepthImageToColorCamera(capture).GetPixels<short>().ToArray();
                }

                capture.Dispose();
            }
        }
    }

    public enum ColorCameraMode
    {
        _1280_x_720_30fps = 1,
        _1920_x_1080_30fps = 2,
        _2560_x_1440_30fps = 3,
        _2048_x_1536_30fps = 4,
        _3840_x_2160_30fps = 5,
        _4096_x_3072_15fps = 6
    }

    public enum DepthCameraMode
    {
        _320_x_288_30fps = 1,
        _640_x_576_30fps = 2,
        _512_x_512_30fps = 3,
        _1024x1024_15fps = 4,
        PassiveIROnly_30fps = 5
    }
}
