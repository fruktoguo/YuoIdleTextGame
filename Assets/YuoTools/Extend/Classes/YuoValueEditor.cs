#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace YuoTools
{
    public partial class YuoValue
    {
        protected class YuoValueDrawer : OdinValueDrawer<YuoValue>
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
                        EditorGUI.TextField(valueRect, value.GetValueOnInspector(), valueStyle);
                    }
                }

                // 恢复GUI状态
                GUI.color = originalColor;
            }
        }
    }
}
#endif