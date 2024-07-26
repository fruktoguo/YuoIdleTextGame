using System;
using SimpleJSON;
using YuoTools;

public static class ConditionHelper
{
    public static bool Check(JSONNode node, out string error)
    {
        error = String.Empty;
        var type = node[a.Type].Value;
        var name = node[a.Name].Value;

        switch (type)
        {
            case a.Item:
                var itemNum = node[a.Num].AsInt - ItemHelper.GetItemNum(name);
                if (itemNum > 0)
                {
                    error = $"[{name.ColorizeName()}] 不足 还差 [{itemNum.ToString().ColorizeNum()}] 个";
                }

                return itemNum <= 0;

            case a.Property:
                var propertyNum = node[a.Num].AsFloat - PropertyHelper.Get(name);
                if (propertyNum > 0)
                {
                    error = $"[{name.ColorizeName()}] 不足 还差 [{propertyNum.ToString("f0").ColorizeNum()}] 点";
                }

                return propertyNum <= 0;
        }

        $"{type} 作为判断类型 不支持".Log();

        return false;
    }
}