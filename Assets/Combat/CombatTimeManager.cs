using System.Collections.Generic;
using Combat.Role;
using UnityEngine;
using YuoTools.Main.Ecs;


[AutoAddToScene]
public class CombatTimeManager : YuoComponentGet<CombatTimeManager>
{
}

public class CombatTimeComponent : YuoComponent
{
    public float Time;
}

public class CombatTimeManagerUpdateSystem : YuoSystem<CombatTimeComponent,RoleComponent>, IUpdate
{
    public override string Group => "Combat";

    public override void Run(CombatTimeComponent time, RoleComponent role)
    {
        throw new System.NotImplementedException();
    }
}