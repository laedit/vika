﻿using System;
using System.ComponentModel.Composition;

namespace NVika
{
    [Export]
    internal sealed class LocalBuildServer : IBuildServer
    {
        private bool _applyToCurrentContext = false;

        public string Name
        {
            get { return "Local console"; }
        }

        public bool CanApplyToCurrentContext()
        {
            return _applyToCurrentContext;
        }

        public void WriteMessage(string message, string category, string details, string filename, string line, string offset, string projectName)
        {
            Console.WriteLine("{0} {1} '{2}' - Line {3}: {4}", category, message, filename, line, details);
        }
    }
}
