using Nik.Extensions.Configurations.Models;

namespace Nik.Extensions.Configurations;

public static class Context
{
    private const string DotnetVariable = "DOTNET_ENVIRONMENT";
    private const string AspNetCoreVariable = "ASPNETCORE_ENVIRONMENT";

    private const string Development = "Development";
    private const string Staging = "Staging";
    private const string Production = "Production";

    private static readonly string[] ValidEnvironments = [Development, Staging, Production];

    private static string? _environment;

    private static IConfigurationRoot? _configuration;

    private static string AppSettingsFile => GetFullPath("appsettings.json");

    public static string Environment
    {
        get
        {
            CheckIfInitialized();

            return _environment!;
        }
    }

    public static IConfigurationRoot Configuration
    {
        get
        {
            CheckIfInitialized();

            return _configuration!;
        }
    }

    public static bool IsProduction => Environment.Equals("production", StringComparison.OrdinalIgnoreCase);

    public static bool IsStaging => Environment.Equals("staging", StringComparison.OrdinalIgnoreCase);

    public static bool IsDevelopment => Environment.Equals("development", StringComparison.OrdinalIgnoreCase);

    public static IServiceCollection InitContext(this IServiceCollection services, params string[] additionalFiles)
    {
        _environment = GetEnvironmentFromAppSettingsFile();

        System.Environment.SetEnvironmentVariable(DotnetVariable, _environment);
        System.Environment.SetEnvironmentVariable(AspNetCoreVariable, _environment);

        IConfigurationBuilder builder = new ConfigurationBuilder()
                    .AddJsonFile(AppSettingsFile)
                    .AddJsonFile(GetEnvironmentFile(_environment));

        if (additionalFiles.Length > 0)
        {
            foreach (var file in additionalFiles)
            {
                builder.AddJsonFile(file);
            }
        }

        DeleteOtherSettingsFiles();

        _configuration = builder.Build();

        return services;
    }

    public static T? GetSection<T>(this IConfiguration configuration, string name)
    {
        return configuration.GetSection(name).Get<T>();
    }

    public static T? GetSection<T>(this IConfiguration configuration)
    {
        string? name = null;

        if (Attribute.IsDefined(typeof(T), typeof(ConfigurationNameAttribute)))
        {
            var attribute = Attribute.GetCustomAttribute(typeof(T), typeof(ConfigurationNameAttribute)) as ConfigurationNameAttribute;
            if (attribute is not null)
            {
                name = attribute.Name;
            }
        }
        if (string.IsNullOrWhiteSpace(name))
        {
            name = typeof(T).Name;
        }

        return configuration.GetSection<T>(name);
    }

    private static void CheckIfInitialized()
    {
        if (_configuration is null || _environment is null)
        {
            throw new Exception("Context has not been initialized yet, call services.InitContext()");
        }
    }

    private static string GetEnvironmentFromAppSettingsFile()
    {
        if (!File.Exists(AppSettingsFile))
        {
            throw new Exception($"App settings file not found: {AppSettingsFile}");
        }

        var jsonEnvironment = new ConfigurationBuilder()
                   .AddJsonFile(AppSettingsFile)
                   .Build()
                   .GetValue<string>("EnvironmentName")!;

        if (string.IsNullOrWhiteSpace(jsonEnvironment))
        {
            jsonEnvironment = Development;
        }
        if (!ValidEnvironments.Contains(jsonEnvironment))
        {
            throw new Exception($"Unknown environment name: {jsonEnvironment}");
        }

        return jsonEnvironment;
    }

    private static string GetEnvironmentFile(string environment)
    {
        return GetFullPath($"appsettings.{environment}.json");
    }

    private static string GetFullPath(string fileName) =>
        Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, fileName);

    private static void DeleteOtherSettingsFiles()
    {
        IEnumerable<string> otherFiles = ValidEnvironments
        .Except(new string[] { _environment! })
        .Select(env => GetEnvironmentFile(env));

        foreach (var file in otherFiles)
        {
            if (File.Exists(file))
            {
                try
                {
                    File.Delete(file);
                }
                catch
                {
                }
            }
        }
    }
}