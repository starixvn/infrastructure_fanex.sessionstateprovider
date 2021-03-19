namespace Fanex.SessionStateProvider.AspNetCore
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using Serializer = MessagePack.MessagePackSerializer.Typeless;

    public class MessagePackSerializer : ISerializer
    {
        public byte[] Serialize<T>(T obj)
            => Serializer.Serialize(obj);

        public Task SerializeAsync<T>(Stream writeStream, T obj, CancellationToken cancellationToken)
            => Serializer.SerializeAsync(writeStream, obj, cancellationToken: cancellationToken);

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

        public async Task<T> DeserializeAsync<T>(Stream readStream, CancellationToken cancellationToken)
        {
            try
            {
                return (T)await Serializer.DeserializeAsync(readStream, cancellationToken: cancellationToken);
            }
            catch
            {
                return default;
            }
        }
    }
}