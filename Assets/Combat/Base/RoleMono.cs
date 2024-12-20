using Combat.Role;
using Sirenix.OdinInspector;
using UnityEngine;
using YuoTools.Extend;
using YuoTools.Main.Ecs;
using YuoTools.UI;

public class RoleMono : MonoBehaviour
{
    void Start()
    {
        var role = YuoWorld.Scene.AddChild<RoleComponent, Transform>(transform);
        GameObjectMapManager.Get.AddMap(gameObject, role.Entity);
        role.Entity.EntityName = transform.name;

        var data = role.AddComponent<RoleDataComponent>();

        var anima = role.Entity.AddComponent<RoleAttackAnimator, Transform>(transform);
    }

    [Button]
    public void TestAddBar()
    {
        var bar = View_FollowBarComponent.GetView().Follow(transform);
    }
}