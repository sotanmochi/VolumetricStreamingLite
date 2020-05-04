
namespace VolumetricVideoStreaming.Client
{
    public interface ITextureReceiverClient
    {
        int ClientId { get; }

        K4A.Calibration Calibration { get; }
        int FrameCount { get; }
        bool IsKeyFrame { get; }
        int DepthWidth { get; }
        int DepthHeight { get; }
        int DepthImageSize { get; }
        CompressionMethod CompressionMethod { get; }
        byte[] EncodedDepthData { get; }
        int ColorWidth { get; }
        int ColorHeight { get; }
        byte[] ColorImageData { get; }

        bool StartClient(string address, int port);
        void StopClient();

        void OnReceivedCalibration(K4A.Calibration calibration);
        void OnReceivedDepthData(int frameCount, bool isKeyFrame, int depthWidth, int depthHeight,
                                 CompressionMethod compressionMethod, byte[] encodedDepthData);
        void OnReceivedDepthAndColorData(int frameCount, bool isKeyFrame, int depthWidth, int depthHeight,
                                         CompressionMethod compressionMethod, byte[] encodedDepthData, 
                                         int colorWidth, int colorHeight, byte[] colorImageData);
        void RegisterTextureReceiver(int streamingClientId);
        void UnregisterTextureReceiver(int streamingClientId);
    }
}
