namespace Fanex.SessionStateProvider.Redis
{
    using System;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Web.Configuration;

    public sealed class SessionStorageOptions
    {
        public string ApplicationName { get; private set; }

        public string ConnectionString { get; private set; }

        public TimeSpan RequestTimeout { get; private set; }

        public TimeSpan RetryTimeout { get; private set; }

        public TimeSpan SessionTimeout { get; private set; }

        public ISerializer Serializer { get; private set; }

        public static SessionStorageOptions TryParse(NameValueCollection config)
            => new()
            {
                ApplicationName = GetApplicationName(config),
                ConnectionString = GetConnectionString(config),
                RequestTimeout = GetRequestTimeout(),
                RetryTimeout = GetRetryTimeout(config),
                SessionTimeout = GetSessionTimeout(),
                Serializer = GetSerializer(config)
            };

        private static string GetApplicationName(NameValueCollection config)
        {
            const string attributeName = "applicationName";

            return GetStringValue(attributeName, config);
        }

        private static string GetConnectionString(NameValueCollection config)
        {
            const string attributeName = "connectionString";

            return GetStringValue(attributeName, config);
        }

        private static TimeSpan GetRequestTimeout()
        {
            const string httpRuntimeSectionName = "system.web/httpRuntime";

            var httpRuntimeSection = ConfigurationManager.GetSection(httpRuntimeSectionName) as HttpRuntimeSection;

            return httpRuntimeSection.ExecutionTimeout;
        }

        private static TimeSpan GetSessionTimeout()
        {
            const string sessionStateSectionName = "system.web/sessionState";

            var sessionStateSection = ConfigurationManager.GetSection(sessionStateSectionName) as SessionStateSection;

            return sessionStateSection.Timeout;
        }

        private static TimeSpan GetRetryTimeout(NameValueCollection config)
        {
            const string attributeName = "retryTimeoutInMilliseconds";
            const int defaultRetryTimeoutInMilliseconds = 5000;

            var retryTimeoutFromConfig = GetStringValue(attributeName, config);
            var gettingFromConfig = int.TryParse(retryTimeoutFromConfig, out int parsedRetryTimeoutFromConfig);
            var retryTimeoutInMilliseconds = gettingFromConfig ? parsedRetryTimeoutFromConfig : defaultRetryTimeoutInMilliseconds;

            return TimeSpan.FromMilliseconds(retryTimeoutInMilliseconds);
        }

        private static ISerializer GetSerializer(NameValueCollection config)
        {
            const string attributeName = "serializerType";

            var serializerTypeName = GetStringValue(attributeName, config);

            if (!string.IsNullOrEmpty(serializerTypeName))
            {
                var serializerType = Type.GetType(serializerTypeName, true);

                if (serializerType != null)
                {
                    return (ISerializer)Activator.CreateInstance(serializerType);
                }
            }

            return new TypelessMessagePackSerializer();
        }

        private static string GetStringValue(string attributeName, NameValueCollection config)
        {
            var attributeValues = config.GetValues(attributeName);

            if (attributeValues != null && attributeValues.Length > 0 && !string.IsNullOrEmpty(attributeValues[0]))
            {
                return attributeValues[0];
            }

            return default;
        }
    }
}