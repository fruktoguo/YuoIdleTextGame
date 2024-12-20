using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;

#if UNITY_EDITOR_WIN
namespace YuoTools.Editor
{
    public class YuoToolsChangeViewer : EditorWindow
    {
        private Vector2 _scrollPosition;
        private List<FileChangeInfo> _changedFiles = new List<FileChangeInfo>();
        private bool _isAnalyzing;
        private GUIStyle _headerStyle;
        private GUIStyle _fileStyle;

        // 分页相关
        private int _currentPage = 0;
        private const int ItemsPerPage = 50;
        private int _totalPages = 0;

        // 缓存相关
        private static Dictionary<string, DateTime> _fileTimeCache = new Dictionary<string, DateTime>();
        private DateTime _lastAnalyzeTime;

        // 搜索和过滤
        private string _searchString = "";
        private ChangeType? _filterType = null;
        private List<FileChangeInfo> _filteredFiles = new List<FileChangeInfo>();

        public static void ShowWindow()
        {
            var window = GetWindow<YuoToolsChangeViewer>();
            window.titleContent = new GUIContent("YuoTools 文件变更");
            window.minSize = new Vector2(600, 400);
            window.Show();
            window.AnalyzeChangesAsync();
        }

        private void OnGUI()
        {
            InitializeStyles();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("文件变更列表", _headerStyle);
            EditorGUILayout.Space(5);

            if (_isAnalyzing)
            {
                EditorGUILayout.HelpBox("正在分析文件变更...", MessageType.Info);
                return;
            }

            DrawToolbar();

            // 添加全选和应用按钮
            EditorGUILayout.BeginHorizontal();
            bool newSelectAll = EditorGUILayout.ToggleLeft("全选", _selectAll, GUILayout.Width(50));
            if (newSelectAll != _selectAll)
            {
                _selectAll = newSelectAll;
                foreach (var file in _changedFiles)
                {
                    _selectedFiles[file.FilePath] = _selectAll;
                }
            }

            GUILayout.FlexibleSpace();

            // 添加应用变更按钮
            GUI.enabled = _selectedFiles.Any(x => x.Value);
            if (GUILayout.Button("应用选中的变更", GUILayout.Width(120)))
            {
                ApplySelectedChanges();
            }

            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            ApplyFilters();
            DrawPagination();

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            var pageItems = _filteredFiles
                .Skip(_currentPage * ItemsPerPage)
                .Take(ItemsPerPage);

            foreach (var file in pageItems)
            {
                DrawFileChangeInfoWithCheckbox(file);
            }

            EditorGUILayout.EndScrollView();

            if (_filteredFiles.Count == 0)
            {
                EditorGUILayout.HelpBox("没有检测到文件变更", MessageType.Info);
            }

            DrawStatistics();
        }

        private void DrawFileChangeInfoWithCheckbox(FileChangeInfo file)
        {
            EditorGUILayout.BeginHorizontal(_fileStyle);

            // 确保文件在选择字典中有一个值
            if (!_selectedFiles.ContainsKey(file.FilePath))
            {
                _selectedFiles[file.FilePath] = false;
            }

            // 绘制复选框
            _selectedFiles[file.FilePath] = EditorGUILayout.Toggle(_selectedFiles[file.FilePath], GUILayout.Width(20));

            EditorGUILayout.BeginVertical();

            string changeColor = file.ChangeType switch
            {
                ChangeType.Added => "#4CAF50",
                ChangeType.Modified => "#2196F3",
                ChangeType.Deleted => "#F44336",
                _ => "#000000"
            };

            string changeText = file.ChangeType switch
            {
                ChangeType.Added => "新增",
                ChangeType.Modified => "修改",
                ChangeType.Deleted => "删除",
                _ => "未知"
            };

            string content = $"<color={changeColor}>[{changeText}]</color> {file.FilePath}\n";

            if (file.ChangeType == ChangeType.Modified)
            {
                content += $"本地时间: {file.LocalTime:yyyy-MM-dd HH:mm:ss}\n";
                content += $"远程时间: {file.RemoteTime:yyyy-MM-dd HH:mm:ss}";
            }
            else if (file.ChangeType == ChangeType.Added)
            {
                content += $"创建时间: {file.RemoteTime:yyyy-MM-dd HH:mm:ss}";
            }
            else if (file.ChangeType == ChangeType.Deleted)
            {
                content += $"本地时间: {file.LocalTime:yyyy-MM-dd HH:mm:ss}";
            }

            EditorGUILayout.LabelField(content, _fileStyle);

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("刷新", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                AnalyzeChangesAsync();
            }

            GUILayout.Label($"上次更新: {_lastAnalyzeTime:HH:mm:ss}", EditorStyles.miniLabel, GUILayout.Width(100));

            GUILayout.FlexibleSpace();

            // 搜索框
            GUILayout.Label("搜索:", EditorStyles.miniLabel, GUILayout.Width(40));
            _searchString =
                EditorGUILayout.TextField(_searchString, EditorStyles.toolbarSearchField, GUILayout.Width(200));

            // 过滤下拉框
            GUILayout.Label("类型:", EditorStyles.miniLabel, GUILayout.Width(40));
            string[] filterOptions = new[] { "全部", "新增", "修改", "删除" };
            int filterIndex = _filterType.HasValue ? (int)_filterType.Value + 1 : 0;
            filterIndex = EditorGUILayout.Popup(filterIndex, filterOptions, EditorStyles.toolbarPopup,
                GUILayout.Width(60));
            _filterType = filterIndex == 0 ? null : (ChangeType?)(filterIndex - 1);

            EditorGUILayout.EndHorizontal();
        }

