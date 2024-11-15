using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace YuoTools.Extend.AI
{
    public class AIServerHelper
    {
        static async Task GetAwaiter(AsyncOperation asyncOperation)
        {
            var task = new TaskCompletionSource<bool>();
            asyncOperation.completed += _ => { task.SetResult(true); };
            await task.Task;
        }

        public static async Task<string> GetResponse(string apiKey, string url, string body,
            List<(string, string)> headers = null)
        {
            if (apiKey.IsNullOrSpace())
            {
                Debug.LogError("请输入ApiKey");
                return null;
            }

            $"SendingRequest: {body}".Log();

            UnityWebRequest webRequest = new UnityWebRequest(url, "POST");

            headers ??= new List<(string, string)>
            {
                ("Content-Type", "application/json"),
                ("Authorization", $"Bearer {apiKey}")
            };
            foreach (var header in headers)
            {
                webRequest.SetRequestHeader(header.Item1, header.Item2);
            }

            byte[] jsonToSend = new UTF8Encoding().GetBytes(body);
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

        public static async IAsyncEnumerable<string> GetResponseStream(string apiKey, string url, string body,
            Func<string, string> onJson,
            List<(string, string)> headers = null, int messageDelay = 100, int safeCount = 1000)
        {
            if (apiKey.IsNullOrSpace())
            {
                Debug.LogError("请输入ApiKey");
                yield break;
            }

            UnityWebRequest webRequest = new UnityWebRequest(url, "POST");

            headers ??= new List<(string, string)>
            {
                ("Content-Type", "application/json"),
                ("Authorization", $"Bearer {apiKey}")
            };

            foreach (var header in headers)
            {
                webRequest.SetRequestHeader(header.Item1, header.Item2);
            }

            byte[] jsonToSend = new UTF8Encoding().GetBytes(body);
            webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.disposeDownloadHandlerOnDispose = true;
            webRequest.disposeUploadHandlerOnDispose = true;

            $"Sending request to: {webRequest.url} with body: {body} and headers: ".Log();

            var asyncOption = webRequest.SendWebRequest();
            while (!asyncOption.isDone)
            {
                await Task.Delay(messageDelay);

                yield return onJson?.Invoke(webRequest.downloadHandler.text);
                if (safeCount-- < 0)
                {
                    Debug.LogError("请求超时");
                    break;
                }
            }

            webRequest.Dispose();
        }
    }
}