using UnityEngine;
using System.IO;

public partial class YuoToolsSettingsHelper
{
    // 配置文件存放路径
    private static string GetSettingsPath()
    {
        return $"ProjectSettings/YuoToolsSettings.json";
    }

    public static YuoToolsSettings GetOrCreateSettings()
    {
#if UNITY_EDITOR
        string path = GetSettingsPath();
        YuoToolsSettings settings;
        if (File.Exists(path))
        {
            // 如果文件存在，从JSON文件中加载
            settings = ScriptableObject.CreateInstance<YuoToolsSettings>();
            JsonUtility.FromJsonOverwrite(File.ReadAllText(path), settings);
        }
        else
        {
            // 如果文件不存在，创建一个新的实例
            settings = ScriptableObject.CreateInstance<YuoToolsSettings>();
            SaveSettings(settings);
        }

        // 确保设置对象名称
        settings.name = "YuoToolsSettings";

        SaveResource(settings);

        return settings;
#else
    return Resources.Load<YuoToolsSettings>("YuoToolsSettings");
#endif
    }

    static void SaveResource(YuoToolsSettings settings)
    {
#if UNITY_EDITOR
        var path = "Assets/Resources/YuoToolsSettings.asset";

        // 确保Resources文件夹存在
        if (!Directory.Exists("Assets/Resources"))
        {
            Directory.CreateDirectory("Assets/Resources");
        }

        // 再次确保设置对象名称
        if (string.IsNullOrEmpty(settings.name))
        {
            settings.name = "YuoToolsSettings";
        }

        // 检查文件是否已存在
        if (UnityEditor.AssetDatabase.LoadAssetAtPath<YuoToolsSettings>(path) == null)
        {
            // 如果文件不存在，创建新的资源文件
            UnityEditor.AssetDatabase.CreateAsset(settings, path);
        }
        else
        {
            // 如果文件已存在，更新现有资源
            UnityEditor.EditorUtility.CopySerialized(settings,
                UnityEditor.AssetDatabase.LoadAssetAtPath<YuoToolsSettings>(path));
        }

        // 保存所有资源更改
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    // 保存设置到文件
    public static void SaveSettings(YuoToolsSettings settings)
    {
        string path = GetSettingsPath();
        File.WriteAllText(path, JsonUtility.ToJson(settings, true));
        SaveResource(settings);
    }
}