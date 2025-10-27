using Microsoft.Extensions.Configuration;

namespace PM.Infrastructure.Configuration;

public static class DatabasePathResolver
{
    /// <summary>
    /// Resolves the absolute DB path based on:
    /// 1) DB_PATH env var; otherwise
    /// 2) "Database:RelativePath" in configuration (relative to the solution root).
    /// </summary>
    public static string ResolveAbsolutePath(IConfiguration configuration, string solutionRoot)
    {
        var envOverride = Environment.GetEnvironmentVariable("DB_PATH");
        if (!string.IsNullOrWhiteSpace(envOverride))
            return Path.GetFullPath(envOverride);

        var relative = configuration["Database:RelativePath"] ?? "db/portfolio.db";
        return Path.GetFullPath(Path.Combine(solutionRoot, relative));
    }

    public static string BuildSqliteConnectionString(string absolutePath)
        => $"Data Source={absolutePath}";

    public static string? TryFindSolutionRoot(string solutionFileName = "InvestmentPortfolio.sln")
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (current != null)
        {
            if (current.GetFiles(solutionFileName).Any())
                return current.FullName;
            current = current.Parent;
        }
        return null;
    }
}
