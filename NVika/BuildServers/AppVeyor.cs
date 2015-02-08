using System;
using System.ComponentModel.Composition;
using System.Net.Http;

namespace NVika
{
    internal sealed class AppVeyorBuildServer : IBuildServer
    {
        private readonly Logger _logger;

        public string Name
        {
            get { return "AppVeyor"; }
        }

        [ImportingConstructor]
        public AppVeyorBuildServer(Logger logger)
        {
            _logger = logger;
        }

        public bool CanApplyToCurrentContext()
        {
            return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPVEYOR"));
        }

        public async void WriteMessage(string message, string category, string details, string filename, string line, string offset, string projectName)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(Environment.GetEnvironmentVariable("APPVEYOR_API_URL"));

                var response = await httpClient.PostAsJsonAsync("api/build/compilationmessages", new { Message = message, Category = category, Details = details, FileName = filename, Line = line, Column = offset, ProjectName = projectName });

                _logger.Debug("AppVeyor CompilationMessage Response: {0}", response.StatusCode);
                _logger.Debug(await response.Content.ReadAsStringAsync());
            }
        }
    }
}
