// Copyright (c) 2020 Soichiro Sugimoto.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using UnityEngine;
using DepthStreamCompression;
// using DepthStreamCompression.NativePlugin;

namespace VolumetricStreamingLite.Client
{
    public class MultipleKinectReceiverService : MonoBehaviour
    {
        [SerializeField] ReceiverClient _ReceiverClient;

        public int ClientId { get { return _ReceiverClient.ClientId ;} }
        public OnReceivedCalibrationDelegate OnReceivedCalibrationDelegate;

        List<TemporalRVLDecoder> _TrvlDecoders = new List<TemporalRVLDecoder>();
        int _DepthImageSize = 0;
        List<short[]> _DecodedDepthData;
        public List<short[]> DepthImageData => _DecodedDepthData;
        List<byte[]> _ColorImageData;
        public List<byte[]> ColorImageData => _ColorImageData;

        bool _Receiving = false;

        public void Connect(string address, int port)
        {
            _ReceiverClient.StartClient(address, port);
        }

        public void Disconnect()
        {
            _ReceiverClient.StopClient();
        }

        public void StartReceiving()
        {
            Debug.Log("***** Start Receiving *****");
            _ReceiverClient.OnReceivedCalibration += OnReceivedCalibrationDelegate;
            _ReceiverClient.OnReceivedCalibration += OnReceivedCalibration;
            _Receiving = true;
        }

        public void StopReceiving()
        {
            _Receiving = false;
            _ReceiverClient.OnReceivedCalibration -= OnReceivedCalibrationDelegate;
            _ReceiverClient.OnReceivedCalibration -= OnReceivedCalibration;
            Debug.Log("***** Stop Receiving *****");
        }

        public void RegisterReceiverClient(int streamingClientId)
        {
            _ReceiverClient.RegisterTextureReceiver(streamingClientId);
        }

        public void UnregisterReceiverClient(int streamingClientId)
        {
            _ReceiverClient.UnregisterTextureReceiver(streamingClientId);
        }

        void OnReceivedCalibration(int deviceCount, K4A.CalibrationType calibrationType, K4A.Calibration calibration)
        {
            int depthImageSize = calibration.DepthCameraCalibration.resolutionWidth * calibration.DepthCameraCalibration.resolutionHeight;
            _DecodedDepthData = new List<short[]>(deviceCount);
            _ColorImageData = new List<byte[]>(deviceCount);
            for (int i = 0; i < deviceCount; i++)
            {
                _DecodedDepthData.Add(new short[depthImageSize]);
                _ColorImageData.Add(new byte[0]);
            }
        }

        void Update()
        {
            if (_Receiving)
            {
                UpdateReceiving();
            }
        }

        void UpdateReceiving()
        {
            if (_DepthImageSize != _ReceiverClient.DepthImageSize)
            {
                _DepthImageSize = _ReceiverClient.DepthImageSize;
                _TrvlDecoders.Clear();

                for (int deviceNumber = 0; deviceNumber < _ReceiverClient.DeviceCount; deviceNumber++)
                {
                    _TrvlDecoders.Add(new TemporalRVLDecoder(_DepthImageSize));
                }
            }

            for (int deviceNumber = 0; deviceNumber < _ReceiverClient.DeviceCount; deviceNumber++)
            {
                Frame frame = _ReceiverClient.GetFrame(deviceNumber);
                if (frame != null)
                {
                    bool isKeyFrame = frame.IsKeyFrame;
                    byte[] encodedDepthData = frame.EncodedDepthData;

                    if (frame.CompressionMethod == CompressionMethod.TemporalRVL)
                    {
                        _DecodedDepthData[deviceNumber] = _TrvlDecoders[deviceNumber].Decode(encodedDepthData, isKeyFrame);
                        // short[] output = _DecodedDepthData[deviceNumber];
                        // _TrvlDecoders[deviceNumber].Decode(ref encodedDepthData, ref output, isKeyFrame);
                    }
                    else if (frame.CompressionMethod == CompressionMethod.RVL)
                    {
                        // RVL decompression
                        RVL.DecompressRVL(encodedDepthData, _DecodedDepthData[deviceNumber]);
                    }

                    _ColorImageData[deviceNumber] = frame.ColorImageData;
                }
            }
        }
    }
}
