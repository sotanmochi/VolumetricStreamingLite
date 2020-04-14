// Copyright (c) Soichiro Sugimoto.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace AzureKinect4Unity
{
    [RequireComponent(typeof(AzureKinectSensor))]
    public class AzureKinectManager : MonoBehaviour
    {
        private AzureKinectSensor _AzureKinectSensor;
        public AzureKinectSensor Sensor { get { return _AzureKinectSensor; } }

        private CancellationTokenSource _CancellationTokenSource;

        private int _MainThreadID;
        private int _AnotherThreadID;

        void Awake()
        {
            _MainThreadID = Thread.CurrentThread.ManagedThreadId;

            _AzureKinectSensor = gameObject.GetComponent<AzureKinectSensor>();
            _AzureKinectSensor.OpenSensor();

            if (_AzureKinectSensor != null)
            {
                _CancellationTokenSource = new CancellationTokenSource();
                RunAnotherThread(_CancellationTokenSource.Token);
            }
        }

        void RunAnotherThread(CancellationToken cancellationToken)
        {
            Task.Run(() =>
            {
                _AnotherThreadID = Thread.CurrentThread.ManagedThreadId;

                while(true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    _AzureKinectSensor.ProcessCameraFrame();

                    // Multithread
                    // Debug.Log("MainThreadID: " + _MainThreadID);
                    // Debug.Log("AnotherThreadID: " + _AnotherThreadID);
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
            _AzureKinectSensor.CloseSensor();
        }
    }
}
