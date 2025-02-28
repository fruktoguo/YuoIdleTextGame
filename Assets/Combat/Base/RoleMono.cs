using System;
using Combat.Role;
using UnityEngine;
using YuoTools;
using YuoTools.Extend;
using YuoTools.Main.Ecs;
using YuoTools.UI;

public class RoleMono : MonoBehaviour
{

    async void Start()
    {
        var role = YuoWorld.Scene.AddChild<RoleComponent, Transform>(transform);
        GameObjectMapManager.Get.AddMap(gameObject, role.Entity);
        role.Entity.EntityName = transform.name;

        var data = role.AddComponent<RoleDataComponent>();

        var atk = role.Entity.AddComponent<RoleAttackComponent>();

        var anima = role.Entity.AddComponent<RoleAttackAnimator, Transform>(transform);

        await YuoWait.WaitTimeAsync(1);

        // var active = role.Entity.AddComponent<RoleActiveTag>();

       
    }
}