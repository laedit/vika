using System;
using System.IO;
using System.Reflection;
using System.Text;
using Xunit;

namespace NVika.Tests
{
    public class ProgramTest
    {
        [Fact]
        public void Run_NoParameters_ShouldShowHelp()
        {
            // arrange
            var output = new StringBuilder();
            Console.SetOut(new StringWriter(output));

            // act
            var exitCode = new Program().Run(new string[0]);

            // assert
            var consoleOutput = output.ToString();
            Assert.Equal(-1, exitCode);
            Assert.Contains(string.Format("NVika V{0}", Assembly.GetAssembly(typeof(Program)).GetName().Version), consoleOutput);
            Assert.Contains("Invalid number of arguments-- expected 1 more.", consoleOutput);
            Assert.Contains("'buildserver' - Parse the report and show warnings in console or inject them to the build server", consoleOutput);
            Assert.Contains("Expected usage: NVika.Tests buildserver <options> Report to analyze", consoleOutput);
            Assert.Contains("<options> available:", consoleOutput);
            Assert.Contains("--debug                Enable debugging", consoleOutput);
            Assert.Contains("--includesource        Include the source in messages", consoleOutput);
        }

        [Fact]
        public void Run_UnknownCommand_BuildServerIsDefaultCommand()
        {
            // arrange
            var output = new StringBuilder();
            Console.SetOut(new StringWriter(output));

            // act
            var exitCode = new Program().Run(new[] { "unkowncommand" });

            // assert
            var consoleOutput = output.ToString();
            Assert.Equal(1, exitCode);
            Assert.Contains(string.Format("NVika V{0}", Assembly.GetAssembly(typeof(Program)).GetName().Version), consoleOutput);
            Assert.Contains("Executing buildserver (Parse the report and show warnings in console or inject them to the build server):", consoleOutput);
            Assert.Contains("The report 'unkowncommand' was not found.", consoleOutput);
        }

        [Fact]
        public void Run_BuildServer_NonExistingReport_ShouldLogErrorToConsole()
        {
            // arrange
            var output = new StringBuilder();
            Console.SetOut(new StringWriter(output));

            // act
            var exitCode = new Program().Run(new[] { "buildserver", "nonexistingreport.abc" });

            // assert
            var consoleOutput = output.ToString();
            Assert.Equal(1, exitCode);
            Assert.Contains(string.Format("NVika V{0}", Assembly.GetAssembly(typeof(Program)).GetName().Version), consoleOutput);
            Assert.Contains("The report 'nonexistingreport.abc' was not found.", consoleOutput);
        }
    }
}
