using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector;
using UnityEditor;
#endif

namespace YuoTools.Main.Ecs
{
    public partial class YuoComponent
    {
        public virtual Color CustomEditorElementColor()
        {
            using MD5 md5 = MD5.Create();
            byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(Type.Name));
            float h = Math.Abs(hashBytes[0] / 1.3f % 1f);
            return Color.HSVToRGB(h, 0.35f, 0.55f);
        }

        public virtual string CustomEditorDisplayOnInspector() => "";
    }
}

#if UNITY_EDITOR
namespace YuoTools.Main.Ecs
{
    [HideReferenceObjectPicker]
    [GUIColor(1, 1, 1)]
    public partial class YuoComponent
    {
        protected class YuoComponentDrawer : OdinValueDrawer<YuoComponent>
        {
            protected override void DrawPropertyLayout(GUIContent label)
            {
                var value = this.ValueEntry.SmartValue;

                // 保存当前的GUI状态
                var originalColor = GUI.color;

                // 先绘制原有的编辑控件
                CallNextDrawer(label);

                if (!this.ValueEntry.Property.State.Expanded)
                {
                    var extend = value.CustomEditorDisplayOnInspector();
                    if (!string.IsNullOrEmpty(extend))
                    {
                        // 获取最后绘制的矩形区域
                        var rect = GUILayoutUtility.GetLastRect();

                        // 计算值的显示区域
                        var valueRect = rect;
                        valueRect.xMin = EditorGUIUtility.labelWidth;

                        // 使用 Unity 默认的数值编辑器样式
                        var valueStyle = EditorStyles.numberField;

                        // 禁用输入以显示为只读状态
                        using (new EditorGUI.DisabledScope(true))
                        {
                            // 使用 EditorGUI.TextField 而不是 LabelField 来匹配默认外观
                            EditorGUI.TextField(valueRect, extend, valueStyle);
                        }
                    }
                }

                // 恢复GUI状态
                GUI.color = originalColor;
            }
        }

        [ButtonGroup]
        [Button("RemoveComponent")]
        [HorizontalGroup("SelectField")]
        [ShowIf("_privateSelect")]
        void S_Remove()
        {
            $"{Entity.EntityName}移除组件{Type.Name}".Log();
            Entity.RemoveComponent(this);
        }

                [ButtonGroup]
        [Button]
        [HorizontalGroup("SelectField")]
        [ShowIf("_privateSelect")]
        void SelectField()
        {
            var result = AssetDatabase.FindAssets(Type.Name);
            if (result.Length > 0)
            {
                Selection.activeObject =
                    AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(result[0]));
            }
        }

             [ButtonGroup]
        [Button]
        [HorizontalGroup("SelectField")]
        [ShowIf("_privateSelect")]
        void OpenField()
        {
            var result = AssetDatabase.FindAssets(Type.Name);
            if (result.Length > 0)
            {
                AssetDatabase.OpenAsset(
                    AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(result[0])));
            }

            //只是为了去掉警告
            _privateTips = _privateTips.ToString();
            _privateSelect = !_privateSelect;
            _privateSelect = false;
        }

               [ButtonGroup]
        [Button]
        [HorizontalGroup("SelectField")]
        [ShowIf("_privateSelect")]
        void CopyComponentName()
        {
            //复制到剪切板
            GUIUtility.systemCopyBuffer = Type.Name;
        }

        private bool _isPlaying => Application.isPlaying;

        [ShowInInspector] [HorizontalGroup("SelectField", width: 50)] [HideLabel] [ShowIf("_isPlaying")]
        private bool _privateSelect;

        [ShowInInspector] [HorizontalGroup("SelectField")] [HideLabel] [HideIf("_privateSelect")] [ReadOnly]
        private string _privateTips = "选择或者打开组件对应的文件(如果文件名和组件不一致,请手动查找)";

        [ShowIf("BaseComponentType", null)]
        [ShowInInspector]
        [LabelText("父组件类型")]
        private string ShowBase => BaseComponentType?.Name;

        [HorizontalGroup("SelectField")]
             [ButtonGroup]
        [Button(ButtonSizes.Medium)]
        [ShowIf("_privateSelect")]
        private void SavePrefab()
        {
            var path = EditorUtility.OpenFolderPanel("选择保存路径", Application.dataPath, "").Log();
            path = $"{path}/{Type.Name}_Prefab.asset";
            path = AssetDatabase.GenerateUniqueAssetPath(path);
            if (string.IsNullOrEmpty(path)) return;
            var data = YuoComponentPrefabData.Create(this);
            AssetDatabase.CreateAsset(data, path.Log());
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
#endif