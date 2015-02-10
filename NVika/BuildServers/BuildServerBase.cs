using NVika.Parsers;

namespace NVika.BuildServers
{
    internal abstract class BuildServerBase : IBuildServer
    {
        private bool _includeSourceInMessage;

        public abstract string Name { get; }

        public abstract bool CanApplyToCurrentContext();

        public void ApplyParameters(bool includeSourceInMessage)
        {
            _includeSourceInMessage = includeSourceInMessage;
        }

        public virtual void WriteMessage(Issue issue)
        {
            if (_includeSourceInMessage)
            {
                issue.Message = string.Format("[{0}] {1}", issue.Source, issue.Message);
            }

            WriteIntegration(issue);
        }

        protected abstract void WriteIntegration(Issue issue);
    }
}
