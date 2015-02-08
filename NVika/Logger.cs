using System.Collections.Generic;
using System.IO;

namespace NVika
{
    public class Logger
    {
        private readonly List<string> categories;
        private TextWriter writer;

        public Logger()
        {
            categories = new List<string>();
        }

        public void SetWriter(TextWriter textWriter)
        {
            writer = textWriter;
        }

        public void AddCategory(string category)
        {
            categories.Add(category);
        }

        public void Write(string message, string category, params object[] args)
        {
            if (writer == null) return;

            if (categories.Contains(category))
            { 
                writer.WriteLine(message, args);
            }
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
    }
}
