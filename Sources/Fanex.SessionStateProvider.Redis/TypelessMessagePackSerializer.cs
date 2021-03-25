namespace Fanex.SessionStateProvider.Redis
{
    using Serializer = MessagePack.MessagePackSerializer.Typeless;

    public class TypelessMessagePackSerializer : ISerializer
    {
        public byte[] Serialize(object obj)
            => Serializer.Serialize(obj);

        public object Deserialize(byte[] binaryData)
        {
            try
            {
                return Serializer.Deserialize(binaryData);
            }
            catch
            {
                return default;
            }
        }
    }
}