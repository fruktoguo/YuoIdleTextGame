using Sirenix.OdinInspector;
using UnityEngine;
using YuoTools;
using YuoTools.Main.Ecs;
using YuoTools.UI;

namespace AI
{
    public class RoleDataComponent : YuoComponent
    {
        public double Hp = 100;

        /// <summary>
        ///  最大生命值
        /// </summary>
        public YuoValue MaxHp = 100;

        public double Mana = 0;

        /// <summary>
        ///  最大魔法值
        /// </summary>
        public YuoValue MaxMana = 100;

        /// <summary>
        ///  物理攻击
        /// </summary>
        public YuoValue Atk = 65;

        /// <summary>
        ///  魔法攻击
        /// </summary>
        public YuoValue Mag = 1;

        /// <summary>
        ///  护甲
        /// </summary>
        public YuoValue Armor = 10;

        /// <summary>
        ///  护甲穿透
        /// </summary>
        public YuoValue ArmorPenetration = 0;

        /// <summary>
        ///  百分比护甲穿透
        /// </summary>
        public YuoValue ArmorPenetrationPercent = 0;

        /// <summary>
        ///  魔抗
        /// </summary>
        public YuoValue MagicResist = 10;

        /// <summary>
        ///  魔抗穿透
        /// </summary>
        public YuoValue MagicPenetration = 0;

        /// <summary>
        ///  百分比魔抗穿透
        /// </summary>
        public YuoValue MagicPenetrationPercent = 0;

        /// <summary>
        ///  暴击率
        /// </summary>
        public YuoValue Critical = 0.25f;

        /// <summary>
        ///  暴击伤害倍率
        /// </summary>
        public YuoValue CriticalMultiply = 1.5f;

        public Vector2 Size = Vector2.one;

        public YuoValue AttackRange = 1f;

        public YuoValue BaseAttackSpeed = 0.7f;

        public YuoValue ExAttackSpeed = 0f;

        [ShowInInspector] public double AttackSpeed => (1 + ExAttackSpeed.Value) * BaseAttackSpeed.Value;

        public YuoValue MaxEquipNum = 6;
        [ShowInInspector] public double AttackSpace => 1 / AttackSpeed;

        [ShowInInspector] public string IDName { get; private set; }

        public Transform transform;

        // public void Init(RoleDataTemplate roleDataTemplate)
        // {
        //     IDName = roleDataTemplate.idName;
        //     MaxHp.BaseValue = roleDataTemplate.maxHp;
        //     Hp = MaxHp.Value;
        //     MaxMana.BaseValue = roleDataTemplate.maxMp;
        //     Mana = roleDataTemplate.mp;
        //     Atk.BaseValue = roleDataTemplate.atk;
        //     Mag.BaseValue = roleDataTemplate.mag;
        //     Armor.BaseValue = roleDataTemplate.armor;
        //     ArmorPenetration.BaseValue = roleDataTemplate.armorPenetration;
        //     ArmorPenetrationPercent.BaseValue = roleDataTemplate.armorPenetrationPercent;
        //     MagicResist.BaseValue = roleDataTemplate.magicResist;
        //     MagicPenetration.BaseValue = roleDataTemplate.magicPenetration;
        //     MagicPenetrationPercent.BaseValue = roleDataTemplate.magicPenetrationPercent;
        //     Critical.BaseValue = roleDataTemplate.critical;
        //     CriticalMultiply.BaseValue = roleDataTemplate.criticalMultiply;
        //
        //     AttackRange.BaseValue = roleDataTemplate.attackRange;
        //
        //     BaseAttackSpeed.BaseValue = roleDataTemplate.attackSpeed;
        //
        //     MaxEquipNum.BaseValue = roleDataTemplate.maxEquipNum;
        // }
    }

    public class RoleDataAwakeSystem : YuoSystem<RoleDataComponent>, IAwake
    {
        protected override void Run(RoleDataComponent component)
        {
            void OnMaxHpOnValueChange(double oldHp, double maxHp)
            {
                //如果是减少最大生命值,则当前生命值不改变
                if (oldHp < maxHp)
                {
                    //如果是增加最大生命值,则当前生命值增加
                    var oldHpRate = component.Hp / oldHp;
                    component.Hp = maxHp * oldHpRate;
                }
            }

            component.MaxHp.OnValueChange += OnMaxHpOnValueChange;
        }
    }
}