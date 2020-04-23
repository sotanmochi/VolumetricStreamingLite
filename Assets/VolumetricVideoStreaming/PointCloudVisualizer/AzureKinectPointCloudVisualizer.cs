using System;
using UnityEngine;
using Microsoft.Azure.Kinect.Sensor;
using AzureKinect4Unity;

namespace VolumetricVideoStreaming
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
                CameraCalibration calibration = _KinectSensor.DeviceCalibration.ColorCameraCalibration;
                float[] parameters = calibration.Intrinsics.Parameters;

                AzureKinectCalibration.Intrinsics intrinsics = new AzureKinectCalibration.Intrinsics();
                intrinsics.cx = parameters[0];
                intrinsics.cy = parameters[1];
                intrinsics.fx = parameters[2];
                intrinsics.fy = parameters[3];
                intrinsics.k1 = parameters[4];
                intrinsics.k2 = parameters[5];
                intrinsics.k3 = parameters[6];
                intrinsics.k4 = parameters[7];
                intrinsics.k5 = parameters[8];
                intrinsics.k6 = parameters[9];
                intrinsics.codx = parameters[10];
                intrinsics.cody = parameters[11];
                intrinsics.p2 = parameters[12]; // p2: tangential distortion coefficient y
                intrinsics.p1 = parameters[13]; // p1: tangential distortion coefficient x
                intrinsics.metricRadius = parameters[14];

                Debug.Log("***** Camera parameters *****");
                Debug.Log(" Intrinsics.cx: " + intrinsics.cx);
                Debug.Log(" Intrinsics.cy: " + intrinsics.cy);
                Debug.Log(" Intrinsics.fx: " + intrinsics.fx);
                Debug.Log(" Intrinsics.fy: " + intrinsics.fy);
                Debug.Log(" Intrinsics.k1: " + intrinsics.k1);
                Debug.Log(" Intrinsics.k2: " + intrinsics.k2);
                Debug.Log(" Intrinsics.k3: " + intrinsics.k3);
                Debug.Log(" Intrinsics.k4: " + intrinsics.k4);
                Debug.Log(" Intrinsics.k5: " + intrinsics.k5);
                Debug.Log(" Intrinsics.k6: " + intrinsics.k6);
                Debug.Log(" Intrinsics.codx: " + intrinsics.codx);
                Debug.Log(" Intrinsics.cody: " + intrinsics.cody);
                Debug.Log(" Intrinsics.p2: " + intrinsics.p2);
                Debug.Log(" Intrinsics.p1: " + intrinsics.p1);
                Debug.Log(" Intrinsics.metricRadius: " + intrinsics.metricRadius);
                Debug.Log("*****************************");

                Debug.Log("ColorResolution: " + _KinectSensor.ColorImageWidth + "x" + _KinectSensor.ColorImageHeight);
                Debug.Log("DepthResolution: " + _KinectSensor.DepthImageWidth + "x" + _KinectSensor.DepthImageHeight);

                _ColorImageTexture = new Texture2D(_KinectSensor.ColorImageWidth, _KinectSensor.ColorImageHeight, TextureFormat.BGRA32, false);

                _TransformedDepthRawData = new byte[_KinectSensor.ColorImageWidth * _KinectSensor.ColorImageHeight * sizeof(ushort)];
                _TransformedDepthImageTexture = new Texture2D(_KinectSensor.ColorImageWidth, _KinectSensor.ColorImageHeight, TextureFormat.R16, false);

                _PointCloudRenderer = GetComponent<PointCloudRenderer>();
                _PointCloudRenderer.GenerateMesh(_KinectSensor.ColorImageWidth, _KinectSensor.ColorImageHeight, intrinsics, calibration.MetricRadius);
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
