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
                if (networkDataType == NetworkDataType.SendTexture)
                {
                    SendTextureToReceivers(peer.Id, reader);
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

        void SendTextureToReceivers(int streamingClientId, NetPacketReader reader)
        {
            int frameCount = reader.GetInt();
            int width = reader.GetInt();
            int height = reader.GetInt();
            int textureDataLength = reader.GetInt();

            byte[] rawTextureData = new byte[textureDataLength];
            reader.GetBytes(rawTextureData, textureDataLength);

            _dataWriter.Reset();
            _dataWriter.Put((int)NetworkDataType.ReceiveTexture);
            _dataWriter.Put(frameCount);
            _dataWriter.Put(width);
            _dataWriter.Put(height);
            _dataWriter.Put(rawTextureData.Length);
            _dataWriter.Put(rawTextureData);

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
