using System.Collections.Generic;
using UnityEngine;

namespace YuoTools.Editor.EditorExtend
{
    [CreateAssetMenu(fileName = "FavoriteObjectsData", menuName = "YuoTools/FavoriteObjectsData")]
    public class FavoriteObjectsData : ScriptableObject
    {
        // Key: Scene GUID, Value: List of GameObject GUIDs
        public Dictionary<string, List<string>> sceneFavorites = new Dictionary<string, List<string>>();
    }
}