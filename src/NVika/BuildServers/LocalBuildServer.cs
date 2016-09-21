using NVika.Parsers;
using Serilog;
using System.ComponentModel.Composition;

namespace NVika.BuildServers
{
    [Export]
    internal sealed class LocalBuildServer : BuildServerBase
    {
        private const string LineFormat = "{0} {1} {3} - Line {4}: {2}";
        private const string NoFileLineFormat = "{0} {1}: {2}";
        private readonly ILogger _logger;

        public override string Name
        {
            get { return "Local console"; }
        }

        [ImportingConstructor]
        internal LocalBuildServer(ILogger logger)
        {
            _logger = logger;
        }

        public override bool CanApplyToCurrentContext()
        {
            return false;
        }

        public override void WriteMessage(Issue issue)
        {
            var format = LineFormat;
            if(issue.FilePath == null)
            {
                format = NoFileLineFormat;
            }

            if (IncludeSourceInMessage)
            {
                format = string.Concat("[{5}] ", format);
            }

            _logger.Information(format, issue.Severity, issue.Name, issue.Message, issue.FilePath, issue.Line,issue.Source);
        }
    }
}
