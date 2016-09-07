using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace NVika.Parsers
{
    internal abstract class XmlReportParser : ReportParserBase
    {
        private readonly string _rootName;
        private readonly string[] _requiredElements;

        internal XmlReportParser(string rootName, params string[] requiredElements)
            : base(new[] { ".xml"}, '<')
        {
            _rootName = rootName;
            _requiredElements = requiredElements;
        }

        protected override bool CanParse(StreamReader streamReader)
        {
            // Avoid Xml exception caused by the BOM
            using (var xmlReader = new XmlTextReader(streamReader.BaseStream))
            {
                var report = XDocument.Load(xmlReader);

                var canParse = report.Root.Name == _rootName;
                if (canParse && _requiredElements != null)
                {
                    canParse = _requiredElements.All(element => report.Descendants(element).Any());
                }
                return canParse;
            }
        }

        public override IEnumerable<Issue> Parse(string reportPath)
        {
            return Parse(XDocument.Load(FileSystem.File.OpenRead(reportPath)));
        }

        protected abstract IEnumerable<Issue> Parse(XDocument report);
    }
}
