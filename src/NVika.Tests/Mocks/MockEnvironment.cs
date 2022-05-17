using NVika.Abstractions;
using System.Collections.Generic;

namespace NVika.Tests.Mocks
{
    internal class MockEnvironment : IEnvironment
    {
        private readonly Dictionary<string, string> _variables = new Dictionary<string, string>();

        public MockEnvironment(Dictionary<string, string> variables)
        {
            _variables = variables;
        }

        public string GetEnvironmentVariable(string variable)
        {
            if (_variables.TryGetValue(variable, out var value))
            {
                return value;
            }
            return null;
        }
    }
}
