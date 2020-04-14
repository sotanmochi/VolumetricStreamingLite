using System;
using UnityEngine;

namespace AzureKinect4Unity
{
    public class AzureKinectSimpleVisualizer : MonoBehaviour
    {
        [SerializeField] AzureKinectManager _AzureKinectManager;
        [SerializeField] Shader _DepthVisualizer;
        [SerializeField] Material _UnlitTextureMaterial;
        [SerializeField] GameObject _ColorImageObject;
        [SerializeField] GameObject _TransformedColorImageObject;
        [SerializeField] GameObject _DepthImageObject;
        [SerializeField] GameObject _TransformedDepthImageObject;

        Texture2D _ColorImageTexture;
        Texture2D _TransformedColorImageTexture;
        Texture2D _DepthImageTexture;
        Texture2D _TransformedDepthImageTexture;

        AzureKinectSensor _KinectSensor;
        byte[] _DepthRawData;
        byte[] _TransformedDepthRawData;

        void Start()
        {
            _KinectSensor = _AzureKinectManager.Sensor;
            if (_KinectSensor != null)
            {
                _DepthRawData = new byte[_KinectSensor.DepthImageWidth * _KinectSensor.DepthImageHeight * sizeof(ushort)];
                _TransformedDepthRawData = new byte[_KinectSensor.ColorImageWidth * _KinectSensor.ColorImageHeight * sizeof(ushort)];

                _ColorImageTexture = new Texture2D(_KinectSensor.ColorImageWidth, _KinectSensor.ColorImageHeight, TextureFormat.BGRA32, false);
                _TransformedColorImageTexture = new Texture2D(_KinectSensor.DepthImageWidth, _KinectSensor.DepthImageHeight, TextureFormat.BGRA32, false);
                _DepthImageTexture = new Texture2D(_KinectSensor.DepthImageWidth, _KinectSensor.DepthImageHeight, TextureFormat.R16, false);
                _TransformedDepthImageTexture = new Texture2D(_KinectSensor.ColorImageWidth, _KinectSensor.ColorImageHeight, TextureFormat.R16, false);

                MeshRenderer colorMeshRenderer = _ColorImageObject.GetComponent<MeshRenderer>();
                colorMeshRenderer.sharedMaterial = new Material(_UnlitTextureMaterial);
                colorMeshRenderer.sharedMaterial.SetTexture("_MainTex", _ColorImageTexture);

                MeshRenderer transformedColorMeshRenderer = _TransformedColorImageObject.GetComponent<MeshRenderer>();
                transformedColorMeshRenderer.sharedMaterial = new Material(_UnlitTextureMaterial);
                transformedColorMeshRenderer.sharedMaterial.SetTexture("_MainTex", _TransformedColorImageTexture);

                MeshRenderer depthMeshRenderer = _DepthImageObject.GetComponent<MeshRenderer>();
                depthMeshRenderer.sharedMaterial = new Material(_DepthVisualizer);
                depthMeshRenderer.sharedMaterial.SetTexture("_DepthTex", _DepthImageTexture);

                MeshRenderer transformedDepthMeshRenderer = _TransformedDepthImageObject.GetComponent<MeshRenderer>();
                transformedDepthMeshRenderer.sharedMaterial = new Material(_DepthVisualizer);
                transformedDepthMeshRenderer.sharedMaterial.SetTexture("_DepthTex", _TransformedDepthImageTexture);
            }

            Debug.Log("ColorResolution: " + _KinectSensor.ColorImageWidth + "x" + _KinectSensor.ColorImageHeight);
            Debug.Log("DepthResolution: " + _KinectSensor.DepthImageWidth + "x" + _KinectSensor.DepthImageHeight);
        }

        void Update()
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
                ushort[] depthImage = _KinectSensor.RawDepthImage;
                Buffer.BlockCopy(depthImage, 0, _DepthRawData, 0, _DepthRawData.Length);
                _DepthImageTexture.LoadRawTextureData(_DepthRawData);
                _DepthImageTexture.Apply();
            }

            if (_KinectSensor.TransformedDepthImage != null)
            {
                ushort[] depthImage = _KinectSensor.TransformedDepthImage;
                Buffer.BlockCopy(depthImage, 0, _TransformedDepthRawData, 0, _TransformedDepthRawData.Length);
                _TransformedDepthImageTexture.LoadRawTextureData(_TransformedDepthRawData);
                _TransformedDepthImageTexture.Apply();
            }
        }
    }
}
