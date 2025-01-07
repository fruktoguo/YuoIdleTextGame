using YuoTools.Main.Ecs;

namespace Combat.Role
{
    public class RoleFindTargetSystem : YuoSystem<RoleAttackComponent, RoleActiveTag>, IOnBehaviourExecute
    {
        public override void Run(RoleAttackComponent attack, RoleActiveTag active)
        {
            if (!attack.target)
            {
                var allRole = YuoWorld.Instance.GetAllComponents<RoleComponent>();
                foreach (var roleComponent in allRole)
                {
                    if (roleComponent.Entity == attack.Entity) continue;
                    if (roleComponent is RoleComponent role && role.RoleData.Hp > 0)
                    {
                        attack.target = role;
                        return;
                    }
                }
            }
        }
    }
}