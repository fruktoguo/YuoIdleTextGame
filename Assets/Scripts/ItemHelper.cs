using System.Collections.Generic;
using SimpleJSON;
using YuoTools;
using YuoTools.UI;

public class ItemHelper
{
    public static Dictionary<string, long> ItemLibrary = new();

    public static long GetItemNum(string item)
    {
        return ItemLibrary.GetValueOrDefault(item, 0);
    }

    public static bool Owned(string item)
    {
        return ItemLibrary.ContainsKey(item);
    }

    public static void AddItem(string item, long num)
    {
        ItemLibrary[item] = ItemLibrary.GetValueOrDefault(item, 0) + num;
        $"获得了 {num} 个 {item}".ViewLog();
    }

    public static void RemoveItem(string item, long num)
    {
        ItemLibrary[item] = ItemLibrary.GetValueOrDefault(item, 0) - num;
        $"失去了 {num} 个 {item}".ViewLog();
    }

    public static void Load()
    {
        var text = DataHelper.LoadPlayerSave("Item");
        JSONArray array = JSON.Parse(text) as JSONArray;
        if (array != null)
        {
            $"ItemLibrary 加载成功".Log();
            ItemLibrary.Clear();
            foreach (var (_, node) in array)
            {
                ItemLibrary[node[a.Name]] = node[a.Num].AsLong;
            }
        }
        else
        {
            $"ItemLibrary 没有数据".LogError();
            ItemLibrary = new();
        }
    }

    public static void Save()
    {
        var jsonArray = new JSONArray();
        foreach (var item in ItemLibrary)
        {
            var jsonObject = new JSONObject();
            jsonObject[a.Name] = item.Key;
            jsonObject[a.Num] = item.Value;
            jsonArray.Add(jsonObject);
        }

        DataHelper.SavePlayerSave("Item", jsonArray.ToString());
        $"ItemLibrary 保存成功".Log();
    }
}