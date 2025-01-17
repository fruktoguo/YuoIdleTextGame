// using System;
// using System.Collections.Generic;
// using TMPro;
// using UnityEditor;
// using UnityEngine;
// using UnityEngine.UI;
// using YuoTools.Editor;
// using YuoTools.UI;
// using Object = UnityEngine.Object;
//
// [Obsolete("Obsolete")]
// public class SpawnUICodeEditor
// {
//     [MenuItem("GameObject/YuoUI/创建UI", false, -2)]
//     public static void CreateUI()
//     {
//         GameObject go = Resources.Load<GameObject>("YuoUI/UI_Window");
//         go = GameObject.Instantiate(go, Selection.activeGameObject.transform);
//         go.name = "New_UI_Window";
//     }
//
//     [MenuItem("GameObject/YuoUI/将Text改成TMP", false, -2)]
//     public static void ChangeTextToTMP()
//     {
//         if (!SingleCheck()) return;
//         foreach (var text in EditorTools.GetAllSelectComponent<Text>(true))
//         {
//             var go = text.gameObject;
//             var message = text.text;
//             Object.DestroyImmediate(text);
//             go.AddComponent<TextMeshProUGUI>().text = message;
//         }
//     }
//
//     [MenuItem("GameObject/YuoUI/将TMP改成Text", false, -2)]
//     public static void ChangeTMPToText()
//     {
//         if (!SingleCheck()) return;
//         foreach (var text in EditorTools.GetAllSelectComponent<TextMeshProUGUI>(true))
//         {
//             var go = text.gameObject;
//             var message = text.text;
//             Undo.DestroyObjectImmediate(text);
//             Undo.AddComponent<Text>(go).text = message;
//         }
//     }
//
//     [MenuItem("GameObject/YuoUI命名/将UI物体的名字改成身上脚本的", false, -2)]
//     public static void ChangeNameForSprite()
//     {
//         if (!SingleCheck()) return;
//         Undo.SetCurrentGroupName($"将UI物体的名字改成身上脚本的");
//         var images = EditorTools.GetAllSelectComponent<Image>(true);
//         if (images?.Count > 0)
//         {
//             Undo.RecordObjects(images.ToArray(), "ChangeName");
//             foreach (var image in images)
//             {
//                 image.name = image.sprite?.name;
//             }
//         }
//
//         var texts = EditorTools.GetAllSelectComponent<Text>(true);
//         var tmps = EditorTools.GetAllSelectComponent<TextMeshProUGUI>(true);
//         var all = new List<Object>();
//         all.AddRange(texts);
//         all.AddRange(tmps);
//         if (all.Count > 0)
//         {
//             Undo.RecordObjects(all.ToArray(), "ChangeName");
//             foreach (var text in texts)
//             {
//                 text.name = text.text;
//             }
//
//             foreach (var text in tmps)
//             {
//                 text.name = text.text;
//             }
//         }
//
//         var sprites = EditorTools.GetAllSelectComponent<SpriteRenderer>(true);
//         if (sprites?.Count > 0)
//         {
//             Undo.RecordObjects(sprites.ToArray(), "ChangeName");
//             foreach (var sprite in sprites)
//             {
//                 sprite.name = sprite.sprite.name;
//             }
//         }
//
//         Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
//     }
//
//     [MenuItem("GameObject/YuoUI命名/将文本改成UI名字", false, -2)]
//     public static void ChangeTextForName()
//     {
//         if (!SingleCheck()) return;
//         var texts = EditorTools.GetAllSelectComponent<Text>(true);
//         var tmps = EditorTools.GetAllSelectComponent<TextMeshProUGUI>(true);
//
//         var all = new List<Object>();
//         all.AddRange(texts);
//         all.AddRange(tmps);
//         Undo.RecordObjects(all.ToArray(), "ChangeTextForName");
//
//         foreach (var text in texts)
//         {
//             text.text = text.name;
//         }
//
//         foreach (var text in tmps)
//         {
//             text.text = text.name;
//         }
//     }
//
//     private static long _lastTime = long.MinValue;
//
//     private static bool SingleCheck()
//     {
//         if (_lastTime.Equals(System.DateTime.Now.Ticks / 10000000))
//         {
//             return false;
//         }
//
//         _lastTime = DateTime.Now.Ticks / 10000000;
//         return true;
//     }
//
//     static void ChangeUITag(string thisTag, Object go)
//     {
//         foreach (var tag in SpawnUICodeConfig.AllTag)
//         {
//             if (go.name.StartsWith(tag))
//             {
//                 if (tag == thisTag)
//                 {
//                     go.name = go.name.Replace(thisTag, "");
//                 }
//                 else
//                 {
//                     go.name = go.name.Replace(tag, "");
//                     go.name = thisTag + go.name;
//                 }
//
//                 return;
//             }
//         }
//
//         go.name = thisTag + go.name;
//     }
//
//     [MenuItem("GameObject/YuoUI命名/切换是否被框架检索_C", false, -2)]
//     public static void ChangeNameForFrame()
//     {
//         if (!SingleCheck()) return;
//
//         Object[] selections = Selection.objects;
//         Undo.SetCurrentGroupName($"切换是否被框架检索_C [数量:{selections.Length}]");
//         Undo.RecordObjects(selections, "切换是否被框架检索_C");
//         foreach (Object go in selections)
//         {
//             ChangeUITag(SpawnUICodeConfig.UIComponentTag, go);
//         }
//
//         Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
//     }
//
//     [MenuItem("GameObject/YuoUI命名/切换是否为变体的组件_CV", false, -2)]
//     public static void ChangeNameForFrameVariant()
//     {
//         if (!SingleCheck()) return;
//         Object[] selections = Selection.objects;
//         Undo.SetCurrentGroupName($"切换是否为变体的组件_CV [数量:{selections.Length}]");
//         Undo.RecordObjects(selections, "切换是否为变体的组件_CV");
//         foreach (var go in selections)
//         {
//             ChangeUITag(SpawnUICodeConfig.VariantChildComponentTag, go);
//         }
//
//         Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
//     }
//
//     [MenuItem("GameObject/YuoUI命名/切换UI子面板_D", false, -2)]
//     public static void ChangeNameForChild()
//     {
//         if (!SingleCheck()) return;
//         foreach (var go in Selection.gameObjects)
//         {
//             ChangeUITag(SpawnUICodeConfig.ChildUITag, go);
//         }
//     }
//
//     [MenuItem("GameObject/YuoUI命名/切换UI子面板变体_DV", false, -2)]
//     public static void ChangeNameForChildVariant()
//     {
//         if (!SingleCheck()) return;
//         foreach (var go in Selection.gameObjects)
//         {
//             ChangeUITag(SpawnUICodeConfig.VariantChildUITag, go);
//         }
//     }
//
//     [MenuItem("GameObject/YuoUI命名/切换公共UI_G", false, -2)]
//     public static void ChangeNameForG()
//     {
//         if (!SingleCheck()) return;
//         foreach (var go in Selection.gameObjects)
//         {
//             ChangeUITag(SpawnUICodeConfig.GeneralUITag, go);
//         }
//     }
//
//     [MenuItem("GameObject/YuoUI命名/移除空格", false, -2)]
//     public static void ChangeNameRemoveSpace()
//     {
//         if (!SingleCheck()) return;
//         foreach (var go in Selection.gameObjects)
//         {
//             go.name = go.name.Replace(" ", "");
//         }
//     }
//
//     [MenuItem("GameObject/YuoUI命名/正则批量修改", false, -2)]
//     public static void ChangeNameRegular()
//     {
//         if (!SingleCheck()) return;
//
//         RegularRenameObjectsWindow.ShowWindow();
//     }
// }