using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System.IO;

namespace YuoTools.Extend.AI
{
    public class DouBaoTTS
    {
        private const string Url = "https://openspeech.bytedance.com/api/v1/tts";
        private const string AppId = "8583113786";
        private const string Token = "Gg6l6wEsvlnCiPPaospou3dmODBcquGy";

        public static async Task<AudioClip> GenerateAsync(string text,
            string voiceType = "zh_female_tianmeixiaoyuan_moon_bigtts")
        {
            var payload = new JObject
            {
                ["app"] = new JObject
                {
                    ["appid"] = AppId,
                    ["token"] = Token,
                    ["cluster"] = "volcano_tts"
                },
                ["user"] = new JObject
                {
                    ["uid"] = Guid.NewGuid().ToString()
                },
                ["audio"] = new JObject
                {
                    ["voice_type"] = voiceType,
                    ["encoding"] = "mp3",
                    ["speed_ratio"] = 1.0
                },
                ["request"] = new JObject
                {
                    ["reqid"] = Guid.NewGuid().ToString(),
                    ["text"] = text,
                    ["operation"] = "query"
                }
            };

            string response = await GetResponse(Token, Url, payload.ToString());

            if (!string.IsNullOrEmpty(response))
            {
                var result = JObject.Parse(response);

                if (result["code"].Value<int>() == 3000)
                {
                    byte[] audioData = Convert.FromBase64String(result["data"].Value<string>());
                    AudioClip audioClip = await CreateAudioClipFromMP3(audioData);
                    Debug.Log($"音频生成成功, 时长: {result["addition"]["duration"]}ms");
                    return audioClip;
                }
                else
                {
                    Debug.LogError($"错误: {result["message"]}");
                }
            }

            return null;
        }

        private static async Task<AudioClip> CreateAudioClipFromMP3(byte[] mp3Data)
        {
            // 创建一个唯一的临时文件名
            string tempFileName = $"temp_{Guid.NewGuid().ToString()}.mp3";
            string tempPath = Path.Combine(Application.temporaryCachePath, tempFileName);
            await File.WriteAllBytesAsync(tempPath, mp3Data);
            try
            {
                using UnityWebRequest www =
                    UnityWebRequestMultimedia.GetAudioClip("file://" + tempPath, AudioType.MPEG);
                var operation = www.SendWebRequest();

                while (!operation.isDone)
                    await Task.Yield();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                    Debug.Log($"音频加载成功，采样率: {clip.frequency}, 声道数: {clip.channels}, 长度: {clip.length}秒");
                    return clip;
                }
                else
                {
                    Debug.LogError($"加载音频文件时出错: {www.error}");
                    return null;
                }
            }
            finally
            {
                // 清理临时文件
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
        }

        static async Task<string> GetResponse(string token, string url, string body,
            List<(string, string)> headers = null)
        {
            if (string.IsNullOrEmpty(token))
            {
                Debug.LogError("请输入ApiKey");
                return null;
            }

            Debug.Log($"SendingRequest: {body}");

            UnityWebRequest webRequest = new UnityWebRequest(url, "POST");

            headers ??= new List<(string, string)>
            {
                ("Content-Type", "application/json"),
                ("Authorization", $"Bearer;{token}")
            };
            foreach (var header in headers)
            {
                webRequest.SetRequestHeader(header.Item1, header.Item2);
            }

            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(body);
            webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.disposeDownloadHandlerOnDispose = true;
            webRequest.disposeUploadHandlerOnDispose = true;

            await webRequest.SendWebRequest();

            string result = "";
            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError("Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError("HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    result = webRequest.downloadHandler.text;
                    break;
            }

            webRequest.Dispose();
            return result;
        }
    }
}