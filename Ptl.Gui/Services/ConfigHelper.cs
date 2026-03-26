using System.Text.Json;

public static class ConfigHelper
{
    public static string GetConfigPath()
    {
        string apiPath = @"..\..\..\..\WebApplication1\bin\Debug\net8.0\Ptl.Api.exe";
        var apiDir = Path.GetDirectoryName(apiPath);
        return Path.Combine(apiDir!, "appsettings.json");
    }

    public static AppSettings Load()
    {
        var path = GetConfigPath();

        if (!File.Exists(path))
            throw new Exception($"Config not found: {path}");

        var json = File.ReadAllText(path);

        var config = JsonSerializer.Deserialize<AppSettings>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (config == null)
        {
            throw new Exception("Failed to deserialize config");
        }

        return config;
    }

    public static void Save(AppSettings config)
    {
        var path = GetConfigPath();

        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(path, json);
    }
}