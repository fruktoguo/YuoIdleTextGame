using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace YuoTools.Extend.AI
{
    public class DoubaoApi
    {
        public const string URL = "https://ark.cn-beijing.volces.com/api/v3/chat/completions";

        public static Task<string> GenerateText(string prompt)
        {
            var request = new ZhipuGLM.ChatCompletionRequest
            {
                model = AIHelper.AIModel,
                messages = new List<ZhipuGLM.ChatCompletionRequest.Message>
                {
                    new()
                    {
                        role = "user",
                        content = prompt
                    }
                },
                stream = false
            };

            return GenerateText(request);
        }

        public static async Task<string> GenerateChat(string prompt, AIHelper.AIChatModel chat)
        {
            chat.AddMessage("user", prompt);
            var request = new ZhipuGLM.ChatCompletionRequest(chat);
            var response = await GenerateText(request);
            chat.AddMessage("assistant", response);
            return response;
        }

        public static async Task<string> GenerateText(ZhipuGLM.ChatCompletionRequest request)
        {
            var body = JsonConvert.SerializeObject(request);
            var responseJson = await AIServerHelper.GetResponse(AIHelper.ApiKey, URL, body);

            if (string.IsNullOrEmpty(responseJson)) return "发生错误";
            var parsedJson = JsonConvert.DeserializeObject<ChatCompletionResponse>(responseJson);
            return parsedJson.choices[0].message.content;
        }

        public static IAsyncEnumerable<string> GenerateStream(string prompt)
        {
            var request = new ZhipuGLM.ChatCompletionRequest
            {
                model = AIHelper.AIModel,
                messages = new List<ZhipuGLM.ChatCompletionRequest.Message>
                {
                    new()
                    {
                        role = "user",
                        content = prompt
                    }
                },
                stream = true
            };

            return GenerateStream(request);
        }

        public static IAsyncEnumerable<string> GenerateChatStream(string prompt, AIHelper.AIChatModel chat)
        {
            chat.AddMessage("user", prompt);
            var request = new ZhipuGLM.ChatCompletionRequest(chat)
            {
                stream = true
            };
            return GenerateStream(request, r => chat.AddMessage("assistant", r));
        }

        public static async IAsyncEnumerable<string> GenerateStream(ZhipuGLM.ChatCompletionRequest request,
            UnityAction<string> onOver = null)
        {
            var body = JsonConvert.SerializeObject(request);

            string OnJson(ChatCompletionStreamResponse json)
            {
                return json.choices[0].delta.content;
            }

            string OnText(string text)
            {
                return OperateMessage(text, OnJson);
            }

            string result = "";
            await foreach (var line in AIServerHelper.GetResponseStream(AIHelper.ApiKey, URL, body, OnText))
            {
                if (line != null)
                {
                    result = line;
                    yield return line;
                }
            }

            onOver?.Invoke(result);
        }

        public static string OperateMessage(string str, Func<ChatCompletionStreamResponse, string> onJson)
        {
            string split = "data:";
            var lines = str.Split(new[] { split }, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder result = new StringBuilder();
            foreach (var line in lines)
            {
                if (line.Trim() == "[DONE]")
                {
                    break;
                }

                try
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        var trimmedLine = line.Trim();
                        if (trimmedLine.StartsWith("{") && trimmedLine.EndsWith("}"))
                        {
                            var json = JsonConvert.DeserializeObject<ChatCompletionStreamResponse>(trimmedLine);
                            if (json is { choices: { Count: > 0 } })
                            {
                                result.Append(onJson?.Invoke(json));
                            }
                        }
                    }
                }
                catch (JsonReaderException e)
                {
                    Debug.LogError($"JSON解析错误: {e.Message} - Line: {line}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"处理错误: {e.Message}");
                }
            }

            return result.ToString();
        }

        [Serializable]
        public class ChatCompletionRequest
        {
            /// <summary>
            /// 要调用的模型编码
            /// </summary>
            public string model;

            /// <summary>
            /// 调用语言模型时的当前对话消息列表
            /// </summary>
            public List<Message> messages;

            /// <summary>
            /// 是否使用流式输出
            /// </summary>
            public bool stream = false;

            [Serializable]
            public class Message
            {
                /// <summary>
                /// 消息的角色信息
                /// 可选值:system、user、assistant
                /// </summary>
                public string role;

                /// <summary>
                /// 消息内容
                /// </summary>
                public string content;
            }

            public ChatCompletionRequest()
            {
            }

            public AIHelper.AIChatModel ToChatModel()
            {
                var chatModel = new AIHelper.AIChatModel();
                foreach (var message in messages)
                {
                    chatModel.messages.Add(new()
                    {
                        role = message.role,
                        content = message.content
                    });
                }

                return chatModel;
            }

            public ChatCompletionRequest(AIHelper.AIChatModel chat)
            {
                model = AIHelper.AIModel;
                messages = new();
                foreach (var message in chat.messages)
                {
                    messages.Add(new()
                    {
                        role = message.role,
                        content = message.content
                    });
                }
            }
        }

        [Serializable]
        public class ChatCompletionResponse
        {
            public string id { get; set; }
            public string @object { get; set; }
            public long created { get; set; }
            public string model { get; set; }
            public List<Choice> choices { get; set; }
            public Usage usage { get; set; }
        }

        public class ChatCompletionStreamResponse
        {
            public string id { get; set; }
            public string @object { get; set; }
            public long created { get; set; }
            public string model { get; set; }
            public List<StreamChoice> choices { get; set; }
            public Usage usage { get; set; }
        }

        [Serializable]
        public class Choice
        {
            public int index { get; set; }
            public Message message { get; set; }
            public object logprobs { get; set; }
            public string finish_reason { get; set; }

            public override string ToString()
            {
                return message?.content;
            }
        }

        [Serializable]
        public class StreamChoice
        {
            public int index { get; set; }
            public Message delta { get; set; }
            public object logprobs { get; set; }
            public string finish_reason { get; set; }

            public override string ToString()
            {
                return delta?.content;
            }
        }

        [Serializable]
        public class Message
        {
            public string role { get; set; }
            public string content { get; set; }
        }

        [Serializable]
        public class Usage
        {
            public int prompt_tokens { get; set; }
            public int completion_tokens { get; set; }
            public int total_tokens { get; set; }
        }

        // [Serializable]
        // public class ChatCompletionRequest
        // {
        //     public string model { get; set; }
        //     public bool stream { get; set; }
        //
        //     public List<Message> messages { get; set; }
        //     // public int max_tokens { get; set; } = 4096;
        //     // public List<string> stop { get; set; } = new();
        //     // public float frequency_penalty { get; set; } = 0.0f;
        //     // public float presence_penalty { get; set; } = 0.0f;
        //     // public float temperature { get; set; } = 1f;
        //     // public float top_p { get; set; } = 0.7f;
        //     // public bool logprobs { get; set; } = false;
        //     // public int top_logprobs { get; set; } = 0;
        //     // public Dictionary<int, float> logit_bias { get; set; } = new();
        //
        //     public ChatCompletionRequest(string prompt)
        //     {
        //         model = AIHelper.AIModel;
        //         messages = new List<Message>
        //         {
        //             new() { role = "user", content = prompt }
        //         };
        //     }
        //
        //     public ChatCompletionRequest()
        //     {
        //     }
        //
        //     public AIHelper.AIChatModel ToChatModel()
        //     {
        //         var chatModel = new AIHelper.AIChatModel();
        //         foreach (var message in messages)
        //         {
        //             chatModel.messages.Add(new()
        //             {
        //                 role = message.role,
        //                 content = message.content
        //             });
        //         }
        //
        //         return chatModel;
        //     }
        //
        //     public ChatCompletionRequest(AIHelper.AIChatModel chat)
        //     {
        //         model = AIHelper.AIModel;
        //         messages = new();
        //         foreach (var message in chat.messages)
        //         {
        //             messages.Add(new()
        //             {
        //                 role = message.role,
        //                 content = message.content
        //             });
        //         }
        //     }
        // }
    }
}