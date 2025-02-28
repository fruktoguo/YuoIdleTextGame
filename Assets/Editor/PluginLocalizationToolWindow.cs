using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.EditorCoroutines.Editor;

public class PluginLocalizationToolWindow : OdinEditorWindow
{
    [MenuItem("Tools/Custom Editor Window")]
    private static void OpenWindow()
    {
        GetWindow<PluginLocalizationToolWindow>().Show();
    }

    [BoxGroup("文件夹设置")] [FolderPath, ShowInInspector]
    private string currentFolderPath;

    [BoxGroup("文件夹设置")]
    [Button("选择脚本文件夹")]
    private void SelectScriptFolder()
    {
        string path = EditorUtility.OpenFolderPanel("选择脚本文件夹", "", "");
        if (!string.IsNullOrEmpty(path))
        {
            currentFolderPath = path;
            LoadScriptsFromFolderAsync(path);
        }
    }

    [HorizontalGroup("MainLayout")]
    [BoxGroup("MainLayout/Overview")]
    [ListDrawerSettings(ShowIndexLabels = true, OnTitleBarGUI = "DrawRefreshButton")]
    [ShowInInspector]
    private List<string> scriptFiles = new List<string>();

    [HorizontalGroup("MainLayout")] [BoxGroup("MainLayout/Preview")] [TextArea(10, 20)] [ShowInInspector]
    private string previewContent = "";

    [HorizontalGroup("MainLayout")] [BoxGroup("MainLayout/Localized")] [TextArea(10, 20)] [ShowInInspector]
    private string localizedContent = "";

    [BoxGroup("MainLayout/Overview")] [ShowInInspector] [OnValueChanged("UpdatePreview")]
    private int selectedFileIndex = -1;

    [BoxGroup("MainLayout/Overview")] [ShowInInspector]
    private int currentPage = 0;

    [BoxGroup("MainLayout/Overview")] [ShowInInspector]
    private int itemsPerPage = 20;

    private Dictionary<string, string> fileContentCache = new Dictionary<string, string>();

    private void DrawRefreshButton()
    {
        if (GUILayout.Button("刷新", EditorStyles.miniButton))
        {
            RefreshFileList();
        }
    }

    private void RefreshFileList()
    {
        if (!string.IsNullOrEmpty(currentFolderPath))
        {
            LoadScriptsFromFolderAsync(currentFolderPath);
        }
    }

    private async void LoadScriptsFromFolderAsync(string folderPath)
    {
        await Task.Run(() =>
        {
            scriptFiles.Clear();
            string[] files = Directory.GetFiles(folderPath, "*.cs", SearchOption.AllDirectories);
            scriptFiles.AddRange(files);
        });

        ScheduleRepaint();
    }

    private void UpdatePreview()
    {
        if (selectedFileIndex >= 0 && selectedFileIndex < scriptFiles.Count)
        {
            string filePath = scriptFiles[selectedFileIndex];
            previewContent = GetFileContent(filePath);
        }
        else
        {
            previewContent = "";
        }
    }

    private string GetFileContent(string filePath)
    {
        if (!fileContentCache.ContainsKey(filePath))
        {
            fileContentCache[filePath] = File.ReadAllText(filePath);
        }

        return fileContentCache[filePath];
    }

    [BoxGroup("MainLayout/Localized")]
    [Button("翻译")]
    private void TranslateContent()
    {
        // 这里应该调用实际的翻译API
        // 示例中我们只是简单地复制预览内容
        localizedContent = previewContent;
    }

    [BoxGroup("MainLayout/Localized")]
    [Button("保存汉化文本")]
    private void SaveLocalizedText()
    {
        if (selectedFileIndex >= 0 && selectedFileIndex < scriptFiles.Count)
        {
            string filePath = scriptFiles[selectedFileIndex];
            string localizedFilePath = Path.ChangeExtension(filePath, ".localized.cs");
            File.WriteAllText(localizedFilePath, localizedContent);
            Debug.Log($"已保存汉化文本到: {localizedFilePath}");
        }
        else
        {
            Debug.LogWarning("请先选择一个文件");
        }
    }

    [BoxGroup("MainLayout/Overview")]
    [Button("上一页")]
    private void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            ScheduleRepaint();
        }
    }

    [BoxGroup("MainLayout/Overview")]
    [Button("下一页")]
    private void NextPage()
    {
        if ((currentPage + 1) * itemsPerPage < scriptFiles.Count)
        {
            currentPage++;
            ScheduleRepaint();
        }
    }

    private List<string> GetCurrentPageItems()
    {
        return scriptFiles.Skip(currentPage * itemsPerPage).Take(itemsPerPage).ToList();
    }

    private void ScheduleRepaint()
    {
        EditorApplication.delayCall += () =>
        {
            this.Repaint();
            EditorApplication.delayCall -= this.Repaint;
        };
    }

    [Button("加载所有文件内容")]
    private void LoadAllFileContents()
    {
        EditorCoroutineUtility.StartCoroutine(LoadFileContentsCoroutine(), this);
    }

    private System.Collections.IEnumerator LoadFileContentsCoroutine()
    {
        foreach (var file in scriptFiles)
        {
            if (!fileContentCache.ContainsKey(file))
            {
                fileContentCache[file] = File.ReadAllText(file);
                yield return null; // 让出控制权，避免卡顿
            }
        }
    }
}