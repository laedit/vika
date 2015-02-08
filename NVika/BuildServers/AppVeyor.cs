using System;
using System.Net.Http;

namespace NVika
{
    internal sealed class AppVeyorBuildServer : IBuildServer
    {
        public string Name
        {
            get { return "AppVeyor"; }
        }

        public bool CanApplyToCurrentContext()
        {
            return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPVEYOR"));
        }

        public async void WriteMessage(string message, string category, string details, string filename, string line, string offset, string projectName)
        {
            using(var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(Environment.GetEnvironmentVariable("APPVEYOR_API_URL"));

                await httpClient.PostAsJsonAsync("api/build/compilationmessages", new { Message = message, Category = category, Details = details, FileName = filename, Line = line, Column = offset, ProjectName = projectName });
            }
        }
    }
}
