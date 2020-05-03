using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;
using LiteNetLibExtension;

namespace VolumetricVideoStreaming.Client.LiteNetLib
{
    public class TextureReceiverClientLogic : MonoBehaviour, ITextureReceiverClient
    {
        [SerializeField] LiteNetLibClientMain _liteNetLibClient;

        public int FrameCount { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public byte[] RawTextureData { get; private set;  }

        NetDataWriter _dataWriter;
        public int ClientId { get; private set; }

        void Awake()
        {
            ClientId = -1;
            _dataWriter = new NetDataWriter();
            _liteNetLibClient.OnNetworkReceived += OnNetworkReceived;
            // _liteNetLibClient.StartClient();
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
                else if (networkDataType == NetworkDataType.ReceiveTexture)
                {
                    OnReceivedRawTextureData(peer, reader);
                }
            }
        }

        void OnReceivedRawTextureData(NetPeer peer, NetPacketReader reader)
        {
            int frameCount = reader.GetInt();
            int width = reader.GetInt();
            int height = reader.GetInt();

            int textureDataLength = reader.GetInt();
            byte[] rawTextureData = new byte[textureDataLength];
            reader.GetBytes(rawTextureData, textureDataLength);

            OnReceivedRawTextureData(frameCount, width, height, rawTextureData);
        }

        public void OnReceivedRawTextureData(int frameCount, int width, int height, byte[] rawTextureData)
        {
            FrameCount = frameCount;
            Width = width;
            Height = height;
            RawTextureData = rawTextureData;
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
