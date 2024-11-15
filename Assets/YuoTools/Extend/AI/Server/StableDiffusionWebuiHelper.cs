#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace YuoTools.Extend.AI
{
    public partial class StableDiffusionWebuiHelper
    {
        private const string BaseUrl = "http://127.0.0.1:7860";

        private static readonly JsonSerializerSettings JsonSettings = new()
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        // 保持原有的 Draw 方法,但增加错误处理
        public static async Task<Texture2D> Draw(Text2ImgRequestDto data)
        {
            try
            {
                var response = await SendRequest<StableDiffusionResponse>("/sdapi/v1/txt2img", data);
                if (response?.images != null && response.images.Count > 0)
                {
                    var bytes = Convert.FromBase64String(response.images[0]);
                    var texture = new Texture2D((int)(data.width ?? 512), (int)(data.height ?? 512));
                    texture.LoadImage(bytes);
                    texture.Apply();
                    return texture;
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"SD WebUI Draw failed: {ex.Message}");
                return null;
            }
        }

        // 图生图方法
        public static async Task<Texture2D> DrawFromImage(Img2ImgRequestDto data)
        {
            try
            {
                var response = await SendRequest<StableDiffusionResponse>("/sdapi/v1/img2img", data);
                if (response?.images != null && response.images.Count > 0)
                {
                    var bytes = Convert.FromBase64String(response.images[0]);
                    var texture = new Texture2D((int)(data.width ?? 512), (int)(data.height ?? 512));
                    texture.LoadImage(bytes);
                    texture.Apply();
                    return texture;
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"SD WebUI Img2Img failed: {ex.Message}");
                return null;
            }
        }

        // 获取生成进度
        public static async Task<ProgressResponse> GetProgress()
        {
            return await SendRequest<ProgressResponse>("/sdapi/v1/progress", null, "GET");
        }

        // 获取可用模型列表
        public static async Task<List<SDModelItem>> GetModels()
        {
            return await SendRequest<List<SDModelItem>>("/sdapi/v1/sd-models", null, "GET");
        }

        // 切换模型
        public static async Task<bool> SwitchModel(string modelName)
        {
            var options = new SDOptions { sd_model_checkpoint = modelName };
            await SendRequest<object>("/sdapi/v1/options", options);
            return true;
        }

        // 获取当前系统选项
        public static async Task<SDOptions> GetOptions()
        {
            return await SendRequest<SDOptions>("/sdapi/v1/options", null, "GET");
        }

        // 获取采样器列表
        public static async Task<List<SamplerItem>> GetSamplers()
        {
            return await SendRequest<List<SamplerItem>>("/sdapi/v1/samplers", null, "GET");
        }

        // 中断生成
        public static async Task Interrupt()
        {
            await SendRequest<object>("/sdapi/v1/interrupt", null, "POST");
        }

        // 通用请求方法
        private static async Task<T> SendRequest<T>(string endpoint, object data = null, string method = "POST")
        {
            try
            {
                var url = BaseUrl + endpoint;
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = method;
                request.ContentType = "application/json";

                if (data != null && method != "GET")
                {
                    var jsonData = JsonConvert.SerializeObject(data, JsonSettings);
                    var byteArray = Encoding.UTF8.GetBytes(jsonData);
                    request.ContentLength = byteArray.Length;

                    using (var stream = await request.GetRequestStreamAsync())
                    {
                        await stream.WriteAsync(byteArray, 0, byteArray.Length);
                    }
                }

                using var response = await request.GetResponseAsync();
                using var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                var jsonResponse = await reader.ReadToEndAsync();
                return JsonConvert.DeserializeObject<T>(jsonResponse, JsonSettings);
            }
            catch (Exception ex)
            {
                Debug.LogError($"SD WebUI API request failed: {endpoint} - {ex.Message}");
                throw;
            }
        }

        // 工具方法：将Texture2D转换为Base64字符串
        public static string TextureToBase64(Texture2D texture)
        {
            byte[] imageData = texture.EncodeToPNG();
            return Convert.ToBase64String(imageData);
        }

        // 工具方法：检查服务是否可用
        public static async Task<bool> IsServiceAvailable()
        {
            try
            {
                await GetOptions();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // 简化的异步生成方法(保持原有的便捷方法)
        public static async Task<Texture2D> Draw(string parameters, string negativePrompt = "") =>
            await Draw(new Text2ImgRequestDto
            {
                prompt = parameters,
                negative_prompt = negativePrompt
            });
    }
}