using UnityEngine;

namespace YuoTools.Extend.Helper
{
    public static class TransformHelper
    {
        public static string GetRelativePath(Transform child, Transform parent)
        {
            if (child == parent)
            {
                return parent.name;
            }

            var path = child.name;
            Transform nowParent = child.parent;
            while (nowParent != parent && nowParent != null)
            {
                path = nowParent.name + "/" + path;
                nowParent = nowParent.parent;
            }

            return path;
        }
    }
}