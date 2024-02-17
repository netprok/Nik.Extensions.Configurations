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

        var testObject = Context.Configuration.GetSection("TestSection").Get<TestClass>();
        testObject.Key.Should().Be("Name");
        testObject.Value.Should().Be("Nik");
    }

    private class TestClass
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}