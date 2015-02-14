using NVika.Parsers;
using System.ComponentModel.Composition;

namespace NVika.BuildServers
{
    [Export]
    internal sealed class LocalBuildServer : BuildServerBase
    {
        private static string s_lineFormat = "{0} {1} '{2}' - Line {3}: {4}";
        private bool _applyToCurrentContext = false;
        private Logger _logger;

        public override string Name
        {
            get { return "Local console"; }
        }

        [ImportingConstructor]
        public LocalBuildServer(Logger logger)
        {
            _logger = logger;
        }

        public override bool CanApplyToCurrentContext()
        {
            return _applyToCurrentContext;
        }

        public override void WriteMessage(Issue issue)
        {
            string format = s_lineFormat;
            if (_includeSourceInMessage)
            {
                format = string.Concat("[{5}] ", format);
            }
            _logger.Info(format, issue.Severity, issue.Name, issue.FilePath, issue.Line, issue.Message, issue.Source);
        }
    }
}