using System.Collections.Generic;
using YuoTools;

public class ItemHelper
{
    public static Dictionary<string, long> ItemLibrary = new();

    public static long GetItemNum(string item)
    {
        return ItemLibrary.GetValueOrDefault(item, 0);
    }

    public static void AddItem(string item, long num)
    {
        ItemLibrary[item] = ItemLibrary.GetValueOrDefault(item, 0) + num;
        $"获得了 {num} 个 {item}".Log();
    }

    public static void RemoveItem(string item, long num)
    {
        ItemLibrary[item] = ItemLibrary.GetValueOrDefault(item, 0) - num;
        $"失去了 {num} 个 {item}".Log();
    }
}