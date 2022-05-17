using NSubstitute;
using NVika.Abstractions;
using NVika.BuildServers;
using NVika.Parsers;
using NVika.Tests.Mocks;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using Xunit;

namespace NVika.Tests.BuildServers
{
    public class AppVeyorTest
    {
        private StringBuilder _loggerOutput;

        [Fact]
        public void Name()
        {
            // act
            var buildServer = new AppVeyor(GetLogger(), GetEnvironment(), GetHttpClientFactory());

            // arrange
            var name = buildServer.Name;

            // assert
            Assert.Equal("AppVeyor", name);
        }

        [Fact]
        public void CanApplyToCurrentContext()
        {
            // act
            var buildServer = new AppVeyor(GetLogger(), GetEnvironment(), GetHttpClientFactory());

            // arrange
            var result = buildServer.CanApplyToCurrentContext();

            // assert
            Assert.True(result);
        }

        [Fact]
        public void CanApplyToCurrentContext_NotOnAppVeyor()
        {
            // act
            var buildServer = new AppVeyor(GetLogger(), GetEnvironment(false), GetHttpClientFactory());

            // arrange
            var result = buildServer.CanApplyToCurrentContext();

            // assert
            Assert.False(result);
        }

        [Fact]
        public void WriteIntegration()
        {
            // act
            MockHttpClientFactory httpClientFactory = GetHttpClientFactory();
            var buildServer = new AppVeyor(GetLogger(), GetEnvironment(), httpClientFactory);

            var issues = GetIssues();

            // arrange
            foreach (var issue in issues)
            {
                buildServer.WriteMessage(issue);
            }

            // assert
            Assert.Equal(5, httpClientFactory.HttpMessageHandler.Requests.Count);

            Assert.Equal(HttpMethod.Post, httpClientFactory.HttpMessageHandler.Requests[0].Item1.Method);
            Assert.Equal("http://localhost:8080/api/build/compilationmessages", httpClientFactory.HttpMessageHandler.Requests[0].Item1.RequestUri.AbsoluteUri);
            Assert.Equal("utf-8", httpClientFactory.HttpMessageHandler.Requests[0].Item1.Content.Headers.ContentType.CharSet);
            Assert.Equal("application/json", httpClientFactory.HttpMessageHandler.Requests[0].Item1.Content.Headers.ContentType.MediaType);
            Assert.Equal("{\"message\":\"Message1\",\"category\":\"information\",\"fileName\":\"FilePath1\",\"line\":42,\"projectName\":\"Project1\",\"column\":3,\"details\":\"Message1 in FilePath1 on line 42\"}", httpClientFactory.HttpMessageHandler.Requests[0].Item2);

            Assert.Equal(HttpMethod.Post, httpClientFactory.HttpMessageHandler.Requests[1].Item1.Method);
            Assert.Equal("http://localhost:8080/api/build/compilationmessages", httpClientFactory.HttpMessageHandler.Requests[1].Item1.RequestUri.AbsoluteUri);
            Assert.Equal("utf-8", httpClientFactory.HttpMessageHandler.Requests[1].Item1.Content.Headers.ContentType.CharSet);
            Assert.Equal("application/json", httpClientFactory.HttpMessageHandler.Requests[1].Item1.Content.Headers.ContentType.MediaType);
            Assert.Equal("{\"message\":\"Message2\",\"category\":\"warning\",\"fileName\":\"FilePath2\",\"line\":465,\"projectName\":\"Project1\",\"column\":37,\"details\":\"Message2 in FilePath2 on line 465\"}", httpClientFactory.HttpMessageHandler.Requests[1].Item2);

            Assert.Equal(HttpMethod.Post, httpClientFactory.HttpMessageHandler.Requests[2].Item1.Method);
            Assert.Equal("http://localhost:8080/api/build/compilationmessages", httpClientFactory.HttpMessageHandler.Requests[2].Item1.RequestUri.AbsoluteUri);
            Assert.Equal("utf-8", httpClientFactory.HttpMessageHandler.Requests[2].Item1.Content.Headers.ContentType.CharSet);
            Assert.Equal("application/json", httpClientFactory.HttpMessageHandler.Requests[2].Item1.Content.Headers.ContentType.MediaType);
            Assert.Equal("{\"message\":\"Message3\",\"category\":\"error\",\"fileName\":\"FilePath3\",\"line\":82,\"projectName\":\"Project2\",\"details\":\"Message3 in FilePath3 on line 82\"}", httpClientFactory.HttpMessageHandler.Requests[2].Item2);

            Assert.Equal(HttpMethod.Post, httpClientFactory.HttpMessageHandler.Requests[3].Item1.Method);
            Assert.Equal("http://localhost:8080/api/build/compilationmessages", httpClientFactory.HttpMessageHandler.Requests[3].Item1.RequestUri.AbsoluteUri);
            Assert.Equal("utf-8", httpClientFactory.HttpMessageHandler.Requests[3].Item1.Content.Headers.ContentType.CharSet);
            Assert.Equal("application/json", httpClientFactory.HttpMessageHandler.Requests[3].Item1.Content.Headers.ContentType.MediaType);
            Assert.Equal("{\"message\":\"Message4\",\"category\":\"information\",\"projectName\":\"Project2\",\"details\":\"Message4\"}", httpClientFactory.HttpMessageHandler.Requests[3].Item2);

            Assert.Equal(HttpMethod.Post, httpClientFactory.HttpMessageHandler.Requests[4].Item1.Method);
            Assert.Equal("http://localhost:8080/api/build/compilationmessages", httpClientFactory.HttpMessageHandler.Requests[4].Item1.RequestUri.AbsoluteUri);
            Assert.Equal("utf-8", httpClientFactory.HttpMessageHandler.Requests[4].Item1.Content.Headers.ContentType.CharSet);
            Assert.Equal("application/json", httpClientFactory.HttpMessageHandler.Requests[4].Item1.Content.Headers.ContentType.MediaType);
            Assert.Equal(@"{""message"":""Message5"",""category"":""information"",""fileName"":""D:\\Prog\\Github\\vika\\src\\NVika\\Program.cs"",""details"":""Message5 in D:\\Prog\\Github\\vika\\src\\NVika\\Program.cs on line ""}", httpClientFactory.HttpMessageHandler.Requests[4].Item2);

            Assert.Equal(string.Empty, _loggerOutput.ToString());
        }

