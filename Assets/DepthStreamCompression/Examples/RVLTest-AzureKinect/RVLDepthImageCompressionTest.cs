// This code is licensed under CC0.
// http://creativecommons.org/publicdomain/zero/1.0/deed.ja
// https://creativecommons.org/publicdomain/zero/1.0/deed.en

using System;
using UnityEngine;
using UnityEngine.UI;
using AzureKinect4Unity;

namespace DepthStreamCompression.Test
{
    public class RVLDepthImageCompressionTest : MonoBehaviour
    {
        [SerializeField] AzureKinectManager _AzureKinectManager;
        [SerializeField] int _DeviceNumber = 0;
        [SerializeField] Shader _DepthVisualizer;
        [SerializeField] Shader _DiffVisualizer;
        [SerializeField] Material _UnlitTextureMaterial;
        [SerializeField] GameObject _DepthImageObject;
        [SerializeField] GameObject _DecodedDepthImageObject;
        [SerializeField] GameObject _DiffImageObject;
        [SerializeField] GameObject _ColorImageObject;

        [SerializeField] Text _DepthImageSize;
        [SerializeField] Text _CompressedDepthImageSize;
        [SerializeField] Text _ProcessingTime;

        Texture2D _ColorImageTexture;

        Material _DepthVisualizerMaterial;
        Material _DecodedDepthVisualizerMaterial;
        Material _DepthDiffVisualizerMaterial;
        ComputeBuffer _DepthBuffer;
        ComputeBuffer _DecodedDepthBuffer;
        ComputeBuffer _DepthDiffBuffer;

        AzureKinectSensor _KinectSensor;
        byte[] _EncodedDepthData;
        short[] _DecodedDepthData;
        short[] _Diff;

        System.Diagnostics.Stopwatch _Stopwatch = new System.Diagnostics.Stopwatch();

        void Start()
        {
            var kinectSensors = _AzureKinectManager.SensorList;
            if (_DeviceNumber < kinectSensors.Count)
            {
                _KinectSensor = kinectSensors[_DeviceNumber];
                if (_KinectSensor != null)
                {
                    int depthImageSize = _KinectSensor.DepthImageWidth * _KinectSensor.DepthImageHeight;
                    _EncodedDepthData = new byte[depthImageSize];
                    _DecodedDepthData = new short[depthImageSize];
                    _Diff = new short[depthImageSize];

                    _ColorImageTexture = new Texture2D(_KinectSensor.ColorImageWidth, _KinectSensor.ColorImageHeight, TextureFormat.BGRA32, false);

                    _DepthBuffer = new ComputeBuffer(depthImageSize / 2, sizeof(uint));
                    _DecodedDepthBuffer = new ComputeBuffer(depthImageSize / 2, sizeof(uint));
                    _DepthDiffBuffer = new ComputeBuffer(depthImageSize / 2, sizeof(uint));

                    _DepthVisualizerMaterial = new Material(_DepthVisualizer);
                    _DepthVisualizerMaterial.SetInt("_Width", _KinectSensor.DepthImageWidth);
                    _DepthVisualizerMaterial.SetInt("_Height", _KinectSensor.DepthImageHeight);
                    MeshRenderer depthMeshRenderer = _DepthImageObject.GetComponent<MeshRenderer>();
                    depthMeshRenderer.sharedMaterial = _DepthVisualizerMaterial;

                    _DecodedDepthVisualizerMaterial = new Material(_DepthVisualizer);
                    _DecodedDepthVisualizerMaterial.SetInt("_Width", _KinectSensor.DepthImageWidth);
                    _DecodedDepthVisualizerMaterial.SetInt("_Height", _KinectSensor.DepthImageHeight);
                    MeshRenderer decodedDepthMeshRenderer = _DecodedDepthImageObject.GetComponent<MeshRenderer>();
                    decodedDepthMeshRenderer.sharedMaterial = _DecodedDepthVisualizerMaterial;

                    _DepthDiffVisualizerMaterial = new Material(_DepthVisualizer);
                    _DepthDiffVisualizerMaterial.SetInt("_Width", _KinectSensor.DepthImageWidth);
                    _DepthDiffVisualizerMaterial.SetInt("_Height", _KinectSensor.DepthImageHeight);
                    MeshRenderer diffMeshRenderer = _DiffImageObject.GetComponent<MeshRenderer>();
                    diffMeshRenderer.sharedMaterial = _DepthDiffVisualizerMaterial;

                    MeshRenderer colorMeshRenderer = _ColorImageObject.GetComponent<MeshRenderer>();
                    colorMeshRenderer.sharedMaterial = new Material(_UnlitTextureMaterial);
                    colorMeshRenderer.sharedMaterial.SetTexture("_MainTex", _ColorImageTexture);

                    Debug.Log("ColorResolution: " + _KinectSensor.ColorImageWidth + "x" + _KinectSensor.ColorImageHeight);
                    Debug.Log("DepthResolution: " + _KinectSensor.DepthImageWidth + "x" + _KinectSensor.DepthImageHeight);
                }
            }
        }

