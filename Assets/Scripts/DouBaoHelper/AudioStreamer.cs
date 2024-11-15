using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Sirenix.OdinInspector;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

public class AudioStreamer : MonoBehaviour
{
    private ClientWebSocket webSocket;
    private AudioClip microphoneClip;
    private bool isStreaming = false;
    private CancellationTokenSource cancellationTokenSource;
    private bool isReceiving = false;

    // 音频参数配置
    private const int CHUNK = 1600;
    private const int CHANNELS = 1;
    private const int RATE = 16000;

    // WebSocket连接参数
    private const string WS_URL = "wss://openspeech.bytedance.com/api/v3/sauc/bigmodel_nostream";

    private readonly Dictionary<string, string> headers = new Dictionary<string, string>
    {
        { "X-Api-App-Key", "8583113786" }, // 替换为你的APP KEY
        { "X-Api-Access-Key", "Gg6l6wEsvlnCiPPaospou3dmODBcquGy" }, // 替换为你的ACCESS KEY
        { "X-Api-Resource-Id", "volc.bigasr.sauc.duration" },
        { "X-Api-Connect-Id", System.Guid.NewGuid().ToString() }
    };

    async void Start()
    {
        await InitializeWebSocket();
    }

    private async Task InitializeWebSocket()
    {
        webSocket = new ClientWebSocket();
        cancellationTokenSource = new CancellationTokenSource();

        foreach (var header in headers)
        {
            webSocket.Options.SetRequestHeader(header.Key, header.Value);
        }

        string url = WS_URL;
        Debug.Log($"Attempting to connect to: {url}");

        try
        {
            await webSocket.ConnectAsync(new Uri(url), cancellationTokenSource.Token);
            Debug.Log("WebSocket connection established successfully.");
            isReceiving = true;
            StartCoroutine(ReceiveLoop());
        }
        catch (Exception ex)
        {
            Debug.LogError($"Exception during WebSocket.ConnectAsync(): {ex}");
        }
    }

    private IEnumerator ReceiveLoop()
    {
        byte[] buffer = new byte[8192];
        while (isReceiving && webSocket.State == WebSocketState.Open)
        {
            ArraySegment<byte> segment = new ArraySegment<byte>(buffer);
            Task<WebSocketReceiveResult> receiveTask = webSocket.ReceiveAsync(segment, cancellationTokenSource.Token);

            while (!receiveTask.IsCompleted)
            {
                if (!isReceiving || webSocket.State != WebSocketState.Open)
                {
                    yield break;
                }
                yield return null;
            }

            if (receiveTask.IsFaulted)
            {
                Debug.LogError($"Error receiving message: {receiveTask.Exception}");
                isReceiving = false;
                yield break;
            }

            WebSocketReceiveResult result = receiveTask.Result;

            if (result.MessageType == WebSocketMessageType.Close)
            {
                isReceiving = false;
                yield break;
            }

            byte[] receivedBytes = new byte[result.Count];
            Array.Copy(buffer, receivedBytes, result.Count);
            ProcessResponse(receivedBytes);

            yield return null;
        }
    }

    [Button]
    private async void StartStreaming()
    {
        if (webSocket.State != WebSocketState.Open)
        {
            Debug.LogError("WebSocket is not open.");
            return;
        }

        isStreaming = true;
        await SendWebSocketMessage(CreateFullRequest());

        microphoneClip = Microphone.Start(null, true, 1, RATE);
        StartCoroutine(StreamAudio());
    }

    [Button]
    private async void StopStreaming()
    {
        isStreaming = false;
        Microphone.End(null);
        if (webSocket.State == WebSocketState.Open)
        {
            await SendWebSocketMessage(CreateAudioRequest(new byte[0], true));
        }
    }

    private IEnumerator StreamAudio()
    {
        int position = 0;
        float[] samples = new float[CHUNK];

        while (isStreaming && webSocket.State == WebSocketState.Open)
        {
            int newPosition = Microphone.GetPosition(null);
            if (newPosition > position || newPosition < position)
            {
                if (newPosition < position)
                    position = 0;

                int diff = newPosition - position;
                if (diff >= CHUNK)
                {
                    microphoneClip.GetData(samples, position);
                    byte[] audioData = ConvertFloatToPCM16(samples);
                    SendWebSocketMessage(CreateAudioRequest(audioData)).Wait();

                    position += CHUNK;
                    if (position >= microphoneClip.samples)
                        position = 0;
                }
            }

            yield return null;
        }
    }

