using NVika.Parsers;

namespace NVika.BuildServers
{
    internal abstract class BuildServerBase : IBuildServer
    {
        protected bool _includeSourceInMessage;

        public abstract string Name { get; }

        public abstract bool CanApplyToCurrentContext();

        public void ApplyParameters(bool includeSourceInMessage)
        {
            _includeSourceInMessage = includeSourceInMessage;
        }

        public abstract void WriteMessage(Issue issue);
    }
}