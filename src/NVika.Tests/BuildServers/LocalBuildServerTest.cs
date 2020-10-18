using NVika.BuildServers;
using NVika.Parsers;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace NVika.Tests.BuildServers
{
    public class LocalBuildServerTest
    {
        private StringBuilder _loggerOutput;

        [Fact]
        public void Name()
        {
            // act
            var buildServer = new LocalBuildServer(GetLogger());

            // arrange
            var name = buildServer.Name;

            // assert
            Assert.Equal("Local console", name);
        }

        [Fact]
        public void CanApplyToCurrentContext()
        {
            // act
            var buildServer = new LocalBuildServer(GetLogger());

            // arrange
            var result = buildServer.CanApplyToCurrentContext();

            // assert
            Assert.False(result);
        }

        [Fact]
        public void WriteIntegration()
        {
            // act
            var buildServer = new LocalBuildServer(GetLogger());

            var issues = GetIssues();

            // arrange
            foreach (var issue in issues)
            {
                buildServer.WriteMessage(issue);
            }

            // assert
            var outputLines = _loggerOutput.ToString();
            Assert.Contains("Suggestion \"Name1\" \"FilePath1\" - Line 42: \"Message1\"", outputLines);
            Assert.Contains("Warning \"Name2\" \"FilePath2\" - Line 465: \"Message2\"", outputLines);
            Assert.Contains("Error \"Name3\" \"FilePath3\" - Line 82: \"Message3\"", outputLines);
        }

        [Fact]
        public void WriteIntegration_IncludeSourceInMessage()
        {
            // act
            var buildServer = new LocalBuildServer(GetLogger());
            buildServer.ApplyParameters(true);

            var issues = GetIssues();

            // arrange
            foreach (var issue in issues)
            {
                buildServer.WriteMessage(issue);
            }

            // assert
            var outputLines = _loggerOutput.ToString();
            Assert.Contains("[\"Source1\"] Suggestion \"Name1\" \"FilePath1\" - Line 42: \"Message1\"", outputLines);
            Assert.Contains("[\"Source2\"] Warning \"Name2\" \"FilePath2\" - Line 465: \"Message2\"", outputLines);
            Assert.Contains("[\"Source3\"] Error \"Name3\" \"FilePath3\" - Line 82: \"Message3\"", outputLines);
        }

        [Fact]
        public void WriteIntegration_IssueWithoutFilePath()
        {
            // act
            var buildServer = new LocalBuildServer(GetLogger());

            var issues = GetIssuesWithoutFileInfos();

            // arrange
            foreach (var issue in issues)
            {
                buildServer.WriteMessage(issue);
            }

            // assert
            var outputLines = _loggerOutput.ToString();
            Assert.Contains("Suggestion \"Name1\": \"Message1\"", outputLines);
            Assert.Contains("Warning \"Name2\": \"Message2\"", outputLines);
            Assert.Contains("Error \"Name3\": \"Message3\"", outputLines);
        }

        private List<Issue> GetIssues()
        {
            return new List<Issue>
            {
                new Issue{ Category="Category1", Description = "Description1", FilePath = "FilePath1", HelpUri = null, Line = 42u, Message = "Message1", Name = "Name1", Offset = new Offset{ Start = 2u, End = 5u}, Project = "Project1", Severity = IssueSeverity.Suggestion, Source = "Source1" },
                new Issue{ Category="Category2", Description = "Description2", FilePath = "FilePath2", HelpUri = new Uri("https://www.wikipedia.com"), Line = 465u, Message = "Message2", Name = "Name2", Offset = new Offset{ Start = 36u, End = 546u}, Project = "Project1", Severity = IssueSeverity.Warning, Source = "Source2" },
                new Issue{ Category="Category1", Description = "Description3", FilePath = "FilePath3", HelpUri = new Uri("http://helperror.com"), Line = 82u, Message = "Message3", Name = "Name3", Project = "Project2", Severity = IssueSeverity.Error, Source = "Source3" },
            };
        }

        private List<Issue> GetIssuesWithoutFileInfos()
        {
            return new List<Issue>
            {
                new Issue{ Category="Category1", Description = "Description1", HelpUri = null, Message = "Message1", Name = "Name1", Project = "Project1", Severity = IssueSeverity.Suggestion, Source = "Source1" },
                new Issue{ Category="Category2", Description = "Description2", HelpUri = new Uri("https://www.wikipedia.com"), Message = "Message2", Name = "Name2", Project = "Project1", Severity = IssueSeverity.Warning, Source = "Source2" },
                new Issue{ Category="Category1", Description = "Description3", HelpUri = new Uri("http://helperror.com"), Message = "Message3", Name = "Name3", Project = "Project2", Severity = IssueSeverity.Error, Source = "Source3" },
            };
        }

        private ILogger GetLogger()
        {
            _loggerOutput = new StringBuilder();
            var writer = new StringWriter(_loggerOutput);
            var logger = new LoggerConfiguration()
                        .WriteTo.TextWriter(writer, Serilog.Events.LogEventLevel.Information)
                        .CreateLogger();
            return logger;
        }
    }
}
