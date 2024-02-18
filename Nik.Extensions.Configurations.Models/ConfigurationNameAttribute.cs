namespace Nik.Extensions.Configurations.Models;

[AttributeUsage(AttributeTargets.Class)]
public class ConfigurationNameAttribute : Attribute
{
    public string Name { get; }

    public ConfigurationNameAttribute(string description)
    {
        Name = description;
    }
}
