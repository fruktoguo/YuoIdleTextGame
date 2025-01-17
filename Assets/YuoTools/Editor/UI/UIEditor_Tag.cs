using UnityEditor;
using UnityEngine;
using YuoTools.UI;

namespace YuoTools.Editor
{
    public static class UIEditor_Tag
    {
        static void ChangeUITag(string thisTag, Object go)
        {
            foreach (var tag in SpawnUICodeConfig.AllTag)
            {
                if (go.name.StartsWith(tag))
                {
                    if (tag == thisTag)
                    {
                        go.name = go.name.Replace(thisTag, "");
                    }
                    else
                    {
                        go.name = go.name.Replace(tag, "");
                        go.name = thisTag + go.name;
                    }

                    return;
                }
            }

            go.name = thisTag + go.name;
        }

        [MenuItem("GameObject/YuoUI命名/切换是否被框架检索_C", false, -2)]
        public static void ChangeNameForFrame()
        {
            if (!UIEditor_SingleCheck.SingleCheck()) return;

            Object[] selections = Selection.objects;
            Undo.SetCurrentGroupName($"切换是否被框架检索_C [数量:{selections.Length}]");
            Undo.RecordObjects(selections, "切换是否被框架检索_C");
            foreach (Object go in selections)
            {
                ChangeUITag(SpawnUICodeConfig.UIComponentTag, go);
            }

            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
        }

        [MenuItem("GameObject/YuoUI命名/切换是否为变体的组件_CV", false, -2)]
        public static void ChangeNameForFrameVariant()
        {
            if (!UIEditor_SingleCheck.SingleCheck()) return;
            Object[] selections = Selection.objects;
            Undo.SetCurrentGroupName($"切换是否为变体的组件_CV [数量:{selections.Length}]");
            Undo.RecordObjects(selections, "切换是否为变体的组件_CV");
            foreach (var go in selections)
            {
                ChangeUITag(SpawnUICodeConfig.VariantChildComponentTag, go);
            }

            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
        }

        [MenuItem("GameObject/YuoUI命名/切换UI子面板_D", false, -2)]
        public static void ChangeNameForChild()
        {
            if (!UIEditor_SingleCheck.SingleCheck()) return;
            foreach (var go in Selection.gameObjects)
            {
                ChangeUITag(SpawnUICodeConfig.ChildUITag, go);
            }
        }

        [MenuItem("GameObject/YuoUI命名/切换UI子面板变体_DV", false, -2)]
        public static void ChangeNameForChildVariant()
        {
            if (!UIEditor_SingleCheck.SingleCheck()) return;
            foreach (var go in Selection.gameObjects)
            {
                ChangeUITag(SpawnUICodeConfig.VariantChildUITag, go);
            }
        }

        [MenuItem("GameObject/YuoUI命名/切换公共UI_G", false, -2)]
        public static void ChangeNameForG()
        {
            if (!UIEditor_SingleCheck.SingleCheck()) return;
            foreach (var go in Selection.gameObjects)
            {
                ChangeUITag(SpawnUICodeConfig.GeneralUITag, go);
            }
        }

        [MenuItem("GameObject/YuoUI命名/移除空格", false, -2)]
        public static void ChangeNameRemoveSpace()
        {
            if (!UIEditor_SingleCheck.SingleCheck()) return;
            foreach (var go in Selection.gameObjects)
            {
                go.name = go.name.Replace(" ", "");
            }
        }
    }
} 