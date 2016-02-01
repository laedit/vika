using NVika.Parsers;

namespace NVika.BuildServers
{
    internal abstract class BuildServerBase : IBuildServer
    {
        protected bool IncludeSourceInMessage { get; private set; }

        public abstract string Name { get; }

        public abstract bool CanApplyToCurrentContext();

        public void ApplyParameters(bool includeSourceInMessage)
        {
            IncludeSourceInMessage = includeSourceInMessage;
        }

        public abstract void WriteMessage(Issue issue);
    }
}
