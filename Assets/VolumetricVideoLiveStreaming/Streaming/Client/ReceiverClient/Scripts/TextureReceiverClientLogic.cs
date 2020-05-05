using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;
using LiteNetLibExtension;

namespace VolumetricVideoStreaming.Client.LiteNetLib
{
    public class TextureReceiverClientLogic : MonoBehaviour, ITextureReceiverClient
    {
        [SerializeField] LiteNetLibClientMain _liteNetLibClient;

        public K4A.Calibration Calibration { get; private set; }
        public K4A.CalibrationType CalibrationType { get; private set; }

        public int FrameCount { get; private set; }
        public bool IsKeyFrame { get; private set; }
        public int DepthWidth { get; private set; }
        public int DepthHeight { get; private set; }
        public int DepthImageSize { get; private set; }
        public CompressionMethod CompressionMethod { get; private set; }
        public byte[] EncodedDepthData { get; private set;  }
        public int ColorWidth { get; private set; }
        public int ColorHeight { get; private set; }
        public byte[] ColorImageData { get; private set;  }

        public OnReceivedCalibrationDelegate OnReceivedCalibration { get; set; }

        NetDataWriter _dataWriter;
        public int ClientId { get; private set; }

        void Awake()
        {
            FrameCount = -1;
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

            CalibrationType = calibrationType;
            Calibration = calibration;

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
            FrameCount = frameCount;
            IsKeyFrame = isKeyFrame;
            DepthWidth = depthWidth;
            DepthHeight = depthHeight;
            CompressionMethod = compressionMethod;
            EncodedDepthData = encodedDepthData;

            DepthImageSize = depthWidth * DepthHeight;
        }

        public void OnReceivedDepthAndColorData(int frameCount, bool isKeyFrame, int depthWidth, int depthHeight,
                                                CompressionMethod compressionMethod, byte[] encodedDepthData, 
                                                int colorWidth, int colorHeight, byte[] colorImageData)
        {
            FrameCount = frameCount;
            IsKeyFrame = isKeyFrame;
            DepthWidth = depthWidth;
            DepthHeight = depthHeight;
            CompressionMethod = compressionMethod;
            EncodedDepthData = encodedDepthData;
            ColorWidth = colorWidth;
            ColorHeight = colorHeight;
            ColorImageData = colorImageData;

            DepthImageSize = depthWidth * DepthHeight;
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
