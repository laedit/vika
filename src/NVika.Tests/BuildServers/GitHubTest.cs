using NVika.Abstractions;
using NVika.BuildServers;
using NVika.Parsers;
using NVika.Tests.Mocks;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace NVika.Tests.BuildServers
{
    public class GitHubTest
    {
        private StringWriter _consoleOutput;

        public GitHubTest()
        {
            _consoleOutput = new StringWriter();
            Console.SetOut(_consoleOutput);

        }

        [Fact]
        public void Name()
        {
            // act
            var buildServer = new GitHub(GetEnvironment());

            // arrange
            var name = buildServer.Name;

            // assert
            Assert.Equal("GitHub", name);
        }

        [Fact]
        public void CanApplyToCurrentContext_false()
        {
            // act
            var buildServer = new GitHub(GetEnvironment());

            // arrange
            var result = buildServer.CanApplyToCurrentContext();

            // assert
            Assert.False(result);
        }

        [Fact]
        public void CanApplyToCurrentContext_true()
        {
            // act
            var buildServer = new GitHub(GetEnvironment(true));

            // arrange
            var result = buildServer.CanApplyToCurrentContext();

            // assert
            Assert.True(result);
        }

        [Fact]
        public void WriteIntegration()
        {
            // act
            var buildServer = new GitHub(GetEnvironment(true));

            var issues = GetIssues();

            // arrange
            foreach (var issue in issues)
            {
                buildServer.WriteMessage(issue);
            }

            // assert
            var outputLines = _consoleOutput.ToString();
            Assert.Contains("::notice file=FilePath1,col=2,line=42::Message1 in FilePath1 on line 42", outputLines);
            Assert.Contains("::warning file=FilePath2,col=36,line=465::Message2 in FilePath2 on line 465", outputLines);
            Assert.Contains("::error file=FilePath3,line=82::Message3 in FilePath3 on line 82", outputLines);
        }

        [Fact]
        public void WriteIntegration_IncludeSourceInMessage()
        {
            // act
            var buildServer = new GitHub(GetEnvironment(true));
            buildServer.ApplyParameters(true);

            var issues = GetIssues();

            // arrange
            foreach (var issue in issues)
            {
                buildServer.WriteMessage(issue);
            }

            // assert
            var outputLines = _consoleOutput.ToString();
            Assert.Contains("::notice file=FilePath1,col=2,line=42::Message1 in FilePath1 on line 42", outputLines);
            Assert.Contains("::warning file=FilePath2,col=36,line=465::Message2 in FilePath2 on line 465", outputLines);
            Assert.Contains("::error file=FilePath3,line=82::Message3 in FilePath3 on line 82", outputLines);
        }

        [Fact]
        public void WriteIntegration_IssueWithoutFilePath()
        {
            // act
            var buildServer = new GitHub(GetEnvironment(true));

            var issues = GetIssuesWithoutFileInfos();

            // arrange
            foreach (var issue in issues)
            {
                buildServer.WriteMessage(issue);
            }

            // assert
            var outputLines = _consoleOutput.ToString();
            Assert.Contains("::notice ::Message1", outputLines);
            Assert.Contains("::warning ::Message2", outputLines);
            Assert.Contains("::error ::Message3", outputLines);
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

        private IEnvironment GetEnvironment(bool activate = false)
        {
            var variables = new Dictionary<string, string>();
            if (activate)
            {
                variables.Add("GITHUB_ACTIONS", "TRUE");
            }
            return new MockEnvironment(variables);
        }
    }
}
