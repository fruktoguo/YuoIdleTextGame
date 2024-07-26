using SimpleJSON;

public class UnClockHelper
{
    public static bool IsUnClock(JSONObject item)
    {
        JSONArray unClock = item[a.Ask] as JSONArray;
        if (unClock == null) return false;

        foreach (var (key, condition) in unClock)
        {
            if (!Check(condition)) return false;
        }

        return true;
    }

    public static bool Check(JSONNode node)
    {
        var type = node[a.Type].ToString();
        switch (type)
        {
            case a.Item:
                var askNum = node[a.Num].AsInt;
                var itemNum = ItemHelper.GetItemNum(node[a.Name]);
                return itemNum >= askNum;
            case a.ItemOwned:
                return ItemHelper.Owned(node[a.Name]);
            default:
                return true;
        }
    }

    public static bool CheckItem(string itemName, int num)
    {
        return ItemHelper.GetItemNum(itemName) >= num;
    }
}