using UnityEngine;

public static class MapBuilderUtil
{
    public static bool TryParseIndex(string name, out int x, out int y)
    {
        x = y = 0;
        
        var start = name.LastIndexOf('[');
        var end = name.LastIndexOf(']');
        if (start == -1 || end == -1) return false;

        var parts = name.Substring(start + 1, end - start - 1).Split(',');
        return parts.Length == 2
               && int.TryParse(parts[0], out x)
               && int.TryParse(parts[1], out y);
    }
}
