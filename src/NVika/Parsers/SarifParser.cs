using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NVika.Parsers
{
    internal sealed class SarifParser : ReportParserBase
    {
        public override string Name
        {
            get
            {
                return "Static Analysis Results Interchange Format (SARIF)";
            }
        }

        internal SarifParser()
            : base(new[] { ".json", ".sarif" }, '{')
        {

        }

        protected override bool CanParse(StreamReader streamReader)
        {
            var schema = JSchema.Parse(GetEmbeddedResourceContent("Schemas.Sarif.schema.json"));
            using (var jsonTextReader = new JsonTextReader(streamReader))
            {
                var report = JObject.Load(jsonTextReader);
                return report.IsValid(schema);
            }
        }

        public override IEnumerable<Issue> Parse(string reportPath)
        {
            var logContents = FileSystem.File.ReadAllText(reportPath);

            var log = JsonConvert.DeserializeObject<SarifLog>(logContents);

            return log.Runs.SelectMany(run => run.Results.Where(result => result.Suppressions == null || result.Suppressions.All(suppression => suppression.Status == SuppressionStatus.None))
            .Select(result =>
            {
                run.SetRunOnResults();
                var resultRule = result.GetRule();

                string ruleCategory = null;
                resultRule?.TryGetProperty("category", out ruleCategory);

                string filePath = null;
                Region region = null;
                if (result.Locations.Count > 0)
                {
                    if (result.Locations[0].PhysicalLocation != null)
                    {
                        filePath = result.Locations[0].PhysicalLocation.ArtifactLocation.Uri.LocalPath;
                        region = result.Locations[0].PhysicalLocation.Region;
                    }
                    else
                    {
                        filePath = result.AnalysisTarget.Uri.LocalPath;
                    }
                }

                return new Issue
                {
                    Category = ruleCategory,
                    Description = resultRule?.ShortDescription.Text,
                    FilePath = filePath,
                    HelpUri = resultRule?.HelpUri,
                    Message = resultRule == null ? result.Message.Text : result.GetMessageText(resultRule),
                    Name = result.RuleId,
                    Line = region == null ? 0u : (uint)region.StartLine,
                    Offset = region == null ? new Offset() : new Offset { Start = (uint)region.StartColumn, End = (uint)region.EndColumn },
                    Project = null,
                    Severity = LevelToSeverity(result.Level),
                    Source = "SARIF"
                };
            }));
        }

        private static IssueSeverity LevelToSeverity(FailureLevel level)
        {
            switch (level)
            {
                case FailureLevel.Warning: return IssueSeverity.Warning;

                case FailureLevel.Error: return IssueSeverity.Error;

                case FailureLevel.Note:
                default: return IssueSeverity.Suggestion;
            }
        }
    }
}
