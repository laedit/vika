using System;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;

namespace NVika.Abstractions
{
	[ExcludeFromCodeCoverage]
	[Export(typeof(IEnvironment))]
    internal class EnvironmentWrapper : IEnvironment
    {
        public string GetEnvironmentVariable(string variable)
        {
            return Environment.GetEnvironmentVariable(variable);
        }
    }
}