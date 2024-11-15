using System;
using System.Globalization;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using YuoTools.Extend.Helper;

#if UNITY_EDITOR_WIN
namespace YuoTools.Editor
{
    public class YuoToolsManager : EditorWindow
    {
        [MenuItem("Tools/YuoTools Manager")]
        private static void OpenWindow()
        {
            var window = GetWindow<YuoToolsManager>();

            window.titleContent = new GUIContent("YuoTools Manager");
            window.minSize = new Vector2(625, 125);
            window.maxSize = new Vector2(626, 126);
            window.Show();
        }

        private string _cachedFolderSize;
        private string _lastWriteTime;
        private string _localWriteTime;

        public const string FilePath = @"C:\YuoTools\Main";
        public const string BackupPath = @"C:\YuoTools\Backup";

        string LocalPath => $"{Application.dataPath}/YuoTools";

        GUIStyle labelStyle;
        GUIStyle buttonStyle;

        private void OnGUI()
        {
            labelStyle ??= new GUIStyle(EditorStyles.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.gray }
            };

            buttonStyle ??= new GUIStyle(GUI.skin.button)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white },
                fixedHeight = 40,
                fixedWidth = 300,
                border = new RectOffset(12, 12, 12, 12),
                margin = new RectOffset(10, 10, 10, 10),
                padding = new RectOffset(10, 10, 10, 10)
            };

            _localWriteTime = ReadTime(LocalPath);
            EditorGUILayout.LabelField("最后修改时间", GetNewWriteTime(), labelStyle);
            EditorGUILayout.LabelField("本地最后修改时间", _localWriteTime, labelStyle);
            EditorGUILayout.LabelField("文件夹大小", GetCachedFolderSize(), labelStyle);

            EditorGUILayout.BeginHorizontal(); // 开始水平布局


            if (GUILayout.Button("上传", buttonStyle))
            {
                Upload();
            }

            string downloadButtonText;
            bool isUpdated = false;
            if (Directory.Exists(FilePath))
            {
                downloadButtonText = "更新";
                if (DateTime.TryParse(GetNewWriteTime(), out DateTime newWriteTime) &&
                    DateTime.TryParse(_localWriteTime, out DateTime localWriteTime) && newWriteTime <= localWriteTime)
                {
                    downloadButtonText += " √️";
                }
                else
                {
                    downloadButtonText += " ⬆️";
                    isUpdated = true;
                }
            }
            else
            {
                downloadButtonText = "无源文件";
            }

            if (!isUpdated)
            {
                buttonStyle.normal.textColor = Color.gray;
            }

            if (GUILayout.Button(downloadButtonText, buttonStyle))
            {
                Download();
            }

            // 恢复按钮样式
            buttonStyle.normal.textColor = Color.white;

            EditorGUILayout.EndHorizontal(); // 结束水平布局
        }

        string GetNewWriteTime()
        {
            var time = "没有文件夹";
            if (Directory.Exists(FilePath))
            {
                return ReadTime(FilePath);
            }

            return time;
        }

        string GetFolderSize()
        {
            if (!Directory.Exists(FilePath))
            {
                return "没有文件夹";
            }

            long size = Directory.GetFiles(FilePath, "*", SearchOption.AllDirectories)
                .Sum(t => (new FileInfo(t)).Length);
            return FormatSize(size);
        }

        string FormatSize(long size)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = size;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }

        string GetCachedFolderSize()
        {
            var currentWriteTime = ReadTime(FilePath);
            if (currentWriteTime != _lastWriteTime)
            {
                _lastWriteTime = currentWriteTime;
                _cachedFolderSize = GetFolderSize();
            }

            return _cachedFolderSize;
        }

        const int MaxBackupCount = 10; // 最大备份数量

        public async void Upload()
        {
            EditorUtility.DisplayProgressBar("上传", "正在上传文件...", 0.0f);
            var cts = new CancellationTokenSource();
            var timeoutTask = Task.Delay(30000, cts.Token); // 30秒超时
            var uploadTask = Task.Run(() =>
            {
                // 获取所有备份目录
                var backupDirs = Directory.GetDirectories(BackupPath)
                    .OrderBy(Directory.GetCreationTime)
                    .ToList();

                // 删除多余的备份
                while (backupDirs.Count >= MaxBackupCount)
                {
                    Directory.Delete(backupDirs[0], true);
                    backupDirs.RemoveAt(0);
                }

                // 创建新的备份目录
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
                string backupDir = Path.Combine(BackupPath, timestamp);
                FileHelper.CheckOrCreateDirectoryPath(backupDir);

                // 将当前本地文件夹内容复制到备份目录
                FileHelper.CopyDirectory(LocalPath, backupDir);

                // 清理主路径并复制新的文件
                FileHelper.CheckOrCreateDirectoryPath(FilePath);
                WriteTime(LocalPath, DateTime.Now.ToString(CultureInfo.InvariantCulture));
                FileHelper.CleanDirectory(FilePath);
                FileHelper.CopyDirectory(LocalPath, FilePath);
                WindowsHelper.OpenDirectory(FilePath);
            }, cts.Token);

            float progress = 0.0f;
            while (!uploadTask.IsCompleted && !timeoutTask.IsCompleted)
            {
                progress += 0.01f;
                EditorUtility.DisplayProgressBar("上传", "正在上传文件...", progress);
                await Task.Delay(100, cts.Token); // 每0.1秒更新一次进度条
            }

            var completedTask = await Task.WhenAny(uploadTask, timeoutTask);
            if (completedTask == timeoutTask)
            {
                Debug.LogError("上传操作超时");
            }
            else
            {
                cts.Cancel(); // 取消超时任务
            }

            EditorUtility.ClearProgressBar();

            _cachedFolderSize = GetFolderSize();
        }


        public async void Download()
        {
            var sourceTime = ReadTime(FilePath);
            if (sourceTime.IsNullOrSpace())
            {
                Debug.Log("没有找到YuoTools文件夹");
                return;
            }

            var localTime = ReadTime(LocalPath);
            if (sourceTime == localTime)
            {
                var isConfirm = EditorUtility.DisplayDialog("提示", "本地YuoTools文件夹已经是最新版本,确认重新下载？", "覆盖", "取消");
                if (!isConfirm)
                {
                    return;
                }
            }
            else
            {
                var isConfirm = EditorUtility.DisplayDialog("提示", "是否下载最新YuoTools文件夹？", "覆盖", "取消");
                if (!isConfirm)
                {
                    return;
                }
            }

            EditorUtility.DisplayProgressBar("下载", "正在下载文件...", 0.0f);
            var cts = new CancellationTokenSource();
            var timeoutTask = Task.Delay(30000, cts.Token); // 30秒超时
            var downloadTask = Task.Run(() =>
            {
                FileHelper.CheckOrCreateDirectoryPath(BackupPath);
                FileHelper.CleanDirectory(BackupPath);
                FileHelper.CopyDirectory(LocalPath, BackupPath);
                FileHelper.CleanDirectory(LocalPath);
                FileHelper.CopyDirectory(FilePath, LocalPath);
                return Task.CompletedTask;
            }, cts.Token);

            float progress = 0.0f;
            while (!downloadTask.IsCompleted && !timeoutTask.IsCompleted)
            {
                progress += 0.01f;
                EditorUtility.DisplayProgressBar("下载", "正在下载文件...", progress);
                await Task.Delay(100, cts.Token); // 每0.1秒更新一次进度条
            }

            var completedTask = await Task.WhenAny(downloadTask, timeoutTask);
            if (completedTask == timeoutTask)
            {
                Debug.LogError("下载操作超时");
            }
            else
            {
                cts.Cancel(); // 取消超时任务
            }

            EditorUtility.ClearProgressBar();
            
            //刷新
            AssetDatabase.Refresh();
        }

        const string WriteTimePath = "WriteTime.txt";

        string ReadTime(string path)
        {
            if (!Directory.Exists(path))
            {
                return "";
            }

            var readmePath = $"{path}/{WriteTimePath}";
            FileHelper.CheckOrCreateFile(readmePath);
            var readme = FileHelper.ReadAllText(readmePath);

            return readme;
        }

        void WriteTime(string path, string time)
        {
            var readmePath = $"{path}/{WriteTimePath}";
            FileHelper.CheckOrCreateFile(readmePath);
            FileHelper.WriteAllText(readmePath, time);
        }
    }
}
#endif