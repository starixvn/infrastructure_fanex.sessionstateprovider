namespace Fanex.SessionStateProvider.Redis
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class RedisSessionStateProviderException : Exception
    {
        public RedisSessionStateProviderException()
        {
        }

        public RedisSessionStateProviderException(string message)
            : base(message)
        {
        }

        public RedisSessionStateProviderException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        private RedisSessionStateProviderException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}