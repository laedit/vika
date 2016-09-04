using NSubstitute;
using NVika.BuildServers;
using NVika.Parsers;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Xunit;

namespace NVika.Tests
{
    public class ParseReportCommandTest
    {
        private StringBuilder _loggerOutput;
        private bool _mockBuildServer_IncludeSource;
        private bool _mockParser_Alternate;

        [Fact]
        public void Execute_NoArguments_ShouldLogError_NoReportSpecified()
        {
            // arrange
            var logger = GetLogger();
            var buildServerCommand = new ParseReportCommand(logger, new MockFileSystem(), Enumerable.Empty<IBuildServer>(), new LocalBuildServer(logger), Enumerable.Empty<IReportParser>());
            buildServerCommand.GetActualOptions().Parse(new string[] { });

            // act
            var exitCode = buildServerCommand.Run(new string[] { });

            // assert
            Assert.Equal(1, exitCode);
            Assert.Contains("No report was specified. You must indicate at least one report file.", _loggerOutput.ToString().Trim());
        }

        [Fact]
        public void Execute_OnlyDebug_ShouldLogError_NoResportSpecified()
        {
            // arrange
            var logger = GetLogger();
            var buildServerCommand = new ParseReportCommand(logger, new MockFileSystem(), Enumerable.Empty<IBuildServer>(), new LocalBuildServer(logger), Enumerable.Empty<IReportParser>());
            buildServerCommand.GetActualOptions().Parse(new[] { "--debug" });

            // act
            var exitCode = buildServerCommand.Run(new string[] { });

            // assert
            Assert.Equal(1, exitCode);
            var logs = _loggerOutput.ToString();
            Assert.Contains("No report was specified. You must indicate at least one report file.", logs);
        }

        [Fact]
        public void Execute_NonExistingReport_ShouldLogError_ReportNotFound()
        {
            // arrange
            var logger = GetLogger();
            var fileSystem = new MockFileSystem();
            var buildServerCommand = new ParseReportCommand(logger, fileSystem, Enumerable.Empty<IBuildServer>(), new LocalBuildServer(logger), Enumerable.Empty<IReportParser>());
            var remainingArgs = buildServerCommand.GetActualOptions().Parse(new[] { "report.xml" });

            // act
            var exitCode = buildServerCommand.Run(remainingArgs.ToArray());

            // assert
            Assert.Equal(2, exitCode);
            var logs = _loggerOutput.ToString();
            Assert.Contains("The report \"report.xml\" was not found.", logs);
        }

        [Fact]
        public void Execute_ExistingButEmptyReport_ShouldLogErrorOnLoad_DefaultBuildServerIsLocal()
        {
            // arrange
            var logger = GetLogger();
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> { { "report.xml", MockFileData.NullObject } });
            var localBuildServer = new LocalBuildServer(logger);
            var mockBuildServer = GetMockBuildServer();
            var buildServers = new List<IBuildServer> { localBuildServer, mockBuildServer };
            var buildServerCommand = new ParseReportCommand(logger, fileSystem, buildServers, localBuildServer, Enumerable.Empty<IReportParser>());
            var remainingArgs = buildServerCommand.GetActualOptions().Parse(new[] { "report.xml" });

            // act
            var exitCode = buildServerCommand.Run(remainingArgs.ToArray());

            // assert
            Assert.Equal(4, exitCode);
            var logs = _loggerOutput.ToString();
            Assert.Contains("\t- \"Local console\"", logs);
            Assert.Contains("The adequate parser for this report was not found. You are welcome to address us an issue.", logs);
        }

        [Fact]
        public void Execute_ExistingBuEmptyReport_ShouldLogErrorOnLoad_MockBuildServerIsSelected()
        {
            // arrange
            var logger = GetLogger();
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> { { "report.xml", MockFileData.NullObject } });
            var localBuildServer = new LocalBuildServer(logger);
            var mockBuildServer = GetMockBuildServer(true);
            var buildServers = new List<IBuildServer> { localBuildServer, mockBuildServer };
            var buildServerCommand = new ParseReportCommand(logger, fileSystem, buildServers, localBuildServer, Enumerable.Empty<IReportParser>());
            var remainingArgs = buildServerCommand.GetActualOptions().Parse(new[] { "report.xml" });

            // act
            var exitCode = buildServerCommand.Run(remainingArgs.ToArray());

