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
    public class BuildServerCommand : CommandBase
    {
        private string reportPath;
        private IFileSystem _fileSystem;
        private bool _includeSourceInMessage;

#pragma warning disable 0649
        [ImportMany]
        private IEnumerable<IBuildServer> _buildServers;
        [Import]
        private LocalBuildServer _localBuildServer;
        [ImportMany]
        private IEnumerable<IReportParser> _parsers;
#pragma warning restore 0649

        [ImportingConstructor]
        public BuildServerCommand(IFileSystem fileSystem, Logger logger)
            : base(logger)
        {
            _fileSystem = fileSystem;
            _logger = logger;

            this.IsCommand("buildserver", "Parse the report and show warnings in console or inject them to the build server");
            this.HasAdditionalArguments(1, "Report to analyze");
            this.HasOption("includesource", "Include the source in messages", s => _includeSourceInMessage = true);

            // TODO
            // force display warnings to console additionally to the build server
            // force to end the build (exitcode > 0) when errors
            // warning as error
        }

        protected override int Execute(string[] remainingArguments)
        {
            if (remainingArguments.Length == 1)
            {
                reportPath = remainingArguments[0];
            }
            _logger.Debug("Report path is '{0}'", reportPath);

            if (!_fileSystem.File.Exists(reportPath))
            {
                _logger.Error("The report '{0}' was not found.", reportPath);
                return 1;
            }

            var applicableBuildServers = GetApplicableBuildServer();
            _logger.Info("The following build servers will be used:");
            foreach (var buildServer in applicableBuildServers)
            {
                _logger.Info("\t- {0}", buildServer.Name);
            }

            XDocument report = null;
            try
            {
                report = XDocument.Load(reportPath);
            }
            catch (Exception ex)
            {
                _logger.Error("An exception happened when loading the report: {0}", ex);
                return 2;
            }

            if (report == null || string.IsNullOrWhiteSpace(report.ToString()))
            {
                _logger.Error("The report file is empty");
                return 2;
            }

            // Report type Detection
            var parser = GetParser(report);
            if (parser == null)
            {
                _logger.Error("The adequate parser for this report was not found. You are welcome to address us an issue.");
                return 3;
            }
            _logger.Info("Report type is '{0}'", parser.Name);

            var issues = parser.Parse(report);
            _logger.Debug("{0} issues types was found", issues.Count());

            foreach (var issue in issues)
            {
                foreach (var buildServer in applicableBuildServers)
                {
                    buildServer.WriteMessage(issue);
                }
            }

            return issues.Any(i => i.Severity == IssueSeverity.Error) ? 4 : 0;
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
