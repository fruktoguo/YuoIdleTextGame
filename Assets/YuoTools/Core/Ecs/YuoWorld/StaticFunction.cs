using System;
using System.Collections.Generic;

namespace YuoTools.Main.Ecs
{
    public partial class YuoWorld
    {
        #region ForEach

        private HashSet<T1> MForEach<T1>() where T1 : YuoComponent
        {
            var allComponents = GetAllComponents<T1>();
            HashSet<T1> result = new();
            foreach (var t1 in allComponents)
            {
                result.Add(t1 as T1);
            }

            return result;
        }

        public static HashSet<T1> ForEach<T1>() where T1 : YuoComponent => Instance.MForEach<T1>();

        private HashSet<(T1, T2)> MForEach<T1, T2>() where T1 : YuoComponent where T2 : YuoComponent
        {
            var allComponents = GetAllComponents<T1>();
            var t2Type = typeof(T2);
            HashSet<(T1, T2)> result = new();
            foreach (var t1 in allComponents)
            {
                if (t1.Entity.TryGetComponent(t2Type, out var t2))
                {
                    result.Add((t1 as T1, t2 as T2));
                }
            }

            return result;
        }

        public static HashSet<(T1, T2)> ForEach<T1, T2>() where T1 : YuoComponent where T2 : YuoComponent =>
            Instance.MForEach<T1, T2>();

        private HashSet<(T1, T2, T3)> MForEach<T1, T2, T3>() where T1 : YuoComponent
            where T2 : YuoComponent
            where T3 : YuoComponent
        {
            var allComponents = GetAllComponents<T1>();
            var t2Type = typeof(T2);
            var t3Type = typeof(T3);
            HashSet<(T1, T2, T3)> result = new();
            foreach (var t1 in allComponents)
            {
                if (t1.Entity.TryGetComponent(t2Type, out var t2))
                {
                    if (t1.Entity.TryGetComponent(t3Type, out var t3))
                    {
                        result.Add((t1 as T1, t2 as T2, t3 as T3));
                    }
                }
            }

            return result;
        }

        public static HashSet<(T1, T2, T3)> ForEach<T1, T2, T3>()
            where T1 : YuoComponent where T2 : YuoComponent where T3 : YuoComponent => Instance.MForEach<T1, T2, T3>();

        #endregion

        #region BuildSystem

        #region BuildSystemForEntity

