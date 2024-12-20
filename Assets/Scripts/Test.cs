using Sirenix.OdinInspector;
using UnityEngine;
using YuoTools;
using YuoTools.Extend.AI;

public class Test : MonoBehaviour
{
    public Texture2D texture;

    [Button]
    public async void RecognizeImage()
    {
        var result = await ZhipuGLM4V.RecognizeImageFromTexture(texture, "请描述这个图片", 3);
        Debug.Log(result);
    }
}