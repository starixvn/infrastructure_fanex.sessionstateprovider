namespace Fanex.SessionStateProvider.RedisCore
{
    using MessagePack.Resolvers;
    using Serializer = MessagePack.MessagePackSerializer;

    public class MessagePackSerializer : ISerializer
    {
        public byte[] Serialize<T>(T obj)
            => Serializer.Serialize(obj, ContractlessStandardResolver.Options);

        public T Deserialize<T>(byte[] binaryData)
        {
            try
            {
                return Serializer.Deserialize<T>(binaryData);
            }
            catch
            {
                return default;
            }
        }
    }
}