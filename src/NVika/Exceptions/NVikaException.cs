using System;

namespace NVika.Exceptions
{
    [Serializable]
    internal abstract class NVikaException : Exception
    {
        public int ExitCode { get; private set; }

        protected NVikaException(string message, int exitCode) : base(message)
        {
            ExitCode = exitCode;
        }

        protected NVikaException(string message, Exception innerException, int exitCode) : base(message, innerException)
        {
            ExitCode = exitCode;
        }
    }
}
