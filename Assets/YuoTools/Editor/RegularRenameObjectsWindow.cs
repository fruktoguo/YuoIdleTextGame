using System.Threading.Tasks;
using YuoTools.Extend.AI;

namespace YuoTools.Editor
{
    using System;
    using System.Text.RegularExpressions;
    using UnityEditor;
    using UnityEngine;
    using Object = UnityEngine.Object;

    public class RegularRenameObjectsWindow : EditorWindow
    {
        Object[] objects;

        [MenuItem("Tools/YuoTools/正则批量修改")]
        public static void ShowWindow()
        {
            var window = GetWindow<RegularRenameObjectsWindow>("Rename Objects");
            window.minSize = new Vector2(900, 600);
        }

        private string regexPattern = "";
        private string regexReplace = "";


        private string suffix = "";
        private string suffixStartNum = "";
        private int suffixOption;

        private string prefix = "";
        private string prefixStartNum = "";
        private int prefixOption;

        private bool showAISection = false; // 折叠区域的状态
        private string aiInput = ""; // 输入框的内容
        private string aiPreview = ""; // 预览框的内容

        private void OnGUI()
        {
            objects = Selection.objects;
            GUILayout.BeginHorizontal();
            GUILayout.Label("原名字", GUILayout.Width(400));
            GUILayout.Label("新名字", GUILayout.Width(400));
            GUILayout.EndHorizontal();

            for (int index = 0; index < objects.Length; index++)
            {
                if (index > 10)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("……", GUILayout.Width(400));
                    GUILayout.Label("……", GUILayout.Width(400));
                    GUILayout.EndHorizontal();
                    break;
                }

                var obj = objects[index];
                GUILayout.BeginHorizontal();
                GUILayout.Label(obj.name, GUILayout.Width(400));
                GUILayout.Label(GetTargetName(obj.name, index), GUILayout.Width(400));
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            GUILayout.Label("正则表达式", GUILayout.Width(400));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("查找", GUILayout.Width(400));
            GUILayout.Label("替换", GUILayout.Width(400));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("/", GUILayout.Width(20));
            regexPattern = GUILayout.TextField(regexPattern, GUILayout.Width(200));
            GUILayout.Label("/", GUILayout.Width(20));
            regexReplace = GUILayout.TextField(regexReplace, GUILayout.Width(200));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("前缀", GUILayout.Width(100));

            prefixOption = GUILayout.Toolbar(prefixOption, new string[] { "自定义文本", "序号" }, GUILayout.Width(200));

            if (prefixOption == 0)
            {
                prefix = GUILayout.TextField(prefix, GUILayout.Width(200));
            }

            else if (prefixOption == 1)
            {
                GUILayout.Label("起始序号", GUILayout.Width(100));
                prefixStartNum = GUILayout.TextField(prefixStartNum, GUILayout.Width(100));

                if (!int.TryParse(prefixStartNum, out int num) && prefixStartNum != "")
                {
                    prefixStartNum = "0";
                }
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("后缀", GUILayout.Width(100));

            suffixOption = GUILayout.Toolbar(suffixOption, new string[] { "自定义文本", "序号" }, GUILayout.Width(200));

            if (suffixOption == 0)
            {
                suffix = GUILayout.TextField(suffix, GUILayout.Width(200));
            }

            else if (suffixOption == 1)
            {
                GUILayout.Label("起始序号", GUILayout.Width(100));
                suffixStartNum = GUILayout.TextField(suffixStartNum, GUILayout.Width(100));

                if (!int.TryParse(suffixStartNum, out int num) && suffixStartNum != "")
                {
                    suffixStartNum = "0";
                }
            }

            GUILayout.EndHorizontal();


            GUILayout.Space(20);


            if (GUILayout.Button("应用正则表达式"))
            {
                Undo.SetCurrentGroupName($"应用正则表达式 [数量:{objects.Length}]");
                Undo.RecordObjects(objects, "应用正则表达式");
                for (int index = 0; index < objects.Length; index++)
                {
                    Object obj = objects[index];
                    obj.name = GetTargetName(obj.name, index);
                }

                Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            }

            // 添加折叠区域
            showAISection = EditorGUILayout.Foldout(showAISection, "AI");
            if (showAISection)
            {
                GUILayout.BeginHorizontal();

                // 左边的不可编辑预览框
                GUILayout.Label(string.IsNullOrEmpty(aiPreview) ? GetOriginalText() : aiPreview, GUILayout.Width(400));

                // 右边的输入框
                aiInput = EditorGUILayout.TextField(aiInput, GUILayout.Width(400));

                GUILayout.EndHorizontal();

                // 输入框下面的按钮并列显示
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("预览", GUILayout.Width(200)))
                {
                    UpdateAIPreview();
                }
                if (GUILayout.Button("应用", GUILayout.Width(200)))
                {
                    ApplyAI();
                }
                GUILayout.EndHorizontal();
            }

            string GetTargetName(string str, int index = 0)
            {
                string result = Regular(str, regexPattern, regexReplace);

                if (prefixOption == 0)
                {
                    result = prefix + result;
                }
                else if (prefixOption == 1)
                {
                    if (!int.TryParse(prefixStartNum, out int num))
                    {
                        num = 0;
                    }

                    result = $"{index + num}_" + result;
                }

                if (suffixOption == 0)
                {
                    result += suffix;
                }
                else if (suffixOption == 1)
                {
                    if (!int.TryParse(suffixStartNum, out int num))
                    {
                        num = 0;
                    }

                    result += $"_{index + num}";
                }


                return result;
            }
        }

        string GetOriginalText()
        {
            string result = "";
            foreach (var obj in objects)
            {
                result += obj.name + "\n";
            }

            return result;
        }

        private async void UpdateAIPreview()
        {
            // 更新预览框的内容
            var message =
                $"我需要你帮助我批量修改名称,你必须且只能返回和原文本格式匹配的文本,不能返回其他内容,原文带有的回车也必须保留,接下来是其余的要求 : {aiInput} 以上就是所有的要求,下一个回车后的所有文本都是原文本,请务必返回符合原文本格式的文本 \n{GetOriginalText()}";
            await foreach (var preview in AIHelper.GenerateTextStream(message))
            {
                aiPreview = preview;
            }
        }
        
        void ApplyAI()
        {
            Undo.SetCurrentGroupName($"AI修改名称 [数量:{objects.Length}]");
            Undo.RecordObjects(objects, "AI修改名称");
            var names = aiPreview.Split('\n');
            for (int index = 0; index < objects.Length; index++)
            {
                Object obj = objects[index];
                obj.name = names[index];
            }

            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
        }

        string Regular(string str, string regex, string replace)
        {
            try
            {
                if (string.IsNullOrEmpty(regex))
                {
                    return str;
                }

                return Regex.Replace(str, regex, replace);
            }
            catch (Exception)
            {
                //Debug.LogError(e);
                return str;
            }
        }
    }
}