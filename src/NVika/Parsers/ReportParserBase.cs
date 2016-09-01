using NVika.Exceptions;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.IO.Abstractions;
using System.Reflection;

namespace NVika.Parsers
{
    internal abstract class ReportParserBase : IReportParser
    {
        private readonly string[] _allowedExtensions;
        private readonly char? _firstChar;

        public abstract string Name { get; }

        [Import]
        public IFileSystem FileSystem { get; set; }

        [Import]
        public ILogger Logger { get; set; }

        protected ReportParserBase(string[] allowedExtensions, char? firstChar)
        {
            _allowedExtensions = allowedExtensions;
            _firstChar = firstChar;
        }

        public bool CanParse(string filePath)
        {
            try
            {
                using (var reader = new StreamReader(FileSystem.File.OpenRead(filePath)))
                {
                    if (!_firstChar.HasValue || (char)reader.Peek() == _firstChar.Value)
                    {
                        reader.BaseStream.Position = 0;
                        reader.DiscardBufferedData();
                        return CanParse(reader);
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw new LoadingReportException(filePath, ex);
            }
        }

        protected abstract bool CanParse(StreamReader streamReader);

        public abstract IEnumerable<Issue> Parse(string filePath);

        internal static string GetEmbeddedResourceContent(string resourceName)
        {
            using (var resourceStreamReader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(string.Concat("NVika.", resourceName))))
            {
                return resourceStreamReader.ReadToEnd();
            }
        }
    }
}
