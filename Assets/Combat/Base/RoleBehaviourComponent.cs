using UnityEngine;
using YuoTools.Main.Ecs;

namespace Combat.Role
{
    public class RoleBehaviourComponent : YuoComponent
    {
        public double Progress = 0;
        public double MaxProgress = 1;
    }

    public class RoleActionComponentUpdateSystem : YuoSystem<RoleBehaviourComponent, RoleDataComponent, RoleActiveTag>,
        IUpdate
    {
        public override string Group => "Combat/Base";
        public override void Run(RoleBehaviourComponent action, RoleDataComponent data, RoleActiveTag active)
        {
            action.Progress += data.AttackSpeed * Time.deltaTime;
            while (action.Progress >= action.MaxProgress)
            {
                action.Progress -= action.MaxProgress;

                data.Entity.RunSystem<IOnBehaviourExecute>();
            }
        }
    }

    public interface IOnBehaviourExecute : ISystemTag
    {
    }

    public class RoleAttackComponent : YuoComponent
    {
        public RoleComponent target;
    }

    public class RoleBehaviourExecuteSystem : YuoSystem<RoleComponent, RoleAttackComponent>, IOnBehaviourExecute
    {
        public override string Group => "Combat/Base";
        public override void Run(RoleComponent role, RoleAttackComponent attack)
        {
            if (attack.target)
            {
                if (role.TryGetComponent<RoleAttackAnimator>(out var animator))
                {
                    animator.attackDirection = (attack.target.transform.position - role.transform.position).normalized;
                    animator.PlayAttack();
                }

                var attackerData = role.RoleData;

                AttackHelper.Atk(new AttackData()
                {
                    Initiator = role,
                    Taker = attack.target,
                    AttackType = AttackType.Physical,
                    CureType = CureType.Heal,
                    DamageValue = attackerData.Atk.Value
                });
            }
        }
    }
}