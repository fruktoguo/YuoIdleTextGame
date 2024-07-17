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
                var num = node[a.Num].AsInt - ItemHelper.GetItemNum(name);
                if (num > 0)
                {
                    error = $"[{name}] 还差 [{num}] 个";
                }

                return num <= 0;
        }

        $"{type} 作为判断类型 不支持".Log();

        return false;
    }
}