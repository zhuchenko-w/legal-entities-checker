using System;

namespace Bookkeeping.Common.Exceptions
{
    [Serializable]
    public class PublicException : Exception
    {
        public PublicException()
        {
        }

        public PublicException(string message) : base(message)
        {
        }

        public PublicException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
