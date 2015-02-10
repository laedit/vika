using NVika.Parsers;
using System.ComponentModel.Composition;

namespace NVika.BuildServers
{
    [InheritedExport]
    internal interface IBuildServer
    {
        string Name { get; }

        bool CanApplyToCurrentContext();

        void ApplyParameters(bool includeSourceInMessage);

        void WriteMessage(Issue issue);
    }
}
