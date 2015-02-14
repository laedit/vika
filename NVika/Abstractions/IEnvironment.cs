namespace NVika.Abstractions
{
    public interface IEnvironment
    {
        string GetEnvironmentVariable(string variable);
    }
}