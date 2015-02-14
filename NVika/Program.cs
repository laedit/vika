using ManyConsole;
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
    internal class Program : IPartImportsSatisfiedNotification
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

        [Import]
        private Logger _logger;

        [ImportMany]
        private IEnumerable<ConsoleCommand> _commands;

#pragma warning restore 0649

        internal int Run(string[] args)
        {
            try
            {
                Compose();

                _logger.Info("NVika V{0}", Assembly.GetExecutingAssembly().GetName().Version);

                return ConsoleCommandDispatcher.DispatchCommand(_commands, args.ToArray(), Console.Out);
            }
            catch (Exception exception)
            {
                var error = string.Format("An unexpected error occurred:\r\n{0}", exception);
                _logger.Error(error);
                return 1;
            }
        }

        public void OnImportsSatisfied()
        {
            _logger.SetWriter(Console.Out);
            _logger.AddCategory("info");
            _logger.AddCategory("error");
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
                _logger.Error(@"Unable to load: \r\n{0}", string.Join("\r\n", ex.LoaderExceptions.Select(e => e.Message)));
                throw;
            }
        }
    }
}
