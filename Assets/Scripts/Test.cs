using System;
using System.Collections.Generic;
using SimpleJSON;
using Sirenix.OdinInspector;
using UnityEngine;
using YuoTools;
using YuoTools.Extend.Helper;

public class Test : MonoBehaviour
{
    public int num1;
    public int num2;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            GenUnClockJson();
        }
    }

    public void GenUnClockJson()
    {
        JSONArray root = new JSONArray();

        for (int i = 0; i < num1; i++)
        {
            AddItem(root, 1);
        }

        //计时
        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        for (int i = 0; i < num2; i++)
        {
            foreach (var (key, value) in root)
            {
                ItemUnClockHelper.IsUnClock(value as JSONObject);
            }
        }

        sw.Stop();

        Debug.Log($"原生耗时：{sw.ElapsedMilliseconds}ms");

        //委托
        sw.Reset();
        Dictionary<string, BoolAction> dic = new Dictionary<string, BoolAction>();
        foreach (var (_, value) in root)
        {
            JSONArray unClock = value[a.Ask] as JSONArray;
            if (unClock == null) continue;
            List<(string, int)> list = new List<(string, int)>();
            foreach (var (_, condition) in unClock)
            {
                var (item, num) = (condition[a.Name].ToString(), condition[a.Num].AsInt);
                list.Add((item, num));
            }

            dic.Add(value[a.Name], () =>
            {
                foreach (var (item, num) in list)
                {
                    if (ItemHelper.GetItemNum(item) < num) return false;
                }

                return true;
            });
        }
        
        sw.Start();

        for (int i = 0; i < num2; i++)
        {
            foreach (var (key, value) in dic)
            {
                if (value())
                {
                    
                }
            }
        }
        sw.Stop();
        
        Debug.Log($"委托耗时：{sw.ElapsedMilliseconds}ms");
    }

    private int index;
    void AddItem(JSONArray root, int num)
    {
        JSONNode item1 = new JSONObject();
        item1[a.Name] = $"测试物品{index++}";
        JSONArray ask = new JSONArray();
        for (int i = 0; i < num; i++)
        {
            var item = new JSONObject
            {
                [a.Type] = a.Item,
                [a.Item] = $"随机物品{i}",
                [a.Num] = 1
            };
            ask.Add(item);
        }

        item1[a.Ask] = ask;
        root.Add(item1);
    }
}