using NVika.Parsers;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using Xunit;

namespace NVika.Tests.Parsers
{
    public class SarifParserTest
    {
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
            });

            // act
            var result = parser.CanParse(reportPath);

            // assert
            Assert.Equal(expectedResult, result);
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
            Assert.Equal(18, results.Count);

            var issue = results[0];
            Assert.Equal("Design", issue.Category);
            Assert.Equal("Use nameof", issue.Description);
            Assert.Equal(@"D:\Prog\Github\vika\src\NVika\BuildServers\AppVeyor.cs", issue.FilePath);
            Assert.Equal("https://code-cracker.github.io/diagnostics/CC0021.html", issue.HelpUri.AbsoluteUri);
            Assert.Equal(20u, issue.Line);
            Assert.Equal("Use 'nameof(AppVeyor)' instead of specifying the program element name.", issue.Message);
            Assert.Equal("CC0021", issue.Name);
            Assert.Equal(26u, issue.Offset.Start);
            Assert.Equal(36u, issue.Offset.End);
            Assert.Null(issue.Project);
            Assert.Equal(IssueSeverity.Warning, issue.Severity);
            Assert.Equal("SARIF", issue.Source);

            issue = results[12];
            Assert.Equal("Style", issue.Category);
            Assert.Equal("Use string interpolation instead of String.Format", issue.Description);
            Assert.Equal(@"D:\Prog\Github\vika\src\NVika\Program.cs", issue.FilePath);
            Assert.Equal("https://code-cracker.github.io/diagnostics/CC0048.html", issue.HelpUri.AbsoluteUri);
            Assert.Equal(64u, issue.Line);
            Assert.Equal("Use string interpolation", issue.Message);
            Assert.Equal("CC0048", issue.Name);
            Assert.Equal(29u, issue.Offset.Start);
            Assert.Equal(93u, issue.Offset.End);
            Assert.Null(issue.Project);
            Assert.Equal(IssueSeverity.Suggestion, issue.Severity);
            Assert.Equal("SARIF", issue.Source);

            issue = results[13];
            Assert.Equal("Maintainability", issue.Category);
            Assert.Equal("Boolean checks should not be inverted", issue.Description);
            Assert.Equal(@"D:\Prog\Github\vika\src\NVika\Program.cs", issue.FilePath);
            Assert.Equal("http://vs.sonarlint.org/rules/index.html#version=1.16.0&ruleId=S1940", issue.HelpUri.AbsoluteUri);
            Assert.Equal(39u, issue.Line);
            Assert.Equal("Use the opposite operator (\"!=\") instead.", issue.Message);
            Assert.Equal("S1940", issue.Name);
            Assert.Equal(17u, issue.Offset.Start);
            Assert.Equal(26u, issue.Offset.End);
            Assert.Null(issue.Project);
            Assert.Equal(IssueSeverity.Error, issue.Severity);
            Assert.Equal("SARIF", issue.Source);
        }

        private const string EmptyReportSample = @"{
  ""$schema"": ""http://json.schemastore.org/sarif-1.0.0"",
  ""version"": ""1.0.0"",
  ""runs"": [
    {
      ""tool"": {
        ""name"": ""Microsoft (R) Visual C# Compiler"",
        ""version"": ""1.3.1.0"",
        ""fileVersion"": ""1.3.1.60616"",
        ""semanticVersion"": ""1.3.1"",
        ""language"": ""en-US""
      }}]}";
    }
}
