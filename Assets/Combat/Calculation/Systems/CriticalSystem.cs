using Combat.Role;
using YuoTools.Main.Ecs;

namespace Combat.Systems
{
    // 暴击系统
    public class CriticalSystem : YuoSystem<ValueComponent, AtkInfoComponent, RoleDataComponent>, IOnAttackValue
    {
        public override string Group => "Combat/Base";
        public override void Run(ValueComponent value, AtkInfoComponent atk, RoleDataComponent data)
        {
            if (value.ContainsValueTag(ValueTagConst.Critical))
            {
                value.Value *= data.CriticalMultiply.Value;
            }
        }
    }
}