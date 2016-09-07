using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace NVika.Parsers
{
    internal sealed class GendarmeParser : XmlReportParser
    {
        private readonly Dictionary<string, string> _categories = new Dictionary<string, string>();

        public override string Name
        {
            get
            {
                return "Gendarme";
            }
        }

        public GendarmeParser()
            : base("gendarme-output")
        {

        }

        protected override IEnumerable<Issue> Parse(XDocument report)
        {
            var rules = report.Root.Element("rules")?.Elements("rule");
            var issues = report.Root.Element("results")?.Elements("rule");

            if (issues != null)
            {
                foreach (var issue in issues)
                {
                    var ruleName = issue.Attribute("Name").Value;

                    foreach (var defect in issue.Descendants("defect"))
                    {
                        var source = ParseSource(defect.Attribute("Source").Value);

                        yield return new Issue
                        {
                            Category = GetCategory(rules, ruleName),
                            Description = issue.Element("problem").Value,
                            HelpUri = new Uri(issue.Attribute("Uri").Value),
                            Message = issue.Element("solution").Value,
                            Name = ruleName,
                            Severity = GetSeverity(defect.Attribute("Severity").Value),
                            Source = Name,
                            FilePath = source.Item1,
                            Line = source.Item2,
                            Project = issue.Element("target").Attribute("Assembly").Value.Split(',')[0]
                        };
                    }
                }
            }
        }

        private static IssueSeverity GetSeverity(string severity)
        {
            switch (severity)
            {
                case "Critical":
                case "High": return IssueSeverity.Error;

                case "Medium":
                default: return IssueSeverity.Warning;

                case "Low": return IssueSeverity.Suggestion;

                case "Audit": return IssueSeverity.Hint;
            }
        }

        private string GetCategory(IEnumerable<XElement> rules, string ruleName)
        {
            if (rules != null && !_categories.ContainsKey(ruleName))
            {
                _categories.Add(ruleName, rules.First(it => it.Attribute("Name").Value == ruleName).Value.Replace("Gendarme.Rules.", string.Empty).Replace("." + ruleName, string.Empty));
            }
            return _categories[ruleName];
        }

        private static Tuple<string, uint?> ParseSource(string source)
        {
            if(string.IsNullOrEmpty(source))
            {
                return new Tuple<string, uint?>(null, null);
            }

            var sourceInfos = source.Split('(');
            var lineInfo = sourceInfos[1];
            return new Tuple<string, uint?>(sourceInfos[0], uint.Parse(lineInfo.Substring(1, lineInfo.Length - 1).Substring(0, lineInfo.Length - 2)));
        }
    }
}
