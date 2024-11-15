using System;
using System.Collections.Generic;

namespace YuoTools.Main.Ecs
{
    public partial class YuoWorld
    {
        #region RunSystemForEntity

        /// <summary>
        /// 为指定实体运行标签类型的系统
        /// </summary>
        private void RunSystemOfTag(Type tagType, YuoEntity entity)
        {
            if (entity.IsDisposed) return;
            if (GetSystemOfTag(tagType, out var systems))
            {
                foreach (var system in systems)
                {
                    system.RunType = tagType;
                    system.m_Run(entity);
                }
            }
        }

        /// <summary>
        /// 为多个实体运行指定类型的系统
        /// </summary>
        private void RunSystemOfTag<T>(List<YuoEntity> entities) where T : ISystemTag
        {
            Type type = typeof(T);
            if (GetSystemOfTag(type, out var systems))
            {
                foreach (var system in systems)
                {
                    system.RunType = type;
                    foreach (var entity in entities)
                    {
                        if (entity.IsDisposed) continue;
                        system.m_Run(entity);
                    }
                }
            }
        }

        /// <summary>
        /// 静态方法：为指定实体运行系统
        /// </summary>
        public static void RunSystem<T>(YuoEntity entity) where T : ISystemTag
        {
            Instance.RunSystemOfTag<T>(entity);
        }

        /// <summary>
        /// 静态方法：为多个实体运行系统
        /// </summary>
        public static void RunSystem<T>(List<YuoEntity> entities) where T : ISystemTag
        {
            Instance.RunSystemOfTag<T>(entities);
        }

        /// <summary>
        /// 静态方法：根据标签类型为指定实体运行系统
        /// </summary>
        public static void RunSystem(Type tagType, YuoEntity entity)
        {
            Instance.RunSystemOfTag(tagType, entity);
        }

        /// <summary>
        /// 为指定实体运行标签类型的系统
        /// </summary>
        private void RunSystemOfTag<T>(YuoEntity entity) where T : ISystemTag
        {
            RunSystemOfTag(typeof(T), entity);
        }

        #endregion

        #region RunSystemForComponent

        /// <summary>
        /// 为指定组件运行标签类型的系统
        /// </summary>
        private void RunSystemOfTag(Type tagType, YuoComponent component)
        {
            if (component.IsDisposed) return;
            if (GetSystemOfComponent(component.Type, out var systems))
            {
                foreach (var system in systems)
                {
                    if (system.HasTag(tagType))
                    {
                        system.RunType = tagType;
                        system.m_Run(component.Entity);
                    }
                }
            }
        }

        /// <summary>
        /// 为指定组件运行标签类型的系统
        /// </summary>
        private void RunSystemOfTag<T>(YuoComponent component) where T : ISystemTag
        {
            RunSystemOfTag(typeof(T), component);
        }

        /// <summary>
        /// 静态方法：为指定组件运行系统
        /// </summary>
        public static void RunSystem<T>(YuoComponent component) where T : ISystemTag
        {
            Instance.RunSystemOfTag<T>(component);
        }

        /// <summary>
        /// 静态方法：根据标签类型为指定组件运行系统
        /// </summary>
        public static void RunSystem(Type tagType, YuoComponent component)
        {
            Instance.RunSystemOfTag(tagType, component);
        }

        #endregion

        #region RunSystemForBaseComponent

