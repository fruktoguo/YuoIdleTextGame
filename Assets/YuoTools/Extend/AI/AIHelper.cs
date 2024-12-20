using System.Collections.Generic;
using System.Threading.Tasks;

namespace YuoTools.Extend.AI
{
    public static class AIHelper
    {
        public static string ApiKey => YuoToolsSettingsHelper.GetOrCreateSettings().AISetting.APIKey;
        public static string AIModel => YuoToolsSettingsHelper.GetOrCreateSettings().AISetting.Model;

        public static async Task<string> GenerateText(string prompt)
        {
            switch (YuoToolsSettingsHelper.GetOrCreateSettings().AISetting.Server)
            {
                // case "gemini":
                //     return await GeminiApi.GenerateText(prompt);
                case AIServerType.豆包:
                    return await DoubaoApi.GenerateText(prompt);
                case AIServerType.智谱:
                    return await ZhipuGLM.GenerateText(prompt);
                default:
                    return "没有配置正确的服务商";
            }
        }

        public static async Task<string> GenerateChat(string prompt, AIChatModel chat)
        {
            switch (YuoToolsSettingsHelper.GetOrCreateSettings().AISetting.Server)
            {
                // case "gemini":
                //     return await GeminiApi.GenerateText(prompt);
                // case AIServerType.豆包:
                //     return await DoubaoApi.GenerateText(prompt);
                case AIServerType.智谱:
                    return await ZhipuGLM.GenerateChat(prompt, chat);
                default:
                    return "没有配置正确的服务商";
            }
        }

        public static async IAsyncEnumerable<string> GenerateTextStream(string prompt)
        {
            switch (YuoToolsSettingsHelper.GetOrCreateSettings().AISetting.Server)
            {
                // case "gemini":
                //     await foreach (var line in GeminiApi.GenerateStream(prompt))
                //     {
                //         yield return line;
                //     }
                //
                //     break;
                case AIServerType.豆包:
                    await foreach (var line in DoubaoApi.GenerateStream(prompt))
                    {
                        yield return line;
                    }

                    break;

                case AIServerType.智谱:
                    await foreach (var line in ZhipuGLM.GenerateStream(prompt))
                    {
                        yield return line;
                    }

                    break;
                default:
                    yield return "没有配置正确的服务商";
                    break;
            }
        }

        public class AIChatModel
        {
            public List<AIChatMessageModel> messages = new();

            public void AddMessage(string role, string content)
            {
                messages.Add(new AIChatMessageModel()
                {
                    role = role,
                    content = content
                });
            }
        }

        public class AIChatMessageModel
        {
            public string role;
            public string content;
        }
    }
}