        /// <summary>
        /// 为指定实体构建标签类型的系统列表
        /// </summary>
        private List<SystemBase> BuildSystemOfTag(Type tagType, YuoEntity entity)
        {
            List<SystemBase> result = new();
            if (entity.IsDisposed) return result;
            if (GetSystemOfTag(tagType, out var systems))
            {
                foreach (var system in systems)
                {
                    if (system.AddComponent(entity))
                    {
                        result.Add(system);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 为多个实体构建指定类型的系统列表
        /// </summary>
        private List<SystemBase> BuildSystemOfTag<T>(List<YuoEntity> entities) where T : ISystemTag
        {
            List<SystemBase> result = new();
            Type type = typeof(T);
            if (GetSystemOfTag(type, out var systems))
            {
                foreach (var system in systems)
                {
                    foreach (var entity in entities)
                    {
                        if (entity.IsDisposed) continue;
                        if (system.AddComponent(entity))
                        {
                            result.Add(system);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 静态方法：为指定实体构建系统列表
        /// </summary>
        public static List<SystemBase> BuildSystem<T>(YuoEntity entity) where T : ISystemTag
        {
            return Instance.BuildSystemOfTag<T>(entity);
        }

        /// <summary>
        /// 静态方法：为多个实体构建系统列表
        /// </summary>
        public static List<SystemBase> BuildSystem<T>(List<YuoEntity> entities) where T : ISystemTag
        {
            return Instance.BuildSystemOfTag<T>(entities);
        }

        /// <summary>
        /// 静态方法：根据标签类型为指定实体构建系统列表
        /// </summary>
        public static List<SystemBase> BuildSystem(Type tagType, YuoEntity entity)
        {
            return Instance.BuildSystemOfTag(tagType, entity);
        }

        /// <summary>
        /// 为指定实体构建标签类型的系统列表
        /// </summary>
        private List<SystemBase> BuildSystemOfTag<T>(YuoEntity entity) where T : ISystemTag
        {
            return BuildSystemOfTag(typeof(T), entity);
        }

        #endregion

        #region BuildSystemForComponent

        /// <summary>
        /// 为指定组件构建标签类型的系统列表
        /// </summary>
        private List<SystemBase> BuildSystemOfTag(Type tagType, YuoComponent component)
        {
            List<SystemBase> result = new();
            if (component.IsDisposed) return result;
            if (GetSystemOfComponent(component.Type, out var systems))
            {
                foreach (var system in systems)
                {
                    if (system.HasTag(tagType) && system.AddComponent(component.Entity))
                    {
                        result.Add(system);
                    }
                }
            }

            return result;
        }

        public static List<SystemBase> BuildSystemOfComponent(params Type[] components)
        {
            List<SystemBase> result = new();
            foreach (var system in Instance.allSystem)
            {
                bool hasComponent = true;
                foreach (var component in components)
                {
                    if (!system.InfluenceTypes().Contains(component))
                    {
                        hasComponent = false;
                        break;
                    }
                }

                if (hasComponent)
                {
                    result.Add(system);
                }
            }

            return result;
        }

        public static List<SystemBase> BuildSystemOfComponent(params YuoComponent[] components)
        {
            List<SystemBase> result = new();
            List<Type> componentsType = new();
            foreach (var component in components)
            {
                componentsType.Add(component.Type);
            }

            var systems = BuildSystemOfComponent(componentsType.ToArray());
            foreach (var system in systems)
            {
                if (system.Entities.Contains(components[0].Entity))
                {
                    result.Add(system);
                }
            }

            return result;
        }


        /// <summary>
        /// 静态方法：为指定组件构建系统列表
        /// </summary>
        public static List<SystemBase> BuildSystemOfTagAndComponent(Type tagType, params Type[] components)
        {
            List<SystemBase> result = new();
            if (Instance.GetSystemOfTag(tagType, out var systems))
            {
                foreach (var system in systems)
                {
                    bool hasComponent = true;
                    foreach (var component in components)
                    {
                        if (!system.InfluenceTypes().Contains(component))
                        {
                            hasComponent = false;
                            break;
                        }
                    }

                    if (hasComponent)
                    {
                        result.Add(system);
                    }
                }
            }

            return result;
        }

        public static List<SystemBase> BuildSystemOfTagAndComponent<T>(params Type[] components) where T : ISystemTag
        {
            return BuildSystemOfTagAndComponent(typeof(T), components);
        }

        public static List<SystemBase> BuildSystemOfTagAndComponent<T, TC1>()
            where T : ISystemTag where TC1 : YuoComponent
        {
            return BuildSystemOfTagAndComponent(typeof(T), typeof(TC1));
        }

        public static List<SystemBase> BuildSystemOfTagAndComponent<T, TC1, TC2>() where T : ISystemTag
            where TC1 : YuoComponent
            where TC2 : YuoComponent
        {
            return BuildSystemOfTagAndComponent(typeof(T), typeof(TC1), typeof(TC2));
        }

        public static List<SystemBase> BuildSystemOfTagAndComponent<T, TC1, TC2, TC3>() where T : ISystemTag
            where TC1 : YuoComponent
            where TC2 : YuoComponent
            where TC3 : YuoComponent
        {
            return BuildSystemOfTagAndComponent(typeof(T), typeof(TC1), typeof(TC2), typeof(TC3));
        }

        public static List<SystemBase> BuildSystemOfTagAndComponent<T, TC1, TC2, TC3, TC4>()
            where T : ISystemTag where TC1 : YuoComponent
        {
            return BuildSystemOfTagAndComponent(typeof(T), typeof(TC1), typeof(TC2), typeof(TC3), typeof(TC4));
        }

        /// <summary>
        /// 为指定组件构建标签类型的系统列表
        /// </summary>
        private List<SystemBase> BuildSystemOfTag<T>(YuoComponent component) where T : ISystemTag
        {
            return BuildSystemOfTag(typeof(T), component);
        }

        /// <summary>
        /// 静态方法：为指定组件构建系统列表
        /// </summary>
        public static List<SystemBase> BuildSystem<T>(YuoComponent component) where T : ISystemTag
        {
            return Instance.BuildSystemOfTag<T>(component);
        }

        /// <summary>
        /// 静态方法：根据标签类型为指定组件构建系统列表
        /// </summary>
        public static List<SystemBase> BuildSystem(Type tagType, YuoComponent component)
        {
            return Instance.BuildSystemOfTag(tagType, component);
        }

        #endregion

        #region BuildSystemForBaseComponent

        /// <summary>
        /// 为基础组件构建标签类型的系统列表
        /// </summary>
        private List<SystemBase> BuildSystemOfBaseComponent(Type tagType, YuoComponent component)
        {
            List<SystemBase> result = new();
            if (component.IsDisposed) return result;
            if (component.BaseComponentType != null)
            {
                if (GetSystemOfBaseComponent(component.BaseComponentType, out var systems))
                {
                    foreach (var system in systems)
                    {
                        if (system.HasTag(tagType) && system.AddComponent(component.Entity))
                        {
                            result.Add(system);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 静态方法：为基础组件构建标签类型的系统列表
        /// </summary>
        public static List<SystemBase> BuildSystemOfBase(Type tagType, YuoComponent component)
        {
            return Instance.BuildSystemOfBaseComponent(tagType, component);
        }

        /// <summary>
        /// 静态方法：为基础组件构建指定类型的系统列表
        /// </summary>
        public static List<SystemBase> BuildSystemOfBase<T>(YuoComponent component) where T : ISystemTag
        {
            return Instance.BuildSystemOfBaseComponent(typeof(T), component);
        }

        #endregion

        #region BuildSystemForAllEntity

        /// <summary>
        /// 根据标签构建对应的系统列表（所有实体）
        /// </summary>
        private List<SystemBase> BuildSystemForAllEntity<T>() where T : ISystemTag
        {
            return BuildSystemForAllEntity(typeof(T));
        }

        /// <summary>
        /// 根据标签类型构建对应的系统列表（所有实体）
        /// </summary>
        private List<SystemBase> BuildSystemForAllEntity(Type tagType)
        {
            List<SystemBase> result = new();
            if (systemsOfTag.TryGetValue(tagType, out var list))
            {
                result.AddRange(list);
            }

            return result;
        }

        /// <summary>
        /// 静态方法：为所有实体构建指定类型的系统列表
        /// </summary>
        public static List<SystemBase> BuildSystemForAll<T>() where T : ISystemTag
        {
            return Instance.BuildSystemForAllEntity<T>();
        }

        /// <summary>
        /// 静态方法：为所有实体构建指定标签类型的系统列表
        /// </summary>
        public static List<SystemBase> BuildSystemForAll(Type tagType)
        {
            return Instance.BuildSystemForAllEntity(tagType);
        }

        #endregion

        public static void RunSystem<T>(List<SystemBase> systems) where T : ISystemTag
        {
            var tag = typeof(T);
            foreach (var system in systems)
            {
                system.RunType = tag;
                system.m_Run();
            }
        }

        public static void RunSystem<T>(List<SystemBase> systems, YuoEntity entity) where T : ISystemTag
        {
            var tag = typeof(T);
            foreach (var system in systems)
            {
                system.RunType = tag;
                system.m_Run(entity);
            }
        }

        public static void RunSystem<T>(List<SystemBase> systems, YuoComponent component) where T : ISystemTag
        {
            var tag = typeof(T);
            var entity = component.Entity;
            foreach (var system in systems)
            {
                system.RunType = tag;
                system.m_Run(entity);
            }
        }

        #endregion


        public static void DisableSystem<T>() where T : SystemBase
        {
            var type = typeof(T);
            var system = Instance.systemDic[type];
            if (system != null)
                system.Enabled = false;
        }

        public static void EnableSystem<T>() where T : SystemBase
        {
            var type = typeof(T);
            var system = Instance.systemDic[type];
            if (system != null)
                system.Enabled = true;
        }
    }

    public static class YuoWorldExtension
    {
        public static List<SystemBase> SplitOfTag<T>(this List<SystemBase> systems) where T : ISystemTag
        {
            List<SystemBase> result = new();
            foreach (var system in systems)
            {
                if (system.HasTag<T>())
                {
                    result.Add(system);
                }
            }

            return result;
        }
    }
}