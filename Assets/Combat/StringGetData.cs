using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;
using YuoTools;
using YuoTools.Extend;
using YuoTools.Main.Ecs;

namespace Combat.Role
{
    public static class StringGetData
    {
        public class RoleDataConfig
        {
            public const string Atk = "Atk";
            public const string Mag = "Mag";
            public const string Armor = "Armor";
            public const string ArmorPenetration = "ArmorPenetration";
            public const string ArmorPenetrationPercent = "ArmorPenetrationPercent";
            public const string MagicResist = "MagicResist";
            public const string MagicPenetration = "MagicPenetration";
            public const string MagicPenetrationPercent = "MagicPenetrationPercent";
            public const string Critical = "Critical";
            public const string CriticalMultiply = "CriticalMultiply";
            public const string AttackRange = "AttackRange";
            public const string BaseAttackSpeed = "BaseAttackSpeed";
            public const string ExAttackSpeed = "ExAttackSpeed";
            public const string AttackSpeed = "AttackSpeed";

            public static readonly List<string> All = new()
            {
                Atk,
                Mag,
                Armor,
                ArmorPenetration,
                ArmorPenetrationPercent,
                MagicResist,
                MagicPenetration,
                MagicPenetrationPercent,
                Critical,
                CriticalMultiply,
                AttackRange,
                BaseAttackSpeed,
                ExAttackSpeed,
                AttackSpeed,
            };
        }

        public static double GetRoleData(string dataType, RoleDataComponent data) =>
            dataType switch
            {
                RoleDataConfig.Atk => data.Atk.Value,
                RoleDataConfig.Mag => data.Mag.Value,
                RoleDataConfig.Armor => data.Armor.Value,
                RoleDataConfig.ArmorPenetration => data.ArmorPenetration.Value,
                RoleDataConfig.ArmorPenetrationPercent => data.ArmorPenetrationPercent.Value,
                RoleDataConfig.MagicResist => data.MagicResist.Value,
                RoleDataConfig.MagicPenetration => data.MagicPenetration.Value,
                RoleDataConfig.MagicPenetrationPercent => data.MagicPenetrationPercent.Value,
                RoleDataConfig.Critical => data.Critical.Value,
                RoleDataConfig.CriticalMultiply => data.CriticalMultiply.Value,
                RoleDataConfig.AttackRange => data.AttackRange.Value,
                RoleDataConfig.BaseAttackSpeed => data.BaseAttackSpeed.Value,
                RoleDataConfig.ExAttackSpeed => data.ExAttackSpeed.Value,
                RoleDataConfig.AttackSpeed => data.AttackSpeed,
                _ => 0f
            };

        public const string Tag = "[UseData]";

        static Dictionary<string, GetDataItem> getDataItems = new Dictionary<string, GetDataItem>();

        static void InitFormat(string str, YuoEntity entity)
        {
            var getDataItem = new GetDataItem();
            getDataItems.Add(str, getDataItem);
            //使用正则表达式获取{}中的字符串
            string pattern = @"\{(.+?)\}";
            MatchCollection matches = Regex.Matches(str, pattern);
            foreach (Match match in matches)
            {
                if (getDataItem.Actions.ContainsKey(match.Value)) continue;
                string text = match.Groups[1].Value;
                if (text.Contains(Operators))
                {
                    var texts = text.Split(Operators);
                    foreach (var t in texts)
                    {
                        if (t.IsNullOrSpace() || getDataItem.Actions2.ContainsKey(t)) continue;
                        var dataAction = GetDataAction(t, out var isConfig);
                        if (isConfig)
                        {
                            getDataItem.Actions2[t] = dataAction;
                        }
                    }
                }

                getDataItem.Actions[match.Value] = GetDataAction(text, out var isConfig2);
            }
        }

        public static string Format(string str, YuoEntity entity)
        {
            try
            {
                if (!entity)
                {
                    Debug.LogError("entity is null");
                    return str;
                }

                if (!getDataItems.ContainsKey(str))
                {
                    InitFormat(str, entity);
                }

                var getDataItem = getDataItems[str];
                foreach (var action in getDataItem.Actions)
                {
                    var result = action.Value?.Invoke(entity);
                    // Debug.Log($"{action.Key}=>{result}");
                    if (result.Contains(Operators))
                    {
                        foreach (var action2 in getDataItem.Actions2)
                        {
                            if (result != null && result.Contains(action2.Key))
                            {
                                result = result.Replace(action2.Key, action2.Value?.Invoke(entity));
                                // Debug.Log($"{action2.Key}=>{result}");
                            }
                        }

                        result = MathfFormat(result, entity);
                    }

                    // Debug.Log("result:" + result);
                    str = str.Replace(action.Key, result);
                }

                return str;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return str;
            }
        }

        private static readonly char[] Operators = { '+', '-', '*', '/', '(', ')', '%' };
        static DataTable dataTable = new DataTable();

        /// <summary>
        ///  用于支持简单数学运算
        /// </summary>
        static string MathfFormat(string str, YuoEntity entity)
        {
            return dataTable.Compute(str, "").ToString();
        }


        static StringAction<YuoEntity> GetDataAction(string value, out bool isConfig)
        {
            // Debug.Log(value);
            isConfig = true;
            if (RoleDataConfig.All.Contains(value))
            {
                return yuoEntity => GetRoleData(value, yuoEntity.GetComponent<RoleDataComponent>())
                    .ToString(CultureInfo.InvariantCulture);
            }

            if (value.StartsWith("Buff:") && !value.Contains(Operators))
            {
                return yuoEntity =>
                    BuffManagerComponent.Get.GetBuffCount(yuoEntity, value.Substring(5)).ToString();
            }

            if (ConfigHelper.ContainsKey(value))
                return _ => ConfigHelper.GetConfig(value).ToString();

            isConfig = false;
            return _ => value;
        }

        class GetDataItem
        {
            public Dictionary<string, StringAction<YuoEntity>> Actions = new();
            public Dictionary<string, StringAction<YuoEntity>> Actions2 = new();
        }

        public static string LanguageUseData(this LanguageEnum key, YuoEntity entity)
        {
            var result = TranslateManagerComponent.Get.LoadText(key);
            if (result.StartsWith(Tag))
            {
                result = result.Substring(Tag.Length);
                return Format(result, entity);
            }

            return result;
        }

        public static string LanguageUseData(this string key, YuoEntity entity)
        {
            var result = TranslateManagerComponent.Get.LoadText(key);
            if (result.StartsWith(Tag))
            {
                result = result.Substring(Tag.Length);
                return Format(result, entity);
            }

            return result;
        }

        public static bool Contains(this string str, char[] chars)
        {
            foreach (var c in chars)
            {
                if (str.Contains(c))
                {
                    return true;
                }
            }

            return false;
        }
    }
}