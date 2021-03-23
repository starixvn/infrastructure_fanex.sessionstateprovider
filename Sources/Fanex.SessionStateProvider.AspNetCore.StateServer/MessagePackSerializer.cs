namespace Fanex.SessionStateProvider.AspNetCore.StateServer
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using MessagePack.Resolvers;

    using Serializer = MessagePack.MessagePackSerializer;

    public class MessagePackSerializer : ISerializer
    {
        public byte[] Serialize<T>(T obj)
            => Serializer.Serialize(obj, ContractlessStandardResolver.Options);

        public Task SerializeAsync<T>(Stream writeStream, T obj, CancellationToken cancellationToken)
            => Serializer.SerializeAsync(
                writeStream,
                obj,
                ContractlessStandardResolver.Options,
                cancellationToken);

        public T Deserialize<T>(byte[] binaryData)
        {
            try
            {
                return Serializer.Deserialize<T>(binaryData, ContractlessStandardResolver.Options);
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
                return await Serializer.DeserializeAsync<T>(readStream, ContractlessStandardResolver.Options, cancellationToken);
            }
            catch
            {
                return default;
            }
        }
    }
}