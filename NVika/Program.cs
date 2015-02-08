using ManyConsole;
using NDesk.Options;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Reflection;

namespace NVika
{
    class Program
    {
        static int Main(string[] args)
        {
            return new Program().Run(args);
        }

        internal IFileSystem FileSystem { get; private set; }
        internal Logger Logger { get; private set; }

        public Program()
        {
            FileSystem = new FileSystem();
            Logger = new Logger();
            Logger.SetWriter(Console.Out);
            Logger.AddCategory("info");
            Logger.AddCategory("error");
        }

        private int Run(string[] args)
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
                Logger.AddCategory("debug");
            }

            Logger.Info("NVika V{0}", Assembly.GetExecutingAssembly().GetName().Version);

            return ConsoleCommandDispatcher.DispatchCommand(GetCommands(), argsList.ToArray(), Console.Out);
        }

        private IEnumerable<ConsoleCommand> GetCommands()
        {
            return new List<ConsoleCommand> { new BuildServerCommand(FileSystem, Logger) };
        }
    }
}