        [Fact]
        public void WriteIntegration_IncludeSourceInMessage()
        {
            // act
            MockHttpClientFactory httpClientFactory = GetHttpClientFactory();
            var buildServer = new AppVeyor(GetLogger(), GetEnvironment(), httpClientFactory);
            buildServer.ApplyParameters(true);

            var output = new StringBuilder();
            var writer = new StringWriter(output);
            Console.SetOut(writer);

            var issues = GetIssues();

            // arrange
            foreach (var issue in issues)
            {
                buildServer.WriteMessage(issue);
            }

            // assert
            Assert.Equal(5, httpClientFactory.HttpMessageHandler.Requests.Count);

            Assert.Equal(HttpMethod.Post, httpClientFactory.HttpMessageHandler.Requests[0].Item1.Method);
            Assert.Equal("http://localhost:8080/api/build/compilationmessages", httpClientFactory.HttpMessageHandler.Requests[0].Item1.RequestUri.AbsoluteUri);
            Assert.Equal("utf-8", httpClientFactory.HttpMessageHandler.Requests[0].Item1.Content.Headers.ContentType.CharSet);
            Assert.Equal("application/json", httpClientFactory.HttpMessageHandler.Requests[0].Item1.Content.Headers.ContentType.MediaType);
            Assert.Equal("{\"message\":\"[Source1] Message1\",\"category\":\"information\",\"fileName\":\"FilePath1\",\"line\":42,\"projectName\":\"Project1\",\"column\":3,\"details\":\"Message1 in FilePath1 on line 42\"}", httpClientFactory.HttpMessageHandler.Requests[0].Item2);

            Assert.Equal(HttpMethod.Post, httpClientFactory.HttpMessageHandler.Requests[1].Item1.Method);
            Assert.Equal("http://localhost:8080/api/build/compilationmessages", httpClientFactory.HttpMessageHandler.Requests[1].Item1.RequestUri.AbsoluteUri);
            Assert.Equal("utf-8", httpClientFactory.HttpMessageHandler.Requests[1].Item1.Content.Headers.ContentType.CharSet);
            Assert.Equal("application/json", httpClientFactory.HttpMessageHandler.Requests[1].Item1.Content.Headers.ContentType.MediaType);
            Assert.Equal("{\"message\":\"[Source2] Message2\",\"category\":\"warning\",\"fileName\":\"FilePath2\",\"line\":465,\"projectName\":\"Project1\",\"column\":37,\"details\":\"Message2 in FilePath2 on line 465\"}", httpClientFactory.HttpMessageHandler.Requests[1].Item2);

            Assert.Equal(HttpMethod.Post, httpClientFactory.HttpMessageHandler.Requests[2].Item1.Method);
            Assert.Equal("http://localhost:8080/api/build/compilationmessages", httpClientFactory.HttpMessageHandler.Requests[2].Item1.RequestUri.AbsoluteUri);
            Assert.Equal("utf-8", httpClientFactory.HttpMessageHandler.Requests[2].Item1.Content.Headers.ContentType.CharSet);
            Assert.Equal("application/json", httpClientFactory.HttpMessageHandler.Requests[2].Item1.Content.Headers.ContentType.MediaType);
            Assert.Equal("{\"message\":\"[Source3] Message3\",\"category\":\"error\",\"fileName\":\"FilePath3\",\"line\":82,\"projectName\":\"Project2\",\"details\":\"Message3 in FilePath3 on line 82\"}", httpClientFactory.HttpMessageHandler.Requests[2].Item2);

            Assert.Equal(HttpMethod.Post, httpClientFactory.HttpMessageHandler.Requests[3].Item1.Method);
            Assert.Equal("http://localhost:8080/api/build/compilationmessages", httpClientFactory.HttpMessageHandler.Requests[3].Item1.RequestUri.AbsoluteUri);
            Assert.Equal("utf-8", httpClientFactory.HttpMessageHandler.Requests[3].Item1.Content.Headers.ContentType.CharSet);
            Assert.Equal("application/json", httpClientFactory.HttpMessageHandler.Requests[3].Item1.Content.Headers.ContentType.MediaType);
            Assert.Equal("{\"message\":\"[Source4] Message4\",\"category\":\"information\",\"projectName\":\"Project2\",\"details\":\"Message4\"}", httpClientFactory.HttpMessageHandler.Requests[3].Item2);

            Assert.Equal(HttpMethod.Post, httpClientFactory.HttpMessageHandler.Requests[4].Item1.Method);
            Assert.Equal("http://localhost:8080/api/build/compilationmessages", httpClientFactory.HttpMessageHandler.Requests[4].Item1.RequestUri.AbsoluteUri);
            Assert.Equal("utf-8", httpClientFactory.HttpMessageHandler.Requests[4].Item1.Content.Headers.ContentType.CharSet);
            Assert.Equal("application/json", httpClientFactory.HttpMessageHandler.Requests[4].Item1.Content.Headers.ContentType.MediaType);
            Assert.Equal(@"{""message"":""[Source4] Message5"",""category"":""information"",""fileName"":""D:\\Prog\\Github\\vika\\src\\NVika\\Program.cs"",""details"":""Message5 in D:\\Prog\\Github\\vika\\src\\NVika\\Program.cs on line ""}", httpClientFactory.HttpMessageHandler.Requests[4].Item2);

            Assert.Equal(string.Empty, _loggerOutput.ToString());
        }

