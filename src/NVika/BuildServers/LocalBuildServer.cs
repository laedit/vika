using NVika.Parsers;
using Serilog;
using System.ComponentModel.Composition;

namespace NVika.BuildServers
{
    [Export]
    internal sealed class LocalBuildServer : BuildServerBase
    {
        private const string LineFormat = "{0} {1} {2} - Line {3}: {4}";
        private readonly ILogger _logger;

        public override string Name
        {
            get { return "Local console"; }
        }

        [ImportingConstructor]
        public LocalBuildServer(ILogger logger)
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
            if (IncludeSourceInMessage)
            {
                format = string.Concat("[{5}] ", format);
            }
            _logger.Information(format, issue.Severity, issue.Name, issue.FilePath, issue.Line, issue.Message, issue.Source);
        }
    }
}
