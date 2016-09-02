using ManyConsole;
using NVika.BuildServers;
using NVika.Parsers;
using Serilog;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO.Abstractions;
using System.Linq;

namespace NVika
{
    [Export(typeof(ConsoleCommand))]
    internal class ParseReportCommand : ConsoleCommand
    {
        private readonly ILogger _logger;
        private readonly IFileSystem _fileSystem;
        private readonly IEnumerable<IBuildServer> _buildServers;
        private readonly LocalBuildServer _localBuildServer;
        private readonly IEnumerable<IReportParser> _parsers;
        private bool _includeSourceInMessage;

        [ImportingConstructor]
        public ParseReportCommand(ILogger logger,
                                  IFileSystem fileSystem,
                                  [ImportMany]IEnumerable<IBuildServer> buildServers,
                                  LocalBuildServer localBuildServer,
                                  [ImportMany]IEnumerable<IReportParser> parsers)
        {
            _logger = logger;
            _fileSystem = fileSystem;
            _buildServers = buildServers;
            _localBuildServer = localBuildServer;
            _parsers = parsers;

            IsCommand("parsereport", "Parse the report and show warnings in console or inject them to the build server");
            HasOption("includesource", "Include the source in messages", s => _includeSourceInMessage = true);
            AllowsAnyAdditionalArguments("Reports to analyze");
        }

        public override int Run(string[] reportPaths)
        {
            var returnCode = ExitCodes.Ok;

            if (reportPaths.Length == 0)
            {
                _logger.Error("No report was specified. You must indicate at least one report file.");
                return 1;
            }

            var applicableBuildServers = GetApplicableBuildServer();
            _logger.Information("The following build servers have been detected:");
            foreach (var buildServer in applicableBuildServers)
            {
                _logger.Information("\t- {buildServerName}", buildServer.Name);
            }

            foreach (var reportPath in reportPaths)
            {
                _logger.Debug("Report path is {reportPath}", reportPath);

                if (!_fileSystem.File.Exists(reportPath))
                {
                    _logger.Error("The report {reportPath} was not found.", reportPath);
                    return ExitCodes.ReportNotFound;
                }

                // Report type Detection
                var parser = GetParser(reportPath);
                if (parser == null)
                {
                    _logger.Error("The adequate parser for this report was not found. You are welcome to address us an issue.");
                    return ExitCodes.NoParserFoundForThisReport;
                }
                _logger.Debug("Report type is {Name}", parser.Name);

                var issues = parser.Parse(reportPath);
                _logger.Information("{Count} issues was found", issues.Count());

                foreach (var issue in issues)
                {
                    foreach (var buildServer in applicableBuildServers)
                    {
                        buildServer.WriteMessage(issue);
                    }
                }

                if (returnCode == ExitCodes.Ok && issues.Any(i => i.Severity == IssueSeverity.Error))
                {
                    returnCode = ExitCodes.IssuesWithErrorWasFound;
                }

            }

            if (returnCode == ExitCodes.IssuesWithErrorWasFound)
            {
                _logger.Fatal("Issues with severity error was found: the build will fail");
            }

            return returnCode;
        }

        private IEnumerable<IBuildServer> GetApplicableBuildServer()
        {
            var applicableBuildServers = _buildServers.Where(bs => bs.CanApplyToCurrentContext());

            if (!applicableBuildServers.Any())
            {
                applicableBuildServers = new List<IBuildServer> { _localBuildServer };
            }

            foreach (var buildServer in applicableBuildServers)
            {
                buildServer.ApplyParameters(_includeSourceInMessage);
            }

            return applicableBuildServers;
        }

        private IReportParser GetParser(string reportPath)
        {
            return _parsers.FirstOrDefault(p => p.CanParse(reportPath));
        }
    }
}