        [Fact]
        public void WriteIntegration_AppVeyorAPIError()
        {
            // act
            MockHttpClientFactory httpClientFactory = GetHttpClientFactory(HttpStatusCode.Forbidden);
            var buildServer = new AppVeyor(GetLogger(), GetEnvironment(), httpClientFactory);

            var issues = GetIssues();

            // arrange
            foreach (var issue in issues)
            {
                buildServer.WriteMessage(issue);
            }

            // assert
            Assert.Equal(5, httpClientFactory.HttpMessageHandler.Requests.Count);

            Assert.Equal(HttpMethod.Post, httpClientFactory.HttpMessageHandler.Requests[0].Item1.Method);
            Assert.Equal("http://localhost:8080/api/build/compilationmessages", httpClientFactory.HttpMessageHandler.Requests[0].Item1.RequestUri.AbsoluteUri);
            Assert.Equal("utf-8", httpClientFactory.HttpMessageHandler.Requests[0].Item1.Content.Headers.ContentType.CharSet);
            Assert.Equal("application/json", httpClientFactory.HttpMessageHandler.Requests[0].Item1.Content.Headers.ContentType.MediaType);
            Assert.Equal("{\"message\":\"Message1\",\"category\":\"information\",\"fileName\":\"FilePath1\",\"line\":42,\"projectName\":\"Project1\",\"column\":3,\"details\":\"Message1 in FilePath1 on line 42\"}", httpClientFactory.HttpMessageHandler.Requests[0].Item2);

            Assert.Equal(HttpMethod.Post, httpClientFactory.HttpMessageHandler.Requests[1].Item1.Method);
            Assert.Equal("http://localhost:8080/api/build/compilationmessages", httpClientFactory.HttpMessageHandler.Requests[1].Item1.RequestUri.AbsoluteUri);
            Assert.Equal("utf-8", httpClientFactory.HttpMessageHandler.Requests[1].Item1.Content.Headers.ContentType.CharSet);
            Assert.Equal("application/json", httpClientFactory.HttpMessageHandler.Requests[1].Item1.Content.Headers.ContentType.MediaType);
            Assert.Equal("{\"message\":\"Message2\",\"category\":\"warning\",\"fileName\":\"FilePath2\",\"line\":465,\"projectName\":\"Project1\",\"column\":37,\"details\":\"Message2 in FilePath2 on line 465\"}", httpClientFactory.HttpMessageHandler.Requests[1].Item2);

            Assert.Equal(HttpMethod.Post, httpClientFactory.HttpMessageHandler.Requests[2].Item1.Method);
            Assert.Equal("http://localhost:8080/api/build/compilationmessages", httpClientFactory.HttpMessageHandler.Requests[2].Item1.RequestUri.AbsoluteUri);
            Assert.Equal("utf-8", httpClientFactory.HttpMessageHandler.Requests[2].Item1.Content.Headers.ContentType.CharSet);
            Assert.Equal("application/json", httpClientFactory.HttpMessageHandler.Requests[2].Item1.Content.Headers.ContentType.MediaType);
            Assert.Equal("{\"message\":\"Message3\",\"category\":\"error\",\"fileName\":\"FilePath3\",\"line\":82,\"projectName\":\"Project2\",\"details\":\"Message3 in FilePath3 on line 82\"}", httpClientFactory.HttpMessageHandler.Requests[2].Item2);

            Assert.Equal(HttpMethod.Post, httpClientFactory.HttpMessageHandler.Requests[3].Item1.Method);
            Assert.Equal("http://localhost:8080/api/build/compilationmessages", httpClientFactory.HttpMessageHandler.Requests[3].Item1.RequestUri.AbsoluteUri);
            Assert.Equal("utf-8", httpClientFactory.HttpMessageHandler.Requests[3].Item1.Content.Headers.ContentType.CharSet);
            Assert.Equal("application/json", httpClientFactory.HttpMessageHandler.Requests[3].Item1.Content.Headers.ContentType.MediaType);
            Assert.Equal("{\"message\":\"Message4\",\"category\":\"information\",\"projectName\":\"Project2\",\"details\":\"Message4\"}", httpClientFactory.HttpMessageHandler.Requests[3].Item2);

            Assert.Equal(HttpMethod.Post, httpClientFactory.HttpMessageHandler.Requests[4].Item1.Method);
            Assert.Equal("http://localhost:8080/api/build/compilationmessages", httpClientFactory.HttpMessageHandler.Requests[4].Item1.RequestUri.AbsoluteUri);
            Assert.Equal("utf-8", httpClientFactory.HttpMessageHandler.Requests[4].Item1.Content.Headers.ContentType.CharSet);
            Assert.Equal("application/json", httpClientFactory.HttpMessageHandler.Requests[4].Item1.Content.Headers.ContentType.MediaType);
            Assert.Equal(@"{""message"":""Message5"",""category"":""information"",""fileName"":""D:\\Prog\\Github\\vika\\src\\NVika\\Program.cs"",""details"":""Message5 in D:\\Prog\\Github\\vika\\src\\NVika\\Program.cs on line ""}", httpClientFactory.HttpMessageHandler.Requests[4].Item2);

            var logs = _loggerOutput.ToString();
            Assert.NotNull(logs);
            Assert.Contains("An error is occurred during the call to AppVeyor API: \"StatusCode: 403, ReasonPhrase: 'Forbidden', Version: 1.1, Content: System.Net.Http.EmptyContent, Headers:", logs);
        }

