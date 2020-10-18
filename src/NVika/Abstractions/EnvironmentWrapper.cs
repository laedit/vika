using System;
using System.ComponentModel.Composition;

namespace NVika.Abstractions
{
    [Export(typeof(IEnvironment))]
    internal class EnvironmentWrapper : IEnvironment
    {
        public string GetEnvironmentVariable(string variable)
        {
            return Environment.GetEnvironmentVariable(variable);
        }
    }
}
