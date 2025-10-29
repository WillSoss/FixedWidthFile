using Shouldly;
using WillSoss.FixedWidth;

namespace WillSoss.FixedWidthFile.Tests;

public class WriterTests
{
    [Fact]
    public async Task Test1Async()
    {
        var path = Path.GetTempFileName();

        using var writer = new FixedWidthWriter(path, [1, 2, 3], new FixedWidthWriterOptions
        {
            Alignment = ValueAlignment.End
        });

        await writer.WriteAsync("a", "b", "c");
        await writer.WriteAsync(1, 2, 3);
        await writer.WriteAsync([3, 2, 1], "oh", null, 9);

        await writer.FlushAsync();
        await writer.DisposeAsync();

        var text = await File.ReadAllTextAsync(path);

        text.ShouldBe("""
            a b  c
            1 2  3
             oh  9

            """);
    }
}
