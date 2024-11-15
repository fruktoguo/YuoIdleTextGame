using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace YuoTools.Main.Ecs
{
    /// <summary>
    /// 场景组件类
    /// </summary>
    public class SceneComponent : YuoComponent
    {
        [ShowInInspector]
        public HashSet<YuoEntity> Children  => Entity.Children;
    }
}