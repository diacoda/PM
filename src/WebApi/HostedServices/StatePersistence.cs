namespace PM.API.HostedServices;

using System.Text.Json;
/// <summary>
/// Handles persistence of the last successful run state.
/// </summary>
public class StatePersistence
{
    private readonly string _path;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="path"></param>
    public StatePersistence(string path)
    {
        _path = path;
    }

    /// <summary>
    /// Loads the date of the last successful run.
    /// </summary>
    /// <returns></returns>
    public DateOnly? LoadLastSuccessfulRun()
    {
        if (!File.Exists(_path)) return null;

        try
        {
            var json = File.ReadAllText(_path);
            return JsonSerializer.Deserialize<DateOnly>(json);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Saves the date of the last successful run.
    /// </summary>
    /// <param name="date"></param>
    public void SaveLastSuccessfulRun(DateOnly date)
    {
        var json = JsonSerializer.Serialize(date);
        File.WriteAllText(_path, json);
    }
}
