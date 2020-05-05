
namespace VolumetricStreamingLite.Client
{
    public delegate void OnReceivedCalibrationDelegate(K4A.CalibrationType calibrationType, K4A.Calibration calibration);

    public interface ITextureReceiverClient
    {
        int ClientId { get; }

        K4A.Calibration Calibration { get; }
        K4A.CalibrationType CalibrationType { get; }
    
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

        OnReceivedCalibrationDelegate OnReceivedCalibration { get; set; }

        bool StartClient(string address, int port);
        void StopClient();

        void OnReceivedDepthData(int frameCount, bool isKeyFrame, int depthWidth, int depthHeight,
                                 CompressionMethod compressionMethod, byte[] encodedDepthData);
        void OnReceivedDepthAndColorData(int frameCount, bool isKeyFrame, int depthWidth, int depthHeight,
                                         CompressionMethod compressionMethod, byte[] encodedDepthData, 
                                         int colorWidth, int colorHeight, byte[] colorImageData);
        void RegisterTextureReceiver(int streamingClientId);
        void UnregisterTextureReceiver(int streamingClientId);
    }
}
