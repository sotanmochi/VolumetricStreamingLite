// Copyright (c) 2020 Soichiro Sugimoto.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using UnityEngine.Rendering;

namespace VolumetricStreamingLite
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class PointCloudRenderer : MonoBehaviour
    {
        [SerializeField] Shader _DepthPointCloud;
        private Mesh _Mesh;
        private Texture2D _ColorTexture;
        private Texture2D _DepthTexture;
        private ComputeBuffer _DepthBuffer;
        private Material _DepthPointCloudMaterial;

        public void UpdateVertices(Vector3[] vertices)
        {
            _Mesh.vertices = vertices;
        }

        public void UpdateColorTexture(byte[] rawTextureData)
        {
            _ColorTexture.LoadRawTextureData(rawTextureData);
            _ColorTexture.Apply();
        }

        public void UpdateColorTextureImageByteArray(byte[] colorImageData)
        {
            _ColorTexture.LoadImage(colorImageData);
            _ColorTexture.Apply();
        }

        public void UpdateDepthTexture(byte[] depthImageRawData)
        {
            _DepthTexture.LoadRawTextureData(depthImageRawData);
            _DepthTexture.Apply();
        }

        public void UpdateDepthBuffer(short[] depthImageData)
        {
            _DepthBuffer.SetData(depthImageData);
            _DepthPointCloudMaterial.SetBuffer("_DepthBuffer", _DepthBuffer);
        }

        public void GenerateMesh(K4A.Calibration calibration, K4A.CalibrationType calibrationType)
        {
            int width = 0;
            int height = 0;
            K4A.CalibrationExtrinsics extrinsics = null;

            switch (calibrationType)
            {
                case K4A.CalibrationType.Depth:    
                    width = calibration.DepthCameraCalibration.resolutionWidth;
                    height = calibration.DepthCameraCalibration.resolutionHeight;

                    extrinsics = calibration.DepthCameraCalibration.extrinsics;
                    extrinsics = calibration.ColorCameraCalibration.extrinsics;
                    extrinsics.translation[0] /= 1000.0f; // [millimeters] -> [meters]
                    extrinsics.translation[1] /= 1000.0f; // [millimeters] -> [meters]
                    extrinsics.translation[2] /= 1000.0f; // [millimeters] -> [meters]

                    break;
                case K4A.CalibrationType.Color:
                    width = calibration.ColorCameraCalibration.resolutionWidth;
                    height = calibration.ColorCameraCalibration.resolutionHeight;

                    extrinsics = calibration.DepthCameraCalibration.extrinsics;

                    break;
                default:
                    Debug.LogError("Unexpected camera calibration type," + 
                                   "should either be K4A.CalibrationType.Depth or K4A.CalibrationType.Color");
                    return;
            }

            if (width <= 0 || height <= 0)
            {
                Debug.LogError("Camera resolution is invalid: " + width + "x" + height);
                return;
            }

            if (_Mesh != null)
            {
                _Mesh.Clear();
            }
            else
            {
                _Mesh = new Mesh();
                _Mesh.indexFormat = IndexFormat.UInt32;
            }

            float[] srcXYZ = new float[3];
            float[] dstXYZ = new float[3];

            Vector3[] vertices = new Vector3[width * height];
            float[] xyTables;
            if (K4A.Transformation.InitXyTables(calibration, calibrationType, out xyTables))
            {
                int pixelCount = 0;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int index = 2 * pixelCount;

                        srcXYZ[0] = xyTables[index]; 
                        srcXYZ[1] = xyTables[index + 1];
                        srcXYZ[2] = 1.0f;

                        K4A.ExtrinsicTransformation.ApplyExtrinsicTransformation(extrinsics, srcXYZ, ref dstXYZ);

                        vertices[x + y * width] = new Vector3(dstXYZ[0], dstXYZ[1], dstXYZ[2]);
                        pixelCount++;
                    }
                }
            }
            else
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        vertices[x + y * width] = new Vector3(0f, 0f, 0f);
                    }
                }
            }
            _Mesh.vertices = vertices;

            Vector2[] uv = new Vector2[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    uv[x + y * width].x = x / (float)width;
                    uv[x + y * width].y = y / (float)height;
                }
            }
            _Mesh.uv = uv;

            int[] indices = new int[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                indices[i] = i;
            }
            _Mesh.SetIndices(indices, MeshTopology.Points, 0, false);

            GetComponent<MeshFilter>().mesh = _Mesh;

            _DepthPointCloudMaterial = new Material(_DepthPointCloud);
            GetComponent<MeshRenderer>().sharedMaterial = _DepthPointCloudMaterial;

            InitializeColorTexture(width, height);
            InitializeDepthTexture(width, height);
        }

        private void InitializeColorTexture(int width, int height)
        {
            _ColorTexture = new Texture2D(width, height, TextureFormat.BGRA32, false, true)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point,
            };

            _DepthPointCloudMaterial.SetTexture("_MainTex", _ColorTexture);
        }

        private void InitializeDepthTexture(int width, int height)
        {
            _DepthTexture = new Texture2D(width, height, TextureFormat.R16, false, true)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point,
            };

            int depthImageSize = width * height;
            _DepthBuffer = new ComputeBuffer(depthImageSize / 2, sizeof(uint));

            _DepthPointCloudMaterial.SetInt("_Width", width);
            _DepthPointCloudMaterial.SetInt("_Height", height);
            _DepthPointCloudMaterial.SetBuffer("_DepthBuffer", _DepthBuffer);
        }
    }
}
