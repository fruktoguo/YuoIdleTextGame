using UnityEngine;
using UnityEditor;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using YuoTools.Extend.Helper;

namespace YuoTools.Extend.AI
{
    public sealed class AICommandWindow : OdinEditorWindow
    {
        [MenuItem("Tools/YuoTools/AI Command")]
        public static void ShowWindow()
        {
            GetWindow<AICommandWindow>("AI Command");
        }

        #region Temporary script file operations

        const string TempFilePath = "Assets/AICommandTemp.cs";

        bool TempFileExists => System.IO.File.Exists(TempFilePath);

        void CreateScriptAsset(string code)
        {
            var flags = BindingFlags.Static | BindingFlags.NonPublic;
            var method = typeof(ProjectWindowUtil).GetMethod("CreateScriptAssetWithContent", flags);
            if (method != null) method.Invoke(null, new object[] { TempFilePath, code });
        }

        #endregion

        #region Script generator

        [EnumToggleButtons, GUIColor(0.4f, 0.8f, 1f),]
        [LabelText("选择命令类型")]
        [PropertySpace(SpaceBefore = 10, SpaceAfter = 10)]
        public CommandType commandType = CommandType.EditorScript;

        public enum CommandType
        {
            EditorScript,
            CmdCommand,
        }

        static string EditorScriptPrompt(string input)
            => "编写一个Unity编辑器脚本。\n" +
               " - 它的功能作为一个菜单项放置在\"Edit\" > \"Do Task\"。\n" +
               " - 它不提供任何编辑器窗口。当菜单项被调用时，它会立即执行任务。\n" +
               " - 不要使用GameObject.FindGameObjectsWithTag。\n" +
               " - 没有选中的对象。手动查找游戏对象。\n" +
               " - 该脚本必须可以在编辑器模式运行\n" +
               "需要完整限定命名空间。\n" +
               "你绝对不能添加除了代码和注释以外的任何解释\n" +
               "你不能使用md,只能返回纯文本\n" +
               "绝对不能使用 ```csharp 开头以 ``` 结尾的格式\n" +
               "\n" +
               input;

        static string CmdPrompt(string input)
            => "编写一个可以在Windows命令行中执行的命令。\n" +
               "不需要任何的注释\n" +
               "这个命令必须要在Windows命令行中执行\n" +
               "必须保证不会严重威胁Windows安全\n" +
               "你绝对不能添加除了代码以外的任何解释\n" +
               "你不能使用md,只能返回纯文本\n" +
               "\n" +
               input;

        [HorizontalGroup("button"), GUIColor(0.6f, 1f, 0.6f)]
        [Button("生成代码", ButtonHeight = 100), Tooltip("点击生成代码")]
        async void CreateGenerator()
        {
            var message = commandType switch
            {
                CommandType.EditorScript => EditorScriptPrompt(prompt),
                CommandType.CmdCommand => CmdPrompt(prompt),
                _ => ""
            };

            await foreach (var line in AIHelper.GenerateTextStream(message))
            {
                result = line;
            }
        }

        [HorizontalGroup("button"), GUIColor(1f, 0.6f, 0.6f)]
        [Button("运行代码", ButtonHeight = 100), Tooltip("点击运行生成的代码")]
        void RunGenerator()
        {
            if (result.IsNullOrWhitespace()) return;
            switch (commandType)
            {
                case CommandType.EditorScript:
                    CreateScriptAsset(result);
                    break;
#if UNITY_EDITOR_WIN
                case CommandType.CmdCommand:
                    WindowsHelper.Command(result);
                    break;
#endif
            }
        }

        #endregion

        #region Editor GUI

        [BoxGroup("输入"), PropertyOrder(-1)]
        [HorizontalGroup("输入/Prompt"), TextArea(5, 100), LabelWidth(100), Tooltip("在此输入命令提示")]
        public string prompt = "输入Command";

        [BoxGroup("结果"), PropertyOrder(1)] [HorizontalGroup("结果/Output"), TextArea(5, 100), Tooltip("生成的代码将显示在这里")]
        public string result = "";

        #endregion

        void OnEditorUpdate()
        {
            Repaint();
        }

        protected override void OnEnable()
        {
            EditorApplication.update += OnEditorUpdate;
        }

        protected override void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
        }
    }
}