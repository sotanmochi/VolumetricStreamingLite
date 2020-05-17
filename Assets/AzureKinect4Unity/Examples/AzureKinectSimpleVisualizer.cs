using System;
using UnityEngine;

namespace AzureKinect4Unity
{
    public class AzureKinectSimpleVisualizer : MonoBehaviour
    {
        [SerializeField] AzureKinectManager _AzureKinectManager;
        [SerializeField] int _DeviceNumber = 0;
        [SerializeField] Shader _DepthVisualizer;
        [SerializeField] Material _UnlitTextureMaterial;
        [SerializeField] GameObject _ColorImageObject;
        [SerializeField] GameObject _TransformedColorImageObject;
        [SerializeField] GameObject _DepthImageObject;
        [SerializeField] GameObject _TransformedDepthImageObject;

        Texture2D _ColorImageTexture;
        Texture2D _TransformedColorImageTexture;

        Material _DepthVisualizerMaterial;
        Material _TransformedDepthVisualizerMaterial;
        ComputeBuffer _DepthBuffer;
        ComputeBuffer _TransformedDepthBuffer;

        AzureKinectSensor _KinectSensor;

        void Start()
        {
            var kinectSensors = _AzureKinectManager.SensorList;
            if (_DeviceNumber < kinectSensors.Count)
            {
                _KinectSensor = kinectSensors[_DeviceNumber];
                if (_KinectSensor != null)
                {
                    _ColorImageTexture = new Texture2D(_KinectSensor.ColorImageWidth, _KinectSensor.ColorImageHeight, TextureFormat.BGRA32, false);
                    _TransformedColorImageTexture = new Texture2D(_KinectSensor.DepthImageWidth, _KinectSensor.DepthImageHeight, TextureFormat.BGRA32, false);

                    int depthImageSize = _KinectSensor.DepthImageWidth * _KinectSensor.DepthImageHeight;
                    int transformedDepthImageSize = _KinectSensor.ColorImageWidth * _KinectSensor.ColorImageHeight;

                    _DepthBuffer = new ComputeBuffer(depthImageSize / 2, sizeof(uint));
                    _TransformedDepthBuffer = new ComputeBuffer(transformedDepthImageSize / 2, sizeof(uint));

                    MeshRenderer colorMeshRenderer = _ColorImageObject.GetComponent<MeshRenderer>();
                    colorMeshRenderer.sharedMaterial = new Material(_UnlitTextureMaterial);
                    colorMeshRenderer.sharedMaterial.SetTexture("_MainTex", _ColorImageTexture);

                    MeshRenderer transformedColorMeshRenderer = _TransformedColorImageObject.GetComponent<MeshRenderer>();
                    transformedColorMeshRenderer.sharedMaterial = new Material(_UnlitTextureMaterial);
                    transformedColorMeshRenderer.sharedMaterial.SetTexture("_MainTex", _TransformedColorImageTexture);

                    _DepthVisualizerMaterial = new Material(_DepthVisualizer);
                    _DepthVisualizerMaterial.SetInt("_Width", _KinectSensor.DepthImageWidth);
                    _DepthVisualizerMaterial.SetInt("_Height", _KinectSensor.DepthImageHeight);
                    MeshRenderer depthMeshRenderer = _DepthImageObject.GetComponent<MeshRenderer>();
                    depthMeshRenderer.sharedMaterial = _DepthVisualizerMaterial;

                    _TransformedDepthVisualizerMaterial = new Material(_DepthVisualizer);
                    _TransformedDepthVisualizerMaterial.SetInt("_Width", _KinectSensor.ColorImageWidth);
                    _TransformedDepthVisualizerMaterial.SetInt("_Height", _KinectSensor.ColorImageHeight);
                    MeshRenderer transformedDepthMeshRenderer = _TransformedDepthImageObject.GetComponent<MeshRenderer>();
                    transformedDepthMeshRenderer.sharedMaterial = _TransformedDepthVisualizerMaterial;
                }

                Debug.Log("ColorResolution: " + _KinectSensor.ColorImageWidth + "x" + _KinectSensor.ColorImageHeight);
                Debug.Log("DepthResolution: " + _KinectSensor.DepthImageWidth + "x" + _KinectSensor.DepthImageHeight);
            }
        }

        void OnDestroy()
        {
            _DepthBuffer?.Dispose();
            _TransformedDepthBuffer?.Dispose();
        }

        void Update()
        {
            if (_KinectSensor != null)
            {
                if (_KinectSensor.RawColorImage != null)
                {
                    _ColorImageTexture.LoadRawTextureData(_KinectSensor.RawColorImage);
                    _ColorImageTexture.Apply();
                }

                if (_KinectSensor.TransformedColorImage != null)
                {
                    _TransformedColorImageTexture.LoadRawTextureData(_KinectSensor.TransformedColorImage);
                    _TransformedColorImageTexture.Apply();
                }

                if (_KinectSensor.RawDepthImage != null)
                {
                    _DepthBuffer.SetData(_KinectSensor.RawDepthImage);
                    _DepthVisualizerMaterial.SetBuffer("_DepthBuffer", _DepthBuffer);
                }

                if (_KinectSensor.TransformedDepthImage != null)
                {
                    _TransformedDepthBuffer.SetData(_KinectSensor.TransformedDepthImage);
                    _TransformedDepthVisualizerMaterial.SetBuffer("_DepthBuffer", _TransformedDepthBuffer);
                }
            }
        }
    }
}
