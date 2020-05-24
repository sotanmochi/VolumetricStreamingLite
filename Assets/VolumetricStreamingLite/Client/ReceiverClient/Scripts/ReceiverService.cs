// Copyright (c) 2020 Soichiro Sugimoto.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEngine;
using DepthStreamCompression;

namespace VolumetricStreamingLite.Client
{
    public class ReceiverService : MonoBehaviour
    {
        [SerializeField] ReceiverClient _ReceiverClient;

        public int ClientId { get { return _ReceiverClient.ClientId ;} }
        public OnReceivedCalibrationDelegate OnReceivedCalibration;

        Texture2D _ColorImageTexture;
        public Texture2D ColorImageTexture { get { return _ColorImageTexture; } }

        TemporalRVLDecoder _TrvlDecoder;
        int _DepthImageSize = 0;
        short[] _DecodedDepthData;
        public short[] DepthImageData { get { return _DecodedDepthData; } }
        byte[] _ColorImageData;
        public byte[] ColorImageData { get { return _ColorImageData; } }

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
            _ReceiverClient.OnReceivedCalibration += OnReceivedCalibration;
            _Receiving = true;
        }

        public void StopReceiving()
        {
            _Receiving = false;
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
                _TrvlDecoder = new TemporalRVLDecoder(_DepthImageSize);
            }

            if (_ColorImageTexture == null || 
                _ColorImageTexture.width != _ReceiverClient.ColorWidth ||
                _ColorImageTexture.height != _ReceiverClient.ColorHeight)
            {
                int width = _ReceiverClient.ColorWidth;
                int height = _ReceiverClient.ColorHeight;
                _ColorImageTexture = new Texture2D(width, height, TextureFormat.BGRA32, false);
            }

            Frame frame = _ReceiverClient.GetFrame();
            if (frame != null)
            {
                bool isKeyFrame = frame.IsKeyFrame;
                byte[] encodedDepthData = frame.EncodedDepthData;

                if (frame.CompressionMethod == CompressionMethod.TemporalRVL)
                {
                    // Temporal RVL decompression
                    _DecodedDepthData = _TrvlDecoder.Decode(encodedDepthData, isKeyFrame);
                }
                else if (frame.CompressionMethod == CompressionMethod.RVL)
                {
                    // RVL decompression
                    RVL.DecompressRVL(encodedDepthData, _DecodedDepthData);
                }

                _ColorImageData = frame.ColorImageData;
                _ColorImageTexture.LoadImage(_ColorImageData);
                _ColorImageTexture.Apply();
            }
        }
    }
}
