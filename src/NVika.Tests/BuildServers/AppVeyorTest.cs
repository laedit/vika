﻿using NSubstitute;
using NVika.Abstractions;
using NVika.BuildServers;
using NVika.Parsers;
using NVika.Tests.Mocks;
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
            Assert.Equal(3, httpClientFactory.HttpMessageHandler.Requests.Count);

            Assert.Equal(HttpMethod.Post, httpClientFactory.HttpMessageHandler.Requests[0].Item1.Method);
            Assert.Equal("http://localhost:8080/api/build/compilationmessages", httpClientFactory.HttpMessageHandler.Requests[0].Item1.RequestUri.AbsoluteUri);
            Assert.Equal("utf-8", httpClientFactory.HttpMessageHandler.Requests[0].Item1.Content.Headers.ContentType.CharSet);
            Assert.Equal("application/json", httpClientFactory.HttpMessageHandler.Requests[0].Item1.Content.Headers.ContentType.MediaType);
            Assert.Equal("{\"Message\":\"Message1\",\"Category\":\"information\",\"FileName\":\"FilePath1\",\"Line\":42,\"ProjectName\":\"Project1\",\"Column\":3,\"Details\":\"Message1 in FilePath1 on line 42\"}", httpClientFactory.HttpMessageHandler.Requests[0].Item2);

            Assert.Equal(HttpMethod.Post, httpClientFactory.HttpMessageHandler.Requests[1].Item1.Method);
            Assert.Equal("http://localhost:8080/api/build/compilationmessages", httpClientFactory.HttpMessageHandler.Requests[1].Item1.RequestUri.AbsoluteUri);
            Assert.Equal("utf-8", httpClientFactory.HttpMessageHandler.Requests[1].Item1.Content.Headers.ContentType.CharSet);
            Assert.Equal("application/json", httpClientFactory.HttpMessageHandler.Requests[1].Item1.Content.Headers.ContentType.MediaType);
            Assert.Equal("{\"Message\":\"Message2\",\"Category\":\"warning\",\"FileName\":\"FilePath2\",\"Line\":465,\"ProjectName\":\"Project1\",\"Column\":37,\"Details\":\"Message2 in FilePath2 on line 465\"}", httpClientFactory.HttpMessageHandler.Requests[1].Item2);

            Assert.Equal(HttpMethod.Post, httpClientFactory.HttpMessageHandler.Requests[2].Item1.Method);
            Assert.Equal("http://localhost:8080/api/build/compilationmessages", httpClientFactory.HttpMessageHandler.Requests[2].Item1.RequestUri.AbsoluteUri);
            Assert.Equal("utf-8", httpClientFactory.HttpMessageHandler.Requests[2].Item1.Content.Headers.ContentType.CharSet);
            Assert.Equal("application/json", httpClientFactory.HttpMessageHandler.Requests[2].Item1.Content.Headers.ContentType.MediaType);
            Assert.Equal("{\"Message\":\"Message3\",\"Category\":\"error\",\"FileName\":\"FilePath3\",\"Line\":82,\"ProjectName\":\"Project2\",\"Details\":\"Message3 in FilePath3 on line 82\"}", httpClientFactory.HttpMessageHandler.Requests[2].Item2);

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
            Assert.Equal(3, httpClientFactory.HttpMessageHandler.Requests.Count);

            Assert.Equal(HttpMethod.Post, httpClientFactory.HttpMessageHandler.Requests[0].Item1.Method);
            Assert.Equal("http://localhost:8080/api/build/compilationmessages", httpClientFactory.HttpMessageHandler.Requests[0].Item1.RequestUri.AbsoluteUri);
            Assert.Equal("utf-8", httpClientFactory.HttpMessageHandler.Requests[0].Item1.Content.Headers.ContentType.CharSet);
            Assert.Equal("application/json", httpClientFactory.HttpMessageHandler.Requests[0].Item1.Content.Headers.ContentType.MediaType);
            Assert.Equal("{\"Message\":\"[Source1] Message1\",\"Category\":\"information\",\"FileName\":\"FilePath1\",\"Line\":42,\"ProjectName\":\"Project1\",\"Column\":3,\"Details\":\"[Source1] Message1 in FilePath1 on line 42\"}", httpClientFactory.HttpMessageHandler.Requests[0].Item2);

            Assert.Equal(HttpMethod.Post, httpClientFactory.HttpMessageHandler.Requests[1].Item1.Method);
            Assert.Equal("http://localhost:8080/api/build/compilationmessages", httpClientFactory.HttpMessageHandler.Requests[1].Item1.RequestUri.AbsoluteUri);
            Assert.Equal("utf-8", httpClientFactory.HttpMessageHandler.Requests[1].Item1.Content.Headers.ContentType.CharSet);
            Assert.Equal("application/json", httpClientFactory.HttpMessageHandler.Requests[1].Item1.Content.Headers.ContentType.MediaType);
            Assert.Equal("{\"Message\":\"[Source2] Message2\",\"Category\":\"warning\",\"FileName\":\"FilePath2\",\"Line\":465,\"ProjectName\":\"Project1\",\"Column\":37,\"Details\":\"[Source2] Message2 in FilePath2 on line 465\"}", httpClientFactory.HttpMessageHandler.Requests[1].Item2);

            Assert.Equal(HttpMethod.Post, httpClientFactory.HttpMessageHandler.Requests[2].Item1.Method);
            Assert.Equal("http://localhost:8080/api/build/compilationmessages", httpClientFactory.HttpMessageHandler.Requests[2].Item1.RequestUri.AbsoluteUri);
            Assert.Equal("utf-8", httpClientFactory.HttpMessageHandler.Requests[2].Item1.Content.Headers.ContentType.CharSet);
            Assert.Equal("application/json", httpClientFactory.HttpMessageHandler.Requests[2].Item1.Content.Headers.ContentType.MediaType);
            Assert.Equal("{\"Message\":\"[Source3] Message3\",\"Category\":\"error\",\"FileName\":\"FilePath3\",\"Line\":82,\"ProjectName\":\"Project2\",\"Details\":\"[Source3] Message3 in FilePath3 on line 82\"}", httpClientFactory.HttpMessageHandler.Requests[2].Item2);

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
            Assert.Equal(3, httpClientFactory.HttpMessageHandler.Requests.Count);

            Assert.Equal(HttpMethod.Post, httpClientFactory.HttpMessageHandler.Requests[0].Item1.Method);
            Assert.Equal("http://localhost:8080/api/build/compilationmessages", httpClientFactory.HttpMessageHandler.Requests[0].Item1.RequestUri.AbsoluteUri);
            Assert.Equal("utf-8", httpClientFactory.HttpMessageHandler.Requests[0].Item1.Content.Headers.ContentType.CharSet);
            Assert.Equal("application/json", httpClientFactory.HttpMessageHandler.Requests[0].Item1.Content.Headers.ContentType.MediaType);
            Assert.Equal("{\"Message\":\"Message1\",\"Category\":\"information\",\"FileName\":\"FilePath1\",\"Line\":42,\"ProjectName\":\"Project1\",\"Column\":3,\"Details\":\"Message1 in FilePath1 on line 42\"}", httpClientFactory.HttpMessageHandler.Requests[0].Item2);

            Assert.Equal(HttpMethod.Post, httpClientFactory.HttpMessageHandler.Requests[1].Item1.Method);
            Assert.Equal("http://localhost:8080/api/build/compilationmessages", httpClientFactory.HttpMessageHandler.Requests[1].Item1.RequestUri.AbsoluteUri);
            Assert.Equal("utf-8", httpClientFactory.HttpMessageHandler.Requests[1].Item1.Content.Headers.ContentType.CharSet);
            Assert.Equal("application/json", httpClientFactory.HttpMessageHandler.Requests[1].Item1.Content.Headers.ContentType.MediaType);
            Assert.Equal("{\"Message\":\"Message2\",\"Category\":\"warning\",\"FileName\":\"FilePath2\",\"Line\":465,\"ProjectName\":\"Project1\",\"Column\":37,\"Details\":\"Message2 in FilePath2 on line 465\"}", httpClientFactory.HttpMessageHandler.Requests[1].Item2);

            Assert.Equal(HttpMethod.Post, httpClientFactory.HttpMessageHandler.Requests[2].Item1.Method);
            Assert.Equal("http://localhost:8080/api/build/compilationmessages", httpClientFactory.HttpMessageHandler.Requests[2].Item1.RequestUri.AbsoluteUri);
            Assert.Equal("utf-8", httpClientFactory.HttpMessageHandler.Requests[2].Item1.Content.Headers.ContentType.CharSet);
            Assert.Equal("application/json", httpClientFactory.HttpMessageHandler.Requests[2].Item1.Content.Headers.ContentType.MediaType);
            Assert.Equal("{\"Message\":\"Message3\",\"Category\":\"error\",\"FileName\":\"FilePath3\",\"Line\":82,\"ProjectName\":\"Project2\",\"Details\":\"Message3 in FilePath3 on line 82\"}", httpClientFactory.HttpMessageHandler.Requests[2].Item2);

            var logs = _loggerOutput.ToString();
            Assert.NotNull(logs);
            Assert.Contains("An error is occurred during the call to AppVeyor API: StatusCode: 403, ReasonPhrase: 'Forbidden', Version: 1.1, Content: <null>, Headers:", logs);
        }

        private MockHttpClientFactory GetHttpClientFactory(HttpStatusCode responseStatusCode = HttpStatusCode.OK)
        {
            var httpClientFactory = new MockHttpClientFactory(responseStatusCode);
            httpClientFactory.SetResponse(new HttpResponseMessage(responseStatusCode));

            return httpClientFactory;
        }

        private Logger GetLogger()
        {
            _loggerOutput = new StringBuilder();
            var writer = new StringWriter(_loggerOutput);
            var logger = new Logger();
            logger.SetWriter(writer);
            logger.AddCategory("error");
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
            };
        }
    }
}
