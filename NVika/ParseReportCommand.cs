using NVika.BuildServers;
using NVika.Parsers;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO.Abstractions;
using System.Linq;
using System.Xml.Linq;

namespace NVika
{
    internal class ParseReportCommand : CommandBase
    {
        private IFileSystem _fileSystem;
        private bool _includeSourceInMessage;
        private IEnumerable<IBuildServer> _buildServers;
        private LocalBuildServer _localBuildServer;
        private IEnumerable<IReportParser> _parsers;

        [ImportingConstructor]
        public ParseReportCommand(Logger logger,
                                  IFileSystem fileSystem,
                                  [ImportMany]IEnumerable<IBuildServer> buildServers,
                                  LocalBuildServer localBuildServer,
                                  [ImportMany]IEnumerable<IReportParser> parsers)
            : base(logger)
        {
            _fileSystem = fileSystem;
            _buildServers = buildServers;
            _localBuildServer = localBuildServer;
            _parsers = parsers;

            this.IsCommand("parsereport", "Parse the report and show warnings in console or inject them to the build server");
            this.HasOption("includesource", "Include the source in messages", s => _includeSourceInMessage = true);

            // TODO
            // force display warnings to console additionally to the build server
            // warning as error
        }

        protected override int Execute(string[] reportPaths)
        {
            int returnCode = 0;

            if (reportPaths.Length == 0)
            {
                _logger.Error("No report was specified. You must indicate at least one report file.");
                return 1;
            }

            var applicableBuildServers = GetApplicableBuildServer();
            _logger.Info("The following build servers has been detected:");
            foreach (var buildServer in applicableBuildServers)
            {
                _logger.Info("\t- {0}", buildServer.Name);
            }

            foreach (var reportPath in reportPaths)
            {
                _logger.Debug("Report path is '{0}'", reportPath);

                if (!_fileSystem.File.Exists(reportPath))
                {
                    _logger.Error("The report '{0}' was not found.", reportPath);
                    return 1;
                }

                XDocument report = null;
                try
                {
                    report = XDocument.Load(_fileSystem.File.OpenRead(reportPath));
                }
                catch (Exception ex)
                {
                    _logger.Error("An exception happened when loading the report '{1}': {0}", reportPath, ex);
                    return 2;
                }

                // Report type Detection
                var parser = GetParser(report);
                if (parser == null)
                {
                    _logger.Error("The adequate parser for this report was not found. You are welcome to address us an issue.");
                    return 3;
                }
                _logger.Debug("Report type is '{0}'", parser.Name);

                var issues = parser.Parse(report);
                _logger.Debug("{0} issues was found", issues.Count());

                foreach (var issue in issues)
                {
                    foreach (var buildServer in applicableBuildServers)
                    {
                        buildServer.WriteMessage(issue);
                    }
                }

                if (returnCode == 0)
                {
                    returnCode = issues.Any(i => i.Severity == IssueSeverity.Error) ? 4 : 0;
                }
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

        private IReportParser GetParser(XDocument document)
        {
            return _parsers.FirstOrDefault(p => p.CanParse(document));
        }
    }
}
