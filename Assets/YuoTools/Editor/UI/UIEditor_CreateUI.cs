using UnityEditor;
using UnityEngine;

namespace YuoTools.Editor
{
    public static class UIEditor_CreateUI
    {
        [MenuItem("GameObject/YuoUI/创建UI", false, -2)]
        public static void CreateUI()
        {
            GameObject go = Resources.Load<GameObject>("YuoUI/UI_Window");
            go = GameObject.Instantiate(go, Selection.activeGameObject.transform);
            go.name = "New_UI_Window";
        }
    }
} 