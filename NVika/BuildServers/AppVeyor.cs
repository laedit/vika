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

        public void WriteMessage(string message, string category, string details, string filename, string line, string offset, string projectName)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(_appVeyorAPIUrl);

                var dashIndex = offset.IndexOf("-");
                if (dashIndex > -1)
                {
                    offset = offset.Substring(0, dashIndex);
                }

                // TODO convert category to an enum
                switch (category)
                {
                    case "ERROR": category = "error";
                        break;

                    case "WARNING": category = "warning";
                        break;

                    default: category = "information";
                        break;
                }

                filename = filename.Replace(projectName + @"\", string.Empty);

                _logger.Debug("Send compilation message to AppVeyor:");
                _logger.Debug("Message: {0}", message);
                _logger.Debug("Category: {0}", category);
                _logger.Debug("FileName: {0}", filename);
                _logger.Debug("Line: {0}", line);
                _logger.Debug("Column: {0}", offset);
                _logger.Debug("ProjectName: {0}", projectName);
                httpClient.PostAsJsonAsync("api/build/compilationmessages", new { Message = details, Category = category, FileName = filename, Line = line, Column = offset, ProjectName = projectName }).Wait();

            }
        }
    }
}
