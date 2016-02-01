using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;

namespace NVika.Parsers
{
    [InheritedExport]
    internal interface IReportParser
    {
        string Name { get; }

        bool CanParse(XDocument document);

        IEnumerable<Issue> Parse(XDocument document);
    }
}