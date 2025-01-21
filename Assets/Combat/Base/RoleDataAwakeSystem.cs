using UnityEngine;
using YuoTools.Extend;
using YuoTools.Main.Ecs;
using YuoTools.UI;

namespace Combat.Role
{
    public class RoleDataAwakeSystem : YuoSystem<RoleDataComponent>, IAwake
    {
        public override string Group => "Combat/Base";

        public override void Run(RoleDataComponent component)
        {
            var role = component.GetComponent<RoleComponent>();
            component.Entity.AddComponent<EntitySelectComponent, GameObject>(
                role.transform.gameObject);

            void OnMaxHpOnValueChange(double oldHp, double maxHp)
            {
                //如果是减少最大生命值,则当前生命值不改变
                if (oldHp < maxHp)
                {
                    //如果是增加最大生命值,则当前生命值增加
                    var oldHpRate = component.Hp / oldHp;
                    component.Hp = maxHp * oldHpRate;
                }

                if (component.Hp > maxHp)
                    component.Hp = maxHp;
            }

            void OnMaxManaOnValueChange(double oldMana, double maxMana)
            {
                //如果是减少最大法力值,则当前法力值不改变
                if (oldMana < maxMana)
                {
                    //如果是增加最大法力值,则当前法力值增加
                    var oldManaRate = component.Mana / oldMana;
                    component.Mana = maxMana * oldManaRate;
                }

                if (component.Mana > maxMana)
                    component.Mana = maxMana;
            }

            component.MaxHp.OnValueChange += OnMaxHpOnValueChange;
            component.MaxMana.OnValueChange += OnMaxManaOnValueChange;

            component.Mana = component.MaxMana;
            component.Hp = component.MaxHp;

            var hpBar = View_FollowBarComponent.GetView().Follow(role.transform,
                () => (float)component.MaxHp.Value, () => (float)component.Hp);
            hpBar.MainYuoBar.textFormat = "f0";
            
            var mpBar = View_FollowBarComponent.GetView().Follow(role.transform,
                () => (float)component.MaxMana.Value, () => (float)component.Mana);
            mpBar.MainYuoBar.textFormat = "f0";

            hpBar.Open();
            mpBar.Open();

            var behaviour = component.Entity.GetOrAddComponent<RoleBehaviourComponent>();
            var behaviourBar = View_FollowBarComponent.GetView().Follow(role.transform,
                () => (float)behaviour.MaxProgress, () => (float)behaviour.Progress);
            behaviourBar.Open();
        }
    }
}