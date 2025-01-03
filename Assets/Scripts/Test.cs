using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using YuoTools.Extend.AI;

public class Test : SerializedMonoBehaviour
{
    public Texture2D texture;

    private async void Start()
    {
        // texture = await ZhipuCogView.GenerateImage("一只可爱的小狗");
        // 生成视频
        var currentVideoPlayer = await ZhipuCogVideo.GenerateVideo(
            prompt: "一只可爱的小猫咪在玩毛线球",
            quality: "quality",
            withAudio: true,
            size: "720x480",
            duration: 5,
            fps: 30
        );
    }
}