using ManyConsole;
using Mono.Options;
using NVika.Exceptions;
using Serilog;
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

                _logger.Information("NVika {Version}", typeof(Program).GetTypeInfo().Assembly.GetName().Version);

                return ConsoleCommandDispatcher.DispatchCommand(_commands, extraArgs.ToArray(), Console.Out);
            }
            catch (NVikaException exception)
            {
                _logger.Fatal(exception, "An unexpected error occurred:");
                return exception.ExitCode;
            }
            catch (Exception exception)
            {
                if (_logger == null)
                {
                    Console.WriteLine("Error: logger is not configured.");
                    Console.WriteLine($"An unexpected error occurred:\r\n{exception}");
                }
                else
                {
                    _logger.Fatal(exception, "An unexpected error occurred:");
                }
                return ExitCodes.UnknownError;
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
                        .WriteTo.Console(outputTemplate: "[{Level}] {Message}{NewLine}{Exception}");
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
                using(var currentAssemblyCatalog = new AssemblyCatalog(Assembly.GetExecutingAssembly()))
                using (var container = new CompositionContainer(currentAssemblyCatalog))
                {
                    var batch = new CompositionBatch();
                    batch.AddExportedValue<IFileSystem>(new FileSystem());
                    batch.AddPart(this);
                    container.Compose(batch);
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                _logger.Error(@"Unable to load: \r\n{0}", string.Join("\r\n", ex.LoaderExceptions.Select(e => e.Message)));
                throw;
            }
        }
    }
}
