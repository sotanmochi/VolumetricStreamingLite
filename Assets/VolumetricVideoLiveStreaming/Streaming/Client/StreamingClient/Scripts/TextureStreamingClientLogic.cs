using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;
using LiteNetLibExtension;

namespace VolumetricVideoStreaming.Client.LiteNetLib
{
    public class TextureStreamingClientLogic : MonoBehaviour, ITextureStreamingClient
    {
        [SerializeField] LiteNetLibClientMain _liteNetLibClient;

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
            }
        }

        public void BroadcastRawTextureData(byte[] rawTextureData, int width, int height, int frameCount = -1)
        {
            _dataWriter.Reset();
            _dataWriter.Put((int)NetworkDataType.SendTexture);
            _dataWriter.Put(frameCount);
            _dataWriter.Put(width);
            _dataWriter.Put(height);
            _dataWriter.Put(rawTextureData.Length);
            _dataWriter.Put(rawTextureData);

            _liteNetLibClient.SendData(_dataWriter, DeliveryMethod.ReliableOrdered);
        }
    }
}
