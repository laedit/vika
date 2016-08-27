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
        private readonly IFileSystem _fileSystem;
        private readonly IEnumerable<IBuildServer> _buildServers;
        private readonly LocalBuildServer _localBuildServer;
        private readonly IEnumerable<IReportParser> _parsers;
        private bool _includeSourceInMessage;

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

            IsCommand("parsereport", "Parse the report and show warnings in console or inject them to the build server");
            HasOption("includesource", "Include the source in messages", s => _includeSourceInMessage = true);

            // TODO
            // force display warnings to console additionally to the build server
            // warning as error
        }

        protected override int Execute(string[] reportPaths)
        {
            var returnCode = 0;

            if (reportPaths.Length == 0)
            {
                Logger.Error("No report was specified. You must indicate at least one report file.");
                return 1;
            }

            var applicableBuildServers = GetApplicableBuildServer();
            Logger.Info("The following build servers has been detected:");
            foreach (var buildServer in applicableBuildServers)
            {
                Logger.Info("\t- {0}", buildServer.Name);
            }

            foreach (var reportPath in reportPaths)
            {
                Logger.Debug("Report path is '{0}'", reportPath);

                if (!_fileSystem.File.Exists(reportPath))
                {
                    Logger.Error("The report '{0}' was not found.", reportPath);
                    return 1;
                }

                XDocument report = null;
                try
                {
                    report = XDocument.Load(_fileSystem.File.OpenRead(reportPath));
                }
                catch (Exception ex)
                {
                    Logger.Error("An exception happened when loading the report '{1}': {0}", reportPath, ex);
                    return 2;
                }

                // Report type Detection
                var parser = GetParser(report);
                if (parser == null)
                {
                    Logger.Error("The adequate parser for this report was not found. You are welcome to address us an issue.");
                    return 3;
                }
                Logger.Debug("Report type is '{0}'", parser.Name);

                var issues = parser.Parse(report);
                Logger.Debug("{0} issues was found", issues.Count());

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
