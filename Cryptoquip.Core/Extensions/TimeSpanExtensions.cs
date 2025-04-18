namespace Cryptoquip.Extensions;

public static class TimeSpanExtensions
{
    public static string ReadableTime(this TimeSpan t)
    {
        var parts = new List<string>();
        Action<int, string> add = (val, unit) => { if (val > 0) parts.Add(val + unit); };

        add(t.Days, "d");
        add(t.Hours, "h");
        add(t.Minutes, "m");
        add(t.Seconds, "s");
        add(t.Milliseconds, "ms");

        if (!parts.Any()) parts.Add("0 ms");

        return string.Join(" ", parts);
    }	
}