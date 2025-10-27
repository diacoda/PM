using Microsoft.Extensions.Configuration;

namespace PM.Infrastructure.Configuration;

public static class DatabasePathResolver
{
    /// <summary>
    /// Resolves the absolute DB path based on:
    /// 1) DB_PATH environment variable
    /// 2) Database:RelativePath in configuration (relative to solution root)
    /// 3) Fallback to AppContext.BaseDirectory (for EF CLI)
    /// </summary>
    public static string ResolveAbsolutePath(IConfiguration configuration)
    {
        // 1️⃣ Environment override
        var envOverride = Environment.GetEnvironmentVariable("DB_PATH");
        if (!string.IsNullOrWhiteSpace(envOverride))
            return Path.GetFullPath(envOverride);

        // 2️⃣ Solution root detection
        var solutionRoot = TryFindSolutionRoot() ?? AppContext.BaseDirectory;

        // 3️⃣ Relative path from config
        var relative = configuration["Database:RelativePath"] ?? "db/portfolio.db";
        var absolute = Path.GetFullPath(Path.Combine(solutionRoot, relative));

        Directory.CreateDirectory(Path.GetDirectoryName(absolute)!);
        return absolute;
    }

    public static string BuildSqliteConnectionString(string absolutePath)
        => $"Data Source={absolutePath}";

    private static string? TryFindSolutionRoot()
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (current != null)
        {
            if (current.GetFiles("*.sln").Any())
                return current.FullName;
            current = current.Parent;
        }

        // Fallback for EF CLI
        var maybeRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../"));
        if (Directory.GetFiles(maybeRoot, "*.sln").Any())
            return maybeRoot;

        return null;
    }
}
