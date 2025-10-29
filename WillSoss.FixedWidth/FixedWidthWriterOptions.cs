using System.Text;

namespace WillSoss.FixedWidth;
public class FixedWidthWriterOptions
{
    public static readonly FixedWidthWriterOptions Default = new();

    public char Padding { get; init; } = ' ';
    public ValueAlignment Alignment { get; init; } = ValueAlignment.Start;
    public Encoding Encoding { get; init; } = Encoding.UTF8;
    public string RecordSeparator { get; init; } = Environment.NewLine;
}
