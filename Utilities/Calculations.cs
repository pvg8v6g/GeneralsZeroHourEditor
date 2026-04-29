namespace GeneralsZeroHourEditor.Utilities;

public class Calculations
{
    public static int GetNextId(int[] ids, int minimum = 1)
    {
        var min = minimum;
        var max = ids.Length > 0 ? ids.Max(x => x) : min;
        for (var i = min; i <= max + 1; i++)
        {
            if (ids.Contains(i)) continue;
            min = i;
            break;
        }

        return min;
    }
}
