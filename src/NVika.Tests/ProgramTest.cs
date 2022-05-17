using System;
using System.IO;
using System.Reflection;
using System.Text;
using Xunit;

namespace NVika.Tests
{
    public class ProgramTest
    {
        [SkippableFact]
        public void Run_NoParameters_ShouldShowHelp()
        {
            // arrange
            var output = new StringBuilder();
            Console.SetOut(new StringWriter(output));

            // act
            var exitCode = new Program().Run(Array.Empty<string>());

            var consoleOutput = output.ToString();

            // skip
            Skip.If(string.IsNullOrEmpty(consoleOutput));

            // assert
            Assert.Equal(1, exitCode);
            Assert.Contains($"NVika \"{Assembly.GetAssembly(typeof(Program)).GetName().Version}\"", consoleOutput);
            Assert.Contains("Executing parsereport (Parse the report and show warnings in console or inject them to the build server):", consoleOutput);
            Assert.Contains("No report was specified. You must indicate at least one report file.", consoleOutput);
        }

        [SkippableFact]
        public void Run_UnknownCommand_BuildServerIsDefaultCommand()
        {
            // arrange
            var output = new StringBuilder();
            Console.SetOut(new StringWriter(output));

            // act
            var exitCode = new Program().Run(new[] { "unkowncommand" });

            var consoleOutput = output.ToString();

            // skip
            Skip.If(string.IsNullOrEmpty(consoleOutput));

            // assert
            Assert.Equal(2, exitCode);
            Assert.Contains($"NVika \"{Assembly.GetAssembly(typeof(Program)).GetName().Version}\"", consoleOutput);
            Assert.Contains("Executing parsereport (Parse the report and show warnings in console or inject them to the build server):", consoleOutput);
            Assert.Contains("The report \"unkowncommand\" was not found.", consoleOutput);
        }

        [SkippableFact]
        public void Run_BuildServer_NonExistingReport_ShouldLogErrorToConsole()
        {
            // arrange
            var output = new StringBuilder();
            Console.SetOut(new StringWriter(output));

            // act
            var exitCode = new Program().Run(new[] { "parsereport", "nonexistingreport.abc" });

            var consoleOutput = output.ToString();

            // skip
            Skip.If(string.IsNullOrEmpty(consoleOutput));

            // assert
            Assert.Equal(2, exitCode);
            Assert.Contains($"NVika \"{Assembly.GetAssembly(typeof(Program)).GetName().Version}\"", consoleOutput);
            Assert.Contains("The report \"nonexistingreport.abc\" was not found.", consoleOutput);
        }

        [SkippableFact]
        public void Run_NoParameters_ExceptionAreLogged()
        {
            // arrange
            var output = new StringBuilder();
            Console.SetOut(new StringWriter(output));

            // act
            var exitCode = new Program().Run(null);

            var consoleOutput = output.ToString();

            // skip
            Skip.If(string.IsNullOrEmpty(consoleOutput));

            // assert
            Assert.Equal(1, exitCode);
            Assert.Contains($"NVika \"{Assembly.GetAssembly(typeof(Program)).GetName().Version}\"", consoleOutput);
            Assert.Contains("An unexpected error occurred:", consoleOutput);
            Assert.Contains("System.NullReferenceException: Object reference not set to an instance of an object", consoleOutput);
        }

        [SkippableFact]
        public void Run_ApplicationException_AreLoggedAndHaveExitCode()
        {
            // arrange
            var output = new StringBuilder();
            Console.SetOut(new StringWriter(output));
            var reportPath = "Data/WrongReport.xml";

            // act
            var exitCode = new Program().Run(new[] { "parsereport", reportPath, "--debug" });

            var consoleOutput = output.ToString();

            // skip
            Skip.If(string.IsNullOrEmpty(consoleOutput));

            // assert
            Assert.Equal(3, exitCode);
            Assert.Contains($"NVika \"{Assembly.GetAssembly(typeof(Program)).GetName().Version}\"", consoleOutput);
            Assert.Contains($"Report path is \"{reportPath}\"", consoleOutput);
            Assert.Contains("An unexpected error occurred:", consoleOutput);
            Assert.Contains("NVika.Exceptions.LoadingReportException: An exception happened when loading the report '" + reportPath + "'", consoleOutput);
        }

    }
}

