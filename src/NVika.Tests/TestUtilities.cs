using System.IO;
using System.Reflection;

namespace NVika.Tests
{
    internal static class TestUtilities
    {
        internal static string GetEmbeddedResourceContent(string resourceName)
        {
            using (Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(string.Concat("NVika.Tests.Data.", resourceName)))
            {
                using (StreamReader resourceStreamReader = new StreamReader(resourceStream))
                {
                    return resourceStreamReader.ReadToEnd();
                }
            }
        }

    }
}
