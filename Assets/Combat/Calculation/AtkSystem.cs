using System;
using Combat;
using UnityEngine;
using YuoTools;
using YuoTools.Main.Ecs;
using YuoTools.UI;

namespace Combat.Role
{
    public class AttackAIComponent : YuoComponent
    {
        public AttackState AttackState = AttackState.Before;
        public RoleComponent AttackTarget;

        public float BeforeAtkTime = -1000;
        public float LastAttackTime = -1000;

        public void AtkCheck(RoleComponent role, RoleDataComponent data)
        {
            if (LastAttackTime + data.AttackSpace <= Time.time)
            {
                if (BeforeAtkTime + data.AttackSpace <= Time.time)
                {
                    // role.AttackAnima();
                }
            }
        }

        public void Attack()
        {
            AttackState = AttackState.Damage;
            Entity.RunSystem<IOnAttackBehaviour>();
        }

        public Action<AttackAIComponent> AttackDamageAction;

        public void AttackBefore(AttackAIComponent atk, RoleComponent role)
        {
            AttackHelper.AtkBefore(role, atk.AttackTarget);
        }
    }



    public enum AttackState
    {
        Before,
        BeforeAnima,
        Damage,
        After
    }

    public class TestAtkSystem : YuoSystem<AttackAIComponent, RoleComponent, RoleDataComponent, ValueComponent>,
        IOnAttackAfter
    {
        private static readonly int AtkSpeed = Animator.StringToHash("AtkSpeed");

        public override string Name => "测试攻击系统";

        protected override void Run(AttackAIComponent atk, RoleComponent role,
            RoleDataComponent data,
            ValueComponent value)
        {
            //每次攻击增加魔法值
            ManaHelper.AddMana(data, 5);
        }
    }

    public class TestHurSystem : YuoSystem<AttackAIComponent, RoleComponent, RoleDataComponent, ValueComponent>,
        IOnHurtAfter
    {
        private static readonly int atkSpeed = Animator.StringToHash("AtkSpeed");

        protected override void Run(AttackAIComponent atk, RoleComponent role,
            RoleDataComponent data,
            ValueComponent value)
        {
            //每次被攻击增加魔法值
            ManaHelper.AddMana(data, 5);
        }
    }

    public class SkillSystem : YuoSystem<RoleComponent, RoleDataComponent, ValueComponent>,
        IOnManaChange
    {
        public override string Group => "Mana";

        protected override void Run(RoleComponent role, RoleDataComponent data, ValueComponent value)
        {
            if (data.Mana >= data.MaxMana.Value)
            {
                // role.SkillAnima();
            }
        }
    }
}