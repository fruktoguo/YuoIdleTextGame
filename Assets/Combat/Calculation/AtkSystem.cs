using YuoTools.Main.Ecs;

namespace Combat.Role
{
    public class AttackBehaviourComponent : YuoComponent,IComponentInit<RoleComponent, RoleComponent>
    {
        public RoleComponent Initiator;
        public RoleComponent Taker;
        public void ComponentInit(RoleComponent attacker, RoleComponent beAttacker)
        {
            Initiator = attacker;
            Taker = beAttacker;
        }
    }

    public class HurtBehaviourComponent : YuoComponent,IComponentInit<AttackBehaviourComponent>
    {
        public RoleComponent Initiator;
        public RoleComponent Taker;

        public void ComponentInit(AttackBehaviourComponent data)
        {
            Initiator = data.Initiator;
            Taker = data.Taker;
        }
    }
}