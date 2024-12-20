using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using YuoTools.Extend.AI;

public class ThoughtStep
{
    public int StepNumber { get; set; }
    public string Question { get; set; }
    public string Answer { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsFinalAnswer { get; set; }
}

public static class AITool
{
    private static List<ThoughtStep> thoughtSteps = new();
    public static AIHelper.AIChatModel chat = new();

    public static async Task<string> Think(string initialPrompt, int maxSteps = 5)
    {
        thoughtSteps.Clear();
        string finalResult = "";

        // 第一步：理解问题
        var firstPrompt = $@"我需要你帮我思考这个问题：'{initialPrompt}'
请按照以下步骤回答：
1. 简单重述问题
2. 确定需要思考的关键点
3. 提出下一步需要思考的具体问题

如果你已经可以直接得出答案，请以'FINAL ANSWER:'开头给出答案。
否则，请以'NEXT QUESTION:'开头提出下一个需要思考的问题。";

        var currentStep = await ExecuteThinkingStep(firstPrompt, 1);

        if (currentStep.IsFinalAnswer)
        {
            return ExtractFinalAnswer(currentStep.Answer);
        }

        // 后续步骤：深入思考
        for (int i = 2; i <= maxSteps; i++)
        {
            var nextQuestion = ExtractNextQuestion(currentStep.Answer);
            var prompt = $@"基于之前的思考过程，我们现在需要回答这个问题：'{nextQuestion}'

请按照以下步骤思考：
1. 分析这个具体问题
2. 给出你的推理过程
3. 得出这一步的结论

如果你认为已经可以回答最初的问题（{initialPrompt}），请以'FINAL ANSWER:'开头给出最终答案。
否则，请以'NEXT QUESTION:'开头提出下一个需要思考的问题。";

            currentStep = await ExecuteThinkingStep(prompt, i);

            if (currentStep.IsFinalAnswer)
            {
                return ExtractFinalAnswer(currentStep.Answer);
            }
        }

        // 如果达到最大步骤还没有结论，则强制生成结论
        var finalPrompt = $@"基于以下的思考过程，请对最初的问题'{initialPrompt}'给出一个最终的答案：

{GetThoughtProcess()}

请直接以'FINAL ANSWER:'开头给出最终答案。";

        var finalStep = await ExecuteThinkingStep(finalPrompt, maxSteps + 1);
        return ExtractFinalAnswer(finalStep.Answer);
    }

    private static async Task<ThoughtStep> ExecuteThinkingStep(string prompt, int stepNumber)
    {
        var step = new ThoughtStep
        {
            StepNumber = stepNumber,
            Question = prompt,
            Timestamp = DateTime.Now
        };

        step.Answer = await AIHelper.GenerateChat(prompt, chat);
        step.IsFinalAnswer = step.Answer.Contains("FINAL ANSWER:");

        thoughtSteps.Add(step);
        Debug.Log($"Step {stepNumber}:\n{step.Answer}");

        return step;
    }

    private static string ExtractFinalAnswer(string response)
    {
        var index = response.IndexOf("FINAL ANSWER:", StringComparison.OrdinalIgnoreCase);
        return index >= 0 ? response.Substring(index + 13).Trim() : response.Trim();
    }

    private static string ExtractNextQuestion(string response)
    {
        var index = response.IndexOf("NEXT QUESTION:", StringComparison.OrdinalIgnoreCase);
        return index >= 0 ? response.Substring(index + 14).Trim() : "";
    }

    public static string GetThoughtProcess()
    {
        var process = new System.Text.StringBuilder();
        foreach (var step in thoughtSteps)
        {
            process.AppendLine($"步骤 {step.StepNumber}:");
            process.AppendLine($"问题：{step.Question}");
            process.AppendLine($"回答：{step.Answer}");
            process.AppendLine("-------------------");
        }

        return process.ToString();
    }
}