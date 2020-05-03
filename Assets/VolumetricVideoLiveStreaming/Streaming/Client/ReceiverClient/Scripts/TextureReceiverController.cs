using UnityEngine;
using UnityEngine.UI;

namespace VolumetricVideoStreaming.Client
{
    public class TextureReceiverController : MonoBehaviour
    {
        [SerializeField] GameObject _textureReceiverClientObject;
        ITextureReceiverClient _textureReceiverClient;

        [SerializeField] bool _useSkybox;

        [SerializeField] RawImage _rawImage;
        [SerializeField] Text _clientIdText;
        [SerializeField] InputField _serverAddress;
        [SerializeField] InputField _serverPort;
        [SerializeField] Button _connect;
        [SerializeField] Button _disconnect;
        [SerializeField] InputField _streamingClientId;
        [SerializeField] Button _registerReceiver;
        [SerializeField] Button _unregisterReceiver;

        Texture2D _textureData;
        Material _skyboxMaterial;
        int _previousStreamingClientId;

        void Start()
        {
            _textureReceiverClient = _textureReceiverClientObject.GetComponent<ITextureReceiverClient>();

            _skyboxMaterial = RenderSettings.skybox;

            if (_rawImage != null)
            {
                _rawImage.texture = _textureData;
            }
            if (_useSkybox)
            {
                _skyboxMaterial.mainTexture = _textureData;
            }

            _connect.onClick.AddListener(OnClickConnect);
            _disconnect.onClick.AddListener(OnClickDisconnect);
            _registerReceiver.onClick.AddListener(OnClickRegisterReceiver);
            _unregisterReceiver.onClick.AddListener(OnClickUnregisterReceiver);
        }

        void Update()
        {
            _clientIdText.text = "Client ID : " + _textureReceiverClient.ClientId;

            int width = _textureReceiverClient.Width;
            int height = _textureReceiverClient.Height;

            if (_textureData == null 
             || _textureData.width != width || _textureData.height != height)
            {
                _textureData = new Texture2D(width, height);
            }

            if (_textureData != null && _textureReceiverClient.RawTextureData != null)
            {
                _textureData.LoadImage(_textureReceiverClient.RawTextureData);
                _textureData.Apply();
            }

            if (_rawImage != null)
            {
                _rawImage.texture = _textureData;
            }
            if (_useSkybox)
            {
                _skyboxMaterial.mainTexture = _textureData;
            }
        }

        void OnClickConnect()
        {
            _textureReceiverClient.StartClient(_serverAddress.text,int.Parse(_serverPort.text));
        }

        void OnClickDisconnect()
        {
            _textureReceiverClient.StopClient();
        }

        void OnClickRegisterReceiver()
        {
            _textureReceiverClient.UnregisterTextureReceiver(_previousStreamingClientId);

            int streamingClientId = int.Parse(_streamingClientId.text);
            _textureReceiverClient.RegisterTextureReceiver(streamingClientId);

            _previousStreamingClientId = streamingClientId;
        }

        void OnClickUnregisterReceiver()
        {
            int streamingClientId = int.Parse(_streamingClientId.text);
            _textureReceiverClient.UnregisterTextureReceiver(streamingClientId);
        }
    }
}
