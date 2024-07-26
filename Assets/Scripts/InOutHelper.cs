using SimpleJSON;

public class InOutHelper
{
    public static bool HasIn(JSONNode node)
    {
        return node.HasKey(a.In);
    }

    public static bool TryCheckIn(JSONNode input, out string error)
    {
        error = "";
        bool condition = true;
        foreach (var (_, item) in input)
        {
            if (!ConditionHelper.Check(item, out var e))
            {
                error += e;
                condition = false;
            }
        }

        return condition;
    }

    public static void GenerateInList(JSONNode input)
    {
        foreach (var (_, item) in input)
        {
            GenerateIn(item);
        }
    }

    public static void GenerateIn(JSONNode item)
    {
        switch (item[a.Type].Value)
        {
            case a.Item:
                ItemHelper.RemoveItem(item[a.Name], item[a.Num].AsInt);
                break;
            case a.Property:
                PropertyHelper.TrySub(item[a.Name], item[a.Num].AsFloat);
                break;
        }
    }

    public static void GenerateOutList(JSONNode output)
    {
        foreach (var (_, item) in output)
        {
            GenerateOut(item);
        }
    }

    public static void GenerateOut(JSONNode item)
    {
        switch (item[a.Type].Value)
        {
            case a.Item:
                ItemHelper.AddItem(item[a.Name], item[a.Num].AsInt);
                break;
            case a.Property:
                PropertyHelper.Add(item[a.Name], item[a.Num].AsFloat);
                break;
        }
    }
}