        /// <summary>
        /// 为基础组件运行标签类型的系统
        /// </summary>
        private void RunSystemOfBaseComponent(Type tagType, YuoComponent component)
        {
            if (component.IsDisposed) return;
            if (component.BaseComponentType != null)
            {
                if (GetSystemOfBaseComponent(component.BaseComponentType, out var systems))
                {
                    foreach (var system in systems)
                    {
                        if (system.HasTag(tagType))
                        {
                            system.RunType = tagType;
#if UNITY_EDITOR
                            system.StartClock();
#endif
                            system.m_Run(component);

#if UNITY_EDITOR
                            system.StopClock();
#endif
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 静态方法：为基础组件运行标签类型的系统
        /// </summary>
        public static void RunSystemOfBase(Type tagType, YuoComponent component)
        {
            Instance.RunSystemOfBaseComponent(tagType, component);
        }

        /// <summary>
        /// 静态方法：为基础组件运行指定类型的系统
        /// </summary>
        public static void RunSystemOfBase<T>(YuoComponent component) where T : ISystemTag
        {
            Instance.RunSystemOfBaseComponent(typeof(T), component);
        }

        #endregion

        /// <summary>
        /// 运行 Awake 系统
        /// </summary>
        private void RunAwakeSystem(YuoEntity entity, Type componentType)
        {
            foreach (var system in systemsOfComponent[componentType])
            {
                if (system.AddComponent(entity))
                {
                    if (system is IAwake)
                    {
                        system.RunType = SystemType.Awake;
#if UNITY_EDITOR
                        system.StartClock();
#endif
                        try
                        {
                            if (system.Enabled)
                                system.m_Run(system.EntityCount - 1);
#if UNITY_EDITOR
                            system.StopClock();
#endif
                        }
                        catch (Exception e)
                        {
                            e.LogError();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 运行基础组件的 Awake 系统
        /// </summary>
        private void RunAwakeSystemOfBase(YuoEntity entity, Type baseComponentType)
        {
            if (GetSystemOfBaseComponent(baseComponentType, out var systems))
            {
                foreach (var system in systems)
                {
                    if (system.AddComponent(entity))
                    {
                        if (system.systemTags.Contains(SystemType.Awake))
                        {
                            system.RunType = SystemType.Awake;
#if UNITY_EDITOR
                            system.StartClock();
#endif
                            try
                            {
                                if (system.Enabled)
                                    system.m_Run(system.EntityCount - 1);
#if UNITY_EDITOR
                                system.StopClock();
#endif
                            }
                            catch (Exception e)
                            {
                                e.LogError();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 运行指定类型的系统
        /// </summary>
        private void RunSystem<T>(YuoEntity entity, Type componentType) where T : ISystemTag
        {
            var runType = typeof(T);

            foreach (var system in systemsOfComponent[componentType])
            {
                if (system.AddComponent(entity))
                {
                    if (system is T)
                    {
                        system.RunType = runType;
#if UNITY_EDITOR
                        system.StartClock();
#endif
                        try
                        {
                            if (system.Enabled)
                                system.m_Run(system.EntityCount - 1);
#if UNITY_EDITOR
                            system.StopClock();
#endif
                        }
                        catch (Exception e)
                        {
                            e.LogError();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 运行基础组件的指定类型系统
        /// </summary>
        private void RunSystemOfBase<T>(YuoEntity entity, Type baseComponentType) where T : ISystemTag
        {
            if (GetSystemOfBaseComponent(baseComponentType, out var systems))
            {
                var runType = typeof(T);
                foreach (var system in systems)
                {
                    if (system.AddComponent(entity))
                    {
                        if (system is T)
                        {
                            system.RunType = runType;
#if UNITY_EDITOR
                            system.StartClock();
#endif
                            try
                            {
                                if (system.Enabled)
                                    system.m_Run(system.EntityCount - 1);
#if UNITY_EDITOR
                                system.StopClock();
#endif
                            }
                            catch (Exception e)
                            {
                                e.LogError();
                            }
                        }
                    }
                }
            }
        }

        #region RunSystemForAllEntity

        /// <summary>
        /// 根据标签执行对应的系统（所有实体）
        /// </summary>
        private void RunSystemForAllEntity<T>() where T : ISystemTag
        {
            RunSystemForAllEntity(typeof(T));
        }

        /// <summary>
        /// 根据标签类型执行对应的系统（所有实体）
        /// </summary>
        private void RunSystemForAllEntity(Type tagType)
        {
            if (systemsOfTag.TryGetValue(tagType, out var list))
            {
                foreach (var system in list)
                {
                    system.RunType = tagType;
                    system.m_Run();
                }
            }
        }

        /// <summary>
        /// 静态方法：为所有实体运行指定类型的系统
        /// </summary>
        public static void RunSystemForAll<T>() where T : ISystemTag
        {
            Instance.RunSystemForAllEntity<T>();
        }

        /// <summary>
        /// 静态方法：为所有实体运行指定标签类型的系统
        /// </summary>
        public static void RunSystemForAll(Type tagType)
        {
            Instance.RunSystemForAllEntity(tagType);
        }

        #endregion
    }
}