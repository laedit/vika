using System;

namespace NVika.Exceptions
{
    [Serializable]
    internal sealed class LoadingReportException : NVikaException
    {
        internal LoadingReportException(string reportPath, Exception innerException)
            : base($"An exception happened when loading the report '{reportPath}'", innerException, ExitCodes.ExceptionDuringReportLoading)
        {

        }
    }
}
