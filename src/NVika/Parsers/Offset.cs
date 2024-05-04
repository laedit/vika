using System.Globalization;
using System.Text;

namespace NVika.Parsers
{
    internal class Offset
    {
        internal uint? Start { get; set; }

        internal uint? End { get; set; }

        override public string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(CultureInfo.InvariantCulture, $"Start: {Start}");
            sb.AppendLine(CultureInfo.InvariantCulture, $"End: {End}");
            return sb.ToString();
        }
    }
}
