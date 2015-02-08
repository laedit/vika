using System;
using System.ComponentModel.Composition;
using System.Net.Http;

namespace NVika
{
    internal sealed class AppVeyorBuildServer : IBuildServer
    {
        private readonly Logger _logger;
        private readonly string _appVeyorAPIUrl;

        public string Name
        {
            get { return "AppVeyor"; }
        }

        [ImportingConstructor]
        public AppVeyorBuildServer(Logger logger)
        {
            _logger = logger;
            _appVeyorAPIUrl = Environment.GetEnvironmentVariable("APPVEYOR_API_URL");
        }

        public bool CanApplyToCurrentContext()
        {
            return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPVEYOR"));
        }

        public async void WriteMessage(string message, string category, string details, string filename, string line, string offset, string projectName)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(_appVeyorAPIUrl);

                _logger.Debug("Send compilation message to AppVeyor:");
                _logger.Debug("Message: {0}", message);
                _logger.Debug("Category: {0}", category);
                _logger.Debug("Details: {0}", details);
                _logger.Debug("FileName: {0}", filename);
                _logger.Debug("Line: {0}", line);
                _logger.Debug("Column: {0}", offset);
                _logger.Debug("ProjectName: {0}", projectName);
                await httpClient.PostAsJsonAsync("api/build/compilationmessages", new { Message = message, Category = category, Details = details, FileName = filename, Line = line, Column = offset, ProjectName = projectName });
            }
        }
    }
}
