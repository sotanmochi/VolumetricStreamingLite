using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;
using LiteNetLibExtension;

namespace VolumetricVideoStreaming.Server.LiteNetLib
{
    public class TextureStreamingServerLogic : MonoBehaviour
    {
        [SerializeField] LiteNetLibServerMain _liteNetLibServer;

        NetDataWriter _dataWriter;
        Dictionary<int, HashSet<int>> _streamingReceivers;

        void Start()
        {
            _dataWriter = new NetDataWriter();
            _streamingReceivers = new Dictionary<int, HashSet<int>>();
            _liteNetLibServer.OnNetworkReceived += OnNetworkReceived;
            _liteNetLibServer.OnPeerConnectedHandler += OnPeerConnected;
            _liteNetLibServer.OnPeerDisconnectedHandler += OnPeerDisconnected;
        }

        void OnPeerConnected(NetPeer peer)
        {
            int connectedClientId = peer.Id;

            if (!_streamingReceivers.ContainsKey(connectedClientId))
            {
                _streamingReceivers.Add(connectedClientId, new HashSet<int>());
            }
        }

        void OnPeerDisconnected(NetPeer peer)
        {
            int disconnectedClientId = peer.Id;

            if (_streamingReceivers.ContainsKey(disconnectedClientId))
            {
                _streamingReceivers.Remove(disconnectedClientId);
            }

            foreach (var streamingReceiver in _streamingReceivers.Values)
            {
                streamingReceiver.Remove(disconnectedClientId);
            }
        }

        void OnNetworkReceived(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            if (reader.UserDataSize >= 4)
            {
                NetworkDataType networkDataType = (NetworkDataType)reader.GetInt();
                if (networkDataType == NetworkDataType.SendCalibration)
                {
                    SendCalibrationToReceivers(peer.Id, reader);
                }
                if (networkDataType == NetworkDataType.SendDepthData)
                {
                    SendDepthDataToReceivers(peer.Id, reader);
                }
                if (networkDataType == NetworkDataType.SendDepthAndColorData)
                {
                    SendDepthAndColorDataToReceivers(peer.Id, reader);
                }
                else if (networkDataType == NetworkDataType.RegisterTextureReceiver)
                {
                    RegisterTextureReceiver(peer.Id, reader);
                }
                else if (networkDataType == NetworkDataType.UnregisterTextureReceiver)
                {
                    UnregisterTextureReceiver(peer.Id, reader);
                }
            }
        }

        void SendCalibrationToReceivers(int streamingClientId, NetPacketReader reader)
        {
            Debug.Log("SendCalibration: " + reader.UserDataSize);

            int dataLength = reader.GetInt();
            byte[] serializedCalibration = new byte[dataLength];
            reader.GetBytes(serializedCalibration, dataLength);

            _dataWriter.Reset();
            _dataWriter.Put((int)NetworkDataType.ReceiveCalibration);
            _dataWriter.Put(serializedCalibration.Length);
            _dataWriter.Put(serializedCalibration);

            foreach (int receiverClientId in _streamingReceivers[streamingClientId])
            {
                _liteNetLibServer.SendData(receiverClientId, _dataWriter, DeliveryMethod.ReliableOrdered);
            }
        }

        void SendDepthDataToReceivers(int streamingClientId, NetPacketReader reader)
        {
            // Debug.Log("SendDepthData: " + reader.UserDataSize);

            int frameCount = reader.GetInt();
            bool isKeyFrame = reader.GetBool();
            int depthWidth = reader.GetInt();
            int depthHeight = reader.GetInt();

            CompressionMethod compressionMethod = (CompressionMethod)reader.GetInt();
            int encodedDepthDataLength = reader.GetInt();
            byte[] encodedDepthData = new byte[encodedDepthDataLength];
            reader.GetBytes(encodedDepthData, encodedDepthDataLength);

            _dataWriter.Reset();
            _dataWriter.Put((int)NetworkDataType.ReceiveDepthData);
            _dataWriter.Put(frameCount);
            _dataWriter.Put(isKeyFrame);
            _dataWriter.Put(depthWidth);
            _dataWriter.Put(depthHeight);
            _dataWriter.Put((int)compressionMethod);
            _dataWriter.Put(encodedDepthData.Length);
            _dataWriter.Put(encodedDepthData);

            foreach (int receiverClientId in _streamingReceivers[streamingClientId])
            {
                _liteNetLibServer.SendData(receiverClientId, _dataWriter, DeliveryMethod.ReliableOrdered);
            }
        }

        void SendDepthAndColorDataToReceivers(int streamingClientId, NetPacketReader reader)
        {
            // Debug.Log("SendDepthAndColorData: " + reader.UserDataSize);

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

            _dataWriter.Reset();
            _dataWriter.Put((int)NetworkDataType.ReceiveDepthAndColorData);
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

            foreach (int receiverClientId in _streamingReceivers[streamingClientId])
            {
                _liteNetLibServer.SendData(receiverClientId, _dataWriter, DeliveryMethod.ReliableOrdered);
            }
        }

        void RegisterTextureReceiver(int receiverClientId, NetPacketReader reader)
        {
            int streamingClientId = reader.GetInt();

            if (_streamingReceivers.ContainsKey(streamingClientId))
            {
                _streamingReceivers[streamingClientId].Add(receiverClientId);
            }
        }

        void UnregisterTextureReceiver(int receiverClientId, NetPacketReader reader)
        {
            int streamingClientId = reader.GetInt();

            if (_streamingReceivers.ContainsKey(streamingClientId))
            {
                _streamingReceivers[streamingClientId].Remove(receiverClientId);
            }
        }
    }
}
