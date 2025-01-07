using YuoTools;
using YuoTools.Main.Ecs;

namespace Combat.Role
{
    public class ManaHelper
    {
        // public static void AddMana(RoleDataComponent roleData, double mp)
        // {
        //     if (roleData.GetComponent<ManaLockComponent>() != null) return;
        //     var valueComponent = roleData.Entity.GetOrAddComponent<ValueComponent>();
        //     valueComponent.Start();
        //     valueComponent.Value = mp;
        //     roleData.RunSystem<IOnManaChangeBefore>();
        //     if (valueComponent.Value == 0)
        //     {
        //         valueComponent.End();
        //         return;
        //     }
        //
        //     roleData.Mana += valueComponent.Value;
        //     roleData.Mana.Clamp(roleData.MaxMana);
        //     valueComponent.End();
        //     roleData.RunSystem<IOnManaChange>();
        // }
        //
        // public static void ExpendMana(RoleDataComponent roleData, double mana)
        // {
        //     var valueComponent = roleData.Entity.GetOrAddComponent<ValueComponent>();
        //     valueComponent.Start();
        //     valueComponent.Value = mana;
        //     roleData.RunSystem<IOnManaChangeBefore>();
        //     if (valueComponent.Value == 0)
        //     {
        //         valueComponent.End();
        //         return;
        //     }
        //
        //     roleData.Mana -= valueComponent.Value;
        //     roleData.Mana.Clamp(roleData.MaxMana);
        //     valueComponent.End();
        //     roleData.RunSystem<IOnManaChange>();
        // }
        //
        // public static void ExpendMana(RoleDataComponent roleData)
        // {
        //     ExpendMana(roleData, roleData.MaxMana);
        // }
    }

    public class ManaLockComponent : BuffComponent
    {
        public override string BuffName => "法力锁";
        public override string BuffDescription => "暂时无法获得法力值";
    }

    public interface IOnManaChangeBefore : ISystemTag
    {
    }

    /// <summary>
    ///  魔法值变化时调用
    /// </summary>
    public interface IOnManaChange : ISystemTag
    {
    }
}