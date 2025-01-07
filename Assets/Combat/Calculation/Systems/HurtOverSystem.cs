using Combat.Role;
using YuoTools;
using YuoTools.Main.Ecs;
using YuoTools.UI;

namespace Combat.Systems
{
    [SystemOrder(short.MaxValue)]
    public class HurtOverSystem : YuoSystem<HurtBehaviourComponent, ValueComponent>, IOnHurtAfter
    {
        public override void Run(HurtBehaviourComponent hurt, ValueComponent value)
        {
            var damage = value.Value;
            hurt.Taker.RoleData.Hp -= damage;
            View_DamageNumComponent.GetView().ShowNum((long)damage, hurt.Taker.transform.position,36);
        }
    }
}