namespace Nik.Extensions.Configurations.Tests;

public class ConfigurationTests
{
    private static IHost PrepareHost()
    {
        return Host.CreateDefaultBuilder()
           .ConfigureServices((services) =>
           {
               services.InitContext();
           })
           .Build();
    }

    [Fact]
    public void TestConfiguration()
    {
        _ = PrepareHost();

        Context.Environment.Should().Be("Staging");

        var options = Context.Configuration.GetSection("Test").Value;
        options.Should().BeNull();

        var secret = Context.Configuration.GetSection("TestSection").Get<MySecret>();
        secret.Key.Should().Be("Name");
        secret.Value.Should().Be("Nik");

        var book = Context.Configuration.GetSection<Book>();
        book.Status.Should().Be("New");
    }

    [ConfigurationName("BookConfig")]
    private class Book
    {
        public string Status { get; set; }
    }

    private class MySecret
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}