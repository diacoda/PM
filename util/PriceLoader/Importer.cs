using System.Globalization;
using PM.Application.Interfaces;
using PM.DTO;
using PM.DTO.Prices;
using PM.SharedKernel;

public class SimpleCsvPriceImporter
{
    private readonly IPriceService _priceService;
    private readonly IMarketCalendar _calendar;

    public SimpleCsvPriceImporter(
        IPriceService priceService,
        IMarketCalendar calendar)
    {
        _priceService = priceService;
        _calendar = calendar;
    }

    public async Task ImportAsync(string filePath)
    {
        var lines = await File.ReadAllLinesAsync(filePath);

        if (lines.Length < 2)
            throw new InvalidOperationException("CSV must contain a header and at least one data row.");

        // HEADER LINE:   ,VFV,VCE,HXQ,...
        string[] headerColumns = SplitCsvLine(lines[0]);

        // Skip index 0 because it's empty (date column)
        var symbols = headerColumns.Skip(1).ToList();

        Console.WriteLine("Detected symbols:");
        Console.WriteLine(string.Join(", ", symbols));

        // Process rows
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] cols = SplitCsvLine(line);

            if (cols.Length < 2)
                continue;

            // Parse date from column 0: "Nov 14, 2025"
            string rawDate = cols[0].Trim('"');

            if (!TryParseDate(rawDate, out var date))
            {
                Console.WriteLine($"Bad date '{rawDate}' on line {i + 1}");
                continue;
            }

            // Skip if market closed
            if (!_calendar.IsMarketOpen(date))
            {
                Console.WriteLine($"Skipping {date} — market closed.");
                continue;
            }

            // Parse each symbol price
            for (int colIndex = 1; colIndex < cols.Length; colIndex++)
            {
                string symbol = symbols[colIndex - 1];
                string rawPrice = cols[colIndex];

                if (string.IsNullOrWhiteSpace(rawPrice))
                    continue;

                if (!decimal.TryParse(rawPrice, NumberStyles.Any, CultureInfo.InvariantCulture, out var price))
                {
                    Console.WriteLine($"Invalid price '{rawPrice}' for {symbol} on {date}");
                    continue;
                }

                try
                {
                    var request = new UpdatePriceRequest
                    {
                        Date = date,
                        Close = price
                    };
                    CancellationToken ct = new CancellationToken();
                    await _priceService.UpdatePriceAsync(symbol, request, ct);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating {symbol} on {date}: {ex.Message}");
                }
            }
        }

        Console.WriteLine("CSV import completed.");
    }

    private static bool TryParseDate(string rawDate, out DateOnly date)
    {
        date = default;

        if (string.IsNullOrWhiteSpace(rawDate))
            return false;

        // Clean up quotes and trim
        rawDate = rawDate.Trim();
        if (rawDate.Length >= 2 && rawDate[0] == '"' && rawDate[^1] == '"')
            rawDate = rawDate.Substring(1, rawDate.Length - 2).Trim();

        // Common formats to accept (single-digit day and two-digit day, short and long month names)
        var formats = new[]
        {
        "MMM d, yyyy",
        "MMM dd, yyyy",
        "MMMM d, yyyy",
        "MMMM dd, yyyy"
    };

        // Try DateOnly exact parse (supports an array of formats)
        // If your target framework doesn't support DateOnly.TryParseExact(string[], ...), use DateTime.TryParseExact and convert.
        if (DateOnly.TryParseExact(rawDate, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            return true;

        // Fallback: try parsing as DateTime with invariant culture and common styles, then convert
        if (DateTime.TryParse(rawDate, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var dt))
        {
            date = DateOnly.FromDateTime(dt);
            return true;
        }

        // Last-ditch: try current culture (in case the CSV used a different month name)
        if (DateTime.TryParse(rawDate, CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces, out dt))
        {
            date = DateOnly.FromDateTime(dt);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Minimal CSV splitter that handles:
    /// - quoted fields
    /// - commas inside quotes
    /// </summary>
    private static string[] SplitCsvLine(string line)
    {
        var result = new List<string>();
        bool inQuotes = false;
        var current = new System.Text.StringBuilder();

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (c == ',' && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
                continue;
            }

            current.Append(c);
        }

        result.Add(current.ToString());
        return result.ToArray();
    }
}
