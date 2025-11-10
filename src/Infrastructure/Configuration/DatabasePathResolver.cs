using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace PM.Infrastructure.Configuration;

public static class DatabasePathResolver
{
    /// <summary>
    /// Resolves the absolute DB path for the given dbType (e.g. "portfolio", "cashFlow", "valuation").
    /// Priority:
    /// 1) Env var "DB_PATH_{dbType}" (explicit override)
    /// 2) Configuration key "Database:RelativePath:{dbType}" relative to the solution root
    /// 3) Throws if solution root cannot be found (avoids accidental bin/... DB files)
    /// </summary>
    public static string ResolveAbsolutePath(string dbType, IConfiguration configuration, IHostEnvironment? env = null)
    {
        var perDbEnv = Environment.GetEnvironmentVariable($"DB_PATH_{dbType.ToUpperInvariant()}");
        if (!string.IsNullOrWhiteSpace(perDbEnv))
            return Path.GetFullPath(perDbEnv);

        string solutionRoot = TryFindSolutionRoot() ?? throw new InvalidOperationException("Could not locate solution root (.sln). Set DB_PATH_{DBTYPE} environment variable if running outside the repo.");

        if (env != null && env.IsEnvironment("E2ETests"))
        {
            var relativee2e = configuration[$"Database:RelativePath:E2ETests:{dbType}"] ?? $"dbe2e/{dbType}.db";
            var absolutee2e = Path.GetFullPath(Path.Combine(solutionRoot, relativee2e));
            Directory.CreateDirectory(Path.GetDirectoryName(absolutee2e)!);
            return absolutee2e;
        }

        string relative = configuration[$"Database:RelativePath:{dbType}"] ?? $"db/{dbType}.db";
        var absolute = Path.GetFullPath(Path.Combine(solutionRoot, relative));
        Directory.CreateDirectory(Path.GetDirectoryName(absolute)!);
        return absolute;
    }

    public static string BuildSqliteConnectionString(string absolutePath)
        => $"Data Source={absolutePath}";

    private static string? TryFindSolutionRoot()
    {
        // Try two starting points: current working directory and AppContext.BaseDirectory
        var startPoints = new[]
        {
            new DirectoryInfo(Directory.GetCurrentDirectory()),
            new DirectoryInfo(AppContext.BaseDirectory)
        };

        foreach (var start in startPoints)
        {
            var root = WalkUpForSolution(start);
            if (root != null)
                return root;
        }

        // As a last resort try some relative heuristics (optional)
        var maybeRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../"));
        if (Directory.Exists(maybeRoot) && Directory.GetFiles(maybeRoot, "*.sln").Any())
            return maybeRoot;

        return null;
    }

    private static string? WalkUpForSolution(DirectoryInfo? start)
    {
        var current = start;
        while (current != null)
        {
            if (current.GetFiles("*.sln").Any())
                return current.FullName;
            current = current.Parent;
        }
        return null;
    }
}
