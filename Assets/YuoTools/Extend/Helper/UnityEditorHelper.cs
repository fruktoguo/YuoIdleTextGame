using System.Threading.Tasks;

namespace YuoTools.Extend.Helper
{
    public class UnityEditorHelper
    {
        public static void ClearConsole()
        {
#if UNITY_EDITOR
            var logEntries = System.Type.GetType("UnityEditor.LogEntries,UnityEditor.dll");
            if (logEntries != null)
            {
                var clearMethod = logEntries.GetMethod("Clear",
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                clearMethod?.Invoke(null, null);
            }
#endif
        }

        public static async Task<bool> ShowConfirmationDialog(string title, string message, string okButton, string cancelButton)
        {
#if UNITY_EDITOR
            var tcs = new TaskCompletionSource<bool>();
            bool result = UnityEditor.EditorUtility.DisplayDialog(title, message, okButton, cancelButton);
            tcs.SetResult(result);
            return await tcs.Task;
#else
            return await Task.FromResult(false);
#endif
        }
    }
}