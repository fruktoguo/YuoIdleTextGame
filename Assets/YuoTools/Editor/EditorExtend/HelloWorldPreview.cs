// namespace YuoTools.Editor.EditorExtend
// {
//     using UnityEngine;
//     using UnityEditor;
//
//     [CustomPreview(typeof(GameObject))]
//     public class HelloWorldPreview : ObjectPreview
//     {
//         public override bool HasPreviewGUI()
//         {
//             return true;
//         }
//
//         public override void OnPreviewGUI(Rect r, GUIStyle background)
//         {
//             GameObject gameObject = target as GameObject;
//             if (gameObject == null) return;
//
//             GUILayout.BeginArea(r);
//         
//             EditorGUILayout.LabelField("Hello World Preview", EditorStyles.boldLabel);
//             EditorGUILayout.Space();
//
//             EditorGUILayout.LabelField("Name: " + gameObject.name);
//             EditorGUILayout.LabelField("Position: " + gameObject.transform.position);
//             EditorGUILayout.LabelField("Active: " + gameObject.activeSelf);
//
//             if (gameObject.GetComponent<Renderer>() != null)
//             {
//                 EditorGUILayout.LabelField("Has Renderer: Yes");
//                 EditorGUILayout.LabelField("Bounds: " + gameObject.GetComponent<Renderer>().bounds);
//             }
//             else
//             {
//                 EditorGUILayout.LabelField("Has Renderer: No");
//             }
//
//             GUILayout.EndArea();
//         }
//     }
// }