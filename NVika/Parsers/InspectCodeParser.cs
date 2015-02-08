using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace NVika.Parsers
{
    public sealed class InspectCodeParser : IReportParser
    {
        private Dictionary<string, XElement> _issueTypes = new Dictionary<string, XElement>();

        public string Name
        {
            get { return "InspectCode"; }
        }

        public bool CanParse(XDocument report)
        {
            return report.FirstNode is XComment && ((XComment)report.FirstNode).Value.Contains("InspectCode");
        }

        public IEnumerable<Issue> Parse(XDocument report)
        {
            var issues = new List<Issue>();
            var issuesType = report.Descendants("IssueType");

            foreach (var project in report.Descendants("Project"))
            {
                foreach (var issue in project.Descendants("Issue"))
                {
                    var issueType = GetIssueType(issuesType, issue.Attribute("TypeId").Value);


                    issues.Add(new Issue
                    {
                        Project = project.Attribute("Name").Value,
                        Category = issueType.Attribute("Category").Value,
                        Description = issueType.Attribute("Description").Value,
                        FilePath = issue.Attribute("File").Value,
                        HelpUri = GetUri(issueType.Attribute("WikiUrl")),
                        Line = GetLine(issue.Attribute("Line")),
                        Message = issue.Attribute("Message").Value,
                        Name = issue.Attribute("TypeId").Value,
                        Severity = GetSeverity(issueType.Attribute("Severity")),
                        Offset = GetOffset(issue.Attribute("Offset")),
                    });

                }
            }
            return issues;
        }

        public Uri GetUri(XAttribute uriAttribute)
        {
            return uriAttribute == null ? null : new Uri(uriAttribute.Value);
        }

        public Offset GetOffset(XAttribute offsetAttribute)
        {
            if (offsetAttribute == null)
            {
                return null;
            }

            string start = null;
            string end = null;

            var dashIndex = offsetAttribute.Value.IndexOf("-");
            if (dashIndex > -1)
            {
                start = offsetAttribute.Value.Substring(0, dashIndex);
                end = offsetAttribute.Value.Substring(dashIndex + 1);
            }
            else
            {
                start = offsetAttribute.Value;
            }

            return new Offset
            {
                Start = string.IsNullOrWhiteSpace(start) ? null : (uint?)uint.Parse(start),
                End = string.IsNullOrWhiteSpace(start) ? null : (uint?)uint.Parse(end)
            };
        }

        public IssueSeverity GetSeverity(XAttribute severityAttribute)
        {
            switch (severityAttribute.Value)
            {
                case "HINT": return IssueSeverity.Hint;

                case "SUGGESTION": return IssueSeverity.Suggestion;

                case "ERROR": return IssueSeverity.Error;

                case "WARNING":
                default:
                    return IssueSeverity.Warning;
            }
        }

        public uint? GetLine(XAttribute lineAttribute)
        {
            if (lineAttribute == null)
            {
                return null;
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
