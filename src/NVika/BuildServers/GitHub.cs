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
                    outputString.Append("::warning ");
                    break;

                case IssueSeverity.Suggestion:
                    outputString.Append("::notice ");
                    break;
            }

            var details = issue.Message;

            if (issue.FilePath != null)
            {
                var absolutePath = issue.FilePath.Replace('\\', '/');
                var relativePath = issue.Project != null ? issue.FilePath.Replace($"{issue.Project.Replace('\\', '/')}/", string.Empty) : absolutePath;

                outputString.Append($"file={absolutePath},");
                details = $"{issue.Message} in {relativePath} on line {issue.Line}";
            }

            if (issue.Offset != null)
                outputString.Append($"col={issue.Offset.Start},");

            if (issue.Line != null)
                outputString.Append($"line={issue.Line}");

            outputString.Append($"::{details}");

            Console.WriteLine(outputString.ToString());
        }
    }
}
