namespace Fanex.SessionStateProvider.RedisCore
{
    public interface ISerializer
    {
        byte[] Serialize<T>(T obj);

        T Deserialize<T>(byte[] binaryData);
    }
}