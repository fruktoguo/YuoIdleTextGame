using Combat;
using UnityEngine;
using YuoTools.Main.Ecs;

namespace Combat.Role
{
    public class CriticalCheckSystem : YuoSystem<ValueComponent, AtkValueComponent>, IOnAttackValueBefore,
        IOnAttackBeforeBehaviour
    {
        public override string Group => "AttackSystem";

        public override string Name => "暴击检测系统";

        public override void Run(ValueComponent value, AtkValueComponent atkValue)
        {
            var attackerData = atkValue.Initiator.GetComponent<RoleDataComponent>();

            //只有没有暴击标签时才会计算暴击
            //如果有无法暴击标签则不会计算暴击
            if (!(value.ContainsValueTag(ValueTagConst.Critical) ||
                  value.ContainsValueTag(ValueTagConst.DontCritical)))
            {
                //计算暴击
                var criticalNum = Random.Range(0, 1f);
                switch (atkValue.AttackType)
                {
                    //物理攻击正常计算暴击
                    case AttackType.Physical:
                        if (criticalNum < attackerData.Critical.Value)
                        {
                            value.AddValueTag(ValueTagConst.Critical);
                        }

                        break;
                    //魔法攻击混合攻击真实攻击,只有当拥有魔法暴击标签才会计算暴击
                    case AttackType.Mix or AttackType.Magic or AttackType.Real:
                        if (value.ContainsValueTag(ValueTagConst.Critical))
                        {
                            if (criticalNum < attackerData.Critical.Value)
                            {
                                value.AddValueTag(ValueTagConst.Critical);
                            }
                        }

                        break;
                }
            }
        }
    }
}