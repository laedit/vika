using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace NVika.Parsers
{
    [InheritedExport]
    internal interface IReportParser
    {
        string Name { get; }

        bool CanParse(string filePath);

        IEnumerable<Issue> Parse(string filePath);
    }
}
