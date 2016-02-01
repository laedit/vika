using System;
using System.IO;
using System.Text;
using Xunit;

namespace NVika.Tests
{
	public class LoggerTest
	{
		[Fact]
		public void Debug_WithoutWriter()
		{
			// arrange
			var logger = new Logger();

			// act
			var ex = Assert.Throws<Exception>(() => logger.Debug("debug message"));

			// assert
			Assert.Equal("The writer must be set with the 'SetWriter' method first.", ex.Message);
		}

		[Fact]
		public void Debug_WithoutCategoryAdded()
		{
			// arrange
			var logger = new Logger();
			var logs = new StringBuilder();
			logger.SetWriter(new StringWriter(logs));

			// act
			logger.Debug("debug message");

			// assert
			Assert.Equal(string.Empty, logs.ToString());
		}

		[Fact]
		public void Debug()
		{
			// arrange
			var logger = new Logger();
			var logs = new StringBuilder();
			logger.SetWriter(new StringWriter(logs));
			logger.AddCategory("debug");

			// act
			logger.Debug("debug message");
			logger.Info("info message");
			logger.Error("error message");

			// assert
			Assert.Equal("debug message", logs.ToString().Trim());
		}

		[Fact]
		public void Info()
		{
			// arrange
			var logger = new Logger();
			var logs = new StringBuilder();
			logger.SetWriter(new StringWriter(logs));
			logger.AddCategory("info");

			// act
			logger.Debug("debug message");
			logger.Info("info message");
			logger.Error("error message");

			// assert
			Assert.Equal("info message", logs.ToString().Trim());
		}

		[Fact]
		public void Error()
		{
			// arrange
			var logger = new Logger();
			var logs = new StringBuilder();
			logger.SetWriter(new StringWriter(logs));
			logger.AddCategory("error");

			// act
			logger.Debug("debug message");
			logger.Info("info message");
			logger.Error("error message");

			// assert
			Assert.Equal("error message", logs.ToString().Trim());
		}
	}
}
