// Copyright (c) Soichiro Sugimoto.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Microsoft.Azure.Kinect.Sensor;

namespace AzureKinect4Unity
{
    public class AzureKinectManager : MonoBehaviour
    {
        public ImageFormat ColorImageFormat = ImageFormat.ColorBGRA32;
        public ColorCameraMode ColorCameraMode = ColorCameraMode._1920_x_1080_30fps;
        public DepthCameraMode DepthCameraMode = DepthCameraMode._512_x_512_30fps;

        private List<AzureKinectSensor> _AzureKinectSensorList = new List<AzureKinectSensor>();
        public List<AzureKinectSensor> SensorList { get { return _AzureKinectSensorList; } }

        private CancellationTokenSource _CancellationTokenSource;

        private int _MainThreadID;

        void Awake()
        {
            _MainThreadID = Thread.CurrentThread.ManagedThreadId;

            int deviceCount = AzureKinectSensor.GetDeviceCount();
            for (int i = 0; i < deviceCount; i++)
            {
                var kinectSensor = new AzureKinectSensor(ColorImageFormat, ColorCameraMode, DepthCameraMode);
                if (kinectSensor.OpenSensor(i))
                {
                    _AzureKinectSensorList.Add(kinectSensor);
                }
            }

            _CancellationTokenSource = new CancellationTokenSource();
            foreach (var kinectSensor in _AzureKinectSensorList)
            {
                RunAnotherThread(_CancellationTokenSource.Token, kinectSensor);
            }
        }

        void RunAnotherThread(CancellationToken cancellationToken, AzureKinectSensor kinectSensor)
        {
            Task.Run(() =>
            {
                // Multithread
                // Debug.Log("********************");
                // Debug.Log(" MainThreadID: " + _MainThreadID);
                // Debug.Log(" AnotherThreadID: " + Thread.CurrentThread.ManagedThreadId);
                // Debug.Log(" KinectSerialNum: " + kinectSensor.Device.SerialNum);
                // Debug.Log("********************");

                while(true)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    kinectSensor.ProcessCameraFrame();
                }
            });
        }

        void OnApplicationQuit()
        {
            OnDestroy();
        }

        void OnDestroy()
        {
            _CancellationTokenSource.Cancel();

            foreach (var kinectSensor in _AzureKinectSensorList)
            {
                kinectSensor.CloseSensor();
            }
        }
    }
}
