using System;
using System.Collections.Generic;

namespace YuoTools.Main.Ecs
{
    public partial class YuoWorld
    {
        /// <summary>
        /// 是否关闭系统的 Try-Catch
        /// </summary>
        public static bool CloseSystemTry = false;

        /// <summary>
        /// YuoWorld 的单例实例
        /// </summary>
        public static YuoWorld Instance { get; private set; }

        /// <summary>
        /// 主实体，通常用于挂载核心组件
        /// </summary>
        public static YuoEntity Main { get; private set; }

        #region Systems

        /// <summary>
        /// 所有系统的列表
        /// </summary>
        private List<SystemBase> allSystem = new();

        /// <summary>
        /// 组件类型到系统的映射
        /// </summary>
        private MultiMap<Type, SystemBase> systemsOfComponent = new();

        /// <summary>
        /// 标签类型到系统的映射
        /// </summary>
        private MultiMap<Type, SystemBase> systemsOfTag = new();

        /// <summary>
        /// 基础组件类型到系统的映射
        /// </summary>
        private MultiMap<Type, SystemBaseBase> systemsOfBaseComponent = new();

        /// <summary>
        /// 系统类型到系统实例的字典
        /// </summary>
        private Dictionary<Type, SystemBase> systemDic = new();

        #endregion

        /// <summary>
        /// 所有实体的字典，以实体ID为键
        /// </summary>
        public Dictionary<long, YuoEntity> Entities { get; private set; } = new();

        /// <summary>
        /// 组件类型到组件实例的映射
        /// </summary>
        private MultiHashSetMap<Type, YuoComponent> components = new();

        /// <summary>
        /// 默认场景实体
        /// </summary>
        public static YuoEntity Scene => Instance.AllScenes[0];

        /// <summary>
        /// 所有场景实体的列表
        /// </summary>
        public readonly List<YuoEntity> AllScenes = new();

        /// <summary>
        /// 待销毁的实体列表
        /// </summary>
        private readonly List<YuoEntity> entityTrash = new();

        /// <summary>
        /// 待销毁的组件列表
        /// </summary>
        private readonly List<YuoComponent> componentsTrash = new();

        /// <summary>
        /// 链接组件的映射
        /// </summary>
        private MultiHashSetMap<Type, Type> linkComponent = new();

        /// <summary>
        /// 单例组件类型集合
        /// </summary>
        private HashSet<Type> singleComponents = new();

        /// <summary>
        /// 基础组件类型映射
        /// </summary>
        [Sirenix.OdinInspector.ShowInInspector]
        private Dictionary<Type, Type> baseComponents = new();

        /// <summary>
        /// IStart 需要在所有组件添加完成后，在每一帧的末尾才会调用
        /// </summary>
        private readonly List<YuoComponent> startSystems = new();

        /// <summary>
        /// 所有组件类型的字典
        /// </summary>
        readonly Dictionary<string, Type> allComponentType = new();

        /// <summary>
        /// 世界是否已销毁的标志
        /// </summary>
        private bool _worldIsDestroy;
    }
}