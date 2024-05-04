using NVika.Parsers;
using Serilog;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using Xunit;

namespace NVika.Tests.Parsers
{
    public class SarifParserTest
    {
        private StringBuilder _loggerOutput;

        [Fact]
        public void Name()
        {
            // arrange
            var parser = new SarifParser();
            parser.FileSystem = new MockFileSystem();

            // act
            var name = parser.Name;

            // assert
            Assert.Equal("Static Analysis Results Interchange Format (SARIF)", name);
        }

        [Theory]
        [InlineData("static-analysis.sarif.json", true)]
        [InlineData("emptyreport.xml", false)]
        [InlineData("onlyissues.xml", false)]
        [InlineData("emptyreport.json", true)]
        [InlineData("emptyreport.sarif", true)]
        [InlineData("falsereport.sarif", false)]
        public void CanParse(string reportPath, bool expectedResult)
        {
            // arrange
            var parser = new SarifParser();
            parser.FileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "static-analysis.sarif.json", new MockFileData(TestUtilities.GetEmbeddedResourceContent("static-analysis.sarif.json")) },
                { "emptyreport.xml", new MockFileData("<Report ToolsVersion=\"8.2\"></Report>") },
                { "onlyissues.xml", new MockFileData("<IssueTypes></IssueTypes>") },
                { "emptyreport.json", new MockFileData(EmptyReportSample) },
                { "emptyreport.sarif", new MockFileData(EmptyReportSample) },
                { "falsereport.sarif", new MockFileData("<" + EmptyReportSample) },
                { "InspectCodeReport.json", new MockFileData(TestUtilities.GetEmbeddedResourceContent("InspectCodeReport.json")) },
            });

            // act
            var result = parser.CanParse(reportPath);

            // assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void CanParseOldSarifFormat()
        {
            // arrange
            var parser = new SarifParser();
            parser.Logger = GetLogger();
            parser.FileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "static-analysis.old.sarif.json", new MockFileData(TestUtilities.GetEmbeddedResourceContent("static-analysis.old.sarif.json")) }
            });

            // act
            var result = parser.CanParse("static-analysis.old.sarif.json");

            // assert
            Assert.False(result);
            var logs = _loggerOutput.ToString();
            Assert.Contains("SARIF 1.0 is not supported, please update your analysis tool to produce SARIF 2.1 format.", logs);
        }

        [Fact]
        public void Parse()
        {
            // arrange
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "static-analysis.sarif.json", new MockFileData(TestUtilities.GetEmbeddedResourceContent("static-analysis.sarif.json")) }
            });
            var parser = new SarifParser();
            parser.FileSystem = fileSystem;

            // act
            var results = parser.Parse("static-analysis.sarif.json").ToList();

            // assert
            Assert.Equal(41, results.Count);

            var issue = results[0];
            Assert.Equal("Major Code Smell", issue.Category);
            Assert.Equal("Utility classes should not have public constructors", issue.Description);
            Assert.Equal(@"C:\Users\jerem\source\repos\Vika\NVika\Program.cs", issue.FilePath);
            Assert.Equal("https://rules.sonarsource.com/csharp/RSPEC-1118", issue.HelpUri.AbsoluteUri);
            Assert.Equal(5u, issue.Line);
            Assert.Equal("Add a 'protected' constructor or the 'static' keyword to the class declaration.", issue.Message);
            Assert.Equal("S1118", issue.Name);
            Assert.Equal(11u, issue.Offset.Start);
            Assert.Equal(18u, issue.Offset.End);
            Assert.Null(issue.Project);
            Assert.Equal(IssueSeverity.Warning, issue.Severity);
            Assert.Equal("SARIF", issue.Source);

            issue = results[12];
            Assert.Equal("Style", issue.Category);
            Assert.Equal("Remove commented code.", issue.Description);
            Assert.Equal(@"C:\Users\jerem\source\repos\Vika\NVika\Parsers\SarifParser.cs", issue.FilePath);
            Assert.Equal("https://code-cracker.github.io/diagnostics/CC0037.html", issue.HelpUri.AbsoluteUri);
            Assert.Equal(12u, issue.Line);
            Assert.Equal("Commented code should be removed.", issue.Message);
            Assert.Equal("CC0037", issue.Name);
            Assert.Equal(1u, issue.Offset.Start);
            Assert.Equal(59u, issue.Offset.End);
            Assert.Null(issue.Project);
            Assert.Equal(IssueSeverity.Suggestion, issue.Severity);
            Assert.Equal("SARIF", issue.Source);

            issue = results[3];
            Assert.Equal("Minor Code Smell", issue.Category);
            Assert.Equal("Unused \"using\" should be removed", issue.Description);
            Assert.Equal(@"C:\Users\jerem\source\repos\Vika\NVika\Parsers\IReportParser.cs", issue.FilePath);
            Assert.Equal("https://rules.sonarsource.com/csharp/RSPEC-1128", issue.HelpUri.AbsoluteUri);
            Assert.Equal(2u, issue.Line);
            Assert.Equal("Remove this unnecessary 'using'.", issue.Message);
            Assert.Equal("S1128", issue.Name);
            Assert.Equal(1u, issue.Offset.Start);
            Assert.Equal(26u, issue.Offset.End);
            Assert.Null(issue.Project);
            Assert.Equal(IssueSeverity.Warning, issue.Severity);
            Assert.Equal("SARIF", issue.Source);

            issue = results[33];
            Assert.Equal("Design", issue.Category);
            Assert.Equal("Use nameof", issue.Description);
            Assert.Equal(@"C:\Users\jerem\source\repos\Vika\NVika\Parsers\FxCopParser.cs", issue.FilePath);
            Assert.Equal("https://code-cracker.github.io/diagnostics/CC0021.html", issue.HelpUri.AbsoluteUri);
            Assert.Equal(34u, issue.Line);
            Assert.Equal("Use 'nameof(Issue)' instead of specifying the program element name.", issue.Message);
            Assert.Equal("CC0021", issue.Name);
            Assert.Equal(56u, issue.Offset.Start);
            Assert.Equal(63u, issue.Offset.End);
            Assert.Null(issue.Project);
            Assert.Equal(IssueSeverity.Warning, issue.Severity);
            Assert.Equal("SARIF", issue.Source);
        }

        [Fact]
        public void Parse_RelativeUris()
        {
            // arrange
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "InspectCodeReport.json", new MockFileData(TestUtilities.GetEmbeddedResourceContent("InspectCodeReport.json")) }
            });
            var parser = new SarifParser();
            parser.FileSystem = fileSystem;

            // act
            var results = parser.Parse("InspectCodeReport.json").ToList();

            // assert
            Assert.Equal(121, results.Count);

            var issue = results[0];
            System.Console.WriteLine();
            System.Console.WriteLine("Issue 0");
            System.Console.WriteLine(issue);
            // Assert.Equal("Major Code Smell", issue.Category);
            // Assert.Equal("Utility classes should not have public constructors", issue.Description);
            // Assert.Equal(@"C:\Users\jerem\source\repos\Vika\NVika\Program.cs", issue.FilePath);
            // Assert.Equal("https://rules.sonarsource.com/csharp/RSPEC-1118", issue.HelpUri.AbsoluteUri);
            // Assert.Equal(5u, issue.Line);
            // Assert.Equal("Add a 'protected' constructor or the 'static' keyword to the class declaration.", issue.Message);
            // Assert.Equal("S1118", issue.Name);
            // Assert.Equal(11u, issue.Offset.Start);
            // Assert.Equal(18u, issue.Offset.End);
            // Assert.Null(issue.Project);
            // Assert.Equal(IssueSeverity.Warning, issue.Severity);
            // Assert.Equal("SARIF", issue.Source);

            issue = results[12];
            System.Console.WriteLine();
            System.Console.WriteLine("Issue 12");
            System.Console.WriteLine(issue);
            // Assert.Equal("Style", issue.Category);
            // Assert.Equal("Remove commented code.", issue.Description);
            // Assert.Equal(@"C:\Users\jerem\source\repos\Vika\NVika\Parsers\SarifParser.cs", issue.FilePath);
            // Assert.Equal("https://code-cracker.github.io/diagnostics/CC0037.html", issue.HelpUri.AbsoluteUri);
            // Assert.Equal(12u, issue.Line);
            // Assert.Equal("Commented code should be removed.", issue.Message);
            // Assert.Equal("CC0037", issue.Name);
            // Assert.Equal(1u, issue.Offset.Start);
            // Assert.Equal(59u, issue.Offset.End);
            // Assert.Null(issue.Project);
            // Assert.Equal(IssueSeverity.Suggestion, issue.Severity);
            // Assert.Equal("SARIF", issue.Source);

            issue = results[46];
            System.Console.WriteLine();
            System.Console.WriteLine("Issue 46");
            System.Console.WriteLine(issue);
            // Assert.Equal("Minor Code Smell", issue.Category);
            // Assert.Equal("Unused \"using\" should be removed", issue.Description);
            // Assert.Equal(@"C:\Users\jerem\source\repos\Vika\NVika\Parsers\IReportParser.cs", issue.FilePath);
            // Assert.Equal("https://rules.sonarsource.com/csharp/RSPEC-1128", issue.HelpUri.AbsoluteUri);
            // Assert.Equal(2u, issue.Line);
            // Assert.Equal("Remove this unnecessary 'using'.", issue.Message);
            // Assert.Equal("S1128", issue.Name);
            // Assert.Equal(1u, issue.Offset.Start);
            // Assert.Equal(26u, issue.Offset.End);
            // Assert.Null(issue.Project);
            // Assert.Equal(IssueSeverity.Warning, issue.Severity);
            // Assert.Equal("SARIF", issue.Source);

            issue = results[101];
            System.Console.WriteLine();
            System.Console.WriteLine("Issue 101");
            System.Console.WriteLine(issue);
            // Assert.Equal("Design", issue.Category);
            // Assert.Equal("Use nameof", issue.Description);
            // Assert.Equal(@"C:\Users\jerem\source\repos\Vika\NVika\Parsers\FxCopParser.cs", issue.FilePath);
            // Assert.Equal("https://code-cracker.github.io/diagnostics/CC0021.html", issue.HelpUri.AbsoluteUri);
            // Assert.Equal(34u, issue.Line);
            // Assert.Equal("Use 'nameof(Issue)' instead of specifying the program element name.", issue.Message);
            // Assert.Equal("CC0021", issue.Name);
            // Assert.Equal(56u, issue.Offset.Start);
            // Assert.Equal(63u, issue.Offset.End);
            // Assert.Null(issue.Project);
            // Assert.Equal(IssueSeverity.Warning, issue.Severity);
            // Assert.Equal("SARIF", issue.Source);
            Assert.Fail();
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

        private const string EmptyReportSample = @"{
    ""$schema"": ""http://json.schemastore.org/sarif-2.1.0"",
    ""version"": ""2.1.0"",
    ""runs"": [{
            ""tool"": {
                ""driver"": {
                    ""name"": ""Microsoft (R) Visual C# Compiler"",
                    ""version"": ""3.8.0-4.20503.2 (75d31ee9)"",
                    ""dottedQuadFileVersion"": ""3.8.0.0"",
                    ""semanticVersion"": ""3.8.0"",
                    ""language"": ""en-US""
    }}}]}";
    }
}
