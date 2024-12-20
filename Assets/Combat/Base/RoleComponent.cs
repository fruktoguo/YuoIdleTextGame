using UnityEngine;
using YuoTools.Main.Ecs;

namespace Combat.Role
{
    public class RoleComponent : YuoComponent,IComponentInit<Transform>
    {
        public Transform transform;

        private RoleDataComponent roleData;
        public RoleDataComponent RoleData => roleData ??= GetComponent<RoleDataComponent>();

        public int GroupID = 1;

        public void ComponentInit(Transform data)
        {
            transform = data;
        }

        public override string ToString()
        {
            return Entity.ToString();
        }
    }

    public static class RoleEx
    {
        public static void AddBuff<T>(this RoleComponent role, float duration = 0, int count = 0)
            where T : BuffComponent, new()
        {
            BuffManagerComponent.Get.AddBuff<T>(role.Entity, duration, count);
        }

        public static T AddBuff<T>(this RoleComponent role) where T : BuffComponent, new() =>
            BuffManagerComponent.Get.AddBuff<T>(role.Entity);

        public static void AddBuff_Void<T>(this RoleComponent role) where T : BuffComponent, new()
        {
            BuffManagerComponent.Get.AddBuff<T>(role.Entity);
        }

        public static void RemoveBuff<T>(this RoleComponent role) where T : BuffComponent, new()
        {
            BuffManagerComponent.Get.RemoveBuff<T>(role.Entity);
        }

        public static void RemoveBuff<T>(this RoleComponent role, int buffCount) where T : BuffComponent, new()
        {
            BuffManagerComponent.Get.RemoveBuff<T>(role.Entity, buffCount);
        }
    }
}