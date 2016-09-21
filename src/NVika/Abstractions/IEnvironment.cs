namespace NVika.Abstractions
{
    internal interface IEnvironment
    {
        string GetEnvironmentVariable(string variable);
    }
}
