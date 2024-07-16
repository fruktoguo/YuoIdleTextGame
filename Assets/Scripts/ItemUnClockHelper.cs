using SimpleJSON;

public class ItemUnClockHelper
{
    public static bool IsUnClock(JSONObject item)
    {
        JSONArray unClock = item[N.Ask] as JSONArray;
        if (unClock == null) return false;

        foreach (var (key, condition) in unClock)
        {
            if (!Check(condition)) return false;
        }

        return true;
    }

    public static bool Check(JSONNode node)
    {
        var type = node[N.Type].ToString();
        switch (type)
        {
            case N.Item:
                var askNum = node[N.Num].AsInt;
                var itemNum = ItemHelper.GetItemNum(node[N.Name]);
                return itemNum >= askNum;
            default:
                return true;
        }
    }
}