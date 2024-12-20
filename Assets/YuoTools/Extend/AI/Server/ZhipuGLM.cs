using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;

namespace YuoTools.Extend.AI
{
    public class ZhipuGLM
    {
        public const string URL = "https://open.bigmodel.cn/api/paas/v4/chat/completions";

        public static Task<string> GenerateText(string prompt)
        {
            var request = new ChatCompletionRequest
            {
                model = AIHelper.AIModel,
                messages = new List<ChatCompletionRequest.Message>
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
            var request = new ChatCompletionRequest(chat);
            var response = await GenerateText(request);
            chat.AddMessage("assistant", response);
            return response;
        }

        public static async Task<string> GenerateText(ChatCompletionRequest request)
        {
            var body = JsonConvert.SerializeObject(request);
            var responseJson = await AIServerHelper.GetResponse(AIHelper.ApiKey, URL, body);

            if (string.IsNullOrEmpty(responseJson)) return "发生错误";
            // Debug.Log(responseJson);
            var parsedJson = JsonConvert.DeserializeObject<ChatCompletionResponse>(responseJson);
            return parsedJson.choices[0].message.content;
        }

        public static IAsyncEnumerable<string> GenerateStream(string prompt)
        {
            var request = new ChatCompletionRequest
            {
                model = AIHelper.AIModel,
                messages = new List<ChatCompletionRequest.Message>
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
            var request = new ChatCompletionRequest(chat)
            {
                stream = true
            };
            return GenerateStream(request, r => chat.AddMessage("assistant", r));
        }

        public static async IAsyncEnumerable<string> GenerateStream(ChatCompletionRequest request,
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
                                //处理新的返回结构
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
            /// 可选值: glm-4-plus、glm-4-0520、glm-4、glm-4-air、glm-4-airx、glm-4-long、glm-4-flashx、glm-4-flash
            /// </summary>
            public string model;

            /// <summary>
            /// 调用语言模型时的当前对话消息列表,以JSON数组形式提供
            /// 例如:{"role": "user", "content": "Hello"}
            /// 可能的消息类型包括系统消息、用户消息、助手消息和工具消息
            /// </summary>
            public List<Message> messages;

            /// <summary>
            /// 是否使用流式输出
            /// 当为false时,模型在生成所有内容后一次性返回所有内容
            /// 当为true时,模型将通过标准Event Stream逐块返回生成的内容
            /// 默认值:false
            /// </summary>
            public bool stream = false;

            [Serializable]
            public class Message
            {
                /// <summary>
                /// 消息的角色信息
                /// 可选值:system、user、assistant、tool
                /// </summary>
                public string role;

                /// <summary>
                /// 消息内容
                /// 当role为tool时,content为工具调用的返回结果
                /// </summary>
                public string content;

                /// <summary>
                /// 模型产生的工具调用消息
                /// 仅当role为assistant且命中函数调用时存在
                /// </summary>
                public List<ToolCall> tool_calls;

                /// <summary>
                /// 工具调用的记录
                /// 仅当role为tool时必需
                /// </summary>
                public string tool_call_id;
            }

            #region 非必须参数

            /// <summary>
            /// 由用户端传递的请求唯一标识符,需要唯一
            /// 用于区分每次请求
            /// 如果用户端未提供,平台将默认生成
            /// 长度要求:6-128个字符
            /// </summary>
            public string request_id;

            /// <summary>
            /// 用于控制模型选择调用哪个函数的方式
            /// 仅在工具类型为function时补充
            /// 默认值:auto,目前仅支持auto
            /// </summary>
            public string tool_choice;

            /// <summary>
            /// 当do_sample为true时,启用采样策略
            /// 当do_sample为false时,温度和top_p等采样策略参数将不生效
            /// 默认值:true
            /// </summary>
            public bool do_sample = true;

            /// <summary>
            /// 采样温度,控制输出的随机性
            /// 必须为正数,取值范围:[0.0, 1.0]
            /// 默认值:0.95
            /// </summary>
            public float temperature = 0.95f;

            /// <summary>
            /// 核采样方法
            /// 取值范围:[0.0, 1.0]
            /// 默认值:0.7
            /// </summary>
            public float top_p = 0.7f;

            /// <summary>
            /// 模型输出的最大token数
            /// 最大输出为4095
            /// 默认值:1024
            /// </summary>
            public int max_tokens = 4095;

            /// <summary>
            /// 模型遇到stop指定的字符时会停止生成
            /// 目前仅支持单个stop词,格式为["stop_word1"]
            /// </summary>
            public List<string> stop;

            /// <summary>
            /// 模型可以调用的工具列表
            /// 支持的工具类型:function、retrieval、web_search
            /// </summary>
            public List<Tool> tools;

            /// <summary>
            /// 终端用户的唯一ID
            /// 用于干预终端用户的非法活动、生成非法不当信息或其他滥用行为
            /// ID长度要求:6-128个字符
            /// </summary>
            public string user_id;

            [Serializable]
            public class ToolCall
            {
                /// <summary>
                /// 工具调用的唯一标识符
                /// </summary>
                public string id;

                /// <summary>
                /// 工具类型,目前仅支持'function'
                /// </summary>
                public string type;

                /// <summary>
                /// 函数调用信息
                /// </summary>
                public Function function;
            }

            [Serializable]
            public class Function
            {
                /// <summary>
                /// 函数名称
                /// </summary>
                public string name;

                /// <summary>
                /// 函数参数,JSON格式
                /// 注意:模型可能会生成无效的JSON,也可能会虚构一些不在函数规范中的参数
                /// 在调用函数之前,请验证这些参数是否有效
                /// </summary>
                public string arguments;
            }

            [Serializable]
            public class Tool
            {
                /// <summary>
                /// 工具类型:function、retrieval、web_search
                /// </summary>
                public string type;

                /// <summary>
                /// 函数定义,仅当type为function时补充
                /// </summary>
                public FunctionDefinition function;

                /// <summary>
                /// 检索工具定义,仅当type为retrieval时补充
                /// </summary>
                public RetrievalDefinition retrieval;

                /// <summary>
                /// 网络搜索工具定义,仅当type为web_search时补充
                /// 如果tools中存在type retrieval,则web_search将不生效
                /// </summary>
                public WebSearchDefinition web_search;
            }

            [Serializable]
            public class FunctionDefinition
            {
                /// <summary>
                /// 函数名称
                /// 只能包含a-z、A-Z、0-9、下划线和连字符
                /// 最大长度限制为64
                /// </summary>
                public string name;

                /// <summary>
                /// 用于描述函数的能力
                /// 模型将根据此描述确定函数调用的方式
                /// </summary>
                public string description;

                /// <summary>
                /// 函数参数定义
                /// 必须传递一个Json Schema对象,以准确定义函数接受的参数
                /// 如果调用函数时不需要参数,则可以省略此参数
                /// </summary>
                public ParametersDefinition parameters;
            }

            [Serializable]
            public class ParametersDefinition
            {
                /// <summary>
                /// 参数类型,通常为"object"
                /// </summary>
                public string type;

                /// <summary>
                /// 参数属性定义
                /// </summary>
                public Dictionary<string, PropertyDefinition> properties;

                /// <summary>
                /// 必需的参数列表
                /// </summary>
                public List<string> required;
            }

            [Serializable]
            public class PropertyDefinition
            {
                /// <summary>
                /// 属性类型
                /// </summary>
                public string type;

                /// <summary>
                /// 属性描述
                /// </summary>
                public string description;

                /// <summary>
                /// 枚举值列表(如果适用)
                /// </summary>
                public List<string> enum_values;
            }

            [Serializable]
            public class RetrievalDefinition
            {
                /// <summary>
                /// 知识库ID
                /// 涉及知识库ID时,请前往开放平台的知识库模块创建或获取
                /// </summary>
                public string knowledge_id;

                /// <summary>
                /// 请求模型时的知识库模板
                /// 默认模板:从文档 "{{ knowledge }}" 中查找问题的答案 "{{question}}" 
                /// 如果找到答案,仅使用文档的陈述来回答问题；
                /// 如果未找到,则使用自己的知识回答,并告知用户此信息不是来自文档。
                /// 不要重复问题,直接开始回答。
                /// </summary>
                public string prompt_template;
            }

            [Serializable]
            public class WebSearchDefinition
            {
                /// <summary>
                /// 是否启用网络搜索功能
                /// 默认为关闭状态(False)
                /// </summary>
                public bool enable;

                /// <summary>
                /// 强制自定义搜索键内容
                /// </summary>
                public string search_query;

                /// <summary>
                /// 是否获取网页搜索来源的详细信息
                /// 默认禁用
                /// </summary>
                public bool search_result;
            }

            #endregion

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
            /// <summary>
            /// 智谱AI开放平台生成的任务序号
            /// </summary>
            public string id;

            /// <summary>
            /// 请求ID
            /// </summary>
            public string request_id;

            /// <summary>
            /// 请求创建时间,为Unix时间戳,单位为秒
            /// </summary>
            public long created;

            /// <summary>
            /// 使用的模型名称
            /// </summary>
            public string model;

            /// <summary>
            /// 当前对话的模型输出内容
            /// </summary>
            public List<Choice> choices;

            /// <summary>
            /// token使用统计
            /// </summary>
            public Usage usage;

            /// <summary>
            /// 网页搜索结果(如果适用)
            /// </summary>
            public List<WebSearch> web_search;

            /// <summary>
            /// 任务状态(用于异步调用)
            /// 可能的值:PROCESSING(处理中)、SUCCESS(成功)、FAIL(失败)
            /// </summary>
            public string task_status;

            [Serializable]
            public class Choice
            {
                /// <summary>
                /// 结果索引
                /// </summary>
                public int index;

                /// <summary>
                /// 模型推理终止的原因
                /// stop: 自然结束或触发stop词
                /// tool_calls: 模型命中函数
                /// length: 达到token长度限制
                /// sensitive: 内容被安全审核接口拦截
                /// network_error: 模型推理异常
                /// </summary>
                public string finish_reason;

                /// <summary>
                /// 模型返回的消息
                /// </summary>
                public Message message;
            }

            [Serializable]
            public class Message
            {
                /// <summary>
                /// 消息角色,默认为assistant(模型)
                /// </summary>
                public string role;

                /// <summary>
                /// 消息内容
                /// 命中函数时为null,否则返回模型推理结果
                /// </summary>
                public string content;

                /// <summary>
                /// 工具调用信息
                /// </summary>
                public List<ToolCall> tool_calls;
            }

            [Serializable]
            public class ToolCall
            {
                /// <summary>
                /// 工具调用的唯一标识符
                /// </summary>
                public string id;

                /// <summary>
                /// 工具类型,目前仅支持'function'
                /// </summary>
                public string type;

                /// <summary>
                /// 函数调用信息
                /// </summary>
                public Function function;
            }

            [Serializable]
            public class Function
            {
                /// <summary>
                /// 函数名称
                /// </summary>
                public string name;

                /// <summary>
                /// 函数参数,JSON格式
                /// </summary>
                public string arguments;
            }

            [Serializable]
            public class Usage
            {
                /// <summary>
                /// 用户输入的token数量
                /// </summary>
                public int prompt_tokens;

                /// <summary>
                /// 模型输出的token数量
                /// </summary>
                public int completion_tokens;

                /// <summary>
                /// 总token数量
                /// </summary>
                public int total_tokens;
            }

            [Serializable]
            public class WebSearch
            {
                /// <summary>
                /// 来源网站的图标
                /// </summary>
                public string icon;

                /// <summary>
                /// 搜索结果的标题
                /// </summary>
                public string title;

                /// <summary>
                /// 搜索结果的网页链接
                /// </summary>
                public string link;

                /// <summary>
                /// 搜索结果网页的媒体来源名称
                /// </summary>
                public string media;

                /// <summary>
                /// 搜索结果网页引用的文本内容
                /// </summary>
                public string content;
            }
        }

        [Serializable]
        public class ChatCompletionStreamResponse
        {
            /// <summary>
            /// 智谱AI开放平台生成的任务序号
            /// </summary>
            public string id { get; set; }

            /// <summary>
            /// 请求ID
            /// </summary>
            public string request_id { get; set; }

            /// <summary>
            /// 请求创建时间,为Unix时间戳,单位为秒
            /// </summary>
            public long created { get; set; }

            /// <summary>
            /// 使用的模型名称
            /// </summary>
            public string model { get; set; }

            /// <summary>
            /// 当前对话的模型输出内容
            /// </summary>
            public List<Choice> choices { get; set; }

            /// <summary>
            /// token使用统计
            /// </summary>
            public Usage usage { get; set; }

            [Serializable]
            public class Choice
            {
                /// <summary>
                /// 结果索引
                /// </summary>
                public int index { get; set; }

                /// <summary>
                /// 模型增量返回的文本信息
                /// </summary>
                public Delta delta { get; set; }

                /// <summary>
                /// 模型推理终止的原因
                /// stop: 自然结束或触发stop词
                /// tool_calls: 模型命中函数
                /// length: 达到token长度限制
                /// sensitive: 内容被安全审核接口拦截
                /// network_error: 模型推理异常
                /// </summary>
                public string finish_reason { get; set; }
            }

            [Serializable]
            public class Delta
            {
                /// <summary>
                /// 当前对话角色,默认为assistant(模型)
                /// </summary>
                public string role { get; set; }

                /// <summary>
                /// 当前对话内容
                /// 命中函数时为null,否则返回模型推理结果
                /// </summary>
                public string content { get; set; }

                /// <summary>
                /// 工具调用信息
                /// </summary>
                public List<ToolCall> tool_calls { get; set; }
            }

            [Serializable]
            public class ToolCall
            {
                /// <summary>
                /// 工具调用的唯一标识符
                /// </summary>
                public string id { get; set; }

                /// <summary>
                /// 工具类型,目前仅支持'function'
                /// </summary>
                public string type { get; set; }

                /// <summary>
                /// 函数调用信息
                /// </summary>
                public Function function { get; set; }
            }

            [Serializable]
            public class Function
            {
                /// <summary>
                /// 函数名称
                /// </summary>
                public string name { get; set; }

                /// <summary>
                /// 函数参数,JSON格式
                /// </summary>
                public string arguments { get; set; }
            }

            [Serializable]
            public class Usage
            {
                /// <summary>
                /// 用户输入的token数量
                /// </summary>
                public int prompt_tokens { get; set; }

                /// <summary>
                /// 模型输出的token数量
                /// </summary>
                public int completion_tokens { get; set; }

                /// <summary>
                /// 总token数量
                /// </summary>
                public int total_tokens { get; set; }
            }
        }

        [Serializable]
        public class ChatCompletionStreamFinishResponse
        {
            /// <summary>
            /// 智谱AI开放平台生成的任务序号
            /// </summary>
            public string id { get; set; }

            /// <summary>
            /// 请求ID
            /// </summary>
            public string request_id { get; set; }

            /// <summary>
            /// 对象类型
            /// </summary>
            public string @object { get; set; }

            /// <summary>
            /// 请求创建时间,为Unix时间戳,单位为秒
            /// </summary>
            public long created { get; set; }

            /// <summary>
            /// 使用的模型名称
            /// </summary>
            public string model { get; set; }

            /// <summary>
            /// 当前对话的模型输出内容
            /// </summary>
            public List<Choice> choices { get; set; }

            /// <summary>
            /// token使用统计
            /// </summary>
            public Usage usage { get; set; }

            [Serializable]
            public class Choice
            {
                /// <summary>
                /// 结果索引
                /// </summary>
                public int index { get; set; }

                /// <summary>
                /// 模型推理终止的原因
                /// stop: 自然结束或触发stop词
                /// tool_calls: 模型命中函数
                /// length: 达到token长度限制
                /// sensitive: 内容被安全审核接口拦截
                /// network_error: 模型推理异常
                /// </summary>
                public string finish_reason { get; set; }
            }

            [Serializable]
            public class Usage
            {
                /// <summary>
                /// 用户输入的token数量
                /// </summary>
                public int prompt_tokens { get; set; }

                /// <summary>
                /// 模型输出的token数量
                /// </summary>
                public int completion_tokens { get; set; }

                /// <summary>
                /// 总token数量
                /// </summary>
                public int total_tokens { get; set; }
            }
        }
    }
}