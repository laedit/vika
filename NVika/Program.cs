using ManyConsole;
using NDesk.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.IO.Abstractions;
using System.Reflection;
using System.ComponentModel.Composition;
using System.Linq;

namespace NVika
{
    class Program
    {
        static int Main(string[] args)
        {
            return new Program().Run(args);
        }

        [Import]
        private Logger _logger;

        [ImportMany]
        private IEnumerable<ConsoleCommand> _commands;

        private int Run(string[] args)
        {
            Compose();

            InitializeLogger();

            args = SetDebug(args);

            _logger.Info("NVika V{0}", Assembly.GetExecutingAssembly().GetName().Version);

            return ConsoleCommandDispatcher.DispatchCommand(_commands, args.ToArray(), Console.Out);
        }

        private void InitializeLogger()
        {
            _logger.SetWriter(Console.Out);
            _logger.AddCategory("info");
            _logger.AddCategory("error");
        }

        private string[] SetDebug(string[] args)
        {
            var argsList = new List<string>(args);

            var debug = false;
            var defaultSet = new OptionSet
                {
                    {"debug", "Enable debugging", p => debug = true}
                };
            defaultSet.Parse(args);

            if (debug)
            {
                argsList.Remove("-debug");
                _logger.AddCategory("debug");
            }

            return argsList.ToArray();
        }

        private void Compose()
        {
            try
            {
                var first = new AssemblyCatalog(Assembly.GetExecutingAssembly());
                var container = new CompositionContainer(first);

                var batch = new CompositionBatch();
                batch.AddExportedValue<IFileSystem>(new FileSystem());
                batch.AddPart(this);
                container.Compose(batch);
            }
            catch (ReflectionTypeLoadException ex)
            {
                _logger.Info(@"Unable to load: \r\n{0}", string.Join("\r\n", ex.LoaderExceptions.Select(e => e.Message)));
                throw;
            }
        }
    }
}