        private void DrawStatistics()
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

            int addedCount = _changedFiles.Count(f => f.ChangeType == ChangeType.Added);
            int modifiedCount = _changedFiles.Count(f => f.ChangeType == ChangeType.Modified);
            int deletedCount = _changedFiles.Count(f => f.ChangeType == ChangeType.Deleted);

            GUILayout.Label($"总计: {_changedFiles.Count} 个文件", GUILayout.Width(120));
            GUILayout.Label($"新增: {addedCount}", GUILayout.Width(80));
            GUILayout.Label($"修改: {modifiedCount}", GUILayout.Width(80));
            GUILayout.Label($"删除: {deletedCount}", GUILayout.Width(80));

            EditorGUILayout.EndHorizontal();
        }

        private void InitializeStyles()
        {
            if (_headerStyle == null)
            {
                _headerStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 14,
                    alignment = TextAnchor.MiddleCenter
                };
            }

            if (_fileStyle == null)
            {
                _fileStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    fontSize = 12,
                    richText = true,
                    padding = new RectOffset(10, 10, 10, 10),
                    margin = new RectOffset(5, 5, 5, 5)
                };
            }
        }

        private void ApplyFilters()
        {
            _filteredFiles = _changedFiles;

            // 应用搜索过滤
            if (!string.IsNullOrEmpty(_searchString))
            {
                _filteredFiles = _filteredFiles
                    .Where(f => f.FilePath.IndexOf(_searchString, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();
            }

            // 应用类型过滤
            if (_filterType.HasValue)
            {
                _filteredFiles = _filteredFiles
                    .Where(f => f.ChangeType == _filterType.Value)
                    .ToList();
            }

            // 更新总页数
            _totalPages = Mathf.CeilToInt(_filteredFiles.Count / (float)ItemsPerPage);
            _currentPage = Mathf.Min(_currentPage, Mathf.Max(0, _totalPages - 1));
        }

        private void DrawPagination()
        {
            if (_totalPages <= 1) return;

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("<<", GUILayout.Width(30)))
                _currentPage = 0;

            if (GUILayout.Button("<", GUILayout.Width(30)))
                _currentPage = Mathf.Max(0, _currentPage - 1);

            GUILayout.Label($"第 {_currentPage + 1}/{_totalPages} 页", GUILayout.Width(80));

            if (GUILayout.Button(">", GUILayout.Width(30)))
                _currentPage = Mathf.Min(_totalPages - 1, _currentPage + 1);

            if (GUILayout.Button(">>", GUILayout.Width(30)))
                _currentPage = _totalPages - 1;

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private async void AnalyzeChangesAsync()
        {
            _isAnalyzing = true;
            _changedFiles.Clear();
            Repaint();

            try
            {
                await Task.Run(() =>
                {
                    string localPath = $"{Application.dataPath}/YuoTools";
                    string remotePath = YuoToolsManager.FilePath;

                    if (!Directory.Exists(remotePath))
                    {
                        return;
                    }

                    CompareDirectories(localPath, remotePath);
                });
            }
            catch (Exception e)
            {
                Debug.LogError($"分析文件变更时发生错误: {e}");
            }
            finally
            {
                _isAnalyzing = false;
                _lastAnalyzeTime = DateTime.Now;
                Repaint();
            }
        }

        private void CompareDirectories(string localPath, string remotePath)
        {
            var localFiles = GetFilesWithCache(localPath);
            var remoteFiles = GetFilesWithCache(remotePath);
            var changes = new List<FileChangeInfo>();

            // 修复路径比较逻辑
            Parallel.ForEach(remoteFiles, remotePath =>
            {
                // 获取相对路径时需要保持一致的路径分隔符
                string remoteRelativePath = remotePath.Key
                    .Substring(YuoToolsManager.FilePath.Length)
                    .Replace('\\', '/');

                string localFullPath = Path.Combine(localPath, remoteRelativePath.TrimStart('/'))
                    .Replace('\\', '/');

                if (!localFiles.ContainsKey(localFullPath))
                {
                    lock (changes)
                    {
                        changes.Add(new FileChangeInfo
                        {
                            FilePath = remoteRelativePath,
                            ChangeType = ChangeType.Added,
                            RemoteTime = remotePath.Value
                        });
                    }
                }
                else
                {
                    DateTime remoteTime = remotePath.Value;
                    DateTime localTime = localFiles[localFullPath];

                    if (Math.Abs((remoteTime - localTime).TotalSeconds) > 1) // 添加1秒的容差
                    {
                        lock (changes)
                        {
                            changes.Add(new FileChangeInfo
                            {
                                FilePath = remoteRelativePath,
                                ChangeType = ChangeType.Modified,
                                LocalTime = localTime,
                                RemoteTime = remoteTime
                            });
                        }
                    }
                }
            });

            // 检查删除的文件
            foreach (var localFile in localFiles)
            {
                string localRelativePath = localFile.Key
                    .Substring(localPath.Length)
                    .Replace('\\', '/')
                    .TrimStart('/');

                string remoteFullPath = Path.Combine(remotePath, localRelativePath)
                    .Replace('\\', '/');

                if (!remoteFiles.ContainsKey(remoteFullPath))
                {
                    changes.Add(new FileChangeInfo
                    {
                        FilePath = "/" + localRelativePath,
                        ChangeType = ChangeType.Deleted,
                        LocalTime = localFile.Value
                    });
                }
            }

            EditorApplication.delayCall += () =>
            {
                _changedFiles = changes.OrderBy(x => x.FilePath).ToList();
                Repaint();
            };
        }

        private Dictionary<string, DateTime> GetFilesWithCache(string path)
        {
            var result = new Dictionary<string, DateTime>();
            if (!Directory.Exists(path)) return result;

            foreach (var file in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
            {
                // 统一使用正斜杠
                string normalizedPath = file.Replace('\\', '/');

                if (!_fileTimeCache.TryGetValue(normalizedPath, out DateTime cachedTime))
                {
                    cachedTime = File.GetLastWriteTime(file);
                    _fileTimeCache[normalizedPath] = cachedTime;
                }

                result[normalizedPath] = cachedTime;
            }

            return result;
        }

        private Dictionary<string, bool> _selectedFiles = new Dictionary<string, bool>();
        
        private bool _selectAll = false;

        private void ApplySelectedChanges()
        {
            if (!EditorUtility.DisplayDialog("确认应用变更",
                    "确定要应用选中的文件变更吗？这个操作将会覆盖本地文件。",
                    "确定", "取消"))
            {
                return;
            }

            var selectedChanges = _changedFiles
                .Where(f => _selectedFiles.ContainsKey(f.FilePath) && _selectedFiles[f.FilePath])
                .ToList();

            int successCount = 0;
            int failCount = 0;

            foreach (var change in selectedChanges)
            {
                try
                {
                    string localPath = Path.Combine(Application.dataPath, "YuoTools", change.FilePath.TrimStart('/'));
                    string remotePath = Path.Combine(YuoToolsManager.FilePath, change.FilePath.TrimStart('/'));

                    switch (change.ChangeType)
                    {
                        case ChangeType.Added:
                        case ChangeType.Modified:
                            // 确保目标目录存在
                            Directory.CreateDirectory(Path.GetDirectoryName(localPath));
                            File.Copy(remotePath, localPath, true);
                            break;

                        case ChangeType.Deleted:
                            if (File.Exists(localPath))
                            {
                                File.Delete(localPath);

                                // 检查并删除空文件夹
                                var directory = Path.GetDirectoryName(localPath);
                                while (!string.IsNullOrEmpty(directory) &&
                                       directory.StartsWith(Path.Combine(Application.dataPath, "YuoTools")) &&
                                       !Directory.EnumerateFileSystemEntries(directory).Any())
                                {
                                    Directory.Delete(directory);
                                    directory = Path.GetDirectoryName(directory);
                                }
                            }

                            break;
                    }

                    successCount++;
                }
                catch (Exception e)
                {
                    Debug.LogError($"应用变更失败: {change.FilePath}\n{e}");
                    failCount++;
                }
            }

            // 刷新资源
            AssetDatabase.Refresh();

            // 显示结果
            EditorUtility.DisplayDialog("应用变更完成",
                $"成功: {successCount}\n失败: {failCount}",
                "确定");

            // 重新分析变更
            AnalyzeChangesAsync();
        }

        private class FileChangeInfo
        {
            public string FilePath { get; set; }
            public ChangeType ChangeType { get; set; }
            public DateTime LocalTime { get; set; }
            public DateTime RemoteTime { get; set; }
        }

        private enum ChangeType
        {
            Added,
            Modified,
            Deleted
        }
    }
}
#endif