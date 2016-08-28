using NVika.Abstractions;
using NVika.Parsers;
using Serilog;
using System;
using System.ComponentModel.Composition;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;

namespace NVika.BuildServers
{
    internal sealed class AppVeyor : BuildServerBase
    {
        private readonly ILogger _logger;
        private readonly IEnvironment _environment;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _appVeyorApiUrl;

        public override string Name
        {
            get { return nameof(AppVeyor); }
        }

        [ImportingConstructor]
        public AppVeyor(ILogger logger, IEnvironment environment, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _environment = environment;
            _httpClientFactory = httpClientFactory;
            _appVeyorApiUrl = _environment.GetEnvironmentVariable("APPVEYOR_API_URL");
        }

        public override bool CanApplyToCurrentContext()
        {
            return !string.IsNullOrEmpty(_environment.GetEnvironmentVariable("APPVEYOR"));
        }

        public override void WriteMessage(Issue issue)
        {
            var message = issue.Message;
            if (IncludeSourceInMessage)
            {
                message = $"[{issue.Source}] {message}";
            }

            var category = "information";
            switch (issue.Severity)
            {
                case IssueSeverity.Error:
                    category = "error";
                    break;

                case IssueSeverity.Warning:
                    category = "warning";
                    break;
            }

            var filePath = issue.FilePath.Replace(issue.Project + @"\", string.Empty);

            _logger.Debug("Send compilation message to AppVeyor:");
            _logger.Debug("Message: {message}", message);
            _logger.Debug("Category: {category}", category);
            _logger.Debug("FileName: {filePath}", filePath);
            _logger.Debug("Line: {line}", issue.Line);
            _logger.Debug("ProjectName: {project}", issue.Project);

            using (var httpClient = _httpClientFactory.Create())
            {
                httpClient.BaseAddress = new Uri(_appVeyorApiUrl);

                var compilationMessage = new CompilationMessage
                {
                    Message = message,
                    Category = category,
                    FileName = filePath,
                    Line = issue.Line,
                    ProjectName = issue.Project
                };

                if (issue.Offset != null)
                {
                    _logger.Debug("Column: {Offset.Start}", issue.Offset.Start);
                    compilationMessage.Column = issue.Offset.Start + 1;
                }

                var jsonFormat = new JsonMediaTypeFormatter();
                jsonFormat.SerializerSettings.DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore;
                jsonFormat.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;

                var response = httpClient.PostAsync("api/build/compilationmessages", compilationMessage, jsonFormat).Result;

                if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.NoContent)
                {
                    _logger.Error("An error is occurred during the call to AppVeyor API: {response}", response);
                }
            }
        }

        private sealed class CompilationMessage
        {
            public string Message { get; set; }

            public string Category { get; set; }

            public string FileName { get; set; }

            public uint? Line { get; set; }

            public string ProjectName { get; set; }

            public uint? Column { get; set; }
        }
    }
}
