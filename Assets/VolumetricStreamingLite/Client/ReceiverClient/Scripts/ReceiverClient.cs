// Copyright (c) 2020 Soichiro Sugimoto.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;
using LiteNetLibExtension;

namespace VolumetricStreamingLite.Client
{
    public delegate void OnReceivedCalibrationDelegate(K4A.CalibrationType calibrationType, K4A.Calibration calibration);

    public class Frame
    {
        public int FrameCount;
        public bool IsKeyFrame;
        public CompressionMethod CompressionMethod;
        public byte[] EncodedDepthData;
        public byte[] ColorImageData;

        public Frame(int frameCount, bool isKeyFrame, CompressionMethod compressionMethod, byte[] encodedDepthData, byte[] colorImageData)
        {
            this.FrameCount = frameCount;
            this.IsKeyFrame = isKeyFrame;
            this.CompressionMethod = compressionMethod;
            this.EncodedDepthData = encodedDepthData;
            this.ColorImageData = colorImageData;
        }
    }

    public class ReceiverClient : MonoBehaviour
    {
        [SerializeField] LiteNetLibClientMain _liteNetLibClient;

        public int ClientId { get; private set; }
        public OnReceivedCalibrationDelegate OnReceivedCalibration;

        public int DepthWidth { get; private set; }
        public int DepthHeight { get; private set; }
        public int DepthImageSize { get; private set; }
        public int ColorWidth { get; private set; }
        public int ColorHeight { get; private set; }

        NetDataWriter _dataWriter;
        Queue<Frame> _frameQueue;
        int _frameCount = -1;

        void Awake()
        {
            ClientId = -1;
            _liteNetLibClient.OnNetworkReceived += OnNetworkReceived;
            _dataWriter = new NetDataWriter();
            _frameQueue = new Queue<Frame>();
        }

        public bool StartClient(string address, int port)
        {
            return _liteNetLibClient.StartClient(address, port);
        }

        public void StopClient()
        {
            _liteNetLibClient.StopClient();
            ClientId = -1;
        }

        void OnNetworkReceived(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            if (reader.UserDataSize >= 4)
            {
                NetworkDataType networkDataType = (NetworkDataType)reader.GetInt();
                if (networkDataType == NetworkDataType.ReceiveOwnCliendId)
                {
                    ClientId = reader.GetInt();
                    Debug.Log("Own Client ID : " + ClientId);
                }
                else if (networkDataType == NetworkDataType.ReceiveCalibration)
                {
                    OnReceivedCalibrationHandler(peer, reader);
                }
                else if (networkDataType == NetworkDataType.ReceiveDepthData)
                {
                    OnReceivedDepthData(peer, reader);
                }
                else if (networkDataType == NetworkDataType.ReceiveDepthAndColorData)
                {
                    OnReceivedDepthAndColorData(peer, reader);
                }
            }
        }

        void OnReceivedCalibrationHandler(NetPeer peer, NetPacketReader reader)
        {
            Debug.Log("OnReceivedCalibration");

            K4A.CalibrationType calibrationType = (K4A.CalibrationType)reader.GetInt();
            Debug.Log("OnReceivedCalibrationType: " + calibrationType);

            int dataLength = reader.GetInt();
            byte[] serializedCalibration = new byte[dataLength];
            reader.GetBytes(serializedCalibration, dataLength);

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream(serializedCalibration);

            K4A.Calibration calibration = (K4A.Calibration)binaryFormatter.Deserialize(memoryStream);

            OnReceivedCalibration?.Invoke(calibrationType, calibration);
        }

        void OnReceivedDepthData(NetPeer peer, NetPacketReader reader)
        {
            int frameCount = reader.GetInt();
            bool isKeyFrame = reader.GetBool();
            int depthWidth = reader.GetInt();
            int depthHeight = reader.GetInt();

            CompressionMethod compressionMethod = (CompressionMethod)reader.GetInt();
            int encodedDepthDataLength = reader.GetInt();
            byte[] encodedDepthData = new byte[encodedDepthDataLength];
            reader.GetBytes(encodedDepthData, encodedDepthDataLength);

            OnReceivedDepthData(frameCount, isKeyFrame, depthWidth, depthHeight, compressionMethod, encodedDepthData);
        }

        void OnReceivedDepthAndColorData(NetPeer peer, NetPacketReader reader)
        {
            int frameCount = reader.GetInt();
            bool isKeyFrame = reader.GetBool();
            int depthWidth = reader.GetInt();
            int depthHeight = reader.GetInt();

            CompressionMethod compressionMethod = (CompressionMethod)reader.GetInt();
            int encodedDepthDataLength = reader.GetInt();
            byte[] encodedDepthData = new byte[encodedDepthDataLength];
            reader.GetBytes(encodedDepthData, encodedDepthDataLength);

            int colorWidth = reader.GetInt();
            int colorHeight = reader.GetInt();

            int colorImageDataLength = reader.GetInt();
            byte[] colorImageData = new byte[colorImageDataLength];
            reader.GetBytes(colorImageData, colorImageDataLength);

            OnReceivedDepthAndColorData(frameCount, isKeyFrame, depthWidth, depthHeight, compressionMethod, encodedDepthData,
                                        colorWidth, colorHeight, colorImageData);
        }

        public void OnReceivedDepthData(int frameCount, bool isKeyFrame, int depthWidth, int depthHeight, 
                                        CompressionMethod compressionMethod, byte[] encodedDepthData)
        {
            DepthWidth = depthWidth;
            DepthHeight = depthHeight;
            DepthImageSize = depthWidth * DepthHeight;

            _frameQueue.Enqueue(new Frame(frameCount, isKeyFrame, compressionMethod, encodedDepthData, null));
        }

        public void OnReceivedDepthAndColorData(int frameCount, bool isKeyFrame, int depthWidth, int depthHeight,
                                                CompressionMethod compressionMethod, byte[] encodedDepthData, 
                                                int colorWidth, int colorHeight, byte[] colorImageData)
        {
            DepthWidth = depthWidth;
            DepthHeight = depthHeight;
            DepthImageSize = depthWidth * DepthHeight;
            ColorWidth = colorWidth;
            ColorHeight = colorHeight;

            if (frameCount > _frameCount)
            {
                _frameCount = frameCount;
                _frameQueue.Enqueue(new Frame(frameCount, isKeyFrame, compressionMethod, encodedDepthData, colorImageData));
            }
            else
            {
                Debug.Log("Frame: " + frameCount + "has been delayed.");
            }
        }

        public Frame GetFrame()
        {
            if (_frameQueue.Count > 0)
            {
                return _frameQueue.Dequeue();
            }
            else
            {
                return null;
            }
        }

        public void RegisterTextureReceiver(int streamingClientId)
        {
            _dataWriter.Reset();
            _dataWriter.Put((int)NetworkDataType.RegisterTextureReceiver);
            _dataWriter.Put(streamingClientId);
            _liteNetLibClient.SendData(_dataWriter, DeliveryMethod.ReliableOrdered);
        }

        public void UnregisterTextureReceiver(int streamingClientId)
        {
            _dataWriter.Reset();
            _dataWriter.Put((int)NetworkDataType.UnregisterTextureReceiver);
            _dataWriter.Put(streamingClientId);
            _liteNetLibClient.SendData(_dataWriter, DeliveryMethod.ReliableOrdered);
        }
    }
}
