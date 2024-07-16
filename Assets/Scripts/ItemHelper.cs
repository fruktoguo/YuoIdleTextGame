    using System.Collections.Generic;

    public class ItemHelper
    {
        public static Dictionary<string, long> ItemLibrary = new();
        
        public static long GetItemNum(string item)
        {
            return ItemLibrary.GetValueOrDefault(item, 0);
        }
    }
