// Copyright (c) 2020 Soichiro Sugimoto.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using UnityEngine.UI;

namespace VolumetricStreamingLite.Client
{
    public class StreamingController : MonoBehaviour
    {
        [SerializeField] StreamingService _StreamingService;

        [SerializeField] Shader _DepthVisualizer;
        [SerializeField] Shader _DiffVisualizer;
        [SerializeField] Material _UnlitTextureMaterial;
        [SerializeField] GameObject _DepthImageObject;
        [SerializeField] GameObject _DecodedDepthImageObject;
        [SerializeField] GameObject _DiffImageObject;
        [SerializeField] GameObject _ColorImageObject;

        [SerializeField] Text _DepthImageSize;
        [SerializeField] Text _CompressedDepthImageSize;
        [SerializeField] Text _ColorImageSize;

        [SerializeField] Text _ClientIdText;
        [SerializeField] InputField _ServerAddress;
        [SerializeField] InputField _ServerPort;
        [SerializeField] Button _Connect;
        [SerializeField] Button _Disconnect;
        [SerializeField] Dropdown _FrameRateDropdown;
        [SerializeField] Dropdown _CompressionMethodDropdown;
        [SerializeField] Button _StartStreaming;
        [SerializeField] Button _StopStreaming;

        Texture2D _ColorImageTexture;

        Material _DepthVisualizerMaterial;
        Material _DecodedDepthVisualizerMaterial;
        Material _DepthDiffVisualizerMaterial;
        ComputeBuffer _DepthBuffer;
        ComputeBuffer _DecodedDepthBuffer;
        ComputeBuffer _DepthDiffBuffer;

        void Start()
        {
            _StreamingService.Initialize();

            _StartStreaming.onClick.AddListener(OnClickStartStreaming);
            _StopStreaming.onClick.AddListener(OnClickStopStreaming);
            _Connect.onClick.AddListener(OnClickConnect);
            _Disconnect.onClick.AddListener(OnClickDisconnect);

            _ColorImageTexture = _StreamingService.ColorImageTexture;

            _DepthBuffer = new ComputeBuffer(_StreamingService.DepthImageSize / 2, sizeof(uint));
            _DecodedDepthBuffer = new ComputeBuffer(_StreamingService.DepthImageSize / 2, sizeof(uint));
            _DepthDiffBuffer = new ComputeBuffer(_StreamingService.DepthImageSize / 2, sizeof(uint));

            _DepthVisualizerMaterial = new Material(_DepthVisualizer);
            _DepthVisualizerMaterial.SetInt("_Width", _StreamingService.DepthImageWidth);
            _DepthVisualizerMaterial.SetInt("_Height", _StreamingService.DepthImageHeight);
            MeshRenderer depthMeshRenderer = _DepthImageObject.GetComponent<MeshRenderer>();
            depthMeshRenderer.sharedMaterial = _DepthVisualizerMaterial;

            _DecodedDepthVisualizerMaterial = new Material(_DepthVisualizer);
            _DecodedDepthVisualizerMaterial.SetInt("_Width", _StreamingService.DepthImageWidth);
            _DecodedDepthVisualizerMaterial.SetInt("_Height", _StreamingService.DepthImageHeight);
            MeshRenderer decodedDepthMeshRenderer = _DecodedDepthImageObject.GetComponent<MeshRenderer>();
            decodedDepthMeshRenderer.sharedMaterial = _DecodedDepthVisualizerMaterial;

            _DepthDiffVisualizerMaterial = new Material(_DiffVisualizer);
            _DepthDiffVisualizerMaterial.SetInt("_Width", _StreamingService.DepthImageWidth);
            _DepthDiffVisualizerMaterial.SetInt("_Height", _StreamingService.DepthImageHeight);
            MeshRenderer diffMeshRenderer = _DiffImageObject.GetComponent<MeshRenderer>();
            diffMeshRenderer.sharedMaterial = _DepthDiffVisualizerMaterial;

            MeshRenderer colorMeshRenderer = _ColorImageObject.GetComponent<MeshRenderer>();
            colorMeshRenderer.sharedMaterial = new Material(_UnlitTextureMaterial);
            colorMeshRenderer.sharedMaterial.SetTexture("_MainTex", _ColorImageTexture);
        }

        void Update()
        {
            _ClientIdText.text = "Client ID : " + _StreamingService.ClientId;

            _DepthImageSize.text = string.Format("Size: {2:#,0} [bytes]  Resolution: {0}x{1}",
                                                 _StreamingService.DepthImageWidth, _StreamingService.DepthImageHeight,
                                                 _StreamingService.OriginalDepthDataSize);

            _CompressedDepthImageSize.text = string.Format("Size: {0:#,0} [bytes]  Data compression ratio: {1:F1}",
                                                           _StreamingService.CompressedDepthDataSize,
                                                           _StreamingService.CompressionRatio);

            _ColorImageSize.text = string.Format("Size of jpeg: {0:#,0} [bytes]", _StreamingService.CompressedColorDataSize);

            _DepthBuffer.SetData(_StreamingService.DepthImageData);
            _DepthVisualizerMaterial.SetBuffer("_DepthBuffer", _DepthBuffer);

            _DepthDiffBuffer.SetData(_StreamingService.DepthDiff);
            _DepthDiffVisualizerMaterial.SetBuffer("_DepthBuffer", _DepthDiffBuffer);

            if (_StreamingService.DecodedDepthData != null)
            {
                _DecodedDepthBuffer.SetData(_StreamingService.DecodedDepthData);
                _DecodedDepthVisualizerMaterial.SetBuffer("_DepthBuffer", _DecodedDepthBuffer);
            }
        }

        void OnClickConnect()
        {
            _StreamingService.Connect(_ServerAddress.text, int.Parse(_ServerPort.text));
        }

        void OnClickDisconnect()
        {
            _StreamingService.Disconnect();
        }

        void OnClickStartStreaming()
        {
            int frameRate = 10;

            string selectedFrameRate = _FrameRateDropdown.options[_FrameRateDropdown.value].text;
            switch(selectedFrameRate)
            {
                case "10fps":
                    frameRate = 10;
                    break;
                case "5fps":
                    frameRate = 5;
                    break;
                case "15fps":
                    frameRate = 15;
                    break;
                case "20fps":
                    frameRate = 20;
                    break;
                case "24fps":
                    frameRate = 24;
                    break;
                case "30fps":
                    frameRate = 30;
                    break;
                default :
                    frameRate = 10;
                    break;
            }

            CompressionMethod compressionMethod = (CompressionMethod)_CompressionMethodDropdown.value;

            _StreamingService.StartStreaming(frameRate, compressionMethod);
        }

        void OnClickStopStreaming()
        {
            _StreamingService.StopStreaming();
        }
    }
}
