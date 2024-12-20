using System.Collections.Generic;
using Combat;
using Sirenix.OdinInspector;
using UnityEngine;
using YuoTools;
using YuoTools.Main.Ecs;

namespace Combat.Role
{
    public class ValueComponent : YuoComponent
    {
        public double Value = 0;

        private bool end = true;

        public void Start()
        {
            if (!end)
            {
                "当前计算未结束".LogError();
                return;
            }

            end = false;
            Value = 0;
            valueTags.Clear();
        }

        public void End()
        {
            if (end)
            {
                "当前计算已结束".LogError();
                return;
            }

            end = true;
        }

        public void TransmitTo(ValueComponent newValueComponent)
        {
            if (newValueComponent == null)
            {
                Debug.LogError($"{Entity.EntityName} newValueComponent == null");
                return;
            }

            if (newValueComponent == this)
                return;
            newValueComponent.Value = Value;

            if (valueTags.Count > 0)
            {
                //复制标签
                newValueComponent.valueTags.AddRange(valueTags);
            }
        }

        public void AddValueTag(string tag)
        {
            if (!valueTags.Contains(tag))
                valueTags.Add(tag);
        }

        public void RemoveValueTag(string tag)
        {
            if (valueTags.Contains(tag))
                valueTags.Remove(tag);
        }

        public bool ContainsValueTag(string tag)
        {
            return valueTags.Contains(tag);
        }
        
        public List<string> GetAllValueTag() => valueTags;

        [ShowInInspector] private List<string> valueTags = new();
    }

    public class AtkValueComponent : YuoComponent
    {
        public AttackType AttackType;
        public CureType CureType;

        /// <summary>
        ///  发起者
        /// </summary>
        public RoleComponent Initiator;

        /// <summary>
        ///  接受者
        /// </summary>
        public RoleComponent Taker;

        bool end = true;

        public void Start()
        {
            if (!end)
            {
                "当前计算未结束".LogError();
                return;
            }

            end = false;
            AttackType = AttackType.Physical;
            CureType = CureType.Heal;
            // Initiator = null;
            // Taker = null;
        }

        public void End()
        {
            if (end)
            {
                "当前计算已结束".LogError();
                return;
            }

            end = true;
        }

        public void TransmitTo(AtkValueComponent newAtkValueComponent)
        {
            if (newAtkValueComponent == null)
            {
                Debug.LogError($"{Entity.EntityName} newAtkValueComponent == null");
                return;
            }

            if (newAtkValueComponent == this)
                return;

            newAtkValueComponent.AttackType = AttackType;
            newAtkValueComponent.CureType = CureType;
            // newAtkValueComponent.Initiator = Initiator;
            // newAtkValueComponent.Taker = Taker;
        }
    }

    public class ValueTagConst
    {
        /// <summary>
        ///  是否暴击
        /// </summary>
        public const string Critical = "Critical";

        /// <summary>
        ///  是否不可以暴击
        /// </summary>
        public const string DontCritical = "DontCritical";

        /// <summary>
        ///  魔法可以暴击
        /// </summary>
        public const string MagicCanCritical = "MagicCanCritical";

        /// <summary>
        ///  攻击无效化
        /// </summary>
        public const string ImmuneAttack = "ImmuneAttack";

        /// <summary>
        ///  技能无效化
        /// </summary>
        public const string ImmuneSkill = "ImmuneSkill";

        /// <summary>
        ///  免疫伤害
        /// </summary>
        public const string ImmuneDamage = "ImmuneDamage";

        /// <summary>
        ///  免疫控制
        /// </summary>
        public const string ImmuneControl = "ImmuneControl";
        
        /// <summary>
        ///  是技能
        /// </summary>
        public const string IsSkill = "IsSkill";
    }
}