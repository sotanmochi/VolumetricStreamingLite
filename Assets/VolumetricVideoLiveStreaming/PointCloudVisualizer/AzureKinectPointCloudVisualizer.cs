#define COLOR_TO_DEPTH

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

        Texture2D _TransformedColorImageTexture;
        byte[] _DepthRawData;
        Texture2D _DepthImageTexture;

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

                _TransformedColorImageTexture = new Texture2D(_KinectSensor.DepthImageWidth, _KinectSensor.DepthImageHeight, TextureFormat.BGRA32, false);
                _DepthRawData = new byte[_KinectSensor.DepthImageWidth * _KinectSensor.DepthImageHeight * sizeof(ushort)];
                _DepthImageTexture = new Texture2D(_KinectSensor.DepthImageWidth, _KinectSensor.DepthImageHeight, TextureFormat.R16, false);

                _ColorImageTexture = new Texture2D(_KinectSensor.ColorImageWidth, _KinectSensor.ColorImageHeight, TextureFormat.BGRA32, false);
                _TransformedDepthRawData = new byte[_KinectSensor.ColorImageWidth * _KinectSensor.ColorImageHeight * sizeof(ushort)];
                _TransformedDepthImageTexture = new Texture2D(_KinectSensor.ColorImageWidth, _KinectSensor.ColorImageHeight, TextureFormat.R16, false);

                _PointCloudRenderer = GetComponent<PointCloudRenderer>();

                CameraCalibration deviceDepthCameraCalibration = _KinectSensor.DeviceCalibration.DepthCameraCalibration;
                CameraCalibration deviceColorCameraCalibration = _KinectSensor.DeviceCalibration.ColorCameraCalibration;
                
                K4A.Calibration calibration = new K4A.Calibration();
                calibration.DepthCameraCalibration = CreateCalibrationCamera(deviceDepthCameraCalibration, _KinectSensor.DepthImageWidth, _KinectSensor.DepthImageHeight);
                calibration.ColorCameraCalibration = CreateCalibrationCamera(deviceColorCameraCalibration, _KinectSensor.ColorImageWidth, _KinectSensor.ColorImageHeight);

#if COLOR_TO_DEPTH
                _PointCloudRenderer.GenerateMesh(calibration, K4A.CalibrationType.Depth);
#else
                _PointCloudRenderer.GenerateMesh(calibration, K4A.CalibrationType.Color);
#endif
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

        void Update()
        {
#if COLOR_TO_DEPTH
            if (_KinectSensor.TransformedColorImage != null)
            {
                _TransformedColorImageTexture.LoadRawTextureData(_KinectSensor.TransformedColorImage);
                _TransformedColorImageTexture.Apply();
            }

            if (_KinectSensor.RawDepthImage != null)
            {
                short[] depthImage = _KinectSensor.RawDepthImage;
                Buffer.BlockCopy(depthImage, 0, _DepthRawData, 0, _DepthRawData.Length);
                _DepthImageTexture.LoadRawTextureData(_DepthRawData);
                _DepthImageTexture.Apply();

                _PointCloudRenderer.UpdateColorTexture(_KinectSensor.TransformedColorImage);
                _PointCloudRenderer.UpdateDepthTexture(_DepthRawData);
            }
#else
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
#endif
        }
    }
}
