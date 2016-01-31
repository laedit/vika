using ManyConsole;
using System.ComponentModel.Composition;

namespace NVika
{
    [InheritedExport(typeof(ConsoleCommand))]
    internal abstract class CommandBase : ConsoleCommand
    {
        private bool _isInDebugMode;

        protected Logger Logger { get; private set; }

        protected CommandBase(Logger logger)
        {
            Logger = logger;
            HasOption("debug", "Enable debugging", s => _isInDebugMode = true);
            AllowsAnyAdditionalArguments("Reports to analyze");
        }

        public override int Run(string[] reportPaths)
        {
            if (_isInDebugMode)
            {
                Logger.AddCategory("debug");
            }

            return Execute(reportPaths);
        }

        protected abstract int Execute(string[] remainingArguments);
    }
}
