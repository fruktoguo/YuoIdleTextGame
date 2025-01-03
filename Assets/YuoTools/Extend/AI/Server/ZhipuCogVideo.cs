using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UnityEngine.UI;

namespace YuoTools.Extend.AI
{
    /// <summary>
    /// 智谱AI视频生成工具
    /// </summary>
    public static class ZhipuCogVideo
    {
        private const string GENERATION_URL = "https://open.bigmodel.cn/api/paas/v4/videos/generations";
        private const string QUERY_URL = "https://open.bigmodel.cn/api/paas/v4/async-result/{0}";

        /// <summary>
        /// 生成视频并返回VideoPlayer
        /// </summary>
        public static async Task<VideoPlayer> GenerateVideo(
            string prompt,
            string imageUrl = null,
            string quality = "quality",
            bool withAudio = false,
            string size = "1920x1080",
            int duration = 5,
            int fps = 30)
        {
            if (string.IsNullOrEmpty(AIHelper.ApiKey))
            {
                Debug.LogError("请先设置ApiKey");
                return null;
            }

            // 1. 发起生成请求
            var taskId = await RequestVideoGeneration(new VideoGenerationRequest
            {
                model = "cogvideox",
                prompt = prompt,
                image_url = imageUrl,
                quality = quality,
                with_audio = withAudio,
                size = size,
                duration = duration,
                fps = fps
            });

            if (string.IsNullOrEmpty(taskId))
                return null;

            // 2. 轮询获取结果
            var result = await PollVideoResult(taskId, 60);
            if (result?.video_result == null || result.video_result.Count == 0)
                return null;

            // 3. 下载并设置视频
            return await DownloadAndSetupVideo(result.video_result[0].url);
        }

        private static async Task<string> RequestVideoGeneration(VideoGenerationRequest request)
        {
            try
            {
                string jsonBody = JsonConvert.SerializeObject(request);
                using UnityWebRequest webRequest = new UnityWebRequest(GENERATION_URL, "POST");
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Authorization", $"Bearer {AIHelper.ApiKey}");

                byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonBody);
                webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
                webRequest.downloadHandler = new DownloadHandlerBuffer();

                await webRequest.SendWebRequest();

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"生成请求失败: {webRequest.error}");
                    return null;
                }

                var response = JsonConvert.DeserializeObject<VideoGenerationResponse>(webRequest.downloadHandler.text);
                return response?.id;
            }
            catch (Exception ex)
            {
                Debug.LogError($"生成请求异常: {ex.Message}");
                return null;
            }
        }

        private static async Task<VideoGenerationResult> PollVideoResult(string taskId, int maxRetries)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    string url = string.Format(QUERY_URL, taskId);
                    using UnityWebRequest webRequest = UnityWebRequest.Get(url);
                    webRequest.SetRequestHeader("Authorization", $"Bearer {AIHelper.ApiKey}");
                    webRequest.downloadHandler = new DownloadHandlerBuffer();

                    await webRequest.SendWebRequest();

                    if (webRequest.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError($"查询失败: {webRequest.error}");
                        continue;
                    }

                    var result = JsonConvert.DeserializeObject<VideoGenerationResult>(webRequest.downloadHandler.text);

                    if (result.task_status == "SUCCESS")
                        return result;

                    if (result.task_status == "FAIL")
                    {
                        Debug.LogError("视频生成失败");
                        return null;
                    }

                    await Task.Delay(100);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"查询异常: {ex.Message}");
                }
            }

            Debug.LogError("超过最大重试次数");
            return null;
        }

        private static async Task<VideoPlayer> DownloadAndSetupVideo(string url)
        {
            try
            {
                using UnityWebRequest webRequest = UnityWebRequest.Get(url);
                await webRequest.SendWebRequest();

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"视频下载失败: {webRequest.error}");
                    return null;
                }

                string tempPath = System.IO.Path.Combine(Application.temporaryCachePath,
                    $"temp_video_{DateTime.Now.Ticks}.mp4");
                System.IO.File.WriteAllBytes(tempPath, webRequest.downloadHandler.data);

                // 创建视频播放器
                GameObject videoObj = new GameObject("VideoPlayer");
                VideoPlayer videoPlayer = videoObj.AddComponent<VideoPlayer>();

                // 设置视频属性
                videoPlayer.url = tempPath;
                videoPlayer.playOnAwake = false;
                videoPlayer.isLooping = true;
                videoPlayer.renderMode = VideoRenderMode.RenderTexture;

                // 创建RenderTexture
                RenderTexture renderTexture = new RenderTexture(1920, 1080, 24);
                videoPlayer.targetTexture = renderTexture;

                // 准备视频
                videoPlayer.Prepare();

                // 等待准备完成
                while (!videoPlayer.isPrepared)
                {
                    await Task.Delay(100);
                }

                // 注册清理事件
                videoPlayer.loopPointReached += _ =>
                {
                    try
                    {
                        if (System.IO.File.Exists(tempPath))
                            System.IO.File.Delete(tempPath);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"删除临时文件失败: {ex.Message}");
                    }
                };

                return videoPlayer;
            }
            catch (Exception ex)
            {
                Debug.LogError($"设置视频时发生错误: {ex.Message}");
                return null;
            }
        }

        #region Data Classes

        [Serializable]
        private class VideoGenerationRequest
        {
            public string model;
            public string prompt;
            public string image_url;
            public string quality;
            public bool with_audio;
            public string size;
            public int duration;
            public int fps;
        }

        [Serializable]
        private class VideoGenerationResponse
        {
            public string id;
            public string request_id;
            public string model;
            public string task_status;
        }

        [Serializable]
        private class VideoGenerationResult
        {
            public string model;
            public string request_id;
            public string task_status;
            public List<VideoResult> video_result;

            [Serializable]
            public class VideoResult
            {
                public string url;
                public string cover_image_url;
            }
        }

        #endregion
    }
}