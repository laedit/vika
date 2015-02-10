using NVika.Parsers;
using System;
using System.ComponentModel.Composition;
using System.Net.Http;

namespace NVika.BuildServers
{
    internal sealed class AppVeyorBuildServer : BuildServerBase
    {
        private readonly Logger _logger;
        private readonly string _appVeyorAPIUrl;

        public override string Name
        {
            get { return "AppVeyor"; }
        }

        [ImportingConstructor]
        public AppVeyorBuildServer(Logger logger)
        {
            _logger = logger;
            _appVeyorAPIUrl = Environment.GetEnvironmentVariable("APPVEYOR_API_URL");
        }

        public override bool CanApplyToCurrentContext()
        {
            return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPVEYOR"));
        }

        protected override void WriteIntegration(Issue issue)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(_appVeyorAPIUrl);

                string category = "information";
                switch (issue.Severity)
                {
                    case IssueSeverity.Error: category = "error";
                        break;

                    case IssueSeverity.Warning: category = "warning";
                        break;
                }

                string filePath = issue.FilePath.Replace(issue.Project + @"\", string.Empty);

                _logger.Debug("Send compilation message to AppVeyor:");
                _logger.Debug("Message: {0}", issue.Message);
                _logger.Debug("Category: {0}", category);
                _logger.Debug("FileName: {0}", filePath);
                _logger.Debug("Line: {0}", issue.Line);
                _logger.Debug("ProjectName: {0}", issue.Project);
                httpClient.PostAsJsonAsync("api/build/compilationmessages", new
                {
                    Message = issue.Message,
                    Category = category,
                    FileName = filePath,
                    Line = issue.Line,
                    ProjectName = issue.Project
                }).Wait();

            }
        }
    }
}
