namespace WillSoss.FixedWidthFile;

internal record FieldDefinition(
    int Width,
    char? PaddingChar,
    ValueAlignment? Alignment);

public class RecordBuilder
{
    internal string Type { get; private set; } = string.Empty;
    internal List<FieldDefinition> Fields { get; } = new();

    internal RecordBuilder() { }

    public RecordBuilder WithTypeName(string name)
    {
        Type = name;
        return this;
    }

    public RecordBuilder AddField(int width, char? paddingChar = null, ValueAlignment? alignment = null)
    {
        if (width < 1)
            throw new ArgumentOutOfRangeException(nameof(width), "Width must be greater than zero.");

        Fields.Add(new FieldDefinition(width, paddingChar, alignment));
        return this;
    }
}
