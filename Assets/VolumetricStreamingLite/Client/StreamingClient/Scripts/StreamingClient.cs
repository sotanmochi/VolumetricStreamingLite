// Copyright (c) 2020 Soichiro Sugimoto.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;
using LiteNetLibExtension;

namespace VolumetricStreamingLite.Client
{
    public class StreamingClient : MonoBehaviour
    {
        [SerializeField] LiteNetLibClientMain _liteNetLibClient;

        NetDataWriter _dataWriter;
        public int ClientId { get; private set; }

        void Awake()
        {
            ClientId = -1;
            _dataWriter = new NetDataWriter();
            _liteNetLibClient.OnNetworkReceived += OnNetworkReceived;
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
            }
        }

        public void SendCalibration(int deviceCount, K4A.CalibrationType calibrationType, K4A.Calibration calibration)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream();

            binaryFormatter.Serialize(memoryStream, calibration);
            byte[] serializedCalibration = memoryStream.ToArray();

            _dataWriter.Reset();
            _dataWriter.Put((int)NetworkDataType.SendCalibration);
            _dataWriter.Put(deviceCount);
            _dataWriter.Put((int)calibrationType);
            _dataWriter.Put(serializedCalibration.Length);
            _dataWriter.Put(serializedCalibration);

            _liteNetLibClient.SendData(_dataWriter, DeliveryMethod.ReliableOrdered);
        }

        public void SendDepthData(int deviceNumber, CompressionMethod compressionMethod, byte[] encodedDepthData, 
                                  int depthWidth, int depthHeight, bool isKeyFrame, int frameCount = -1)
        {
            _dataWriter.Reset();
            _dataWriter.Put((int)NetworkDataType.SendDepthData);
            _dataWriter.Put(deviceNumber);
            _dataWriter.Put(frameCount);
            _dataWriter.Put(isKeyFrame);
            _dataWriter.Put(depthWidth);
            _dataWriter.Put(depthHeight);
            _dataWriter.Put((int)compressionMethod);
            _dataWriter.Put(encodedDepthData.Length);
            _dataWriter.Put(encodedDepthData);

            _liteNetLibClient.SendData(_dataWriter, DeliveryMethod.ReliableUnordered);
        }

        public void SendDepthAndColorData(CompressionMethod compressionMethod, byte[] encodedDepthData, int depthWidth, int depthHeight, bool isKeyFrame,
                                          byte[] colorImageData, int colorWidth, int colorHeight, int frameCount = -1)
        {
            SendDepthAndColorData(0, compressionMethod, encodedDepthData, depthWidth, depthHeight, isKeyFrame, colorImageData, colorWidth, colorHeight, frameCount);
        }

        public void SendDepthAndColorData(int deviceNumber, CompressionMethod compressionMethod, byte[] encodedDepthData, int depthWidth, int depthHeight, 
                                          bool isKeyFrame, byte[] colorImageData, int colorWidth, int colorHeight, int frameCount = -1)
        {
            _dataWriter.Reset();
            _dataWriter.Put((int)NetworkDataType.SendDepthAndColorData);
            _dataWriter.Put(deviceNumber);
            _dataWriter.Put(frameCount);
            _dataWriter.Put(isKeyFrame);
            _dataWriter.Put(depthWidth);
            _dataWriter.Put(depthHeight);
            _dataWriter.Put((int)compressionMethod);
            _dataWriter.Put(encodedDepthData.Length);
            _dataWriter.Put(encodedDepthData);
            _dataWriter.Put(colorWidth);
            _dataWriter.Put(colorHeight);
            _dataWriter.Put(colorImageData.Length);
            _dataWriter.Put(colorImageData);

            _liteNetLibClient.SendData(_dataWriter, DeliveryMethod.ReliableUnordered);
        }
    }
}
