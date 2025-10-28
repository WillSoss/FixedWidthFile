namespace WillSoss.FixedWidthFile.Tests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        new FixedWidthWriterBuilder()
            .AddRecordDefinition(record =>
            {
                record.WithTypeName("Person")
                      .AddField(field => field.WithName("FirstName").WithWidth(10))
                      .AddField(field => field.WithName("LastName").WithWidth(10));
            });
    }
}
