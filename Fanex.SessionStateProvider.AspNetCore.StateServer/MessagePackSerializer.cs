namespace Fanex.SessionStateProvider.AspNetCore.StateServer
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using Serializer = MessagePack.MessagePackSerializer;

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
                return Serializer.Deserialize<T>(binaryData);
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
                return await Serializer.DeserializeAsync<T>(readStream, cancellationToken: cancellationToken);
            }
            catch
            {
                return default;
            }
        }
    }
}