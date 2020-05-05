using System;
using UnityEngine;
using DepthStreamCompression;

namespace VolumetricStreamingLite.Client
{
    public class VolumetricVideoReceiverService : MonoBehaviour
    {
        [SerializeField] GameObject _textureReceiverClientObject;
        ITextureReceiverClient _textureReceiverClient;

        Texture2D _DepthImageTexture;
        public Texture2D DepthImageTexture { get { return _DepthImageTexture; } }
        Texture2D _ColorImageTexture;
        public Texture2D ColorImageTexture { get { return _ColorImageTexture; } }

        int _DepthImageSize = 0;
        TemporalRVLDepthStreamDecoder _Decoder;
        short[] _DecodedDepthData;
        byte[] _DepthImageRawData;
        public byte[] DepthImageRawData { get { return _DepthImageRawData; } }
        byte[] _ColorImageData;
        public byte[] ColorImageData { get { return _ColorImageData; } }

        bool _Receiving = false;
        int _ProcessedFrameCount = -1;

        public void Initialize(ITextureReceiverClient textureReceiverClient)
        {
            _textureReceiverClient = textureReceiverClient;
        }

        public void StartReceiving()
        {
            Debug.Log("***** Start Receiving *****");
            _Receiving = true;
        }

        public void StopReceiving()
        {
            _Receiving = false;
            Debug.Log("***** Stop Receiving *****");
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
            if (_DepthImageSize != _textureReceiverClient.DepthImageSize)
            {
                _DepthImageSize = _textureReceiverClient.DepthImageSize;
                _Decoder = new TemporalRVLDepthStreamDecoder(_DepthImageSize);
                _DepthImageRawData = new byte[_DepthImageSize * sizeof(short)];
            }

            if (_DepthImageTexture == null || 
                _DepthImageTexture.width != _textureReceiverClient.DepthWidth ||
                _DepthImageTexture.height != _textureReceiverClient.DepthHeight)
            {
                int width = _textureReceiverClient.DepthWidth;
                int height = _textureReceiverClient.DepthHeight;
                _DepthImageTexture = new Texture2D(width, height, TextureFormat.R16, false);
            }

            if (_ColorImageTexture == null || 
                _ColorImageTexture.width != _textureReceiverClient.ColorWidth ||
                _ColorImageTexture.height != _textureReceiverClient.ColorHeight)
            {
                int width = _textureReceiverClient.ColorWidth;
                int height = _textureReceiverClient.ColorHeight;
                _ColorImageTexture = new Texture2D(width, height, TextureFormat.BGRA32, false);
            }

            if (_textureReceiverClient.FrameCount >= 0 &&
                _ProcessedFrameCount != _textureReceiverClient.FrameCount)
            {
                bool isKeyFrame = _textureReceiverClient.IsKeyFrame;
                byte[] encodedDepthData = _textureReceiverClient.EncodedDepthData;

                // Temporal RVL decompression
                _DecodedDepthData = _Decoder.Decode(encodedDepthData, isKeyFrame);
                // RVL decompression
                // RVLDepthImageCompressor.DecompressRVL(encodedDepthData, _DecodedDepthData);

                Buffer.BlockCopy(_DecodedDepthData, 0, _DepthImageRawData, 0, _DepthImageRawData.Length * sizeof(byte));
                _DepthImageTexture.LoadRawTextureData(_DepthImageRawData);
                _DepthImageTexture.Apply();

                _ColorImageData = _textureReceiverClient.ColorImageData;
                _ColorImageTexture.LoadImage(_ColorImageData);
                _ColorImageTexture.Apply();

                _ProcessedFrameCount = _textureReceiverClient.FrameCount;
            }
        }
    }
}
