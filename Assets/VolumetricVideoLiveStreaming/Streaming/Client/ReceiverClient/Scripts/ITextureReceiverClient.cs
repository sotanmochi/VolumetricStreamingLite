
namespace VolumetricVideoStreaming.Client
{
    public interface ITextureReceiverClient
    {
        int ClientId { get; }

        int FrameCount { get; }
        int Width { get; }
        int Height { get; }
        byte[] RawTextureData { get; }

        bool StartClient(string address, int port);
        void StopClient();

        void OnReceivedRawTextureData(int frameCount, int width, int height, byte[] rawTextureData);
        void RegisterTextureReceiver(int streamingClientId);
        void UnregisterTextureReceiver(int streamingClientId);
    }
}
