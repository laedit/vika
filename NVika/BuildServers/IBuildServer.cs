using System.ComponentModel.Composition;

namespace NVika
{
    [InheritedExport]
    internal interface IBuildServer
    {
        string Name { get; }

        bool CanApplyToCurrentContext();

        void WriteMessage(string message, string category, string details, string filename, string line, string offset, string projectName);
    }
}
