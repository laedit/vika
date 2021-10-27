using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace NVika.Parsers
{
    internal sealed class InspectCodeParser : XmlReportParser
    {
        private readonly Dictionary<string, XElement> _issueTypes = new Dictionary<string, XElement>();

        public override string Name
        {
            get { return "InspectCode"; }
        }

        internal InspectCodeParser()
            : base("Report", "Information")
        {

        }

        protected override IEnumerable<Issue> Parse(XDocument report)
        {
            var rootFolder = FileSystem.Path.GetDirectoryName(report.Root.Element("Information").Element("Solution").Value);
            var issuesType = report.Descendants("IssueType");

            foreach (var project in report.Descendants("Project"))
            {
                foreach (var issue in project.Descendants("Issue"))
                {
                    var issueType = GetIssueType(issuesType, issue.Attribute("TypeId").Value);
                    var sourceFilePath = string.IsNullOrEmpty(rootFolder) ? issue.Attribute("File").Value : FileSystem.Path.Combine(rootFolder, issue.Attribute("File").Value);
                    var offsetAttribute = issue.Attribute("Offset");
                    var lineNumber = GetLine(issue.Attribute("Line"), offsetAttribute != null);

                    yield return new Issue
                    {
                        Project = project.Attribute("Name").Value,
                        Category = issueType.Attribute("Category").Value,
                        Description = issueType.Attribute("Description").Value,
                        FilePath = sourceFilePath,
                        HelpUri = GetUri(issueType.Attribute("WikiUrl")),
                        Line = lineNumber,
                        Message = issue.Attribute("Message").Value,
                        Name = issue.Attribute("TypeId").Value,
                        Severity = GetSeverity(issue.Attribute("Severity"), issueType.Attribute("Severity")),
                        Offset = GetOffset(offsetAttribute, sourceFilePath, lineNumber),
                        Source = Name,
                    };
                }
            }
        }

        private static Uri GetUri(XAttribute uriAttribute)
        {
            return uriAttribute == null ? null : new Uri(uriAttribute.Value);
        }

        private Offset GetOffset(XAttribute offsetAttribute, string sourceFilePath, uint? lineNumber)
        {
            if (offsetAttribute == null || !FileSystem.File.Exists(sourceFilePath))
            {
                return null;
            }

            string start = null;
            string end = null;

            var dashIndex = offsetAttribute.Value.IndexOf("-", StringComparison.OrdinalIgnoreCase);
            if (dashIndex > -1)
            {
                start = offsetAttribute.Value.Substring(0, dashIndex);
                end = offsetAttribute.Value.Substring(dashIndex + 1);
            }

            var lines = FileSystem.File.ReadLines(sourceFilePath);
            var issueLineOffset = lines.Take((int)lineNumber.Value - 1).Sum(line => line.Length);

            return new Offset
            {
                Start = string.IsNullOrWhiteSpace(start) ? null : (uint?)(int.Parse(start) - (issueLineOffset + lineNumber.Value - 1)),
                End = string.IsNullOrWhiteSpace(end) ? null : (uint?)(int.Parse(end) - (issueLineOffset + lineNumber.Value - 1))
            };
        }

        private static IssueSeverity GetSeverity(XAttribute issueSeverity, XAttribute issueTypeSeverity)
        {
            // The severity type is always present on the issue type element,
            // but it is also overridden by an individual issue's severity, if one is present.
            // See: https://www.jetbrains.com/help/resharper/InspectCode.html#understanding-output
            var finalSeverity = issueSeverity?.Value ?? issueTypeSeverity.Value;

            switch (finalSeverity)
            {
                case "HINT": return IssueSeverity.Hint;

                case "SUGGESTION": return IssueSeverity.Suggestion;

                case "ERROR": return IssueSeverity.Error;

                default:
                    return IssueSeverity.Warning;
            }
        }

        private static uint? GetLine(XAttribute lineAttribute, bool isOffsetAvailable)
        {
            if (lineAttribute == null)
            {
                return isOffsetAvailable ? 1 : (uint?)null;
            }
            return uint.Parse(lineAttribute.Value);
        }

        private XElement GetIssueType(IEnumerable<XElement> issueTypes, string issueTypeId)
        {
            if (!_issueTypes.ContainsKey(issueTypeId))
            {
                _issueTypes.Add(issueTypeId, issueTypes.First(it => it.Attribute("Id").Value == issueTypeId));
            }
            return _issueTypes[issueTypeId];
        }
    }
}
