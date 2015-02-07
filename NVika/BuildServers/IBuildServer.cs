
namespace NVika
{
    internal interface IBuildServer
    {
        bool CanApplyToCurrentContext();

        void WriteMessage(string message, string category, string details, string filename, string line, string offset, string projectName);
    }
}
