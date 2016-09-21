using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace NVika.Parsers
{
    internal sealed class FxCopParser : XmlReportParser
    {
        private readonly Dictionary<string, XElement> _rules = new Dictionary<string, XElement>();

        public override string Name
        {
            get
            {
                return "FxCop";
            }
        }

        internal FxCopParser()
            : base("FxCopReport")
        {

        }

        protected override IEnumerable<Issue> Parse(XDocument report)
        {
            var rules = report.Descendants("Rule");

            foreach (var message in report.Descendants("Message"))
            {
                var rule = GetRule(rules, message.Attribute("CheckId").Value);

                foreach (var issue in message.Elements("Issue"))
                {
                    yield return new Issue
                    {
                        Category = rule.Attribute("Category").Value,
                        Description = rule.Element("Name").Value,
                        HelpUri = new Uri(rule.Element("Url").Value),
                        Message = issue.Value,
                        Name = message.Attribute("CheckId").Value,
                        Severity = GetSeverity(issue.Attribute("Level")),
                        Source = Name,
                        Line = GetLine(issue.Attribute("Line")),
                        FilePath = GetPath(issue)
                    };

                }
            }
        }

        private string GetPath(XElement issue)
        {
            var path = issue.Attribute("Path")?.Value;
            var file = issue.Attribute("File")?.Value;
            if(path != null && file != null)
            {
                return FileSystem.Path.Combine(path, file);
            }
            return null;
        }

        private static uint? GetLine(XAttribute issueLine)
        {
            if(issueLine == null)
            {
                return null;
            }
            return uint.Parse(issueLine.Value);
        }

        private static IssueSeverity GetSeverity(XAttribute level)
        {
            switch (level.Value)
            {
                case "Error":
                case "CriticalError": return IssueSeverity.Error;

                case "Warning":
                case "CriticalWarning":
                default: return IssueSeverity.Warning;
            }
        }

        private XElement GetRule(IEnumerable<XElement> rules, string ruleId)
        {
            if (!_rules.ContainsKey(ruleId))
            {
                _rules.Add(ruleId, rules.First(it => it.Attribute("CheckId").Value == ruleId));
            }
            return _rules[ruleId];
        }
    }
}
