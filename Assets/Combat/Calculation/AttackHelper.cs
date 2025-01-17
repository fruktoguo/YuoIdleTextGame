using System.Collections.Generic;
using Combat;
using YuoTools;
using YuoTools.Extend.Helper;
using YuoTools.Main.Ecs;

namespace Combat.Role
{
    public class AttackHelper
    {
        public static void Atk(AttackData atkData)
        {
            atkData.Initiator.Entity.GetOrAddComponent<AttackDataListComponent>().DataList.Add(atkData);
        }
    }

    public class AttackProcessingSystem : YuoSystem<AttackDataListComponent>, IUpdate
    {
        public override string Group => "Combat";

        public override void Run(AttackDataListComponent component)
        {
            var dataCount = component.DataList.Count;
            for (var index = 0; index < dataCount; index++)
            {
                var atkData = component.DataList[index];
                var attackerEntity = atkData.Initiator.Entity;
                var beAttackerEntity = atkData.Taker.Entity;

                var data = attackerEntity.GetComponent<RoleDataComponent>();

                var value = attackerEntity.GetOrAddComponent<ValueComponent>();

                var atkBehaviour =
                    attackerEntity.GetOrAddComponent<AttackBehaviourComponent, RoleComponent, RoleComponent>(
                        atkData.Initiator, atkData.Taker);

                var atkInfo = attackerEntity.GetOrAddComponent<AtkInfoComponent, AttackType, CureType>(atkData.AttackType,
                    atkData.CureType);
                value.Start();

                //Init
                value.Value = atkData.DamageValue;

                attackerEntity.RunSystem<IOnAttackBefore>();
            }
            component.DataList.RemoveRange(0, dataCount);
        }
    }

    public class AttackDataListComponent : YuoComponent
    {
        public List<AttackData> DataList = new();
    }

    public class AttackData
    {
        public RoleComponent Initiator;
        public RoleComponent Taker;
        public AttackType AttackType;
        public CureType CureType;
        public double DamageValue;
        public List<string> ValueTagList = new();
    }
}