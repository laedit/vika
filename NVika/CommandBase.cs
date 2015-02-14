using ManyConsole;
using System.ComponentModel.Composition;

namespace NVika
{
    [InheritedExport(typeof(ConsoleCommand))]
    public abstract class CommandBase : ConsoleCommand
    {
        private bool _isInDebugMode;
        protected Logger _logger;

        public CommandBase(Logger logger)
        {
            _logger = logger;
            this.HasOption("debug", "Enable debugging", s => _isInDebugMode = true);
        }

        public override int Run(string[] remainingArguments)
        {
            if (_isInDebugMode)
            {
                _logger.AddCategory("debug");
            }

            return Execute(remainingArguments);
        }

        protected abstract int Execute(string[] remainingArguments);
    }
}