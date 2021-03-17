namespace Fanex.SessionStateProvider.RedisCore
{
    using Serializer = MessagePack.MessagePackSerializer.Typeless;

    public class MessagePackSerializer : ISerializer
    {
        public byte[] Serialize<T>(T obj)
            => Serializer.Serialize(obj);

        public T Deserialize<T>(byte[] binaryData)
        {
            try
            {
                return (T)Serializer.Deserialize(binaryData);
            }
            catch
            {
                return default;
            }
        }
    }
}