using UnityEngine;
using UnityEngine.UI;

namespace VolumetricVideoStreaming.Client
{
    public class TextureReceiverController : MonoBehaviour
    {
        [SerializeField] VolumetricVideoReceiverService _volumetricVideoReceiverService;
        [SerializeField] GameObject _textureReceiverClientObject;
        ITextureReceiverClient _textureReceiverClient;

        [SerializeField] Shader _DepthVisualizer;
        [SerializeField] Material _UnlitTextureMaterial;
        [SerializeField] GameObject _DepthImageObject;
        [SerializeField] GameObject _ColorImageObject;

        [SerializeField] Text _clientIdText;
        [SerializeField] InputField _serverAddress;
        [SerializeField] InputField _serverPort;
        [SerializeField] Button _connect;
        [SerializeField] Button _disconnect;
        [SerializeField] InputField _streamingClientId;
        [SerializeField] Button _registerReceiver;
        [SerializeField] Button _unregisterReceiver;

        int _previousStreamingClientId;
        Texture2D _DepthImageTexture;
        MeshRenderer _DepthMeshRenderer;
        Texture2D _ColorImageTexture;
        MeshRenderer _ColorMeshRenderer;

        void Start()
        {
            _textureReceiverClient = _textureReceiverClientObject.GetComponent<ITextureReceiverClient>();
            _volumetricVideoReceiverService.Initialize(_textureReceiverClient);

            _connect.onClick.AddListener(OnClickConnect);
            _disconnect.onClick.AddListener(OnClickDisconnect);
            _registerReceiver.onClick.AddListener(OnClickRegisterReceiver);
            _unregisterReceiver.onClick.AddListener(OnClickUnregisterReceiver);

            _DepthMeshRenderer = _DepthImageObject.GetComponent<MeshRenderer>();
            _DepthMeshRenderer.sharedMaterial = new Material(_DepthVisualizer);

            _ColorMeshRenderer = _ColorImageObject.GetComponent<MeshRenderer>();
            _ColorMeshRenderer.sharedMaterial = new Material(_UnlitTextureMaterial);
        }

        void Update()
        {
            _clientIdText.text = "Client ID : " + _textureReceiverClient.ClientId;

            _DepthImageTexture = _volumetricVideoReceiverService.DepthImageTexture;
            _ColorImageTexture = _volumetricVideoReceiverService.ColorImageTexture;

            _DepthMeshRenderer.sharedMaterial.SetTexture("_DepthTex", _DepthImageTexture);
            _ColorMeshRenderer.sharedMaterial.SetTexture("_MainTex", _ColorImageTexture);
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

            _volumetricVideoReceiverService.StartReceiving();
        }

        void OnClickUnregisterReceiver()
        {
            _volumetricVideoReceiverService.StopReceiving();

            int streamingClientId = int.Parse(_streamingClientId.text);
            _textureReceiverClient.UnregisterTextureReceiver(streamingClientId);
        }
    }
}
