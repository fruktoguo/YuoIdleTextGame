using System.Collections.Generic;

public class PropertyHelper
{
    public static Dictionary<string, float> Property = new();

    public static void Init()
    {
        Set("体力", 100);
    }

    public static void Set(string name, float value)
    {
        Property[name] = value;
    }

    public static float Get(string name)
    {
        return Property[name];
    }

    public static bool Has(string name)
    {
        return Property.ContainsKey(name);
    }

    public static bool TrySub(string name, float value)
    {
        if (Property.ContainsKey(name))
        {
            if (Property[name] >= value)
            {
                Property[name] -= value;
                return true;
            }
        }
        return false;
    }
    
    public static void Add(string name, float value)
    {
        if (!Property.TryAdd(name, value))
        {
            Property[name] += value;
        }
    }

    public static bool Check(string name, float value)
    {
        return Property.ContainsKey(name) && Property[name] >= value;
    }
}