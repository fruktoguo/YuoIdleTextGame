using Combat;
using YuoTools;
using YuoTools.Extend.Helper;
using YuoTools.Main.Ecs;

namespace Combat.Role
{
    public class AttackHelper
    {
        /// <summary>
        ///  造成伤害
        /// </summary>
        public static double Damage(RoleComponent attacker, RoleComponent beAttacked, double damageValue,
            AttackType attackType = AttackType.Physical, params string[] valueTags)
        {
            var value = attacker.Entity.GetOrAddComponent<ValueComponent>();
            var atkValue = attacker.Entity.GetOrAddComponent<AtkValueComponent>();

            var attackerData = attacker.Entity.GetComponent<RoleDataComponent>();
            var beAttackerData = beAttacked.Entity.GetComponent<RoleDataComponent>();

            var enemy = beAttacked.Entity;
            var targetValue = enemy.GetOrAddComponent<ValueComponent>();
            var targetAtkValue = enemy.GetOrAddComponent<AtkValueComponent>();

            foreach (var valueTag in valueTags)
            {
                value.AddValueTag(valueTag);
            }

            Start();

            value.Value = damageValue;

            atkValue.Initiator = attacker;
            atkValue.Taker = beAttacked;
            targetAtkValue.Initiator = attacker;
            targetAtkValue.Taker = beAttacked;

            atkValue.AttackType = attackType;

            YuoWorld.RunSystem<IOnAttackValueBefore>(value);

            //如果有暴击标签则计算暴击伤害
            if (value.ContainsValueTag(ValueTagConst.Critical))
            {
                value.Value *= attackerData.CriticalMultiply.Value;
            }

            YuoWorld.RunSystem<IOnAttackValue>(attacker.Entity);
            YuoWorld.RunSystem<IOnAttackValueAfter>(attacker.Entity);

            //数值传递
            value.TransmitTo(targetValue);
            atkValue.TransmitTo(targetAtkValue);

            //开始提前计算
            YuoWorld.RunSystem<IOnHurtValueBefore>(enemy);

            //如果有免疫伤害标签则不会触发伤害结算
            if (targetValue.ContainsValueTag(ValueTagConst.ImmuneDamage))
            {
                // ShowText(beAttackerData, "免疫");
                End();
                return 0;
            }

            //计算抗性

            var damage = targetValue.Value;

            //最终抗性
            double armor = 0;

            switch (targetAtkValue.AttackType)
            {
                case AttackType.Physical:
                    armor = (beAttackerData.Armor.Value - attackerData.ArmorPenetration.Value) *
                            (1 - attackerData.ArmorPenetrationPercent.Value);
                    break;
                case AttackType.Magic:
                    armor = (beAttackerData.MagicResist.Value - attackerData.MagicPenetration.Value) *
                            (1 - attackerData.MagicPenetrationPercent.Value);
                    break;
                case AttackType.Real:
                    armor = 0;
                    break;
                case AttackType.Mix:
                    //混合伤害,按照减伤最少的抗性来计算
                    var physical = (beAttackerData.Armor.Value - attackerData.ArmorPenetration.Value) *
                                   (1 - attackerData.ArmorPenetrationPercent.Value);

                    var magic = (beAttackerData.MagicResist.Value - attackerData.MagicPenetration.Value) *
                                (1 - attackerData.MagicPenetrationPercent.Value);
                    armor = physical.RMin(magic);
                    break;
            }

            var armor减伤率 = GameValueHelper.ValueToPercent(armor);

            targetValue.Value *= armor减伤率;

            YuoWorld.RunSystem<IOnHurtValue>(enemy);

            //限制一下最小伤害值为0
            targetValue.Value.Clamp();
            damageValue = targetValue.Value;

            HealthRemove(beAttacked, damageValue);

            //Debug.Log($"{beAttacked.Entity.EntityName} 受到 {attacker.Entity.EntityName},实际伤害值为{targetValue.Value:F},被抗性减伤{(damage - targetValue.Value):F}");
            //给文字生成位置加一个随机偏移
            //待完成,同一帧内多次攻击,只显示一次合并伤害值
            // ShowText(beAttacked, beAttackerData, targetValue, numTipType);


            if (beAttackerData.Hp < 0)
            {
                Cure(beAttacked, beAttacked, 100);
            }

            YuoWorld.RunSystem<IOnHurtValueAfter>(enemy);

            End();
            return damageValue;

            void Start()
            {
                value.Start();
                atkValue.Start();
                targetValue.Start();
                targetAtkValue.Start();
            }

            void End()
            {
                targetValue.End();
                targetAtkValue.End();
                value.End();
                atkValue.End();
            }
        }

