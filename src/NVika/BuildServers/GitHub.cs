using System;
using System.ComponentModel.Composition;
using System.Text;
using NVika.Abstractions;
using NVika.Parsers;

namespace NVika.BuildServers
{
    internal sealed class GitHub : BuildServerBase
    {
        private readonly IEnvironment _environment;

        [ImportingConstructor]
        internal GitHub(IEnvironment environment)
        {
            _environment = environment;
        }

        public override string Name => nameof(GitHub);

        public override bool CanApplyToCurrentContext() => !string.IsNullOrEmpty(_environment.GetEnvironmentVariable("GITHUB_ACTIONS"));

        public override void WriteMessage(Issue issue)
        {
            var outputString = new StringBuilder();

            switch (issue.Severity)
            {
                case IssueSeverity.Error:
                    outputString.Append("::error ");
                    break;

                case IssueSeverity.Warning:
                    outputString.Append("::warning");
                    break;
            }

            if (issue.FilePath != null)
            {
                var file = issue.FilePath.Replace('\\', '/');
                outputString.Append($"file={file},");
            }

            if (issue.Offset != null)
                outputString.Append($"col={issue.Offset.Start},");

            outputString.Append($"line={issue.Line}::{issue.Message}");

            Console.WriteLine(outputString.ToString());
        }
    }
}
