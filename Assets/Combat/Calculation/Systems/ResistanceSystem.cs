using Combat.Role;
using YuoTools;
using YuoTools.Extend.Helper;
using YuoTools.Main.Ecs;

namespace Combat.Systems
{
    [SystemOrder(short.MaxValue)]
    // 抗性系统
    public class ResistanceSystem : YuoSystem<AtkInfoComponent, HurtBehaviourComponent, ValueComponent>, IOnHurtValue
    {
        public override string Group => "Combat/Base";

        public override void Run(AtkInfoComponent atk, HurtBehaviourComponent hurt, ValueComponent value)
        {
            //计算抗性
            var damage = value.Value;

            var beAttackerData = hurt.Entity.GetOrAddComponent<RoleDataComponent>();
            double armorPenetration = 0;
            double armorPenetrationPercent = 0;
            double magicPenetration = 0;
            double magicPenetrationPercent = 0;
            if (hurt.Initiator != null)
            {
                var attackerData = hurt.Initiator.Entity.GetOrAddComponent<RoleDataComponent>();
                armorPenetration = attackerData.ArmorPenetration.Value;
                armorPenetrationPercent = attackerData.ArmorPenetrationPercent.Value;
                magicPenetration = attackerData.MagicPenetration.Value;
                magicPenetrationPercent = attackerData.MagicPenetrationPercent.Value;
            }

            //最终抗性
            double armor = 0;

            switch (atk.AttackType)
            {
                case AttackType.Physical:
                    armor = (beAttackerData.Armor.Value - armorPenetration) *
                            (1 - armorPenetrationPercent);
                    break;
                case AttackType.Magic:
                    armor = (beAttackerData.MagicResist.Value - magicPenetration) *
                            (1 - magicPenetrationPercent);
                    break;
                case AttackType.Real:
                    armor = 0;
                    break;
                case AttackType.Mix:
                    //混合伤害,按照减伤最少的抗性来计算
                    var physical = (beAttackerData.Armor.Value - armorPenetration) *
                                   (1 - armorPenetrationPercent);

                    var magic = (beAttackerData.MagicResist.Value - magicPenetration) *
                                (1 - magicPenetrationPercent);
                    armor = physical.RMin(magic);
                    break;
            }

            var armor减伤率 = GameValueHelper.ValueToPercent(armor);

            value.Value *= armor减伤率;
        }
    }

    //伤害数字和扣血
}