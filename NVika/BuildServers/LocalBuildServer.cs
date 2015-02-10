using NVika.BuildServers;
using NVika.Parsers;
using System;
using System.ComponentModel.Composition;

namespace NVika.BuildServers
{
    [Export]
    internal sealed class LocalBuildServer : BuildServerBase
    {
        private bool _applyToCurrentContext = false;

        public override string Name
        {
            get { return "Local console"; }
        }

        public override bool CanApplyToCurrentContext()
        {
            return _applyToCurrentContext;
        }

        protected override void WriteIntegration(Issue issue)
        {
            Console.WriteLine("{0} {1} '{2}' - Line {3}: {4}", issue.Severity, issue.Name, issue.FilePath, issue.Line, issue.Message);
        }
    }
}
