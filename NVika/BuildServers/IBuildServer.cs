using NVika.Parsers;
using System.ComponentModel.Composition;

namespace NVika
{
    [InheritedExport]
    internal interface IBuildServer
    {
        string Name { get; }

        bool CanApplyToCurrentContext();

        void WriteMessage(Issue issue);
    }
}
