using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace YuoTools.Extend.AI
{
    public class ZhipuCogView
    {
        public const string URL = "https://open.bigmodel.cn/api/paas/v4/images/generations";

        // 默认使用免费版本
        private static string Model => "cogview-3-flash";

        /// <summary>
        /// 生成图片并返回Texture2D
        /// </summary>
        /// <param name="prompt">图片描述</param>
        /// <param name="size">图片尺寸，默认1024x1024</param>
        /// <param name="maxRetries">最大重试次数</param>
        /// <returns>生成的图片Texture2D，失败返回null</returns>
        public static async Task<Texture2D> GenerateImage(string prompt, string size = "1024x1024", int maxRetries = 3)
        {
            string imageUrl = await GenerateImageUrl(prompt, size, maxRetries);
            if (string.IsNullOrEmpty(imageUrl))
                return null;

            return await DownloadTexture(imageUrl);
        }

        private static async Task<string> GenerateImageUrl(string prompt, string size = "1024x1024", int maxRetries = 3)
        {
            if (string.IsNullOrEmpty(AIHelper.ApiKey))
            {
                Debug.LogError("请输入ApiKey");
                return null;
            }

            var requestBody = new CogViewRequestModel
            {
                model = Model,
                prompt = prompt,
                size = size
            };

            string body = JsonConvert.SerializeObject(requestBody);
            Debug.Log($"Request Body: {body}");

            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    using UnityWebRequest webRequest = new UnityWebRequest(URL, "POST");
                    webRequest.SetRequestHeader("Content-Type", "application/json");
                    webRequest.SetRequestHeader("Authorization", $"Bearer {AIHelper.ApiKey}");

                    byte[] jsonToSend = new UTF8Encoding().GetBytes(body);
                    webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
                    webRequest.downloadHandler = new DownloadHandlerBuffer();

                    await webRequest.SendWebRequest();

                    if (webRequest.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError($"Error: {webRequest.error}");
                        Debug.LogError($"Response Code: {webRequest.responseCode}");
                        Debug.LogError($"Response Body: {webRequest.downloadHandler.text}");

                        if (attempt == maxRetries - 1)
                            return null;

                        await Task.Delay(1000 * (attempt + 1)); // 指数退避
                        continue;
                    }

                    string responseJson = webRequest.downloadHandler.text;
                    Debug.Log($"Response: {responseJson}");

                    var response = JsonConvert.DeserializeObject<CogViewResponseModel>(responseJson);
                    return response?.data[0]?.url;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Exception occurred: {ex.Message}");
                    if (attempt == maxRetries - 1)
                        return null;
                }
            }

            return null;
        }

        /// <summary>
        /// 从URL下载图片并转换为Texture2D
        /// </summary>
        private static async Task<Texture2D> DownloadTexture(string url)
        {
            try
            {
                using UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url);
                await webRequest.SendWebRequest();

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"下载图片失败: {webRequest.error}");
                    return null;
                }

                return DownloadHandlerTexture.GetContent(webRequest);
            }
            catch (Exception ex)
            {
                Debug.LogError($"下载图片时发生错误: {ex.Message}");
                return null;
            }
        }

        [Serializable]
        private class CogViewRequestModel
        {
            public string model;
            public string prompt;
            public string size;
        }

        [Serializable]
        private class CogViewResponseModel
        {
            public string created;
            public List<ImageData> data;

            [Serializable]
            public class ImageData
            {
                public string url;
            }
        }
    }
}