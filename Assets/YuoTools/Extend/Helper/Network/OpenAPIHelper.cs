using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

namespace YuoTools.Extend.Helper
{
    public static class OpenAPIHelper
    {
        private static string baseUrl;
        private static JObject apiSpec;
        private static string apiToken;
        private static string authType;
        private static Dictionary<string, string> defaultHeaders;

        public static void Initialize(string apiSpecJson, string token = null, string authorizationType = "Bearer")
        {
            apiSpec = JObject.Parse(apiSpecJson);
            baseUrl = apiSpec["servers"]?[0]?["url"]?.ToString();
            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new InvalidOperationException("Base URL not found in API spec.");
            }
            SetToken(token, authorizationType);
            defaultHeaders = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" }
            };
        }

        public static void SetToken(string token, string authorizationType = "Bearer")
        {
            apiToken = token;
            authType = authorizationType;
        }

        public static void AddDefaultHeader(string key, string value)
        {
            defaultHeaders[key] = value;
        }

        public static async Task<string> CallAPI(string path, string method,
            Dictionary<string, string> parameters = null, string body = null, Dictionary<string, string> headers = null)
        {
            if (apiSpec == null)
            {
                throw new InvalidOperationException("API spec not initialized. Call Initialize first.");
            }

            var pathItem = apiSpec["paths"][path];
            if (pathItem == null)
            {
                throw new ArgumentException($"Path {path} not found in API spec.");
            }

            var operation = pathItem[method.ToLower()];
            if (operation == null)
            {
                throw new ArgumentException($"Method {method} not allowed for path {path}.");
            }

            string url = baseUrl + path;

            // 处理路径参数
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    url = url.Replace("{" + param.Key + "}", Uri.EscapeDataString(param.Value));
                }
            }

            // 处理查询参数
            List<string> queryParams = new List<string>();
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    if (operation["parameters"] != null)
                    {
                        var paramSpec = operation["parameters"].FirstOrDefault(p =>
                            p["name"].ToString() == param.Key && p["in"].ToString() == "query");
                        if (paramSpec != null)
                        {
                            queryParams.Add($"{param.Key}={Uri.EscapeDataString(param.Value)}");
                        }
                    }
                }
            }

            if (queryParams.Count > 0)
            {
                url += "?" + string.Join("&", queryParams);
            }

            // 验证必需参数
            var requiredParams = operation["parameters"]?.Where(p => p["required"]?.ToObject<bool>() == true);
            if (requiredParams != null)
            {
                foreach (var requiredParam in requiredParams)
                {
                    string paramName = requiredParam["name"].ToString();
                    if (!parameters.ContainsKey(paramName))
                    {
                        throw new ArgumentException($"Required parameter '{paramName}' is missing.");
                    }
                }
            }

            Debug.Log($"Calling API: {method} {url}");

            using UnityWebRequest request = new UnityWebRequest(url, method);
            if (!string.IsNullOrEmpty(body))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(body);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            }

            request.downloadHandler = new DownloadHandlerBuffer();

            // 添加默认头部
            foreach (var header in defaultHeaders)
            {
                request.SetRequestHeader(header.Key, header.Value);
            }

            // 添加认证头部
            if (!string.IsNullOrEmpty(apiToken))
            {
                request.SetRequestHeader("Authorization", $"{authType} {apiToken}");
            }

            // 添加自定义头部
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.SetRequestHeader(header.Key, header.Value);
                }
            }

            await request.SendWebRequest().GetAwaiter();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"API call failed: {request.error}");
                return $"{{\"error\": \"{request.error}\"}}";
            }

            Debug.Log($"API call successful. Response: {request.downloadHandler.text}");
            return request.downloadHandler.text;
        }
    }
}