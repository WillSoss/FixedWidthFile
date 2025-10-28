using System.Text;

namespace WillSoss.FixedWidthFile;
public class FixedWidthOptions
{
    public char DefaultPadding { get; internal set; } = ' ';
    public ValueAlignment DefaultAlignment { get; internal set; } = ValueAlignment.Start;
    public Encoding Encoding { get; internal set; } = Encoding.UTF8;
    public string RecordSeparator { get; internal set; } = "\r\n";
}
