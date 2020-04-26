using System;
using UnityEngine;
using Microsoft.Azure.Kinect.Sensor;
using AzureKinect4Unity;

namespace VolumetricVideoLiveStreaming
{
    [RequireComponent(typeof(PointCloudRenderer))]
    public class AzureKinectPointCloudVisualizer : MonoBehaviour
    {
        [SerializeField] AzureKinectManager _AzureKinectManager;

        AzureKinectSensor _KinectSensor;

        PointCloudRenderer _PointCloudRenderer;
        Texture2D _ColorImageTexture;

        byte[] _TransformedDepthRawData;
        Texture2D _TransformedDepthImageTexture;

        void Start()
        {
            _KinectSensor = _AzureKinectManager.Sensor;
            if (_KinectSensor != null)
            {
                Debug.Log("ColorResolution: " + _KinectSensor.ColorImageWidth + "x" + _KinectSensor.ColorImageHeight);
                Debug.Log("DepthResolution: " + _KinectSensor.DepthImageWidth + "x" + _KinectSensor.DepthImageHeight);

                _ColorImageTexture = new Texture2D(_KinectSensor.ColorImageWidth, _KinectSensor.ColorImageHeight, TextureFormat.BGRA32, false);

                _TransformedDepthRawData = new byte[_KinectSensor.ColorImageWidth * _KinectSensor.ColorImageHeight * sizeof(ushort)];
                _TransformedDepthImageTexture = new Texture2D(_KinectSensor.ColorImageWidth, _KinectSensor.ColorImageHeight, TextureFormat.R16, false);

                _PointCloudRenderer = GetComponent<PointCloudRenderer>();

                CameraCalibration deviceColorCameraCalibration = _KinectSensor.DeviceCalibration.ColorCameraCalibration;
                float[] parameters = deviceColorCameraCalibration.Intrinsics.Parameters;

                K4A.CalibrationCamera cameraCalibration = new K4A.CalibrationCamera();
                cameraCalibration.resolutionWidth = _KinectSensor.ColorImageWidth;
                cameraCalibration.resolutionHeight = _KinectSensor.ColorImageHeight;
                cameraCalibration.metricRadius = deviceColorCameraCalibration.MetricRadius;
                cameraCalibration.intrinsics.cx = parameters[0];
                cameraCalibration.intrinsics.cy = parameters[1];
                cameraCalibration.intrinsics.fx = parameters[2];
                cameraCalibration.intrinsics.fy = parameters[3];
                cameraCalibration.intrinsics.k1 = parameters[4];
                cameraCalibration.intrinsics.k2 = parameters[5];
                cameraCalibration.intrinsics.k3 = parameters[6];
                cameraCalibration.intrinsics.k4 = parameters[7];
                cameraCalibration.intrinsics.k5 = parameters[8];
                cameraCalibration.intrinsics.k6 = parameters[9];
                cameraCalibration.intrinsics.codx = parameters[10];
                cameraCalibration.intrinsics.cody = parameters[11];
                cameraCalibration.intrinsics.p2 = parameters[12]; // p2: tangential distortion coefficient y
                cameraCalibration.intrinsics.p1 = parameters[13]; // p1: tangential distortion coefficient x
                cameraCalibration.intrinsics.metricRadius = parameters[14];

                Debug.Log("***** Camera parameters *****");
                Debug.Log(" Intrinsics.cx: " + cameraCalibration.intrinsics.cx);
                Debug.Log(" Intrinsics.cy: " + cameraCalibration.intrinsics.cy);
                Debug.Log(" Intrinsics.fx: " + cameraCalibration.intrinsics.fx);
                Debug.Log(" Intrinsics.fy: " + cameraCalibration.intrinsics.fy);
                Debug.Log(" Intrinsics.k1: " + cameraCalibration.intrinsics.k1);
                Debug.Log(" Intrinsics.k2: " + cameraCalibration.intrinsics.k2);
                Debug.Log(" Intrinsics.k3: " + cameraCalibration.intrinsics.k3);
                Debug.Log(" Intrinsics.k4: " + cameraCalibration.intrinsics.k4);
                Debug.Log(" Intrinsics.k5: " + cameraCalibration.intrinsics.k5);
                Debug.Log(" Intrinsics.k6: " + cameraCalibration.intrinsics.k6);
                Debug.Log(" Intrinsics.codx: " + cameraCalibration.intrinsics.codx);
                Debug.Log(" Intrinsics.cody: " + cameraCalibration.intrinsics.cody);
                Debug.Log(" Intrinsics.p2: " + cameraCalibration.intrinsics.p2);
                Debug.Log(" Intrinsics.p1: " + cameraCalibration.intrinsics.p1);
                Debug.Log(" Intrinsics.metricRadius: " + cameraCalibration.intrinsics.metricRadius);
                Debug.Log(" MetricRadius: " + cameraCalibration.metricRadius);
                Debug.Log("*****************************");

                K4A.Calibration calibration = new K4A.Calibration();
                calibration.ColorCameraCalibration = cameraCalibration;

                _PointCloudRenderer.GenerateMesh(calibration, K4A.CalibrationType.Color);
            }
        }

        void Update()
        {
            if (_KinectSensor.RawColorImage != null)
            {
                _ColorImageTexture.LoadRawTextureData(_KinectSensor.RawColorImage);
                _ColorImageTexture.Apply();
            }

            if (_KinectSensor.TransformedDepthImage != null)
            {
                short[] depthImage = _KinectSensor.TransformedDepthImage;
                Buffer.BlockCopy(depthImage, 0, _TransformedDepthRawData, 0, _TransformedDepthRawData.Length);
                _TransformedDepthImageTexture.LoadRawTextureData(_TransformedDepthRawData);
                _TransformedDepthImageTexture.Apply();

                _PointCloudRenderer.UpdateColorTexture(_KinectSensor.RawColorImage);
                _PointCloudRenderer.UpdateDepthTexture(_TransformedDepthRawData);
            }
        }
    }
}
