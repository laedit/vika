using NVika.Parsers;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace NVika.Tests.Parsers
{
    public class InspectCodeParserTest
    {
        [Fact]
        public void Name()
        {
            // arrange
            var parser = new InspectCodeParser();
            parser.FileSystem = new MockFileSystem();

            // act
            var name = parser.Name;

            // assert
            Assert.Equal("InspectCode", name);
        }

        [Theory]
        [InlineData("inspectcodereport.xml", true)]
        [InlineData("inspectcodereport_2016.2.xml", true)]
        [InlineData("emptyreport.xml", false)]
        [InlineData("onlyissues.xml", false)]
        [InlineData("onlyissues.json", false)]
        [InlineData("falsereport.xml", false)]
        public void CanParse(string reportPath, bool expectedResult)
        {
            // arrange
            var parser = new InspectCodeParser();
            parser.FileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "inspectcodereport.xml", new MockFileData(TestUtilities.GetEmbeddedResourceContent("inspectcodereport.xml")) },
                { "inspectcodereport_2016.2.xml", new MockFileData(TestUtilities.GetEmbeddedResourceContent("inspectcodereport_2016.2.xml")) },
                { "emptyreport.xml", new MockFileData("<Report ToolsVersion=\"8.2\"></Report>") },
                { "onlyissues.xml", new MockFileData("<IssueTypes></IssueTypes>") },
                { "falsereport.xml", new MockFileData("{<IssueTypes></IssueTypes>") },
            });

            // act
            var result = parser.CanParse(reportPath);

            // assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void CanParse_LoadingException()
        {
            // arrange
            var parser = new InspectCodeParser();
            parser.FileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "wrongreport.xml", new MockFileData("<Report ToolsVersion=\"8.2\"></Rport>") },
            });

            // act
            var exception = Assert.Throws<Exceptions.LoadingReportException>(() => parser.CanParse("wrongreport.xml"));

            // assert
            Assert.Equal(3, exception.ExitCode);
            Assert.Equal("An exception happened when loading the report 'wrongreport.xml'", exception.Message);
            Assert.NotNull(exception.InnerException);
        }

        [Fact]
        public void Parse()
        {
            // arrange
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "inspectcodereport.xml", new MockFileData(TestUtilities.GetEmbeddedResourceContent("inspectcodereport.xml")) },
                { @"NVika\BuildServers\AppVeyor.cs", new MockFileData(TestUtilities.GetEmbeddedResourceContent("AppVeyor.txt")) },
                { @"NVika\BuildServers\LocalBuildServer.cs", new MockFileData(TestUtilities.GetEmbeddedResourceContent("LocalBuildServer.txt")) },
                { @"NVika\ParseReportCommand.cs", new MockFileData(TestUtilities.GetEmbeddedResourceContent("ParseReportCommand.txt")) },
                { @"NVika\Parsers\InspectCodeParser.cs", new MockFileData(TestUtilities.GetEmbeddedResourceContent("InspectCodeParser.txt")) },
                { @"NVika\Program.cs", new MockFileData(TestUtilities.GetEmbeddedResourceContent("Program.txt")) },
            });
            var parser = new InspectCodeParser();
            parser.FileSystem = fileSystem;

            // act
            var result = parser.Parse("inspectcodereport.xml");

            // assert
            Assert.Equal(41, result.Count());

            var issue = result.First();
            Assert.Equal("Constraints Violations", issue.Category);
            Assert.Equal("Inconsistent Naming", issue.Description);
            Assert.Equal(@"NVika\BuildServers\AppVeyor.cs", issue.FilePath);
            Assert.Null(issue.HelpUri);
            Assert.Equal(15u, issue.Line);
            Assert.Equal("Name '_appVeyorAPIUrl' does not match rule 'Instance fields (private)'. Suggested name is '_appVeyorApiUrl'.", issue.Message);
            Assert.Equal("InconsistentNaming", issue.Name);
            Assert.Equal(32u, issue.Offset.Start);
            Assert.Equal(47u, issue.Offset.End);
            Assert.Equal("NVika", issue.Project);
            Assert.Equal(IssueSeverity.Warning, issue.Severity);
            Assert.Equal("InspectCode", issue.Source);

            issue = result.Skip(7).First();
            Assert.Equal("Common Practices and Code Improvements", issue.Category);
            Assert.Equal("Convert local variable or field to constant: Private accessibility", issue.Description);
            Assert.Equal(@"NVika\BuildServers\LocalBuildServer.cs", issue.FilePath);
            Assert.Null(issue.HelpUri);
            Assert.Equal(9u, issue.Line);
            Assert.Equal("Convert to constant", issue.Message);
            Assert.Equal("ConvertToConstant.Local", issue.Name);
            Assert.Equal(30u, issue.Offset.Start);
            Assert.Equal(42u, issue.Offset.End);
            Assert.Equal("NVika", issue.Project);
            Assert.Equal(IssueSeverity.Suggestion, issue.Severity);
            Assert.Equal("InspectCode", issue.Source);

            issue = result.Skip(21).First();
            Assert.Equal("Redundancies in Code", issue.Category);
            Assert.Equal("Redundant 'this.' qualifier", issue.Description);
            Assert.Equal(@"NVika\ParseReportCommand.cs", issue.FilePath);
            Assert.Null(issue.HelpUri);
            Assert.Equal(33u, issue.Line);
            Assert.Equal("Qualifier 'this.' is redundant", issue.Message);
            Assert.Equal("RedundantThisQualifier", issue.Name);
            Assert.Equal(12u, issue.Offset.Start);
            Assert.Equal(17u, issue.Offset.End);
            Assert.Equal("NVika", issue.Project);
            Assert.Equal(IssueSeverity.Warning, issue.Severity);
            Assert.Equal("InspectCode", issue.Source);

            issue = result.Skip(33).First();
            Assert.Equal("Language Usage Opportunities", issue.Category);
            Assert.Equal("Loop can be converted into LINQ-expression", issue.Description);
            Assert.Equal(@"NVika\Parsers\InspectCodeParser.cs", issue.FilePath);
            Assert.Equal("http://confluence.jetbrains.net/display/ReSharper/Loop+can+be+converted+into+a+LINQ+expression", issue.HelpUri.AbsoluteUri);
            Assert.Equal(27u, issue.Line);
            Assert.Equal("Loop can be converted into LINQ-expression", issue.Message);
            Assert.Equal("LoopCanBeConvertedToQuery", issue.Name);
            Assert.Equal(12u, issue.Offset.Start);
            Assert.Equal(19u, issue.Offset.End);
            Assert.Equal("NVika", issue.Project);
            Assert.Equal(IssueSeverity.Suggestion, issue.Severity);
            Assert.Equal("InspectCode", issue.Source);

            issue = result.Skip(38).First();
            Assert.Equal("Potential Code Quality Issues", issue.Category);
            Assert.Equal("Auto-implemented property accessor is never used: Private accessibility", issue.Description);
            Assert.Equal(@"NVika\Parsers\InspectCodeParser.cs", issue.FilePath);
            Assert.Null(issue.HelpUri);
            Assert.Equal(1u, issue.Line);
            Assert.Equal("Auto-implemented property accessor is never used", issue.Message);
            Assert.Equal("UnusedAutoPropertyAccessor.Local", issue.Name);
            Assert.Equal(0u, issue.Offset.Start);
            Assert.Equal(5u, issue.Offset.End);
            Assert.Equal("NVika", issue.Project);
            Assert.Equal(IssueSeverity.Error, issue.Severity);
            Assert.Equal("InspectCode", issue.Source);

            issue = result.Skip(39).First();
            Assert.Equal("Redundancies in Code", issue.Category);
            Assert.Equal("Redundant 'case' label", issue.Description);
            Assert.Equal(@"NVika\Parsers\InspectCodeParser.cs", issue.FilePath);
            Assert.Null(issue.HelpUri);
            Assert.Null(issue.Line);
            Assert.Equal("Redundant case label", issue.Message);
            Assert.Equal("RedundantCaseLabel", issue.Name);
            Assert.Null(issue.Offset);
            Assert.Equal("NVika", issue.Project);
            Assert.Equal(IssueSeverity.Hint, issue.Severity);
            Assert.Equal("InspectCode", issue.Source);

            issue = result.Last();
            Assert.Equal("Common Practices and Code Improvements", issue.Category);
            Assert.Equal("Parameter type can be IEnumerable<T>: Non-private accessibility", issue.Description);
            Assert.Equal(@"NVika\Program.cs", issue.FilePath);
            Assert.Null(issue.HelpUri);
            Assert.Equal(35u, issue.Line);
            Assert.Equal("Parameter can be of type 'IEnumerable<string>'", issue.Message);
            Assert.Equal("ParameterTypeCanBeEnumerable.Global", issue.Name);
            Assert.NotNull(issue.Offset);
            Assert.Equal(25u, issue.Offset.Start);
            Assert.Equal(33u, issue.Offset.End);
            Assert.Equal("NVika", issue.Project);
            Assert.Equal(IssueSeverity.Suggestion, issue.Severity);
            Assert.Equal("InspectCode", issue.Source);
        }

        [Fact]
        public void Parse_SlnInSubFolder()
        {
            // arrange
            var report = XDocument.Parse(TestUtilities.GetEmbeddedResourceContent("inspectcodereport.xml"));
            report.Root.Element("Information").Element("Solution").Value = Path.Combine("src", "Vika.sln");

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "inspectcodereport.xml", new MockFileData(report.ToString()) },
                { Path.Combine("src", @"NVika\BuildServers\AppVeyor.cs"), new MockFileData(TestUtilities.GetEmbeddedResourceContent("AppVeyor.txt")) }
            });
            var parser = new InspectCodeParser();
            parser.FileSystem = fileSystem;

            // act
            var result = parser.Parse("inspectcodereport.xml");

            // assert
            Assert.Equal(41, result.Count());

            var issue = result.First();
            Assert.Equal("Constraints Violations", issue.Category);
            Assert.Equal("Inconsistent Naming", issue.Description);
            Assert.Equal(Path.Combine("src", @"NVika\BuildServers\AppVeyor.cs"), issue.FilePath);
            Assert.Null(issue.HelpUri);
            Assert.Equal(15u, issue.Line);
            Assert.Equal("Name '_appVeyorAPIUrl' does not match rule 'Instance fields (private)'. Suggested name is '_appVeyorApiUrl'.", issue.Message);
            Assert.Equal("InconsistentNaming", issue.Name);
            Assert.Equal(32u, issue.Offset.Start);
            Assert.Equal(47u, issue.Offset.End);
            Assert.Equal("NVika", issue.Project);
            Assert.Equal(IssueSeverity.Warning, issue.Severity);
            Assert.Equal("InspectCode", issue.Source);
        }
    }
}
