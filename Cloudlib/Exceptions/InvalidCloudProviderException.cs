using System;
namespace Cloudlib.Exceptions
{
    public class InvalidCloudProviderException : Exception
    {
        public InvalidCloudProviderException() { }

        public InvalidCloudProviderException(string message) : base(message) { }
    }
}

