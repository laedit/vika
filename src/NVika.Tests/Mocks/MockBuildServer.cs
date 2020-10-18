using NVika.BuildServers;
using NVika.Parsers;
using System;

namespace NVika.Tests.Mocks
{
    internal class MockBuildServer : IBuildServer
    {
        public string Name
        {
            get
            {
                return "MockBuildServer";
            }
        }

        public void ApplyParameters(bool includeSourceInMessage)
        {
            throw new NotImplementedException();
        }

        public bool CanApplyToCurrentContext()
        {
            throw new NotImplementedException();
        }

        public void WriteMessage(Issue issue)
        {
            throw new NotImplementedException();
        }
    }
}
