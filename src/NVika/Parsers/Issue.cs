using System;

namespace NVika.Parsers
{
    internal class Issue
    {
        internal string Name { get; set; }

        internal string Message { get; set; }

        internal uint? Line { get; set; }

        internal Offset Offset { get; set; }

        internal IssueSeverity Severity { get; set; }

        internal string FilePath { get; set; }

        internal string Category { get; set; }

        internal string Description { get; set; }

        internal Uri HelpUri { get; set; }

        internal string Project { get; set; }

        internal string Source { get; set; }
    }
}
