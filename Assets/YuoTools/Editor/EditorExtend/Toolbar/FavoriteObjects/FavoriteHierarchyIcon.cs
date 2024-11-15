// using UnityEditor;
// using UnityEngine;
//
// namespace YuoTools.Editor.EditorExtend.YuoTools.Editor.EditorExtend
// {
//     [InitializeOnLoad]
//     public static class FavoriteHierarchyIcon
//     {
//         static FavoriteHierarchyIcon()
//         {
//             EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
//         }
//
//         private static void OnHierarchyGUI(int instanceID, Rect selectionRect)
//         {
//             // 获取对应的GameObject
//             GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
//             if (obj == null) return;
//
//             // 定义Toggle的尺寸和位置
//             float toggleSize = 16f;
//             Rect toggleRect = new Rect(selectionRect.x - toggleSize - 2,
//                 selectionRect.y + (selectionRect.height - toggleSize) / 2, toggleSize, toggleSize);
//
//             // 绘制Toggle
//             bool isFavorite = FavoriteManager.IsFavorite(obj);
//             bool newFavorite = GUI.Toggle(toggleRect, isFavorite, isFavorite ? "★" : "☆", "Button");
//
//             if (newFavorite != isFavorite)
//             {
//                 FavoriteManager.ToggleFavorite(obj);
//             }
//
//             // 高亮显示收藏物体
//             if (isFavorite)
//             {
//                 // 绘制一个背景颜色
//                 EditorGUI.DrawRect(
//                     new Rect(selectionRect.x, selectionRect.y, selectionRect.width, selectionRect.height),
//                     new Color(1f, 0.92f, 0.016f, 0.3f));
//             }
//         }
//     }
// }