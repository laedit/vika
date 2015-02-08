using NVika.Parsers;
using System;
using System.ComponentModel.Composition;

namespace NVika
{
    [Export]
    internal sealed class LocalBuildServer : IBuildServer
    {
        private bool _applyToCurrentContext = false;

        public string Name
        {
            get { return "Local console"; }
        }

        public bool CanApplyToCurrentContext()
        {
            return _applyToCurrentContext;
        }

        public void WriteMessage(Issue issue)
        {
            Console.WriteLine("{0} {1} '{2}' - Line {3}: {4}", issue.Severity, issue.Message, issue.FilePath, issue.Line, issue.Message);
        }
    }
}
