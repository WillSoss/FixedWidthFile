using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace WillSoss.FixedWidthFile;

public class FixedWidthWriter
{
    private FixedWidthOptions _options;
    private Dictionary<string, RecordBuilder> _recordTypes;
    private readonly StreamWriter _writer;


    internal FixedWidthWriter(FixedWidthWriterBuilder builder)
    {
        if (builder.Stream is null)
            throw new ArgumentNullException(nameof(builder.Stream), "A stream must be provided to write to. Use the WithStream method on FixedWidthWriterBuilder to provide a stream.");

        if (!builder.Stream.CanWrite)
            throw new ArgumentException("The provided stream must be writable.", nameof(builder.Stream));

        _writer = new StreamWriter(builder.Stream, builder.Options.Encoding, leaveOpen: true);
        _options = builder.Options;
        _recordTypes = builder.RecordTypes;

        if (_recordTypes.Count == 0)
            throw new InvalidOperationException("At least one record definition must be provided. Use the AddRecordDefinition method on FixedWidthWriterBuilder to define record layouts.");
    }

    public async Task WriteAsync(params string[] values)
    {
        await WriteAsync("Default", values);
    }

    public async Task WriteAsync(string recordType, params string[] values)
    {
        if (!_recordTypes.ContainsKey(recordType))
            throw new ArgumentException($"Record type '{recordType}' is not defined.", nameof(recordType));

        var record = _recordTypes[recordType];

        if (values.Length != record.Fields.Count)
            throw new ArgumentException($"The number of values provided ({values.Length}) does not match the number of fields defined for record type '{recordType}' ({record.FieldCount}).", nameof(values));

        for (int i = 0; i < values.Length; i++)
        {
            var field = record.Fields[i];
            var value = values[i] ?? string.Empty;

            if (value.Length > field.Width)
            {
                value = value.Substring(0, field.Width);
            }
            else if (value.Length < field.Width)
            {
                int paddingNeeded = field.Width - value.Length;
                string padding = new string(field.PaddingChar, paddingNeeded);
                value = field.Alignment switch
                {
                    ValueAlignment.Left => value + padding,
                    ValueAlignment.Right => padding + value,
                    _ => throw new InvalidOperationException("Unsupported alignment type."),
                };
            }

            await _writer.WriteAsync(value);
        }
    }
}
