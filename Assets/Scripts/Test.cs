using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using Unity.Sentis;
using UnityEngine;
using YuoTools.Extend.AI;

public class Test : MonoBehaviour
{
    [Button]
    public async void AI(string prompt)
    {
        var result = await AITool.Think(prompt);
    }
    [Button]
    public async void AI2(string prompt)
    {
        var result = await AIHelper.GenerateText(prompt);
    }
}

public static class AITool
{
    // 存储思考过程的列表
    private static List<string> thoughtProcess = new List<string>();

    // 修改后的AI方法，现在返回字符串结果
    public static async Task<string> AI(string prompt)
    {
        string result = await AIHelper.GenerateText(prompt);
        thoughtProcess.Add($"提示: {prompt}\n回答: {result}");
        return result;
    }

    // 新增的Think方法，用于模拟AI的思考过程
    public static async Task<string> Think(string initialPrompt, int maxSteps = 5)
    {
        thoughtProcess.Clear();
        string currentPrompt = initialPrompt;
        string finalResult = "";

        for (int i = 0; i < maxSteps; i++)
        {
            string result = await AI(currentPrompt);
            Debug.Log($"第{i}步 {result}");
            if (result.Contains("FINAL ANSWER:"))
            {
                finalResult = result.Substring(result.IndexOf("FINAL ANSWER:", StringComparison.Ordinal) + 13).Trim();
                break;
            }

            // 生成下一步的提示
            currentPrompt = $"基于之前的回答：\"{result}\"，继续思考。如果你认为已经得到了最终答案，请以'FINAL ANSWER:'开头给出答案。";
        }

        // 如果没有得到最终答案，使用最后一次的结果
        if (string.IsNullOrEmpty(finalResult))
        {
            finalResult = await AI("根据之前的思考过程，给出一个最终答案。");
        }

        return finalResult;
    }

    // 获取思考过程
    public static string GetThoughtProcess()
    {
        return string.Join("\n\n", thoughtProcess);
    }
}