namespace Identity;

using System;

public static class Extensions
{
    public static string ToSecondsString(this TimeSpan timeSpan)
    {
        return ((long)timeSpan.TotalSeconds).ToString();
    }
}