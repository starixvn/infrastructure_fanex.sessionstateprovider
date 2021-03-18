namespace Fanex.SessionStateProvider.AspNetCore
{
    public interface ISerializer
    {
        byte[] Serialize<T>(T obj);

        T Deserialize<T>(byte[] binaryData);
    }
}