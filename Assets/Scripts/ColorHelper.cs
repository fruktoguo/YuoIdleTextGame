using System;

public static class ColorHelper
{
    public static string Name = "#FFA500"; // 橙色
    public static string Num = "#ADD8E6"; // 淡蓝色

    public static string ColorizeName(this string text)
    {
        return text.Colorize(Name);
    }

    public static string ColorizeNum(this string text)
    {
        return text.Colorize(Num);
    }
    
    public static string Colorize(this string text, string color)
    {
        return $"<color={color}>{text}</color>";
    }
}