using System.Collections.Generic;
using SimpleJSON;

public class TagHelper
{
    public static HashSet<string> Tags = new();

    public static void Load(JSONArray array)
    {
        Tags = new();
        foreach (var (_, value) in array)
        {
            AddTag(value.Value);
        }
    }

    public static void AddTag(string tag) => Tags.Add(tag);
    
    public static void RemoveTag(string tag) => Tags.Remove(tag);

    public static bool HasTag(string tag) => Tags.Contains(tag);
}