        /// <summary>
        ///  造成治疗效果
        /// </summary>
        public static void Cure(RoleComponent curer, RoleComponent beCurer, float cureValue,
            CureType cureType = CureType.Heal, params string[] valueTags)
        {
            var value = curer.Entity.GetOrAddComponent<ValueComponent>();
            var atkValue = curer.Entity.GetOrAddComponent<AtkValueComponent>();
            foreach (var valueTag in valueTags)
            {
                value.AddValueTag(valueTag);
            }

            value.Start();
            atkValue.Start();
            value.Value = cureValue;
            atkValue.CureType = cureType;

            atkValue.Initiator = curer;
            atkValue.Taker = beCurer;

            // Debug.Log($"计算治疗值_发起方 {value.Value}");
            YuoWorld.RunSystem<IOnCureValueBefore>(curer.Entity);
            YuoWorld.RunSystem<IOnCureValue>(curer.Entity);
            YuoWorld.RunSystem<IOnCureValueAfter>(curer.Entity);

            // Debug.Log($"计算治疗值_承受方 {value.Value}");
            var target = beCurer.Entity;
            //数值传递
            var targetValue = target.GetOrAddComponent<ValueComponent>();
            var targetAtkValue = target.GetOrAddComponent<AtkValueComponent>();

            value.TransmitTo(targetValue);
            atkValue.TransmitTo(targetAtkValue);

            //触发治疗结算
            YuoWorld.RunSystem<IOnBeCureValueBefore>(target);
            YuoWorld.RunSystem<IOnBeCureValue>(target);
            YuoWorld.RunSystem<IOnBeCureValueAfter>(target);

            // Debug.Log($"计算治疗值_最终值 {targetValue.Value}");

            var data = beCurer.Entity.GetComponent<RoleDataComponent>();
            var beCurerData = beCurer.Entity.GetComponent<RoleDataComponent>();

            //限制一下最小恢复量为0
            targetValue.Value.Clamp();

            switch (targetAtkValue.CureType)
            {
                case CureType.Heal:
                    HealthAdd(beCurer, targetValue.Value);
                    break;
                case CureType.Shield:
                    break;
                case CureType.Mana:
                    ManaHelper.AddMana(beCurerData, targetValue.Value);
                    break;
            }

            YuoWorld.RunSystem<IOnCureAfter>(curer.Entity);
            YuoWorld.RunSystem<IOnBeCureAfter>(target);

            targetValue.End();
            targetAtkValue.End();
        }

        public static void HealthRemove(RoleComponent role, double value)
        {
            // Debug.Log($"{role.Entity.EntityName} 扣血 {value}");
            var data = role.Entity.GetComponent<RoleDataComponent>();
            data.Hp -= value;
            data.Hp.Clamp(data.MaxHp.Value);
            // role.GetComponent<View_HPSliderComponent>().UpdateHp(data.Hp, data.MaxHp.Value);
        }

        public static void HealthAdd(RoleComponent role, double value)
        {
            // Debug.Log($"{role.Entity.EntityName} 加血 {value}");
            var data = role.Entity.GetComponent<RoleDataComponent>();
            data.Hp += value;
            data.Hp.Clamp(data.MaxHp.Value);
            // role.GetComponent<View_HPSliderComponent>().UpdateHp(data.Hp, data.MaxHp.Value);
        }

        public static void AtkBefore(RoleComponent attacker, RoleComponent beAttacker)
        {
            YuoWorld.RunSystem<IOnAttackBeforeBehaviour>(attacker.Entity);
        }

        public static void Atk(RoleComponent attacker, RoleComponent beAttacker)
        {
            var attackerEntity = attacker.Entity;
            var beAttackerEntity = beAttacker.Entity;

            var data = attackerEntity.GetComponent<RoleDataComponent>();

            YuoWorld.RunSystem<IOnAttackBefore>(attackerEntity);

            YuoWorld.RunSystem<IOnHurtBefore>(beAttackerEntity);

            //伤害数值触发
            Damage(attacker, beAttacker, data.Atk.Value);

            //攻击完成了
            YuoWorld.RunSystem<IOnAttackAfter>(attackerEntity);

            YuoWorld.RunSystem<IOnHurtAfter>(beAttackerEntity);
        }
    }
}