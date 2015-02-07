using ManyConsole;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;

namespace NVika
{
    class Program
    {
        static int Main(string[] args)
        {
            return new Program().Run(args);
        }

        internal IFileSystem FileSystem { get; private set; }

        public Program()
        {
            FileSystem = new FileSystem();
            // TODO Log like in Pretzel
        }

        private int Run(string[] args)
        {
            // TODO Display program name and version
            return ConsoleCommandDispatcher.DispatchCommand(GetCommands(), args, Console.Out);
        }

        private IEnumerable<ConsoleCommand> GetCommands()
        {
            return new List<ConsoleCommand> { new BuildServerCommand(FileSystem) };
        }
    }
}
