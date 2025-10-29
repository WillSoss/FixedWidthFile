using System.Text;

namespace WillSoss.FixedWidth;

/// <summary>
/// Writes fixed-width files.
/// </summary>
public class FixedWidthWriter : IDisposable, IAsyncDisposable
{
    private readonly int[] _widths;
    private readonly FixedWidthWriterOptions _options;

    private readonly StreamWriter _writer;
    private readonly bool _leaveOpen = true;
    private bool _disposed = false;

    /// <summary>
    /// Creates a <see cref="FixedWidthWriter"/> that writes to a file at <paramref name="filePath"/>.
    /// </summary>
    /// <param name="filePath">The path of the file to write to.</param>
    /// <param name="widths">The field widths of each record.</param>
    public FixedWidthWriter(string filePath, params int[] widths)
        : this(filePath, widths, FixedWidthWriterOptions.Default) { }

    /// <summary>
    /// Creates a <see cref="FixedWidthWriter"/> that writes to a file at <paramref name="filePath"/>.
    /// </summary>
    /// <param name="filePath">The path of the file to write to.</param>
    /// <param name="widths">The field widths of each record.</param>
    /// <param name="options">Options affecting the way the file is written.</param>
    public FixedWidthWriter(string filePath, int[] widths, FixedWidthWriterOptions options)
        : this(File.Open(filePath, FileMode.Create), widths, options) 
    {
        _leaveOpen = false;
    }

    /// <summary>
    /// Creates a <see cref="FixedWidthWriter"/> that writes to the <paramref name="stream"/>.
    /// </summary>
    /// <param name="stream">The stream to write to.</param>
    /// <param name="widths">The field widths of each record.</param>
    public FixedWidthWriter(Stream stream, params int[] widths)
        : this(stream, widths, FixedWidthWriterOptions.Default) { }

    /// <summary>
    /// Creates a <see cref="FixedWidthWriter"/> that writes to the <paramref name="stream"/>.
    /// </summary>
    /// <param name="stream">The stream to write to.</param>
    /// <param name="widths">The field widths of each record.</param>
    /// <param name="options">Options affecting the way the file is written.</param>
    public FixedWidthWriter(Stream stream, int[] widths, FixedWidthWriterOptions options)
    {
        if (widths.Length < 1)
            throw new ArgumentException("Widths must have at least one field width.", nameof(widths));

        _widths = widths;
        _options = options;
        _writer = new StreamWriter(stream, options.Encoding);
    }

    /// <summary>
    /// Writes a record to the file.
    /// </summary>
    /// <param name="values">The record's values for each field.</param>
    public async Task WriteAsync(params object?[] values) =>
        await WriteAsync(values, _widths);

    /// <summary>
    /// Writes a record to the file.
    /// </summary>
    /// <param name="widths">Field widths for the record, overriding the default specified in the constructor.</param>
    /// <param name="values">The record's values for each field.</param>
    public async Task WriteAsync(int[] widths, params object?[] values) =>
        await WriteAsync(widths, values.Select(v => v.ToString()).ToArray());

    /// <summary>
    /// Writes a record to the file.
    /// </summary>
    /// <param name="values">The record's values for each field.</param>
    public async Task WriteAsync(params string?[] values) =>
        await WriteAsync(_widths, values);

    /// <summary>
    /// Writes a record to the file.
    /// </summary>
    /// <param name="widths">Field widths for the record, overriding the default specified in the constructor.</param>
    /// <param name="values">The record's values for each field.</param>
    public async Task WriteAsync(int[] widths, params string?[] values)
    {
        if (values.Length != widths.Length)
            throw new ArgumentException($"The number of values ({values.Length}) does not match the number of field widths ({widths.Length}).", nameof(values));

        StringBuilder builder = new();

        for (int i = 0; i < values.Length; i++)
        {
            var width = widths[i];
            var value = values[i] ?? string.Empty;

            if (value.Length > width)
            {
                throw new ArgumentException($"Value too long. Field index: {i}, width: {width}, value: {value} ({value.Length} chars).");
            }
            else if (value.Length < width)
            {
                string padding = new string(_options.Padding, width - value.Length);

                if (_options.Alignment == ValueAlignment.End)
                    builder.Append(padding);

                builder.Append(value);

                if (_options.Alignment == ValueAlignment.Start)
                    builder.Append(builder);
            }
        }

        builder.Append(_options.RecordSeparator);

        await _writer.WriteAsync(builder.ToString());
    }

    /// <summary>
    /// Clears all buffers for the writer and causes any buffered data to be written to the underlying stream.
    /// </summary>
    public void Flush()
    {
        ThrowIfDisposed();
        _writer.Flush();
    }

    /// <summary>
    /// Clears all buffers for the stream asynchronously and causes any buffered data to be written to the underlying device.
    /// </summary>
    public async Task FlushAsync()
    {
        ThrowIfDisposed();
        await _writer.FlushAsync();
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(FixedWidthWriter), "Cannot invoke method after the object is disposed.");
    }

    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            _disposed = true;

            await _writer.FlushAsync();

            if (!_leaveOpen)
                await _writer.DisposeAsync();
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;

            _writer?.Flush();

            if (!_leaveOpen)
                _writer?.Dispose();
        }
    }
}
