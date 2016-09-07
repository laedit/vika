using NVika.Parsers;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using Xunit;

namespace NVika.Tests.Parsers
{
    public class FxCopParserTest
    {
        [Fact]
        public void Name()
        {
            // arrange
            var parser = new FxCopParser();
            parser.FileSystem = new MockFileSystem();

            // act
            var name = parser.Name;

            // assert
            Assert.Equal("FxCop", name);
        }

        [Theory]
        [InlineData("CodeAnalysisLog.xml", true)]
        [InlineData("emptyreport.xml", true)]
        [InlineData("onlymessages.xml", true)]
        [InlineData("onlyrules.xml", true)]
        [InlineData("onlyissues.json", false)]
        [InlineData("falsereport.xml", false)]
        public void CanParse(string reportPath, bool expectedResult)
        {
            // arrange
            var parser = new FxCopParser();
            parser.FileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "CodeAnalysisLog.xml", new MockFileData(TestUtilities.GetEmbeddedResourceContent("CodeAnalysisLog.xml")) },
                { "emptyreport.xml", new MockFileData("<FxCopReport Version=\"10.0\"></FxCopReport>") },
                { "onlymessages.xml", new MockFileData("<FxCopReport Version=\"10.0\"><Messages><Message /></Messages></FxCopReport>") },
                { "onlyrules.xml", new MockFileData("<FxCopReport Version=\"10.0\"><Rules><Rule /></Rules></FxCopReport>") },
                { "falsereport.xml", new MockFileData("{<FxCopReport Version=\"10.0\"><Rules><Rule /></Rules></FxCopReport>") },
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
                { "CodeAnalysisLog.xml", new MockFileData(TestUtilities.GetEmbeddedResourceContent("CodeAnalysisLog.xml")) },
            });
            var parser = new FxCopParser();
            parser.FileSystem = fileSystem;

            // act
            var result = parser.Parse("CodeAnalysisLog.xml").ToList();

            // assert
            Assert.Equal(26, result.Count);

            AssertIssue(result[0],
                        "Microsoft.Naming",
                        "Identifiers should be spelled correctly",
                        "http://msdn.microsoft.com/library/bb264492.aspx",
                        "Correct the spelling of 'Vika' in namespace name 'NVika.Abstractions'.",
                        "CA1704",
                        IssueSeverity.Warning);

            AssertIssue(result[4],
                        "Microsoft.Design",
                        "Mark assemblies with CLSCompliantAttribute",
                        "http://msdn.microsoft.com/library/ms182156.aspx",
                        "Mark 'NVika.exe' with CLSCompliant(true) because it exposes externally visible types.",
                        "CA1014",
                        IssueSeverity.Error);

            AssertIssue(result[10],
                        "Microsoft.Design",
                        "Do not catch general exception types",
                        "http://msdn.microsoft.com/library/ms182137.aspx",
                        "Modify 'Program.Run(string[])' to catch a more specific exception than 'Exception' or rethrow the exception.",
                        "CA1031",
                        IssueSeverity.Error,
                        Path.Combine(@"D:\Prog\Github\vika\src\NVika", "Program.cs"),
                        56);

            AssertIssue(result[16],
                        "Microsoft.Performance",
                        "Avoid uncalled private code",
                        "http://msdn.microsoft.com/library/ms182264.aspx",
                        "'AppVeyor.CompilationMessage.FileName.get()' appears to have no upstream public or protected callers.",
                        "CA1811",
                        IssueSeverity.Warning,
                        Path.Combine(@"D:\Prog\Github\vika\src\NVika\BuildServers", "AppVeyor.cs"),
                        105);

            AssertIssue(result[20],
                        "Microsoft.Design",
                        "Implement standard exception constructors",
                        "http://msdn.microsoft.com/library/ms182151.aspx",
                        "Add the following constructor to 'LoadingReportException': private LoadingReportException(SerializationInfo, StreamingContext).",
                        "CA1032",
                        IssueSeverity.Error);

            AssertIssue(result[25],
                        "Microsoft.Globalization",
                        "Specify IFormatProvider",
                        "http://msdn.microsoft.com/library/ms182190.aspx",
                        "Because the behavior of 'int.Parse(string)' could vary based on the current user's locale settings, replace this call in 'InspectCodeParser.GetOffset(XAttribute, string, uint?)' with a call to 'int.Parse(string, IFormatProvider)'. If the result of 'int.Parse(string, IFormatProvider)' will be based on input from the user, specify 'CultureInfo.CurrentCulture' as the 'IFormatProvider' parameter. Otherwise, if the result will based on input stored and accessed by software, such as when it is loaded from disk or from a database, specify 'CultureInfo.InvariantCulture'.",
                        "CA1305",
                        IssueSeverity.Error,
                        Path.Combine(@"D:\Prog\Github\vika\src\NVika\Parsers", "InspectCodeParser.cs"),
                        94);
        }

        private void AssertIssue(Issue result, string expectedCategory, string expectedDescription, string expectedUri, string expectedMessage, string expectedName, IssueSeverity expectedSeverity, string expectedFilePath = null, uint? expectedLine = null)
        {
            Assert.Equal(expectedCategory, result.Category);
            Assert.Equal(expectedDescription, result.Description);
            Assert.Equal(expectedFilePath, result.FilePath);
            Assert.Equal(expectedUri, result.HelpUri.AbsoluteUri);
            Assert.Equal(expectedLine, result.Line);
            Assert.Equal(expectedMessage, result.Message);
            Assert.Equal(expectedName, result.Name);
            Assert.Null(result.Offset);
            Assert.Null(result.Project);
            Assert.Equal(expectedSeverity, result.Severity);
            Assert.Equal("FxCop", result.Source);
        }
    }
}
