using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO.Abstractions;
using System.Linq;
using System.Xml.Linq;

namespace NVika.Parsers
{
    internal sealed class InspectCodeParser : IReportParser
    {
        private readonly Dictionary<string, XElement> _issueTypes = new Dictionary<string, XElement>();
        private readonly IFileSystem _fileSystem;

        public string Name
        {
            get { return "InspectCode"; }
        }

        [ImportingConstructor]
        public InspectCodeParser(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public bool CanParse(XDocument report)
        {
            return report.FirstNode is XComment && ((XComment)report.FirstNode).Value.Contains("InspectCode");
        }

        public IEnumerable<Issue> Parse(XDocument report)
        {
            var issuesType = report.Descendants("IssueType");

            foreach (var project in report.Descendants("Project"))
            {
                foreach (var issue in project.Descendants("Issue"))
                {
                    var issueType = GetIssueType(issuesType, issue.Attribute("TypeId").Value);
                    var sourceFilePath = issue.Attribute("File").Value;
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
                        Severity = GetSeverity(issueType.Attribute("Severity")),
                        Offset = GetOffset(offsetAttribute, sourceFilePath, lineNumber),
                        Source = Name,
                    };
                }
            }
        }

        private Uri GetUri(XAttribute uriAttribute)
        {
            return uriAttribute == null ? null : new Uri(uriAttribute.Value);
        }

        private Offset GetOffset(XAttribute offsetAttribute, string sourceFilePath, uint? lineNumber)
        {
            if (offsetAttribute == null || !_fileSystem.File.Exists(sourceFilePath))
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

            var lines = _fileSystem.File.ReadLines(sourceFilePath);
            var issueLineOffset = lines.Take((int)lineNumber.Value - 1).Sum(line => line.Length);

            return new Offset
            {
                Start = string.IsNullOrWhiteSpace(start) ? null : (uint?)(int.Parse(start) - (issueLineOffset + lineNumber.Value - 1)),
                End = string.IsNullOrWhiteSpace(end) ? null : (uint?)(int.Parse(end) - (issueLineOffset + lineNumber.Value - 1))
            };
        }

        private IssueSeverity GetSeverity(XAttribute severityAttribute)
        {
            switch (severityAttribute.Value)
            {
                case "HINT": return IssueSeverity.Hint;

                case "SUGGESTION": return IssueSeverity.Suggestion;

                case "ERROR": return IssueSeverity.Error;

                default:
                    return IssueSeverity.Warning;
            }
        }

        private uint? GetLine(XAttribute lineAttribute, bool isOffsetAvailable)
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
