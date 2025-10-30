using System.Text;

namespace WillSoss.FixedWidth;

public enum RecordSeparator
{
    /// <summary>
    /// No line break at the end of each record
    /// </summary>
    None,
    /// <summary>
    /// A line break (line feed, carriage return, or combination) is expected at the end of a record.
    /// </summary>
    LineBreak
}

public class FixedWidthReaderOptions
{
    public static readonly FixedWidthReaderOptions Default = new();

    /// <summary>
    /// The record separator, usually line breaks, but can be set to None if the file doesn't contain line breaks.
    /// </summary>
    public RecordSeparator RecordSeparator { get; init; } = RecordSeparator.LineBreak;
    /// <summary>
    /// Ignores characters located at the end of a record before the line break,
    /// otherwise an exception will be thrown. Does not apply when RecordSeparator = None.
    /// </summary>
    public bool IgnoreExtraCharactersAtEndOfRecord { get; init; } = false;
    /// <summary>
    /// The character encoding to use. If null, the <see cref="StreamReader"/> will attempt to detect the encoding used.
    /// </summary>
    public Encoding? Encoding { get; init; } = null;
    /// <summary>
    /// Determines whether the stream is left open when the reader is disposed.
    /// </summary>
    public bool LeaveStreamOpen { get; init; } = false;
    /// <summary>
    /// Determines whether white space is trimmed from values being read.
    /// </summary>
    public bool TrimWhiteSpace { get; init; } = true;
}
