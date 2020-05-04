
namespace VolumetricVideoStreaming.Client
{
    public interface ITextureStreamingClient
    {
        int ClientId { get; }

        bool StartClient(string address, int port);
        void StopClient();

        void SendCalibration(K4A.Calibration calibration);
        void SendDepthData(CompressionMethod compressionMethod, byte[] encodedDepthData, 
                           int depthWidth, int depthHeight, bool isKeyFrame, int frameCount = -1);
        void SendDepthAndColorData(CompressionMethod compressionMethod, byte[] encodedDepthData, int depthWidth, int depthHeight, bool isKeyFrame, 
                                   byte[] colorImageData, int colorWidth, int colorHeight, int frameCount = -1);
    }
}
