using NVika.Abstractions;
using System.Collections.Generic;

namespace NVika.Tests.Mocks
{
    public class EnvironmentMock : IEnvironment
    {
        public Dictionary<string, string> EnvironmentVariables { get; set; }

        public EnvironmentMock()
        {
            EnvironmentVariables = new Dictionary<string, string>();
        }

        public string GetEnvironmentVariable(string variable)
        {
            return EnvironmentVariables.ContainsKey(variable) ? EnvironmentVariables[variable] : null;
        }
    }
}