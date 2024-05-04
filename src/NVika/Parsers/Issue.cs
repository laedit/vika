using System;
using System.Globalization;
using System.Text;

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

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(CultureInfo.InvariantCulture, $"Name: {Name}");
            sb.AppendLine(CultureInfo.InvariantCulture, $"Message: {Message}");
            sb.AppendLine(CultureInfo.InvariantCulture, $"Line: {Line}");
            sb.AppendLine(CultureInfo.InvariantCulture, $"Offset: {Offset}");
            sb.AppendLine(CultureInfo.InvariantCulture, $"Severity: {Severity}");
            sb.AppendLine(CultureInfo.InvariantCulture, $"FilePath: {FilePath}");
            sb.AppendLine(CultureInfo.InvariantCulture, $"Category: {Category}");
            sb.AppendLine(CultureInfo.InvariantCulture, $"Description: {Description}");
            sb.AppendLine(CultureInfo.InvariantCulture, $"HelpUri: {HelpUri}");
            sb.AppendLine(CultureInfo.InvariantCulture, $"Project: {Project}");
            sb.AppendLine(CultureInfo.InvariantCulture, $"Source: {Source}");
            return sb.ToString();
        }
    }
}
