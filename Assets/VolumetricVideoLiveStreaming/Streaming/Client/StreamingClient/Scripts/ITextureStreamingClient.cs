
namespace VolumetricVideoStreaming.Client
{
    public interface ITextureStreamingClient
    {
        int ClientId { get; }

        bool StartClient(string address, int port);
        void StopClient();

        void BroadcastRawTextureData(byte[] rawTextureData, int width, int height, int frameCount = -1);    
    }
}
