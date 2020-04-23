// Copyright (c) Soichiro Sugimoto.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using UnityEngine.Rendering;

namespace VolumetricVideoStreaming
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class PointCloudRenderer : MonoBehaviour
    {
        private Mesh _Mesh;
        private Texture2D _ColorTexture;
        private Texture2D _DepthTexture;

        public void UpdateVertices(Vector3[] vertices)
        {
            _Mesh.vertices = vertices;
        }

        public void UpdateColorTexture(byte[] _TransformedColorData)
        {
            _ColorTexture.LoadRawTextureData(_TransformedColorData);
            _ColorTexture.Apply();
        }

        public void UpdateDepthTexture(byte[] depthImageData)
        {
            _DepthTexture.LoadRawTextureData(depthImageData);
            _DepthTexture.Apply();
        }

        public void GenerateMesh(int width, int height, AzureKinectCalibration.Intrinsics depthIntrinsics, float depthMetricRadius)
        {
            if (_Mesh != null)
            {
                _Mesh.Clear();
            }
            else
            {
                _Mesh = new Mesh();
                _Mesh.indexFormat = IndexFormat.UInt32;
            }

            Vector3[] vertices = new Vector3[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float[] xy = new float[2];
                    int valid = 0;
                    if (AzureKinectIntrinsicTransformation.Unproject(depthIntrinsics, depthMetricRadius,
                                                                     new float[2] { x, y }, ref xy, ref valid))
                    {
                        vertices[x + y * width] = new Vector3(xy[0], xy[1], 1.0f);
                    }
                    else
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
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterial.SetTexture("_MainTex", _ColorTexture);
        }

        private void InitializeDepthTexture(int width, int height)
        {
            _DepthTexture = new Texture2D(width, height, TextureFormat.R16, false, true)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point,
            };
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterial.SetTexture("_DepthTex", _DepthTexture);
        }
    }
}
