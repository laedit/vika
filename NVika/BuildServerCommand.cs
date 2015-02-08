using ManyConsole;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Xml.Linq;

namespace NVika
{
    internal sealed class BuildServerCommand : ConsoleCommand
    {
        private string reportPath;
        private ReportTypes _reportType;
        private IFileSystem _fileSystem;
        private readonly Logger _logger;

        public BuildServerCommand(IFileSystem fileSystem, Logger logger)
        {
            _fileSystem = fileSystem;
            _logger = logger;

            this.IsCommand("buildserver", "Parse the report and show warnings in console or inject them to the build server");
            this.HasOption("r|report=", "Report to analyze", s => reportPath = s);
            this.HasAdditionalArguments(1, "Report to analyze");
            this.HasOption<ReportTypes>("t|type=", "Type of the report", s => _reportType = s);

            // TODO
            // force display warnings to console additionally to the build server
            // force to end the build (exitcode > 0) when errors
            // warning as error
        }

        public override int Run(string[] remainingArguments)
        {
            if(string.IsNullOrWhiteSpace(reportPath) && remainingArguments.Length == 1)
            {
                reportPath = remainingArguments[0];
            }

            _logger.Debug("Report path is '{0}'", reportPath);

            if(!_fileSystem.File.Exists(reportPath))
            {
                _logger.Error("The report '{0}' was not found.", reportPath);
                return 1;
            }

            var applicableBuildServers = GetApplicableBuildServer();
            _logger.Debug("The following build servers will be used:");
            foreach (var buildServer in applicableBuildServers)
            {
                _logger.Debug("\t- {0}", buildServer.Name);
            }

            var doc = XDocument.Load(reportPath);

            // Report type Detection
            if(doc.FirstNode is XComment && ((XComment)doc.FirstNode).Value.Contains("InspectCode"))
            {
                _reportType = ReportTypes.InspectCode;
            }

            _logger.Debug("Report type is '{0}'", _reportType);

            // Parsing TODO export to another lib
            var issuesType = doc.Descendants("IssueType");
            _logger.Debug("{0} issues types was found", issuesType.Count());

            foreach (var project in doc.Descendants("Project"))
            {
                foreach (var issue in project.Descendants("Issue"))
                {
                    var issueType = GetIssueType(issuesType, issue.Attribute("TypeId").Value);
                    
                    var lineAttribute = issue.Attribute("Line");
                    var line = lineAttribute == null ? string.Empty : lineAttribute.Value;

                    var offsetAttribute = issue.Attribute("Offset");
                    var offset = offsetAttribute == null ? string.Empty : offsetAttribute.Value;

                    foreach (var buildServer in applicableBuildServers)
                    {
                        buildServer.WriteMessage(issue.Attribute("TypeId").Value, issueType.Attribute("Severity").Value, issue.Attribute("Message").Value, issue.Attribute("File").Value, line, offset, project.Attribute("Name").Value);
                    }
                }
            }
            
            return issuesType.Any(it => it.Attribute("Severity").Value == "ERROR") ? 1 : 0;
        }

        private Dictionary<string, XElement> _issueTypes = new Dictionary<string,XElement>();

        private XElement GetIssueType(IEnumerable<XElement> issueTypes, string issueTypeId)
        {
            if(!_issueTypes.ContainsKey(issueTypeId))
            {
                _issueTypes.Add(issueTypeId, issueTypes.First(it => it.Attribute("Id").Value == issueTypeId));
            }
            return _issueTypes[issueTypeId];
        }

        private IEnumerable<IBuildServer> _buildServers;
        private IBuildServer _localBuildServer;

        private IEnumerable<IBuildServer> GetApplicableBuildServer()
        {
            if(_buildServers == null)
            {
                _localBuildServer = new LocalBuildServer();
                _buildServers = new List<IBuildServer>
                {
                    _localBuildServer,
                    new AppVeyorBuildServer()
                };
            }

            var applicableBuildServers = _buildServers.Where(bs => bs.CanApplyToCurrentContext());
            if(!applicableBuildServers.Any())
            {
                applicableBuildServers = new List<IBuildServer> { _localBuildServer };
            }
            return applicableBuildServers;
        }
    }
}
