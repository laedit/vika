using ManyConsole;
using NDesk.Options;
using Serilog;
using Serilog.Configuration;
using Serilog.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;

namespace NVika
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var exitCode = new Program().Run(args);
            if (Debugger.IsAttached)
            {
                Console.ReadKey();
            }

            Environment.Exit(exitCode);
        }

#pragma warning disable 0649

        [Export]
        private ILogger _logger;

        [ImportMany]
        private IEnumerable<ConsoleCommand> _commands;

#pragma warning restore 0649

        internal int Run(string[] args)
        {
            try
            {
                var extraArgs = Initialize(args);

                Compose();

                _logger.Information("NVika V{Version}", Assembly.GetExecutingAssembly().GetName().Version);

                return ConsoleCommandDispatcher.DispatchCommand(_commands, extraArgs.ToArray(), Console.Out);
            }
            catch (Exception exception)
            {
                if (_logger == null)
                {
                    Console.WriteLine("Error: logger is not configured.");
                    Console.WriteLine("An unexpected error occurred:\r\n{0}", exception);
                }
                else
                {
                    _logger.Error(exception, "An unexpected error occurred:");
                }
                return 1;
            }
        }

        private List<string> Initialize(string[] args)
        {
            var isInDebugMode = false;
            List<string> extraArgs = null;

            if (args != null)
            {
                var globalArguments = new OptionSet
                {
                    { "debug", "Enable debugging", s => isInDebugMode = true }
                };

                extraArgs = globalArguments.Parse(args);
            }

            var loggerConf = new LoggerConfiguration()
                        .Enrich.WithExceptionDetails()
                        .WriteTo.LiterateConsole(outputTemplate: "[{Level}] {Message}{NewLine}{Exception}");
            if (isInDebugMode)
            {
                loggerConf.MinimumLevel.Debug();
            }

            _logger = loggerConf.CreateLogger();

            return extraArgs;
        }

        private void Compose()
        {
            try
            {
                var first = new AssemblyCatalog(Assembly.GetExecutingAssembly());
                using (var container = new CompositionContainer(first))
                {
                    var batch = new CompositionBatch();
                    batch.AddExportedValue<IFileSystem>(new FileSystem());
                    batch.AddPart(this);
                    container.Compose(batch);
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                _logger.Error(ex, @"Unable to load: \r\n{0}", string.Join("\r\n", ex.LoaderExceptions.Select(e => e.Message)));
                throw;
            }
        }
    }
}
