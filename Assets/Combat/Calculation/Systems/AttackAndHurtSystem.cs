using Combat.Role;
using YuoTools;
using YuoTools.Main.Ecs;

namespace Combat.Systems
{
    [SystemOrder(short.MinValue)]
    public class AttackBehaviourSystem : YuoSystem<AttackBehaviourComponent>, IOnAttackBefore
    {
        public override string Group => "Combat/Base";

        public override void Run(AttackBehaviourComponent behaviour)
        {
            var attacker = behaviour.Initiator.Entity;

            var taker = behaviour.Taker.Entity;

            var value = attacker.GetOrAddComponent<ValueComponent>();
            var targetValue = taker.GetOrAddComponent<ValueComponent>();

            attacker.RunSystem<IOnAttackBehaviour>();
            attacker.RunSystem<IOnAttackValueBefore>();
            attacker.RunSystem<IOnAttackValue>();
            attacker.RunSystem<IOnAttackValueAfter>();
            attacker.RunSystem<IOnAttackAfter>();

            targetValue.Start();
            value.End();
            
            value.TransmitTo(targetValue);

            var hurt = taker.GetOrAddComponent<HurtBehaviourComponent, AttackBehaviourComponent>(behaviour);
            taker.RunSystem<IOnHurtBefore>();
        }
    }

    [SystemOrder(short.MinValue)]
    public class HurtBehaviourSystem : YuoSystem<HurtBehaviourComponent>, IOnHurtBefore
    {
        public override string Group => "Combat/Base";

        public override void Run(HurtBehaviourComponent behaviour)
        {
            var attacker = behaviour.Initiator.Entity;

            var taker = behaviour.Taker.Entity;

            var value = taker.GetOrAddComponent<ValueComponent>();

            taker.RunSystem<IOnHurt>();
            taker.RunSystem<IOnHurtValueBefore>();
            taker.RunSystem<IOnHurtValue>();
            taker.RunSystem<IOnHurtValueAfter>();
            taker.RunSystem<IOnHurtAfter>();
            value.End();
        }
    }
}