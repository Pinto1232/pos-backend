using System;
using System.IO;

public static class DotEnv
{
    public static void Load(string? filePath = null)
    {
        var envPath = filePath ?? Path.Combine(Directory.GetCurrentDirectory(), ".env");

        if (!File.Exists(envPath))
        {
            Console.WriteLine($"Warning: .env file not found at {envPath}");
            return;
        }

        foreach (var line in File.ReadAllLines(envPath))
        {
            var parts = line.Split('=', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2)
                continue;

            var key = parts[0].Trim();
            var value = parts[1].Trim();

            // Don't override existing environment variables
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }
}
