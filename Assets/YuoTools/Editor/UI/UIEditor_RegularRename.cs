using UnityEditor;

namespace YuoTools.Editor
{
    public static class UIEditor_RegularRename
    {
        [MenuItem("GameObject/YuoUI命名/正则批量修改", false, -2)]
        public static void ChangeNameRegular()
        {
            if (!UIEditor_SingleCheck.SingleCheck()) return;

            RegularRenameObjectsWindow.ShowWindow();
        }
    }
} 