using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using YuoTools.Extend.Helper;

namespace YuoTools.Extend.AI
{
    public class ZhipuGLM4V
    {
        public const string URL = "https://open.bigmodel.cn/api/paas/v4/chat/completions";

        public static string ApiKey = "";
        
        // private static string Model => "glm-4v";
        private static string Model => "glm-4v-plus";

        public static async Task<string> RecognizeImageFromUrl(string imageUrl, string prompt = "请描述这个图片",
            int maxRetries = 3)
        {
            return await SendRecognitionRequest(new GLM4VRequestModel(Model, imageUrl, prompt), maxRetries);
        }

        public static async Task<string> RecognizeImageFromBytes(byte[] imageData, string prompt = "请描述这个图片",
            int maxRetries = 3)
        {
            string base64Image = Convert.ToBase64String(imageData);
            return await SendRecognitionRequest(new GLM4VRequestModel(Model, base64Image, true, prompt), maxRetries);
        }

        public static async Task<string> RecognizeImageFromTexture(Texture2D texture, string prompt = "请描述这个图片",
            int maxRetries = 3)
        {
            byte[] imageData = GetBytesFromTexture2D(texture);
            return await RecognizeImageFromBytes(imageData, prompt, maxRetries);
        }

        private static async Task<string> SendRecognitionRequest(GLM4VRequestModel requestBody, int maxRetries)
        {
            if (string.IsNullOrEmpty(ApiKey))
            {
                Debug.LogError("请输入ApiKey");
                return null;
            }

            string body = JsonConvert.SerializeObject(requestBody);
            Debug.Log($"Request Body: {body}");

            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    using UnityWebRequest webRequest = new UnityWebRequest(URL, "POST");
                    webRequest.SetRequestHeader("Content-Type", "application/json");
                    webRequest.SetRequestHeader("Authorization", $"Bearer {ApiKey}");

                    byte[] jsonToSend = new UTF8Encoding().GetBytes(body);
                    webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
                    webRequest.downloadHandler = new DownloadHandlerBuffer();

                    await webRequest.SendWebRequest().GetEtAwaiter();

                    if (webRequest.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError($"Error: {webRequest.error}");
                        Debug.LogError($"Response Code: {webRequest.responseCode}");
                        Debug.LogError($"Response Body: {webRequest.downloadHandler.text}");

                        if (attempt == maxRetries - 1)
                        {
                            return null;
                        }

                        await Task.Delay(1000 * (attempt + 1)); // 指数退避
                        continue;
                    }

                    string responseJson = webRequest.downloadHandler.text;
                    Debug.Log($"Response: {responseJson}");

                    var response = JsonConvert.DeserializeObject<GLM4VResponseModel>(responseJson);
                    return response?.choices[0]?.message.content;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Exception occurred: {ex.Message}");
                    if (attempt == maxRetries - 1)
                    {
                        return null;
                    }
                }
            }

            return null;
        }

        // 辅助方法：从文件路径加载图片并转换为byte[]
        public static byte[] LoadImageFromFile(string filePath)
        {
            return File.ReadAllBytes(filePath);
        }

        // 辅助方法：从Texture2D获取byte[]
        public static byte[] GetBytesFromTexture2D(Texture2D texture)
        {
            return texture.EncodeToPNG();
        }

        [Serializable]
        public class GLM4VRequestModel
        {
            public string model;
            public List<Message> messages;

            public GLM4VRequestModel(string glmV, string imageSource, bool isBase64 = false, string prompt = "请描述这个图片")
            {
                model = glmV;
                messages = new List<Message>
                {
                    new Message
                    {
                        role = "user",
                        content = new List<Content>
                        {
                            new Content
                            {
                                type = "image_url",
                                image_url = new ImageUrl
                                {
                                    url = isBase64 ? $"data:image/png;base64,{imageSource}" : imageSource
                                }
                            },
                            new Content
                            {
                                type = "text",
                                text = prompt
                            }
                        }
                    }
                };
            }

            public GLM4VRequestModel(string glmV, string imageUrl, string prompt)
            {
                model = glmV;
                messages = new List<Message>
                {
                    new Message
                    {
                        role = "user",
                        content = new List<Content>
                        {
                            new Content
                            {
                                type = "image_url",
                                image_url = new ImageUrl
                                {
                                    url = imageUrl
                                }
                            },
                            new Content
                            {
                                type = "text",
                                text = prompt
                            }
                        }
                    }
                };
            }

            [Serializable]
            public class Message
            {
                public string role;
                public List<Content> content;
            }

            [Serializable]
            public class Content
            {
                public string type;
                public string text;
                public ImageUrl image_url;
            }

            [Serializable]
            public class ImageUrl
            {
                public string url;
            }
        }

        [Serializable]
        public class GLM4VResponseModel
        {
            public string id;
            public long created;
            public string model;
            public List<Choice> choices;
            public Usage usage;

            [Serializable]
            public class Choice
            {
                public int index;
                public string finish_reason;
                public Message message;
            }

            [Serializable]
            public class Message
            {
                public string role;
                public string content;
            }

            [Serializable]
            public class Usage
            {
                public int prompt_tokens;
                public int completion_tokens;
                public int total_tokens;
            }
        }
    }
}