        private MockHttpClientFactory GetHttpClientFactory(HttpStatusCode responseStatusCode = HttpStatusCode.OK)
        {
            var httpClientFactory = new MockHttpClientFactory(responseStatusCode);
            httpClientFactory.SetResponse(new HttpResponseMessage(responseStatusCode));

            return httpClientFactory;
        }

        private ILogger GetLogger()
        {
            _loggerOutput = new StringBuilder();
            var writer = new StringWriter(_loggerOutput);
            var logger = new LoggerConfiguration()
                        .MinimumLevel.Error()
                        .WriteTo.TextWriter(writer)
                        .CreateLogger();
            return logger;
        }

        private IEnvironment GetEnvironment(bool isOnAppVeyor = true)
        {
            var environment = Substitute.For<IEnvironment>();
            environment.GetEnvironmentVariable("APPVEYOR_API_URL").Returns("http://localhost:8080");

            if (isOnAppVeyor)
            {
                environment.GetEnvironmentVariable("APPVEYOR").Returns("TRUE");
            }

            return environment;
        }

        private List<Issue> GetIssues()
        {
            return new List<Issue>
            {
                new Issue{ Category="Category1", Description = "Description1", FilePath = "FilePath1", HelpUri = null, Line = 42u, Message = "Message1", Name = "Name1", Offset = new Offset{ Start = 2u, End = 5u}, Project = "Project1", Severity = IssueSeverity.Suggestion, Source = "Source1" },
                new Issue{ Category="Category2", Description = "Description2", FilePath = "FilePath2", HelpUri = new Uri("https://www.wikipedia.com"), Line = 465u, Message = "Message2", Name = "Name2", Offset = new Offset{ Start = 36u, End = 546u}, Project = "Project1", Severity = IssueSeverity.Warning, Source = "Source2" },
                new Issue{ Category="Category1", Description = "Description3", FilePath = "FilePath3", HelpUri = new Uri("http://helperror.com"), Line = 82u, Message = "Message3", Name = "Name3", Project = "Project2", Severity = IssueSeverity.Error, Source = "Source3" },
                new Issue{ Category="Category3", Description = "Description4", HelpUri = new Uri("http://nosolution.com"), Message = "Message4", Name = "Name4", Project = "Project2", Severity = IssueSeverity.Hint, Source = "Source4" },
                new Issue{ Category="Category4", Description = "Path fix", FilePath = @"D:\Prog\Github\vika\src\NVika\Program.cs", HelpUri = null, Message = "Message5", Name = "Name5", Project = null, Severity = IssueSeverity.Hint, Source = "Source4" },
            };
        }
    }
}
