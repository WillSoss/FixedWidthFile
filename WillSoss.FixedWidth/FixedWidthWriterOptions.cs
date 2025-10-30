using System.Text;

namespace WillSoss.FixedWidth;
public class FixedWidthWriterOptions
{
    public static readonly FixedWidthWriterOptions Default = new();

    /// <summary>
    /// Character to use as padding to reach the required field width.
    /// </summary>
    public char Padding { get; init; } = ' ';
    /// <summary>
    /// Determines the alignment of values in fixed-width fields. Left-aligned by default.
    /// </summary>
    public ValueAlignment Alignment { get; init; } = ValueAlignment.Left;
    /// <summary>
    /// The record separator placed after each record, <see cref="Environment.NewLine"/> by default.
    /// </summary>
    public string RecordSeparator { get; init; } = Environment.NewLine;
    /// <summary>
    /// The character encoding to use. When null, the <see cref="StreamWriter"/> default will be used.
    /// </summary>
    public Encoding? Encoding { get; init; } = null;
    /// <summary>
    /// Determines whether the stream is left open when the <see cref="FixedWidthWriter"/> is disposed.
    /// </summary>
    public bool LeaveStreamOpen { get; init; } = false;
}
