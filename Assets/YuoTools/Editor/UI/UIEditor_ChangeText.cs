using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using YuoTools.Editor;

namespace YuoTools.Editor
{
    public static class UIEditor_ChangeText
    {
        [MenuItem("GameObject/YuoUI/将Text改成TMP", false, -2)]
        public static void ChangeTextToTMP()
        {
            if (!UIEditor_SingleCheck.SingleCheck()) return;
            foreach (var text in EditorTools.GetAllSelectComponent<Text>(true))
            {
                var go = text.gameObject;
                var message = text.text;
                Object.DestroyImmediate(text);
                go.AddComponent<TextMeshProUGUI>().text = message;
            }
        }

        [MenuItem("GameObject/YuoUI/将TMP改成Text", false, -2)]
        public static void ChangeTMPToText()
        {
            if (!UIEditor_SingleCheck.SingleCheck()) return;
            foreach (var text in EditorTools.GetAllSelectComponent<TextMeshProUGUI>(true))
            {
                var go = text.gameObject;
                var message = text.text;
                Undo.DestroyObjectImmediate(text);
                Undo.AddComponent<Text>(go).text = message;
            }
        }
    }
} 