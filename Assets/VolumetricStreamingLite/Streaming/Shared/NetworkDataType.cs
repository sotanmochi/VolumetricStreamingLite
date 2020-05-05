
namespace LiteNetLibExtension
{
    public enum NetworkDataType
    {
        ReceiveOwnCliendId,
        SendCalibration,
        ReceiveCalibration,
        SendDepthData,
        ReceiveDepthData,
        SendDepthAndColorData,
        ReceiveDepthAndColorData,
        RegisterTextureReceiver,
        UnregisterTextureReceiver,
    }
}
