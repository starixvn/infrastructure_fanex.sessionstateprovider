namespace Fanex.SessionStateProvider.AspNetCore.StateServer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;

    public sealed class StateServerCache : ICache
    {
        public const string MimeType = "application/x-msgpack";

        private readonly HttpClient _httpClient;
        private readonly ISerializer _serializer;

        public StateServerCache(HttpClient httpClient, ISerializer serializer)
        {
            _httpClient = httpClient;
            _serializer = serializer;
        }

        public ILocalStore Get(string key)
            => Task.Run(() => GetAsync(key)).GetAwaiter().GetResult();

        public async Task<ILocalStore> GetAsync(string key, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var requestUri = new Uri(_httpClient.BaseAddress, $"api/get/{key}");
            var response = await _httpClient.GetAsync(requestUri, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseResult = await response.Content.ReadAsStreamAsync();
                var result = await _serializer.DeserializeAsync<Dictionary<string, byte[]>>(responseResult, cancellationToken);

                return new LocalStore(result);
            }

            return default;
        }

        public async Task RefreshAsync(string key, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var requestUri = new Uri(_httpClient.BaseAddress, $"api/refresh/{key}");

            await _httpClient.GetAsync(requestUri, cancellationToken);
        }

        public async Task SetAsync(
            string key,
            ILocalStore store,
            TimeSpan sessionTimeout,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var formData = new
            {
                ModifiedItems = store.GetModifiedItems(),
                DeletedKeys = store.GetDeletedKeys(),
                SessionTimeout = sessionTimeout
            };
            var requestUri = new Uri(_httpClient.BaseAddress, $"api/set/{key}");
            StreamContent requestContent;

            using (var memoryStream = new MemoryStream())
            {
                await _serializer.SerializeAsync(memoryStream, formData, cancellationToken);
                requestContent = new StreamContent(memoryStream);
            }

            requestContent.Headers.ContentType = new MediaTypeHeaderValue(MimeType);
            await _httpClient.PostAsync(requestUri, requestContent, cancellationToken);
        }
    }
}