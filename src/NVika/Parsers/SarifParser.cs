using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
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

        public SarifParser()
            : base(new[] { ".json", ".sarif" }, '{')
        {

        }

        protected override bool CanParse(StreamReader reader)
        {
            string schemaText;
            var schemaFilePath = FileSystem.Path.Combine("Schemas", "sarif.schema.json");
            if (FileSystem.File.Exists(schemaFilePath))
            {
                schemaText = FileSystem.File.ReadAllText(schemaFilePath);
            }
            else
            {
                schemaText = GetEmbeddedResourceContent("Schemas.Sarif.schema.json");
            }

            var schema = JSchema.Parse(schemaText);
            using (var jsonTextReader = new JsonTextReader(reader))
            {
                var report = JObject.Load(jsonTextReader);
                return report.IsValid(schema);
            }
        }

        public override IEnumerable<Issue> Parse(string reportPath)
        {
            var logContents = FileSystem.File.ReadAllText(reportPath);

            var settings = new JsonSerializerSettings
            {
                ContractResolver = SarifContractResolver.Instance
            };

            var log = JsonConvert.DeserializeObject<SarifLog>(logContents, settings);

            return log.Runs.SelectMany(run => run.Results.Where(result => result.SuppressionStates == SuppressionStates.None).Select(result =>
            {
                Rule resultRule;
                run.Rules.TryGetValue(result.RuleId, out resultRule);

                string ruleCategory = null;
                resultRule?.TryGetProperty("category", out ruleCategory);

                var resultLocation = GetLocation(result);

                return new Issue
                {
                    Category = ruleCategory,
                    Description = resultRule?.ShortDescription,
                    FilePath = resultLocation.Uri.LocalPath,
                    HelpUri = resultRule?.HelpUri,
                    Message = resultRule == null ? result.Message : result.GetMessageText(resultRule),
                    Name = result.RuleId,
                    Line = resultLocation.Region == null ? 0u : (uint)resultLocation.Region.StartLine,
                    Offset = resultLocation.Region == null ? new Offset() : new Offset { Start = (uint)resultLocation.Region.StartColumn, End = (uint)resultLocation.Region.EndColumn },
                    Project = null,
                    Severity = LevelToSeverity(result.Level),
                    Source = "SARIF"
                };
            }));
        }

        private static PhysicalLocation GetLocation(Result result)
        {
            PhysicalLocation location = null;

            if (result.Locations.Count > 0)
            {
                location = result.Locations[0].ResultFile;
                if(location == null)
                {
                    location = result.Locations[0].AnalysisTarget;
                }
            }

            return location;
        }

        private static IssueSeverity LevelToSeverity(ResultLevel level)
        {
            switch (level)
            {
                case ResultLevel.Default:
                case ResultLevel.Warning: return IssueSeverity.Warning;

                case ResultLevel.Error: return IssueSeverity.Error;

                case ResultLevel.NotApplicable:
                case ResultLevel.Pass:
                case ResultLevel.Note:
                default: return IssueSeverity.Suggestion;
            }
        }
    }
}
