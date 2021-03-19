namespace Fanex.SessionStateProvider.AspNetCore
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public interface ISerializer
    {
        byte[] Serialize<T>(T obj);

        Task SerializeAsync<T>(Stream writeStream, T obj, CancellationToken cancellationToken);

        T Deserialize<T>(byte[] binaryData);

        Task<T> DeserializeAsync<T>(Stream readStream, CancellationToken cancellationToken);
    }
}