using ManyConsole;
using System.ComponentModel.Composition;

namespace NVika
{
    [InheritedExport(typeof(ConsoleCommand))]
    internal abstract class CommandBase : ConsoleCommand
    {
        private bool _isInDebugMode;
        protected Logger _logger;

        public CommandBase(Logger logger)
        {
            _logger = logger;
            this.HasOption("debug", "Enable debugging", s => _isInDebugMode = true);
            this.AllowsAnyAdditionalArguments("Reports to analyze");
        }

        public override int Run(string[] reportPaths)
        {
            if (_isInDebugMode)
            {
                _logger.AddCategory("debug");
            }

            return Execute(reportPaths);
        }

        protected abstract int Execute(string[] remainingArguments);
    }
}
