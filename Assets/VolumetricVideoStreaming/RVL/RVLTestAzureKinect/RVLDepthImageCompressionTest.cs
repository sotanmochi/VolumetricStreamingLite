using System;
using UnityEngine;
using AzureKinect4Unity;

namespace RVL.Test
{
    public class RVLDepthImageCompressionTest : MonoBehaviour
    {
        [SerializeField] AzureKinectManager _AzureKinectManager;
        [SerializeField] Shader _DepthVisualizer;
        [SerializeField] Shader _DiffVisualizer;
        [SerializeField] Material _UnlitTextureMaterial;
        [SerializeField] GameObject _DepthImageObject;
        [SerializeField] GameObject _DecodedDepthImageObject;
        [SerializeField] GameObject _DiffImageObject;
        [SerializeField] GameObject _ColorImageObject;

        Texture2D _DepthImageTexture;
        Texture2D _DecodedDepthImageTexture;
        Texture2D _DiffImageTexture;
        Texture2D _ColorImageTexture;

        AzureKinectSensor _KinectSensor;
        byte[] _DepthRawData;
        byte[] _EncodedDepthData;
        ushort[] _DecodedDepthData;
        ushort[] _Diff;

        void Start()
        {
            _KinectSensor = _AzureKinectManager.Sensor;
            if (_KinectSensor != null)
            {
                int depthImageSize = _KinectSensor.DepthImageWidth * _KinectSensor.DepthImageHeight;
                _DepthRawData = new byte[depthImageSize * sizeof(ushort)];
                _EncodedDepthData = new byte[depthImageSize];
                _DecodedDepthData = new ushort[depthImageSize];
                _Diff = new ushort[depthImageSize];

                _DepthImageTexture = new Texture2D(_KinectSensor.DepthImageWidth, _KinectSensor.DepthImageHeight, TextureFormat.R16, false);
                _DecodedDepthImageTexture = new Texture2D(_KinectSensor.DepthImageWidth, _KinectSensor.DepthImageHeight, TextureFormat.R16, false);
                _DiffImageTexture = new Texture2D(_KinectSensor.DepthImageWidth, _KinectSensor.DepthImageHeight, TextureFormat.R16, false);
                _ColorImageTexture = new Texture2D(_KinectSensor.ColorImageWidth, _KinectSensor.ColorImageHeight, TextureFormat.BGRA32, false);

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

                Debug.Log("ColorResolution: " + _KinectSensor.ColorImageWidth + "x" + _KinectSensor.ColorImageHeight);
                Debug.Log("DepthResolution: " + _KinectSensor.DepthImageWidth + "x" + _KinectSensor.DepthImageHeight);

                int numBytes = depthImageSize * sizeof(ushort);
                Debug.Log("********************");
                Debug.Log(" Depth image data size: " + numBytes + " [bytes]");
                Debug.Log("********************");
            }
        }

        void Update()
        {
            if (_KinectSensor.RawDepthImage != null)
            {
                // Visualize original depth image
                ushort[] depthImage = _KinectSensor.RawDepthImage;
                Buffer.BlockCopy(depthImage, 0, _DepthRawData, 0, _DepthRawData.Length * sizeof(byte));
                _DepthImageTexture.LoadRawTextureData(_DepthRawData);
                _DepthImageTexture.Apply();

                // RVL compression
                int encodedDataBytes = RVLDepthImageCompressor.CompressRVL(depthImage, _EncodedDepthData);
                Debug.Log("********************");
                Debug.Log(" Encoded data size: " + encodedDataBytes + " [bytes]");
                Debug.Log(" Compression ratio: " + ((float) (depthImage.Length * sizeof(ushort)) / encodedDataBytes));
                Debug.Log("********************");

                // RVL decompression
                RVLDepthImageCompressor.DecompressRVL(_EncodedDepthData, _DecodedDepthData);

                // Visualize decoded depth image
                Buffer.BlockCopy(_DecodedDepthData, 0, _DepthRawData, 0, _DepthRawData.Length * sizeof(byte));
                _DecodedDepthImageTexture.LoadRawTextureData(_DepthRawData);
                _DecodedDepthImageTexture.Apply();

                // Diff
                for (int i = 0; i < depthImage.Length; i++)
                {
                    _Diff[i] = (ushort)Math.Abs(depthImage[i] - _DecodedDepthData[i]);
                }

                // Visualize diff image
                Buffer.BlockCopy(_Diff, 0, _DepthRawData, 0, _DepthRawData.Length * sizeof(byte));
                _DiffImageTexture.LoadRawTextureData(_DepthRawData);
                _DiffImageTexture.Apply();
            }

            if (_KinectSensor.RawColorImage != null)
            {
                _ColorImageTexture.LoadRawTextureData(_KinectSensor.RawColorImage);
                _ColorImageTexture.Apply();
            }
        }
    }
}
