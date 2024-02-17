namespace Nik.Common.Tests;

public class ConfigurationTests
{
    private static IHost PrepareHost()
    {
        return Host.CreateDefaultBuilder()
           .ConfigureServices((services) =>
           {
               services.InitAppContext();
           })
           .Build();
    }

    [Fact]
    public void TestConfiguration()
    {
        _ = PrepareHost();

        AppContext.Environment.Should().Be("Staging");

        var options = AppContext.Configuration.GetSection("Test").Value;
        options.Should().BeNull();

        var testObject = AppContext.Configuration.GetSection("TestSection").Get<TestClass>();
        testObject.Key.Should().Be("Name");
        testObject.Value.Should().Be("Nik");
    }

    private class TestClass
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}