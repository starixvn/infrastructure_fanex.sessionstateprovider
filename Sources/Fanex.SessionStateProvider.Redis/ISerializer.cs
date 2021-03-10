namespace Fanex.SessionStateProvider.Redis
{
    public interface ISerializer
    {
        byte[] Serialize(object obj);

        object Deserialize(byte[] binaryData);
    }
}