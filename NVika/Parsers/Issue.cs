using System;

namespace NVika.Parsers
{
    public class Issue
    {
        public string Name { get; internal set; }

        public string Message { get; internal set; }

        public uint? Line { get; internal set; }

        public Offset Offset { get; internal set; }

        public IssueSeverity Severity { get; internal set; }

        public string FilePath { get; internal set; }

        public string Category { get; internal set; }

        public string Description { get; internal set; }

        public Uri HelpUri { get; internal set; }

        public string Project { get; internal set; }

        public string Source { get; internal set; }
    }

    public class Offset
    {
        public uint? Start { get; internal set; }

        public uint? End { get; internal set; }
    }
}