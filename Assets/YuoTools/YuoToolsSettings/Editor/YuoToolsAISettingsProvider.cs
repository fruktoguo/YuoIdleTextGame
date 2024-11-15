using UnityEditor;
using UnityEngine;

public class YuoToolsAISettingsProvider : SettingsProvider
{
    private SerializedObject _settings;

    private YuoToolsSettings _yuoSettings;

    private static YuoToolsAISettingsProvider instance;

    public YuoToolsAISettingsProvider(string path, SettingsScope scope)
        : base(path, scope)
    {
    }

    [SettingsProvider]
    public static SettingsProvider CreateYuoToolsSettingsProvider()
    {
        var provider = new YuoToolsAISettingsProvider("Project/YuoTools Settings/AISetting", SettingsScope.Project);
        instance = provider;
        provider._settings = new SerializedObject(YuoToolsSettingsHelper.GetOrCreateSettings());
        provider._yuoSettings = provider._settings.targetObject as YuoToolsSettings;
        return provider;
    }

    public override void OnGUI(string searchContext)
    {
        _settings.Update();

        EditorGUI.BeginChangeCheck();

        DrawProperties(_settings);

        if (EditorGUI.EndChangeCheck())
        {
            _settings.ApplyModifiedProperties();
            YuoToolsSettingsHelper.SaveSettings(_settings.targetObject as YuoToolsSettings);
        }
    }

    public static void DrawProperties(SerializedObject serializedObject)
    {
        SerializedProperty aiSetting = serializedObject.FindProperty("AISetting");

        EditorGUILayout.LabelField("AI Settings", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        var setting = instance._yuoSettings.AISetting;

        // 绘制服务商选择
        SerializedProperty currentServer = aiSetting.FindPropertyRelative("Server");
        EditorGUILayout.PropertyField(currentServer, new GUIContent("Service Provider"));
        // 获取当前选中的服务商索引

        // 绘制 API Key
        string newApiKey = EditorGUILayout.TextField("API Key", setting.GetServer().APIKey);
        setting.GetServer().APIKey = newApiKey;

        // 绘制 Model
        string newModel = EditorGUILayout.TextField("Model", setting.GetServer().Model);
        setting.GetServer().Model = newModel;

        EditorGUI.indentLevel--;
    }
}