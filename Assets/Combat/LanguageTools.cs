using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;
using YuoTools;
using YuoTools.Main.Ecs;

namespace Manager.Tool
{
    [AutoAddToMain]
    public class LanguageTools : YuoComponentGet<LanguageTools>
    {
        private const string MainText = "MainText";

        private List<StringTable> _tables;

        public async void InitTable()
        {
            LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;
            var tables = LocalizationSettings.StringDatabase.GetAllTables();
            await tables.Task;
            if (tables.Status == AsyncOperationStatus.Succeeded)
            {
                _tables = tables.Result as List<StringTable>;
                if (_tables != null)
                {
                    foreach (var table in _tables)
                    {
                        table.name.Log();
                    }
                }

                if (Application.isPlaying)
                    YuoWorld.RunSystemForAll<ILanguageChange>();
            }
        }

        private void OnSelectedLocaleChanged(Locale obj)
        {
            InitTable();
        }

        static bool ContainsKey(StringTable table, string key)
        {
            return table.SharedData.GetId(key) != 0;
        }

        public string GetText(string key)
        {
            if (_tables == null || _tables.Count == 0)
            {
                Debug.LogError("文本表为空");
                return key;
            }

            var result = key;
            bool isFind = false;
            foreach (var table in _tables)
            {
                if (ContainsKey(table, key))
                {
                    try
                    {
                        result = table[key].GetLocalizedString();
                        isFind = true;
                    }
                    catch
                    {
                        isFind = false;
                    }

                    break;
                }
            }

            if (isFind && !string.IsNullOrEmpty(result))
            {
                result = result.Replace("\\n", "\n");
            }
            else
            {
                Debug.LogError("文本表中没有找到对应的key：" + key);
#if UNITY_EDITOR
                //todo:没有的key直接加进去
#endif
            }

            return result;
        }

        public static string SGetText(string key)
        {
            return Get.GetText(key);
        }

        public static string GetTextFormat(string key, params object[] args) => string.Format(SGetText(key), args);
    }

    public class LanguageToolsAwakeSystem : YuoSystem<LanguageTools>, IAwake
    {
        protected override void Run(LanguageTools component)
        {
            component.InitTable();
        }
    }

    public static class LanguageExtension
    {
        public static string Translate(this string key)
        {
            return LanguageTools.SGetText(key);
        }
    }

    public interface ILanguageChange : ISystemTag
    {
    }
}