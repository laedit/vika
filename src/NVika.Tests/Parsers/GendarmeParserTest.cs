using NVika.Parsers;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using Xunit;

namespace NVika.Tests.Parsers
{
    public class GendarmeParserTest
    {
        [Fact]
        public void Name()
        {
            // arrange
            var parser = new GendarmeParser();
            parser.FileSystem = new MockFileSystem();

            // act
            var name = parser.Name;

            // assert
            Assert.Equal("Gendarme", name);
        }

        [Theory]
        [InlineData("GendarmeReport.xml", true)]
        [InlineData("emptyreport.xml", true)]
        [InlineData("onlymessages.xml", true)]
        [InlineData("onlyrules.xml", true)]
        [InlineData("onlyissues.json", false)]
        [InlineData("falsereport.xml", false)]
        public void CanParse(string reportPath, bool expectedResult)
        {
            // arrange
            var parser = new GendarmeParser();
            parser.FileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "GendarmeReport.xml", new MockFileData(TestUtilities.GetEmbeddedResourceContent("GendarmeReport.xml")) },
                { "emptyreport.xml", new MockFileData("<gendarme-output date=\"09/05/2016 17:56:58\"></gendarme-output>") },
                { "onlymessages.xml", new MockFileData("<gendarme-output date=\"09/05/2016 17:56:58\"><rules><rule /></rules></gendarme-output>") },
                { "onlyrules.xml", new MockFileData("<gendarme-output date=\"09/05/2016 17:56:58\"><results><rule /></results></gendarme-output>") },
                { "falsereport.xml", new MockFileData("{<gendarme-output date=\"09/05/2016 17:56:58\"><results><rule /></results></gendarme-output>") },
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
                { "GendarmeReport.xml", new MockFileData(TestUtilities.GetEmbeddedResourceContent("GendarmeReport.xml")) },
            });
            var parser = new GendarmeParser();
            parser.FileSystem = fileSystem;

            // act
            var result = parser.Parse("GendarmeReport.xml").ToList();

            // assert
            Assert.Equal(4, result.Count);

            AssertIssue(result[0],
                        "Performance",
                        "Due to performance issues, types which are not visible outside of the assembly and which have no derived types should be sealed.",
                        "https://github.com/spouliot/gendarme/wiki/Gendarme.Rules.Performance.AvoidUnsealedUninheritedInternalTypeRule(2.10)",
                        "You should seal this type, unless you plan to inherit from this type in the near-future.",
                        "AvoidUnsealedUninheritedInternalTypeRule",
                        IssueSeverity.Warning,
                        @"d:\system\me\documents\visual studio 2015\Projects\GendarmeTest\GendarmeTest\Program.cs",
                        11);

            AssertIssue(result[1],
                        "Performance",
                        "The method contains one or more unused parameters.",
                        "https://github.com/spouliot/gendarme/wiki/Gendarme.Rules.Performance.AvoidUnusedParametersRule(2.10)",
                        "You should remove or use the unused parameters.",
                        "AvoidUnusedParametersRule",
                        IssueSeverity.Suggestion,
                        @"d:\system\me\documents\visual studio 2015\Projects\GendarmeTest\GendarmeTest\Program.cs",
                        11);

            AssertIssue(result[2],
                        "Design",
                        "This type contains only static fields and methods and should be static.",
                        "https://github.com/spouliot/gendarme/wiki/Gendarme.Rules.Design.ConsiderUsingStaticTypeRule(2.10)",
                        "Change this type into a static (or sealed for 1.x) type gain clarity and better error reporting.",
                        "ConsiderUsingStaticTypeRule",
                        IssueSeverity.Hint,
                        @"d:\system\me\documents\visual studio 2015\Projects\GendarmeTest\GendarmeTest\Program.cs",
                        11);

            AssertIssue(result[3],
                        "Design",
                        "This assembly is not decorated with the [CLSCompliant] attribute.",
                        "https://github.com/spouliot/gendarme/wiki/Gendarme.Rules.Design.MarkAssemblyWithCLSCompliantRule(2.10)",
                        "Add this attribute to ease the use (or non-use) of your assembly by CLS consumers.",
                        "MarkAssemblyWithCLSCompliantRule",
                        IssueSeverity.Error);
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
            Assert.Equal("GendarmeTest", result.Project);
            Assert.Equal(expectedSeverity, result.Severity);
            Assert.Equal("Gendarme", result.Source);
        }
    }
}
