using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;

namespace NVika
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class Logger
    {
        private readonly List<string> _categories;
        private TextWriter _writer;

        public Logger()
        {
            _categories = new List<string>();
        }

        public void SetWriter(TextWriter textWriter)
        {
            _writer = textWriter;
        }

        public void AddCategory(string category)
        {
            _categories.Add(category);
        }

        public void Debug(string message, params object[] args)
        {
            Write(message, "debug", args);
        }

        public void Info(string message, params object[] args)
        {
            Write(message, "info", args);
        }

        public void Error(string message, params object[] args)
        {
            Write(message, "error", args);
        }

        private void Write(string message, string category, params object[] args)
        {
            if (_writer == null)
            {
                throw new Exception("The writer must be set with the 'SetWriter' method first.");
            }

            if (_categories.Contains(category))
            {
                _writer.WriteLine(message, args);
            }
        }
    }
}