    private async Task SendWebSocketMessage(byte[] message)
    {
        if (webSocket.State == WebSocketState.Open)
        {
            await webSocket.SendAsync(new ArraySegment<byte>(message), WebSocketMessageType.Binary, true, cancellationTokenSource.Token);
        }
        else
        {
            Debug.LogWarning("WebSocket is not open. Cannot send message.");
        }
    }

    private void ProcessResponse(byte[] response)
    {
        try
        {
            byte[] header = new byte[4];
            Array.Copy(response, header, 4);

            byte msgType = (byte)((header[1] >> 4) & 0xF);

            if (msgType == 0xF) // error message
            {
                int errorCode = BitConverter.ToInt32(new byte[] { response[7], response[6], response[5], response[4] }, 0);
                int errorSize = BitConverter.ToInt32(new byte[] { response[11], response[10], response[9], response[8] }, 0);
                string errorMsg = Encoding.UTF8.GetString(response, 12, errorSize);
                Debug.LogError($"Error: {errorMsg} (code: {errorCode})");
                return;
            }

            if (msgType == 0x9) // full server response
            {
                int payloadSize = BitConverter.ToInt32(new byte[] { response[11], response[10], response[9], response[8] }, 0);
                byte[] payload = new byte[payloadSize];
                Array.Copy(response, 12, payload, 0, payloadSize);

                string jsonData;
                try
                {
                    jsonData = Encoding.UTF8.GetString(Decompress(payload));
                }
                catch
                {
                    jsonData = Encoding.UTF8.GetString(payload);
                }

                Debug.Log($"Received JSON: {jsonData}");

                var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonData);
                if (result != null && result.ContainsKey("result"))
                {
                    var resultObject = result["result"];
                    if (resultObject is Dictionary<string, object> resultDict && resultDict.ContainsKey("text"))
                    {
                        string recognizedText = resultDict["text"].ToString();
                        Debug.Log($"识别结果: {recognizedText}");
                    }
                    else if (resultObject is JObject resultJObject)
                    {
                        string recognizedText = resultJObject["text"]?.ToString();
                        if (!string.IsNullOrEmpty(recognizedText))
                        {
                            Debug.Log($"识别结果: {recognizedText}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Unexpected result structure: {resultObject}");
                    }
                }
                else
                {
                    Debug.LogWarning($"Response doesn't contain 'result' key or is null. Full response: {jsonData}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error processing response: {ex.Message}\nStack Trace: {ex.StackTrace}");
        }
    }

    private byte[] CreateFullRequest()
    {
        var request = new Dictionary<string, object>
        {
            { "app_id", "8583113786" },
            { "user_id", "test_user" },
            { "audio_format", "pcm16k16bit" },
            { "asr_mode", "online" },
            { "enable_vad", true },
            { "enable_punctuation", true },
            { "enable_intermediate_result", true },
            { "enable_itn", true },
            { "language", "zh" }
        };

        string jsonRequest = JsonConvert.SerializeObject(request);
        byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonRequest);

        byte[] header = new byte[12];
        header[0] = 0x0;
        header[1] = 0x10;
        BitConverter.GetBytes(jsonBytes.Length).CopyTo(header, 8);

        return header.Concat(jsonBytes).ToArray();
    }

    private byte[] CreateAudioRequest(byte[] audioData, bool isEnd = false)
    {
        byte[] header = new byte[12];
        header[0] = 0x0;
        header[1] = (byte)(isEnd ? 0x40 : 0x20);
        BitConverter.GetBytes(audioData.Length).CopyTo(header, 8);

        return header.Concat(audioData).ToArray();
    }

    private byte[] ConvertFloatToPCM16(float[] samples)
    {
        byte[] pcm = new byte[samples.Length * 2];
        for (int i = 0; i < samples.Length; i++)
        {
            short value = (short)(samples[i] * 32767);
            byte[] bytes = BitConverter.GetBytes(value);
            pcm[i * 2] = bytes[0];
            pcm[i * 2 + 1] = bytes[1];
        }
        return pcm;
    }

    private byte[] Decompress(byte[] data)
    {
        using (var compressedStream = new MemoryStream(data))
        using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
        using (var resultStream = new MemoryStream())
        {
            zipStream.CopyTo(resultStream);
            return resultStream.ToArray();
        }
    }

    async void OnApplicationQuit()
    {
        isReceiving = false;
        isStreaming = false;
        if (webSocket != null && webSocket.State == WebSocketState.Open)
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Application quitting", CancellationToken.None);
        }
        cancellationTokenSource?.Cancel();
    }
}
