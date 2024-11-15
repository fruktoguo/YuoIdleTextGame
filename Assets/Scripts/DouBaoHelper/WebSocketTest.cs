using UnityEngine;
using WebSocketSharp;
using System;

public class WebSocketTest : MonoBehaviour
{
    private WebSocket ws;

    void Start()
    {
        TestWebSocket();
    }

    void TestWebSocket()
    {
        ws = new WebSocket("wss://echo.websocket.org");

        ws.OnOpen += (sender, e) =>
        {
            Debug.Log("Connection opened");
            ws.Send("Hello WebSocket!");
        };

        ws.OnMessage += (sender, e) =>
        {
            Debug.Log("Received message: " + e.Data);
        };

        ws.OnError += (sender, e) =>
        {
            Debug.LogError("WebSocket Error: " + e.Message);
        };

        ws.OnClose += (sender, e) =>
        {
            Debug.Log("Connection closed. Code: " + e.Code + ", Reason: " + e.Reason);
        };

        Debug.Log("Attempting to connect...");
        ws.Connect();
    }

    void OnDestroy()
    {
        if (ws != null && ws.ReadyState == WebSocketState.Open)
        {
            ws.Close();
        }
    }
}