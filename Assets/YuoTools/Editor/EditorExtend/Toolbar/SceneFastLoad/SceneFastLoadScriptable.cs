using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YuoTools.Editor.EditorExtend
{
    [CreateAssetMenu(fileName = "SceneFastLoad", menuName = "YuoTools/SceneFastLoad")]
    public class SceneFastLoadScriptable : ScriptableObject
    {
        public List<SceneAsset> scenes;
    }
}