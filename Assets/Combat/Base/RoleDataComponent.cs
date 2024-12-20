using Sirenix.OdinInspector;
using UnityEngine;
using YuoTools;
using YuoTools.Extend;
using YuoTools.Main.Ecs;
using YuoTools.UI;

namespace Combat.Role
{
    public class RoleDataComponent : YuoComponent
    {
        public override Color CustomEditorElementColor()
        {
            return YuoColor.马卡龙色系.紫.RSetA(0.5f);
        }

        public override string CustomEditorDisplayOnInspector() => "角色属性组件";

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
        public YuoValue Mag = 0;

        /// <summary>
        ///  护甲
        /// </summary>
        public YuoValue Armor = new(10) { CustomDisplay = v => $"{v.Value} (减伤{1 - v.RatioValue:p2})" };

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
        public YuoValue MagicResist = new(10) { CustomDisplay = v => $"{v.Value} (减伤{1 - v.RatioValue:p2})" };

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

        public YuoValue AttackRange = 1f;

        public YuoValue BaseAttackSpeed = 0.7f;

        public YuoValue ExAttackSpeed = 0f;

        [ShowInInspector] public double AttackSpeed => (1 + ExAttackSpeed.Value) * BaseAttackSpeed.Value;

        [ShowInInspector] public double AttackSpace => 1 / AttackSpeed;
    }

    public class RoleDataAwakeSystem : YuoSystem<RoleDataComponent>, IAwake
    {
        protected override void Run(RoleDataComponent component)
        {
            component.Entity.AddComponent<EntitySelectComponent, GameObject>(component.GetComponent<RoleComponent>()
                .transform.gameObject);

            void OnMaxHpOnValueChange(double oldHp, double maxHp)
            {
                //如果是减少最大生命值,则当前生命值不改变
                if (oldHp < maxHp)
                {
                    //如果是增加最大生命值,则当前生命值增加
                    var oldHpRate = component.Hp / oldHp;
                    component.Hp = maxHp * oldHpRate;
                }

                if (component.Hp > maxHp)
                {
                    component.Hp = maxHp;
                }
            }

            component.MaxHp.OnValueChange += OnMaxHpOnValueChange;
        }
    }
}