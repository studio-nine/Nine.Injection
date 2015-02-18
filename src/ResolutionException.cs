namespace Nine.Ioc
{
    using System;

    public class ResolutionException : Exception
    {
        public ResolutionException(string message) : base(message) { }
        public ResolutionException(string message, Exception innerException) : base(message, innerException) { }
    }
}