// using System.Collections.Generic;
// using System.IO;
// using UnityEditor;
// using UnityEngine;
//
// namespace YuoTools.Editor.EditorExtend.YuoTools.Editor.EditorExtend
// {
//     [InitializeOnLoad]
//     public static class FavoriteManager
//     {
//         private static FavoriteObjectsData favoriteData;
//
//         static FavoriteManager()
//         {
//             EditorApplication.quitting += SaveFavorites;
//         }
//
//         public static FavoriteObjectsData Data
//         {
//             get
//             {
//                 if (favoriteData == null)
//                 {
//                     favoriteData = Resources.Load<FavoriteObjectsData>("SceneFastLoad");
//                 }
//
//                 return favoriteData;
//             }
//         }
//
//         public static void SaveFavorites()
//         {
//             if (favoriteData != null)
//             {
//                 EditorUtility.SetDirty(favoriteData);
//                 AssetDatabase.SaveAssets();
//             }
//         }
//
//         public static bool IsFavorite(GameObject obj)
//         {
//             string sceneGuid = GetCurrentSceneGuid();
//             if (string.IsNullOrEmpty(sceneGuid)) return false;
//
//             if (Data.sceneFavorites.TryGetValue(sceneGuid, out List<string> favorites))
//             {
//                 string objGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj));
//                 return favorites.Contains(objGuid);
//             }
//
//             return false;
//         }
//
//         public static void ToggleFavorite(GameObject obj)
//         {
//             string sceneGuid = GetCurrentSceneGuid();
//             if (string.IsNullOrEmpty(sceneGuid)) return;
//
//             if (!Data.sceneFavorites.ContainsKey(sceneGuid))
//             {
//                 Data.sceneFavorites[sceneGuid] = new List<string>();
//             }
//
//             string objGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj));
//
//             if (Data.sceneFavorites[sceneGuid].Contains(objGuid))
//             {
//                 Data.sceneFavorites[sceneGuid].Remove(objGuid);
//             }
//             else
//             {
//                 Data.sceneFavorites[sceneGuid].Add(objGuid);
//             }
//
//             SaveFavorites();
//         }
//
//         private static string GetCurrentSceneGuid()
//         {
//             string path = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
//             if (string.IsNullOrEmpty(path)) return null;
//             return AssetDatabase.AssetPathToGUID(path);
//         }
//     }
// }