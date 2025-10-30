using Shouldly;
using System.Text;

namespace WillSoss.FixedWidth.Tests;
public class ReaderTests
{
    [Fact]
    public async Task MultipleRecords()
    {
        var file = """
            A  1234     hello
            B1C2
            """;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(file));

        using var reader = new FixedWidthReader(stream, async r =>
        {
            var peek = await r.Peek([1]);

            return peek?[0] switch
            {
                "A" => [1, 6, 10],
                "B" => [1, 1, 1, 1],
                _ => null
            };
        });

        var rec1 = await reader.Read();
        var rec2 = await reader.Read();
        var eof = await reader.Read();

        rec1.ShouldBe(new string[]
        {
            "A",
            "1234",
            "hello"
        });

        rec2.ShouldBe(new string[]
        {
            "B", "1", "C", "2"
        });

        eof.ShouldBeNull();
    }

    [Fact]
    public async Task NoLineBreaks()
    {
        var file = "A  1234     helloB1C2";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(file));

        using var reader = new FixedWidthReader(stream, async r =>
        {
            var peek = await r.Peek(new int[] { 1 });

            return peek?[0] switch
            {
                "A" => [1, 6, 10],
                "B" => [1, 1, 1, 1],
                _ => null
            };
        }, new FixedWidthReaderOptions
        {
            RecordSeparator = RecordSeparator.None,
        });

        var rec1 = await reader.Read();
        var rec2 = await reader.Read();
        var eof = await reader.Read();

        rec1.ShouldBe(new string[]
        {
            "A",
            "1234",
            "hello"
        });

        rec2.ShouldBe(new string[]
        {
            "B", "1", "C", "2"
        });

        eof.ShouldBeNull();
    }

    [Fact]
    public async Task ExtraCharactersThrowsException()
    {
        var file = "A  1234     helloB1C2";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(file));

        using var reader = new FixedWidthReader(stream, [1, 6, 10]) ;

       await Should.ThrowAsync<FormatException>(async () => await reader.Read());
    }

    [Fact]
    public async Task ExtraCharactersIgnored()
    {
        var file = "A  1234     helloB1C2";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(file));

        using var reader = new FixedWidthReader(stream, [1, 6, 10], new FixedWidthReaderOptions
        {
            IgnoreExtraCharactersAtEndOfRecord = true
        });

        var rec1 = await reader.Read();
        var eof = await reader.Read();

        rec1.ShouldBe(new string[]
        {
            "A",
            "1234",
            "hello"
        });

        eof.ShouldBeNull();
    }

    [Fact]
    public async Task PeekingIntoNextRecord()
    {
        // Tests a specific scenario where peeking reads
        // part of the next record into the buffer and
        // the current record's line break is consumed, 
        // leaving the first part of the next record for
        // the subsequent peek and read.

        var file = """
            A 1
             123B    john     doe
            """;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(file));

        using var reader = new FixedWidthReader(stream, async r =>
        {
            var peek = await r.Peek([1, 3, 1]);

            if (peek?[0] == "A")
                return [1, 2];
            else if (peek?[2] == "B")
                return [4, 1, 8, 8];
            else
                return null;
        });

        var rec1 = await reader.Read();
        var rec2 = await reader.Read();
        var eof = await reader.Read();

        rec1.ShouldBe(new string[]
        {
            "A",
            "1"
        });

        rec2.ShouldBe(new string[]
        {
            "123", "B", "john", "doe"
        });

        eof.ShouldBeNull();
    }
}
