using UnityEngine;
using Microsoft.Azure.Kinect.Sensor;

namespace AzureKinect4Unity
{
    [RequireComponent(typeof(PointCloudMesh))]
    public class AzureKinectPointCloudVisualizer : MonoBehaviour
    {
        [SerializeField] AzureKinectManager _AzureKinectManager;

        AzureKinectSensor _KinectSensor;

        PointCloudMesh _PointCloudMesh;
        Texture2D _ColorImageTexture;

        void Start()
        {
            _KinectSensor = _AzureKinectManager.Sensor;
            if (_KinectSensor != null)
            {
                Debug.Log("ColorResolution: " + _KinectSensor.ColorImageWidth + "x" + _KinectSensor.ColorImageHeight);
                Debug.Log("DepthResolution: " + _KinectSensor.DepthImageWidth + "x" + _KinectSensor.DepthImageHeight);

                _ColorImageTexture = new Texture2D(_KinectSensor.ColorImageWidth, _KinectSensor.ColorImageHeight, TextureFormat.BGRA32, false);

                _PointCloudMesh = GetComponent<PointCloudMesh>();
                _PointCloudMesh.GenerateMesh(_KinectSensor.ColorImageWidth, _KinectSensor.ColorImageHeight);
            }
        }

        void Update()
        {
            if (_KinectSensor.RawColorImage != null)
            {
                _ColorImageTexture.LoadRawTextureData(_KinectSensor.RawColorImage);
                _ColorImageTexture.Apply();
            }

            if (_KinectSensor.PointCloud != null)
            {
                Short3[] pointCloud = _KinectSensor.PointCloud;
                
                Vector3[] vertices = new Vector3[pointCloud.Length];
                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i] = new Vector3(pointCloud[i].X * 0.001f, pointCloud[i].Y * -0.001f, pointCloud[i].Z * 0.001f);
                }

                _PointCloudMesh.UpdateVertices(vertices);
                _PointCloudMesh.UpdateColorTexture(_KinectSensor.RawColorImage);
            }
        }
    }
}
