// Copyright (c) 2020 Soichiro Sugimoto.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VolumetricStreamingLite.Client
{
    public class MultipleKinectReceiverController : MonoBehaviour
    {
        [SerializeField] MultipleKinectReceiverService _ReceiverService;
        [SerializeField] GameObject _PointCloudRendererPrefab;

        [SerializeField] Text _ClientIdText;
        [SerializeField] InputField _ServerAddress;
        [SerializeField] InputField _ServerPort;
        [SerializeField] Button _Connect;
        [SerializeField] Button _Disconnect;
        [SerializeField] InputField _StreamingClientId;
        [SerializeField] Button _RegisterReceiver;
        [SerializeField] Button _UnregisterReceiver;

        public List<PointCloudRenderer> _PointCloudRendererList = new List<PointCloudRenderer>();

        int _PreviousStreamingClientId;

        void Start()
        {
            _ReceiverService.OnReceivedCalibrationDelegate += OnReceivedCalibration;

            _Connect.onClick.AddListener(OnClickConnect);
            _Disconnect.onClick.AddListener(OnClickDisconnect);
            _RegisterReceiver.onClick.AddListener(OnClickRegisterReceiver);
            _UnregisterReceiver.onClick.AddListener(OnClickUnregisterReceiver);
        }

        void Update()
        {
            _ClientIdText.text = "Client ID : " + _ReceiverService.ClientId;

            for (int i = 0; i < _PointCloudRendererList.Count; i++)
            {
                var pointCloudRenderer = _PointCloudRendererList[i];
                if (_ReceiverService.DepthImageData != null)
                {
                    pointCloudRenderer.UpdateDepthBuffer(_ReceiverService.DepthImageData[i]);
                }
                if (_ReceiverService.ColorImageData != null)
                {
                    pointCloudRenderer.UpdateColorTextureImageByteArray(_ReceiverService.ColorImageData[i]);
                }
            }
        }

        void OnReceivedCalibration(int deviceCount, K4A.CalibrationType calibrationType, K4A.Calibration calibration)
        {
            Debug.Log("DeviceCount: " + deviceCount);
            for (int i = 0; i < deviceCount; i++)
            {
                var gameObject = GameObject.Instantiate(_PointCloudRendererPrefab);
                var pointCloudRenderer = gameObject.GetComponent<PointCloudRenderer>();
                if (pointCloudRenderer != null)
                {
                    pointCloudRenderer.GenerateMesh(calibration, calibrationType);
                    _PointCloudRendererList.Add(pointCloudRenderer);
                }
            }

            Debug.Log("CalibrationType: " + calibrationType);
            
            K4A.CalibrationCamera depthCalibrationCamera = calibration.DepthCameraCalibration;
            K4A.CalibrationCamera colorCalibrationCamera = calibration.ColorCameraCalibration;

            Debug.Log("Calibration.DepthImage: " + depthCalibrationCamera.resolutionWidth + "x" + depthCalibrationCamera.resolutionHeight);
            Debug.Log("Calibration.ColorImage: " + colorCalibrationCamera.resolutionWidth + "x" + colorCalibrationCamera.resolutionHeight);
        }

        void OnClickConnect()
        {
            _ReceiverService.Connect(_ServerAddress.text, int.Parse(_ServerPort.text));
        }

        void OnClickDisconnect()
        {
            _ReceiverService.Disconnect();
        }

        void OnClickRegisterReceiver()
        {
            _ReceiverService.UnregisterReceiverClient(_PreviousStreamingClientId);

            int streamingClientId = int.Parse(_StreamingClientId.text);
            _ReceiverService.RegisterReceiverClient(streamingClientId);

            _PreviousStreamingClientId = streamingClientId;

            _ReceiverService.StartReceiving();
        }

        void OnClickUnregisterReceiver()
        {
            _ReceiverService.StopReceiving();

            int streamingClientId = int.Parse(_StreamingClientId.text);
            _ReceiverService.UnregisterReceiverClient(streamingClientId);
        }
    }
}
