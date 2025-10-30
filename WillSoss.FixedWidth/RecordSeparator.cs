namespace WillSoss.FixedWidth;
/// <summary>
/// Sets the expected characters found between records when reading fixed-width files.
/// </summary>
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
