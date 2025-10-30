using System.Text;

namespace WillSoss.FixedWidth;
public class FixedWidthReader : IDisposable
{
    private readonly StreamReader _reader;
    private readonly FixedWidthReaderOptions _options;
    private bool _disposed;

    private readonly Func<FixedWidthReader, Task<int[]?>> _getWidths;
    private char[] _buffer = new char[256];
    private int _index = 0;
    private int _length = 0;

    /// <summary>
    /// Creates a new <see cref="FixedWidthReader"/> to read the file located at <paramref name="filePath"/>.
    /// </summary>
    /// <param name="filePath">The file to read.</param>
    /// <param name="widths">The field widths of each record.</param>
    /// <param name="options">Options affecting the way the file is read.</param>
    public FixedWidthReader(string filePath, int[] widths, FixedWidthReaderOptions? options = null)
        : this(File.OpenRead(filePath), (FixedWidthReader r) => Task.FromResult<int[]?>(widths), options) { }

    /// <summary>
    /// Creates a new <see cref="FixedWidthReader"/> to read the provided <paramref name="stream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to read.</param>
    /// <param name="widths">The field widths of each record.</param>
    /// <param name="options">Options affecting the way the file is read.</param>
    public FixedWidthReader(Stream stream, int[] widths, FixedWidthReaderOptions? options = null)
        : this(stream, (FixedWidthReader r) => Task.FromResult<int[]?>(widths), options) { }

    /// <summary>
    /// Creates a new <see cref="FixedWidthReader"/> to read the file located at <paramref name="filePath"/>.
    /// </summary>
    /// <param name="filePath">The file to read.</param>
    /// <param name="getWidths">A function to determine the field widths for the next record. 
    /// Used when the file contains multiple record layouts. The function should use <see cref="Peek(int[])"/>
    /// to read enough of the next record to determine the field widths.</param>
    /// <param name="options">Options affecting the way the file is read.</param>
    public FixedWidthReader(string filePath, Func<FixedWidthReader, Task<int[]?>> getWidths, FixedWidthReaderOptions? options = null)
        : this(File.OpenRead(filePath), getWidths, options) { }

    /// <summary>
    /// Creates a new <see cref="FixedWidthReader"/> to read the provided <paramref name="stream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to read.</param>
    /// <param name="getWidths">A function to determine the field widths for the next record. 
    /// Used when the file contains multiple record layouts. The function should use <see cref="Peek(int[])"/>
    /// to read enough of the next record to determine the field widths.</param>

    /// <param name="options">Options affecting the way the file is read.</param>
    public FixedWidthReader(Stream stream, Func<FixedWidthReader, Task<int[]?>> getWidths, FixedWidthReaderOptions? options = null)
    {
        _options = options ?? FixedWidthReaderOptions.Default;

        _reader = new StreamReader(stream, _options.Encoding, _options.Encoding == null, -1, _options.LeaveStreamOpen);
        _getWidths = getWidths;
    }

    public async Task<string[]?> Peek(int[] widths) =>
        await ReadOrPeek(widths, true);

    public async Task<string[]?> Read()
    {
        var widths = await _getWidths(this);

        if (widths is null)
            return null;

        return await Read(widths);
    }

    public async Task<string[]?> Read(int[] widths) =>
        await ReadOrPeek(widths, false);

    private async Task<string[]?> ReadOrPeek(int[] widths, bool peek)
    {
        var length = widths.Sum(w => w);

        var chars = await ReadOrPeek(length, peek);

        if (chars is null)
            return null;

        var span = new Span<char>(chars);
        var record = new string[widths.Length];

        int index = 0;
        for (int i = 0; i < widths.Length;  i++)
        {
            record[i] = span.Slice(index, widths[i]).ToString();

            if (_options.TrimWhiteSpace)
                record[i] = record[i].Trim();

            index += widths[i];
        }

        return record;
    }

    private async Task<char[]?> ReadOrPeek(int length, bool peek)
    {
        // expand buffer if it's not big enough
        if (length > _buffer.Length)
        {
            var buffer = new char[length];

            // copy contents of buffer if not empty
            if (_length > 0)
                Array.Copy(_buffer, _index, buffer, 0, _length);

            _buffer = buffer;
        }

        // shift array contents if we'll run out of space
        if (_index + length > _buffer.Length)
        {
            var buffer = new char[_buffer.Length];

            Array.Copy(_buffer, _index, buffer, 0, _length);

            _buffer = buffer;
        }

        if (length > _length)
        {
            _length += await _reader.ReadAsync(_buffer, _length, length - _length);

            if (_length == 0)
                return null;
        }

        var ret = new ReadOnlySpan<char>(_buffer, _index, length);

        // If reading, reset index and length to
        // effectively clear the buffer.
        if (!peek)
        {
            if (length == _length)
            {
                _index = 0;
                _length = 0;
            }
            else
            {
                _index = _index + length;
                _length = _length - length;
            }
        }

        if (!peek && _options.RecordSeparator == RecordSeparator.LineBreak)
        {
            var extra = ReadToNextRecord();

            if (!string.IsNullOrEmpty(extra) && !_options.IgnoreExtraCharactersAtEndOfRecord)
                throw new FormatException($"Unread characters found. Set '{nameof(_options.IgnoreExtraCharactersAtEndOfRecord)}' to true to turn off this exception.");
        }

        return ret.ToArray();
    }

    private string ReadToNextRecord()
    {
        var unreadBuffer = new Span<char>(_buffer, _index, _length);
        StringBuilder sb = new StringBuilder();

        int pos = 0;
        bool foundInBuffer = false;
        while (pos < unreadBuffer.Length)
        {
            var ch = unreadBuffer[pos];
            if (ch == '\r' || ch == '\n')
            {
                foundInBuffer = true;
                pos++;

                if (ch == '\r')
                {
                    if (unreadBuffer.Length > pos)
                    {
                        if (unreadBuffer[pos] == '\n')
                            pos++;
                    }
                    else if (_reader.Peek() == '\n')
                    {
                        _reader.Read();
                    }
                }

                break;
            }
            else
            {
                sb.Append(ch);
                pos++;
            }
        }

        // adjust buffer by chars read
        _index += pos;
        _length -= pos;

        if (_length == 0)
            _index = 0;

        if (!foundInBuffer)
        {
            sb.Append(_reader.ReadLine());
        }

        return sb.ToString();
    }

    public void Dispose() => _reader?.Dispose();
}
