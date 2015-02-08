using System;

namespace NVika.Parsers
{
    public class Issue
    {
        public string Name { get; set; }
        public string Message { get; set; }
        public uint? Line { get; set; }
        public Offset Offset { get; set; }
        public IssueSeverity Severity { get; set; }
        public string FilePath { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public Uri HelpUri { get; set; }
        public string Project { get; set; }
    }

    public class Offset
    {
        public uint? Start { get; set; }
        public uint? End { get; set; }
    }
}
