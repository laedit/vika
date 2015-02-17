using NVika.Abstractions;
using NVika.Parsers;
using System;
using System.ComponentModel.Composition;
using System.Net;
using System.Net.Http;

namespace NVika.BuildServers
{
    internal sealed class AppVeyor : BuildServerBase
    {
        private readonly Logger _logger;
        private readonly IEnvironment _environment;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _appVeyorAPIUrl;

        public override string Name
        {
            get { return "AppVeyor"; }
        }

        [ImportingConstructor]
        public AppVeyor(Logger logger, IEnvironment environment, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _environment = environment;
            _httpClientFactory = httpClientFactory;
            _appVeyorAPIUrl = _environment.GetEnvironmentVariable("APPVEYOR_API_URL");
        }

        public override bool CanApplyToCurrentContext()
        {
            return !string.IsNullOrEmpty(_environment.GetEnvironmentVariable("APPVEYOR"));
        }

        public override void WriteMessage(Issue issue)
        {
            var message = issue.Message;
            if (_includeSourceInMessage)
            {
                message = string.Format("[{0}] {1}", issue.Source, message);
            }

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
            _logger.Debug("Message: {0}", message);
            _logger.Debug("Category: {0}", category);
            _logger.Debug("FileName: {0}", filePath);
            _logger.Debug("Line: {0}", issue.Line);
            _logger.Debug("ProjectName: {0}", issue.Project);

            using (var httpClient = _httpClientFactory.Create())
            {
                httpClient.BaseAddress = new Uri(_appVeyorAPIUrl);

                var response = httpClient.PostAsJsonAsync("api/build/compilationmessages", new
                {
                    Message = message,
                    Category = category,
                    FileName = filePath,
                    Line = issue.Line,
                    ProjectName = issue.Project
                }).Result;

                if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.NoContent)
                {
                    _logger.Error("An error is occurred during the call to AppVeyor API: {0}", response);
                }
            }
        }
    }
}
