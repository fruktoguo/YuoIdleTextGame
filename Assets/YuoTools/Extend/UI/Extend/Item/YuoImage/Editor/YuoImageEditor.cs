using System.Linq;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;


[CustomEditor(typeof(YuoImage), true)]
[CanEditMultipleObjects]
public class YuoImageEditor : ImageEditor
{
    private SerializedProperty mUseCustomShape;
    private SerializedProperty mShape;
    private GUIStyle headerStyle;

    protected override void OnEnable()
    {
        base.OnEnable();
        mUseCustomShape = serializedObject.FindProperty("UseCustomShape");
        mShape = serializedObject.FindProperty("Shape");
    }

    private void InitStyles()
    {
        if (headerStyle == null)
        {
            headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                margin = new RectOffset(0, 0, 8, 8)
            };
        }
    }

    public override void OnInspectorGUI()
    {
        InitStyles();

        EditorGUILayout.Space(8);
        DrawSeparator();
        EditorGUILayout.LabelField("自定义形状设置", headerStyle);

        EditorGUI.BeginChangeCheck();

        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.PropertyField(mUseCustomShape, new GUIContent("启用自定义形状"));

            if (mUseCustomShape.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.Space(2);

                // 当前形状类型和更改按钮放在同一行
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("当前类型:", GUILayout.Width(80));

                    // 左半部分：显示当前类型
                    using (new EditorGUILayout.HorizontalScope(
                               GUILayout.Width(EditorGUIUtility.currentViewWidth / 2 - 65)))
                    {
                        if (mShape.managedReferenceValue != null)
                        {
                            // 修改这里，使用 ShapeName 而不是类型名
                            var shape = mShape.managedReferenceValue as IYuoUIShape;
                            EditorGUILayout.LabelField(shape?.ShapeName ?? "未知形状",
                                EditorStyles.boldLabel);
                        }
                    }


                    // 右半部分：类型选择按钮
                    if (GUILayout.Button("更改类型", EditorStyles.miniButton,
                            GUILayout.Width(EditorGUIUtility.currentViewWidth / 2 - 65)))
                    {
                        ShowShapeTypeMenu();
                    }
                }

                // 形状属性
                if (mShape.managedReferenceValue != null)
                {
                    EditorGUILayout.Space(5);
                    DrawPropertiesBox();
                }

                EditorGUI.indentLevel--;
            }
        }

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
        }

        EditorGUILayout.Space(8);
        DrawSeparator();
        base.OnInspectorGUI();
    }

    private void DrawPropertiesBox()
    {
        EditorGUILayout.LabelField("形状参数", EditorStyles.boldLabel);
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            SerializedProperty iterator = mShape.Copy();
            SerializedProperty endProperty = iterator.GetEndProperty();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, endProperty))
            {
                if (iterator.propertyPath == mShape.propertyPath) continue;

                EditorGUILayout.PropertyField(iterator, true);
                enterChildren = false;
            }
        }
    }

    private void ShowShapeTypeMenu()
    {
        var menu = new GenericMenu();
        var types = GetShapeTypes();

        // 获取当前类型
        var currentShape = mShape.managedReferenceValue as IYuoUIShape;
        var currentType = currentShape?.GetType();

        foreach (var type in types)
        {
            // 创建临时实例以获取 ShapeName
            var tempInstance = System.Activator.CreateInstance(type) as IYuoUIShape;
            if (tempInstance != null)
            {
                // 检查是否与当前类型相同
                bool isCurrentType = currentType == type;

                menu.AddItem(new GUIContent(tempInstance.ShapeName), isCurrentType, () =>
                {
                    // 如果选择的类型与当前类型不同，才进行更改
                    if (!isCurrentType)
                    {
                        mShape.managedReferenceValue = System.Activator.CreateInstance(type);
                        serializedObject.ApplyModifiedProperties();
                    }
                });
            }
        }

        menu.ShowAsContext();
    }


    private System.Type[] GetShapeTypes()
    {
        return System.AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => typeof(IYuoUIShape).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
            .ToArray();
    }

    private void DrawSeparator()
    {
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
    }
}