using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace NVika.Parsers
{
    internal sealed class FxCopParser : ReportParserBase
    {
        private readonly Dictionary<string, XElement> _rules = new Dictionary<string, XElement>();

        public override string Name
        {
            get
            {
                return "FxCop";
            }
        }

        public FxCopParser()
            : base(new[] { ".xml" }, '<')
        {

        }

        protected override bool CanParse(StreamReader streamReader)
        {
            // Avoid Xml exception caused by the BOM
            using (var xmlReader = new XmlTextReader(streamReader.BaseStream))
            {
                var report = XDocument.Load(xmlReader);

                return report.Root.Name == "FxCopReport";
            }
        }

        public override IEnumerable<Issue> Parse(string reportPath)
        {
            var report = XDocument.Load(FileSystem.File.OpenRead(reportPath));

            var rules = report.Descendants("Rule");

            foreach (var message in report.Descendants("Message"))
            {
                var rule = GetRule(rules, message.Attribute("CheckId").Value);
                var issue = message.Element("Issue");

                yield return new Issue
                {
                    Category = rule.Attribute("Category").Value,
                    Description = rule.Element("Name").Value,
                    HelpUri = new Uri(rule.Element("Url").Value),
                    Message = issue.Value,
                    Name = message.Attribute("CheckId").Value,
                    Severity = GetSeverity(issue.Attribute("Level")),
                    Source = Name,
                };
            }
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
