using UnityEngine;
using UnityEngine.UI;

namespace VolumetricVideoStreaming.Client
{
    public class VolumetricVideoStreamingController : MonoBehaviour
    {
        [SerializeField] VolumetricVideoStreamingService _volumetricVideoStreamingService;
        [SerializeField] GameObject _textureSteamingClientObject;
        ITextureStreamingClient _textureSteamingClient;

        [SerializeField] int _intervalTimeMillisec = 100;

        [SerializeField] Shader _DepthVisualizer;
        [SerializeField] Shader _DiffVisualizer;
        [SerializeField] Material _UnlitTextureMaterial;
        [SerializeField] GameObject _DepthImageObject;
        [SerializeField] GameObject _DecodedDepthImageObject;
        [SerializeField] GameObject _DiffImageObject;
        [SerializeField] GameObject _ColorImageObject;

        [SerializeField] Text _depthImageSize;
        [SerializeField] Text _compressedDepthImageSize;
        [SerializeField] Text _colorImageSize;

        [SerializeField] Text _clientIdText;
        [SerializeField] InputField _serverAddress;
        [SerializeField] InputField _serverPort;
        [SerializeField] Button _connect;
        [SerializeField] Button _disconnect;
        [SerializeField] Button _startStreaming;
        [SerializeField] Button _stopStreaming;

        Texture2D _DepthImageTexture;
        Texture2D _DecodedDepthImageTexture;
        Texture2D _DiffImageTexture;
        Texture2D _ColorImageTexture;

        void Start()
        {
            _textureSteamingClient = _textureSteamingClientObject.GetComponent<ITextureStreamingClient>();
            _volumetricVideoStreamingService.Initialize(_textureSteamingClient, _intervalTimeMillisec);

            _startStreaming.onClick.AddListener(OnClickStartStreaming);
            _stopStreaming.onClick.AddListener(OnClickStopStreaming);
            _connect.onClick.AddListener(OnClickConnect);
            _disconnect.onClick.AddListener(OnClickDisconnect);

            _DepthImageTexture = _volumetricVideoStreamingService.DepthImageTexture;
            _DecodedDepthImageTexture = _volumetricVideoStreamingService.DecodedDepthImageTexture;
            _DiffImageTexture = _volumetricVideoStreamingService.DiffImageTexture;
            _ColorImageTexture = _volumetricVideoStreamingService.ColorImageTexture;

            MeshRenderer depthMeshRenderer = _DepthImageObject.GetComponent<MeshRenderer>();
            depthMeshRenderer.sharedMaterial = new Material(_DepthVisualizer);
            depthMeshRenderer.sharedMaterial.SetTexture("_DepthTex", _DepthImageTexture);

            MeshRenderer decodedDepthMeshRenderer = _DecodedDepthImageObject.GetComponent<MeshRenderer>();
            decodedDepthMeshRenderer.sharedMaterial = new Material(_DepthVisualizer);
            decodedDepthMeshRenderer.sharedMaterial.SetTexture("_DepthTex", _DecodedDepthImageTexture);

            MeshRenderer diffMeshRenderer = _DiffImageObject.GetComponent<MeshRenderer>();
            diffMeshRenderer.sharedMaterial = new Material(_DiffVisualizer);
            diffMeshRenderer.sharedMaterial.SetTexture("_DepthTex", _DiffImageTexture);

            MeshRenderer colorMeshRenderer = _ColorImageObject.GetComponent<MeshRenderer>();
            colorMeshRenderer.sharedMaterial = new Material(_UnlitTextureMaterial);
            colorMeshRenderer.sharedMaterial.SetTexture("_MainTex", _ColorImageTexture);
        }

        void Update()
        {
            _clientIdText.text = "Client ID : " + _textureSteamingClient.ClientId;
            _depthImageSize.text = string.Format("Size: {2:#,0} [bytes]  Resolution: {0}x{1}",
                                                 _DepthImageTexture.width, _DepthImageTexture.height,
                                                 _volumetricVideoStreamingService.OriginalDepthDataSize);
            _compressedDepthImageSize.text = string.Format("Size: {0:#,0} [bytes]  Compression ratio: {1:F2}",
                                                           _volumetricVideoStreamingService.CompressedDepthDataSize,
                                                           _volumetricVideoStreamingService.CompressionRation);
            _colorImageSize.text = string.Format("Size of jpeg: {0:#,0} [bytes]", _volumetricVideoStreamingService.CompressedColorDataSize);
        }

        void OnClickConnect()
        {
            _textureSteamingClient.StartClient(_serverAddress.text,int.Parse(_serverPort.text));
        }

        void OnClickDisconnect()
        {
            _textureSteamingClient.StopClient();
        }

        void OnClickStartStreaming()
        {
            _volumetricVideoStreamingService.StartStreaming();
        }

        void OnClickStopStreaming()
        {
            _volumetricVideoStreamingService.StopStreaming();
        }
    }
}
