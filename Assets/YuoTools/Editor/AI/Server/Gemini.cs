using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace YuoTools.Extend.AI
{
    public class GeminiApi
    {
        static async Task GetAwaiter(AsyncOperation asyncOperation)
        {
            var task = new TaskCompletionSource<bool>();
            asyncOperation.completed += _ => { task.SetResult(true); };
            await task.Task;
        }

        static async Task<string> GetResponse(string body, List<(string, string)> headers = null)
        {
            if (AIHelper.ApiKey.IsNullOrSpace()) "请输入ApiKey".LogError();
            var url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent?key=" + AIHelper.ApiKey;

            UnityWebRequest webRequest = new UnityWebRequest(url, "POST");

            headers ??= new List<(string, string)>
            {
                ("Content-Type", "application/json")
            };
            for (int i = 0; i < headers.Count; i++)
            {
                webRequest.SetRequestHeader(headers[i].Item1, headers[i].Item2);
            }

            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(body);
            webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.disposeDownloadHandlerOnDispose = true;
            webRequest.disposeUploadHandlerOnDispose = true;

            await GetAwaiter(webRequest.SendWebRequest());

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

        public static async Task<string> GenerateText(string prompt)
        {
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };
            string body = JsonUtility.ToJson(requestBody);
            var response = await GetResponse(body);
            return response;
        }
    }
}