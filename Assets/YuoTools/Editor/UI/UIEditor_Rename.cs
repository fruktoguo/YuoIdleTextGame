using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using YuoTools.Editor;
using YuoTools.Extend.AI;
using YuoTools.Extend.Helper;

namespace YuoTools.Editor
{
    public static class UIEditor_Rename
    {
        [MenuItem("GameObject/YuoUI命名/将UI物体的名字改成身上脚本的", false, -2)]
        public static void ChangeNameForObject()
        {
            if (!UIEditor_SingleCheck.SingleCheck()) return;

            var allObjects = Selection.gameObjects; // 直接获取选中的 GameObject

            if (allObjects.Length > 0)
            {
                Undo.SetCurrentGroupName($"将UI物体的名字改成身上脚本的");
                Undo.RecordObjects(allObjects, "ChangeName"); // 记录选中的 GameObject

                foreach (var go in allObjects)
                {
                    var image = go.GetComponent<Image>();
                    if (image != null)
                    {
                        go.name = image.sprite?.name;
                    }

                    var text = go.GetComponent<Text>();
                    if (text != null)
                    {
                        go.name = text.text;
                    }

                    var tmp = go.GetComponent<TextMeshProUGUI>();
                    if (tmp != null)
                    {
                        go.name = tmp.text;
                    }

                    var sprite = go.GetComponent<SpriteRenderer>();
                    if (sprite != null)
                    {
                        go.name = sprite.sprite?.name;
                    }
                }
            }

            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
        }

        [MenuItem("GameObject/YuoUI命名/AI辅助命名", false, -2)]
        public static async void ChangeNameForAI()
        {
            if (!UIEditor_SingleCheck.SingleCheck()) return;
            var objs = Selection.gameObjects;


            string[] names = new string[objs.Length];
            for (int i = 0; i < objs.Length; i++)
            {
                var obj = objs[i];
                var info = "根据以下信息给出一个符合Unity规范的名字,使用英文,只返回文本不要加任何符号\n";
                var texts = obj.GetComponentsInChildren<Text>(true);
                var tmps = obj.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var text in texts)
                {
                    info +=
                        $"内容: {text.text} path {TransformHelper.GetRelativePath(text.transform, obj.transform)}\n";
                }

                foreach (var text in tmps)
                {
                    info +=
                        $"内容: {text.text} path {TransformHelper.GetRelativePath(text.transform, obj.transform)}\n";
                }

                var result = await AIHelper.GenerateText(info);
                names[i] = result;
            }

            Undo.SetCurrentGroupName($"AI辅助命名");

            // 在修改之前记录所有 GameObject
            Undo.RecordObjects(objs, "ChangeName");

            for (int i = 0; i < objs.Length; i++)
            {
                objs[i].name = names[i];
            }

            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
        }

        [MenuItem("GameObject/YuoUI命名/将文本改成UI名字", false, -2)]
        public static void ChangeTextForName()
        {
            if (!UIEditor_SingleCheck.SingleCheck()) return;
            var allObjects = Selection.gameObjects; // 直接获取选中的 GameObject

            if (allObjects.Length > 0)
            {
                Undo.RecordObjects(allObjects, "ChangeTextForName"); // 记录选中的 GameObject

                foreach (var go in allObjects)
                {
                    var text = go.GetComponent<Text>();
                    if (text != null)
                    {
                        text.text = go.name;
                    }

                    var tmp = go.GetComponent<TextMeshProUGUI>();
                    if (tmp != null)
                    {
                        tmp.text = go.name;
                    }
                }
            }

            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
        }
    }
}