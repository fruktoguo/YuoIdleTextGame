using SimpleJSON;
using Sirenix.OdinInspector;
using UnityEngine;
using YuoTools.Extend.Helper;

public class Test : MonoBehaviour
{
    [Button]
    public void GenUnClockJson()
    {
        JSONArray root = new JSONArray();
        JSONObject item1 = new JSONObject();
        JSONArray ask = new JSONArray();
        for (int i = 0; i < 3; i++)
        {
            var item = new JSONObject();
            item[N.Type] = N.Item;
            item[N.Item] = $"随机物品{i}";
            item[N.Num] = 1;
        }
        item1[N.Ask] = ask;
        
        root.Add(item1);
        
        FileHelper.WriteAllText(Application.dataPath+"/UnClock.json",root.ToString());
    }
}