            // assert
            Assert.Equal(4, exitCode);
            var logs = _loggerOutput.ToString();
            Assert.Contains("\t- \"MockBuildServer\"", logs);
            Assert.Contains("The adequate parser for this report was not found. You are welcome to address us an issue.", logs);
        }

        [Fact]
        public void Execute_NoParser_ShouldLogError()
        {
            // arrange
            var logger = GetLogger();
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> { { "report.xml", new MockFileData("<root></root>") } });
            var localBuildServer = new LocalBuildServer(logger);
            var mockBuildServer = GetMockBuildServer(true);
            var buildServers = new List<IBuildServer> { localBuildServer, mockBuildServer };
            var buildServerCommand = new ParseReportCommand(logger, fileSystem, buildServers, localBuildServer, Enumerable.Empty<IReportParser>());
            var remainingArgs = buildServerCommand.GetActualOptions().Parse(new[] { "report.xml" });

            // act
            var exitCode = buildServerCommand.Run(remainingArgs.ToArray());

            // assert
            Assert.Equal(4, exitCode);
            var logs = _loggerOutput.ToString();
            Assert.Contains("The adequate parser for this report was not found. You are welcome to address us an issue.", logs);
        }

        [Fact]
        public void Execute_ParserCantParse_ShouldLogError()
        {
            // arrange
            var logger = GetLogger();
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> { { "report.xml", new MockFileData("<root></root>") } });
            var localBuildServer = new LocalBuildServer(logger);
            var mockBuildServer = GetMockBuildServer(true);
            var buildServers = new List<IBuildServer> { localBuildServer, mockBuildServer };
            var parsers = new List<IReportParser> { GetMockReportParser() };
            var buildServerCommand = new ParseReportCommand(logger, fileSystem, buildServers, localBuildServer, parsers);
            var remainingArgs = buildServerCommand.GetActualOptions().Parse(new[] { "report.xml" });

            // act
            var exitCode = buildServerCommand.Run(remainingArgs.ToArray());

            // assert
            Assert.Equal(4, exitCode);
            var logs = _loggerOutput.ToString();
            Assert.Contains("The adequate parser for this report was not found. You are welcome to address us an issue.", logs);
        }

        [Fact]
        public void Execute_ParserCanParse_ShouldWriteMessageFromIssues()
        {
            // arrange
            var logger = GetLogger();
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> { { "report.xml", new MockFileData("<root></root>") } });
            var localBuildServer = new LocalBuildServer(logger);
            var mockBuildServer = GetMockBuildServer(true);
            var buildServers = new List<IBuildServer> { localBuildServer, mockBuildServer };
            var parsers = new List<IReportParser> { GetMockReportParser(true) };
            var buildServerCommand = new ParseReportCommand(logger, fileSystem, buildServers, localBuildServer, parsers);
            var remainingArgs = buildServerCommand.GetActualOptions().Parse(new[] { "report.xml" });

            // act
            var exitCode = buildServerCommand.Run(remainingArgs.ToArray());

            // assert
            Assert.Equal(5, exitCode);
            var logs = _loggerOutput.ToString();
            Assert.Contains("Message1", logs);
            Assert.Contains("Message2", logs);
            Assert.Contains("Message3", logs);
        }

        [Fact]
        public void Execute_ParserCanParse_WithDebug_ShouldWriteMessageFromIssuesAndWriteIssuesCount()
        {
            // arrange
            var logger = GetLogger(Serilog.Events.LogEventLevel.Debug);
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> { { "report.xml", new MockFileData("<root></root>") } });
            var localBuildServer = new LocalBuildServer(logger);
            var mockBuildServer = GetMockBuildServer(true);
            var buildServers = new List<IBuildServer> { localBuildServer, mockBuildServer };
            var parsers = new List<IReportParser> { GetMockReportParser(true) };
            var buildServerCommand = new ParseReportCommand(logger, fileSystem, buildServers, localBuildServer, parsers);
            var remainingArgs = buildServerCommand.GetActualOptions().Parse(new[] { "report.xml"});

            // act
            var exitCode = buildServerCommand.Run(remainingArgs.ToArray());

            // assert
            Assert.Equal(5, exitCode);
            var logs = _loggerOutput.ToString();
            Assert.Contains("Report path is \"report.xml\"", logs);
            Assert.Contains("3 issues was found", logs);
            Assert.Contains("Message1", logs);
            Assert.Contains("Message2", logs);
            Assert.Contains("Message3", logs);
        }

        [Fact]
        public void Execute_NoErrorInIssues_ShouldReturnZero()
        {
            // arrange
            var logger = GetLogger();
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> { { "report.xml", new MockFileData("<root></root>") } });
            var localBuildServer = new LocalBuildServer(logger);
            var mockBuildServer = GetMockBuildServer(true);
            var buildServers = new List<IBuildServer> { localBuildServer, mockBuildServer };
            var parsers = new List<IReportParser> { GetMockReportParser(true, false) };
            var buildServerCommand = new ParseReportCommand(logger, fileSystem, buildServers, localBuildServer, parsers);
            var remainingArgs = buildServerCommand.GetActualOptions().Parse(new[] { "report.xml" });

            // act
            var exitCode = buildServerCommand.Run(remainingArgs.ToArray());

            // assert
            Assert.Equal(0, exitCode);
            var logs = _loggerOutput.ToString();
            Assert.Contains("Message1", logs);
            Assert.Contains("Message2", logs);
            Assert.Contains("Message3", logs);
        }

        [Fact]
        public void Execute_IncludeSource_BuildServerShouldIncludeSource()
        {
            // arrange
            var logger = GetLogger();
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> { { "report.xml", new MockFileData("<root></root>") } });
            var localBuildServer = new LocalBuildServer(logger);
            var mockBuildServer = GetMockBuildServer(true);
            var buildServers = new List<IBuildServer> { localBuildServer, mockBuildServer };
            var parsers = new List<IReportParser> { GetMockReportParser(true, false) };
            var buildServerCommand = new ParseReportCommand(logger, fileSystem, buildServers, localBuildServer, parsers);
            var remainingArgs = buildServerCommand.GetActualOptions().Parse(new[] { "report.xml", "--includesource" });

            // act
            var exitCode = buildServerCommand.Run(remainingArgs.ToArray());

            // assert
            Assert.Equal(0, exitCode);
            var logs = _loggerOutput.ToString();
            Assert.Contains("Message1 - Source1", logs);
            Assert.Contains("Message2 - Source2", logs);
            Assert.Contains("Message3 - Source3", logs);
        }

        [Fact]
        public void Execute_MultipleReports_ShouldWriteMessageFromIssues()
        {
            // arrange
            var logger = GetLogger(Serilog.Events.LogEventLevel.Debug);
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "report.xml", new MockFileData("<root></root>") },
                { "report2.xml", new MockFileData("<root></root>") }
            });
            var localBuildServer = new LocalBuildServer(logger);
            var mockBuildServer = GetMockBuildServer(true);
            var buildServers = new List<IBuildServer> { localBuildServer, mockBuildServer };
            var parsers = new List<IReportParser> { GetMockReportParser(true, true, true) };
            var buildServerCommand = new ParseReportCommand(logger, fileSystem, buildServers, localBuildServer, parsers);
            var remainingArgs = buildServerCommand.GetActualOptions().Parse(new[] { "report.xml", "report2.xml"});

            // act
            var exitCode = buildServerCommand.Run(remainingArgs.ToArray());

            // assert
            Assert.Equal(5, exitCode);
            var logs = _loggerOutput.ToString();
            Assert.Contains("3 issues was found", logs);
            Assert.Contains("Message1", logs);
            Assert.Contains("Message2", logs);
            Assert.Contains("Message3", logs);
            Assert.Contains("Message4", logs);
            Assert.Contains("Message5", logs);
            Assert.Contains("Message6", logs);
        }

        [Fact]
        public void Execute_WarningAsError_ShouldExitWithCode5()
        {
            // arrange
            var logger = GetLogger();
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> { { "report.xml", new MockFileData("<root></root>") } });
            var localBuildServer = new LocalBuildServer(logger);
            var mockBuildServer = GetMockBuildServer(true);
            var buildServers = new List<IBuildServer> { localBuildServer, mockBuildServer };
            var parsers = new List<IReportParser> { GetMockReportParser(true, false) };
            var buildServerCommand = new ParseReportCommand(logger, fileSystem, buildServers, localBuildServer, parsers);
            var remainingArgs = buildServerCommand.GetActualOptions().Parse(new[] { "report.xml", "--warningaserror" });

            // act
            var exitCode = buildServerCommand.Run(remainingArgs.ToArray());

            // assert
            Assert.Equal(5, exitCode);
            var logs = _loggerOutput.ToString();
            Assert.Contains("[Suggestion] Message1", logs);
            Assert.Contains("[Error] Message2", logs);
            Assert.Contains("[Error] Message3", logs);
            Assert.Contains("[Fatal] Issues with severity error was found: the build will fail", logs);
        }

        private IReportParser GetMockReportParser(bool canParse = false, bool issuesContainError = true, bool alternate = false)
        {
            var mockReportParser = Substitute.For<IReportParser>();
            mockReportParser.Name.Returns("MockReportParser");
            mockReportParser.CanParse(Arg.Any<string>()).Returns(canParse);

            mockReportParser.Parse(Arg.Any<string>()).Returns((ci) =>
            {
                if (alternate)
                {
                    if (issuesContainError && _mockParser_Alternate)
                    {
                        issuesContainError = false;
                    }
                    _mockParser_Alternate = !_mockParser_Alternate;
                    if (_mockParser_Alternate)
                    {
                        return GetIssues(issuesContainError);
                    }
                    else
                    {
                        return GetIssues2(issuesContainError);
                    }
                }
                else
                {
                    return GetIssues(issuesContainError);
                }
            });

            return mockReportParser;
        }

        private IBuildServer GetMockBuildServer(bool canApplyToCurrentContext = false)
        {
            var mockBuildServer = Substitute.For<IBuildServer>();
            mockBuildServer.Name.Returns("MockBuildServer");
            mockBuildServer.CanApplyToCurrentContext().Returns(canApplyToCurrentContext);
            mockBuildServer.When(bs => bs.ApplyParameters(Arg.Any<bool>())).Do(ci => _mockBuildServer_IncludeSource = ci.Arg<bool>());
            mockBuildServer.When(bs => bs.WriteMessage(Arg.Any<Issue>())).Do(ci => _loggerOutput.AppendLine("[" + ci.Arg<Issue>().Severity + "] " + ci.Arg<Issue>().Message + (_mockBuildServer_IncludeSource ? " - " + ci.Arg<Issue>().Source : string.Empty)));
            return mockBuildServer;
        }

        private ILogger GetLogger(Serilog.Events.LogEventLevel logEventLevel = Serilog.Events.LogEventLevel.Information)
        {
            _loggerOutput = new StringBuilder();
            var writer = new StringWriter(_loggerOutput);
            var loggerConfiguration = new LoggerConfiguration()
                        .WriteTo.TextWriter(writer);
            loggerConfiguration.MinimumLevel.Is(logEventLevel);
            var logger = loggerConfiguration.CreateLogger();
            return logger;
        }

        private List<Issue> GetIssues(bool containError = true)
        {
            return new List<Issue>
            {
                new Issue{ Category="Category1", Description = "Description1", FilePath = "FilePath1", HelpUri = null, Line = 42u, Message = "Message1", Name = "Name1", Offset = new Offset{ Start = 2u, End = 5u}, Project = "Project1", Severity = IssueSeverity.Suggestion, Source = "Source1" },
                new Issue{ Category="Category2", Description = "Description2", FilePath = "FilePath2", HelpUri = new Uri("https://www.wikipedia.com"), Line = 465u, Message = "Message2", Name = "Name2", Offset = new Offset{ Start = 36u, End = 546u}, Project = "Project1", Severity = IssueSeverity.Warning, Source = "Source2" },
                new Issue{ Category="Category1", Description = "Description3", FilePath = "FilePath3", HelpUri = new Uri("http://helperror.com"), Line = 82u, Message = "Message3", Name = "Name3", Project = "Project2", Severity = containError ? IssueSeverity.Error : IssueSeverity.Warning, Source = "Source3" },
            };
        }

        private List<Issue> GetIssues2(bool containError = true)
        {
            return new List<Issue>
            {
                new Issue{ Category="Category1", Description = "Description4", FilePath = "FilePath4", HelpUri = null, Line = 42u, Message = "Message4", Name = "Name4", Offset = new Offset{ Start = 2u, End = 5u}, Project = "Project1", Severity = IssueSeverity.Suggestion, Source = "Source1" },
                new Issue{ Category="Category2", Description = "Description5", FilePath = "FilePath5", HelpUri = new Uri("https://www.wikipedia.com"), Line = 465u, Message = "Message5", Name = "Name5", Offset = new Offset{ Start = 36u, End = 546u}, Project = "Project1", Severity = IssueSeverity.Warning, Source = "Source2" },
                new Issue{ Category="Category1", Description = "Description6", FilePath = "FilePath6", HelpUri = new Uri("http://helperror.com"), Line = 82u, Message = "Message6", Name = "Name6", Project = "Project2", Severity = containError ? IssueSeverity.Error : IssueSeverity.Warning, Source = "Source3" },
            };
        }
    }
}