        void Update()
        {
            if (_KinectSensor != null)
            {
                if (_KinectSensor.RawDepthImage != null)
                {
                    // Visualize original depth image
                    short[] depthImage = _KinectSensor.RawDepthImage;
                    _DepthBuffer.SetData(depthImage);
                    _DepthVisualizerMaterial.SetBuffer("_DepthBuffer", _DepthBuffer);

                    _Stopwatch.Reset();
                    _Stopwatch.Start();

                    // RVL compression
                    // int encodedDataBytes = RVL.CompressRVL(depthImage, _EncodedDepthData);
                    int encodedDataBytes = NativePlugin.RVL.EncodeRVL(ref depthImage, ref _EncodedDepthData, depthImage.Length);
                    // Debug.Log("Encoded.Length: " + _EncodedDepthData.Length);

                    _Stopwatch.Stop();
                    long encodingTimeMillseconds = _Stopwatch.ElapsedMilliseconds;

                    _Stopwatch.Reset();
                    _Stopwatch.Start();

                    // RVL decompression
                    // RVL.DecompressRVL(_EncodedDepthData, _DecodedDepthData);
                    NativePlugin.RVL.DecodeRVL(ref _EncodedDepthData, ref _DecodedDepthData, _DecodedDepthData.Length);

                    _Stopwatch.Stop();
                    long decodingTimeMillseconds = _Stopwatch.ElapsedMilliseconds;

                    // Visualize decoded depth image
                    _DecodedDepthBuffer.SetData(_DecodedDepthData);
                    _DecodedDepthVisualizerMaterial.SetBuffer("_DepthBuffer", _DecodedDepthBuffer);

                    // Difference of depth images
                    for (int i = 0; i < depthImage.Length; i++)
                    {
                        _Diff[i] = (short)Math.Abs(depthImage[i] - _DecodedDepthData[i]);
                    }

                    // Visualize diff image
                    _DepthDiffBuffer.SetData(_Diff);
                    _DepthDiffVisualizerMaterial.SetBuffer("_DepthBuffer", _DepthDiffBuffer);

                    // Display info
                    int originalDepthDataSize = depthImage.Length * sizeof(short);
                    int compressedDepthDataSize = encodedDataBytes;
                    float compressionRatio = originalDepthDataSize / compressedDepthDataSize;

                    _DepthImageSize.text = string.Format("Size: {2:#,0} [bytes]  Resolution: {0}x{1}",
                                                        _KinectSensor.DepthImageWidth, _KinectSensor.DepthImageHeight, originalDepthDataSize);
                    _CompressedDepthImageSize.text = string.Format("Size: {0:#,0} [bytes]  Data compression ratio: {1:F1}",
                                                                compressedDepthDataSize, compressionRatio);
                    _ProcessingTime.text = string.Format("Processing time:\n Encode: {0} [ms]\n Decode: {1} [ms]",
                                                        encodingTimeMillseconds, decodingTimeMillseconds);
                }

                if (_KinectSensor.RawColorImage != null)
                {
                    _ColorImageTexture.LoadRawTextureData(_KinectSensor.RawColorImage);
                    _ColorImageTexture.Apply();
                }               
            }
        }
    }
}
