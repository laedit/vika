using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace NVika.Parsers
{
    [InheritedExport]
    internal interface IReportParser
    {
        string Name { get; }

        bool CanParse(string reportPath);

        IEnumerable<Issue> Parse(string reportPath);
    }
}
