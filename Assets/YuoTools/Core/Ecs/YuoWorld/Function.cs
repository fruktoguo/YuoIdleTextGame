using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace YuoTools.Main.Ecs
{
    /// <summary>
    /// YuoWorld 类是 ECS 系统的核心类，管理实体、组件和系统
    /// </summary>
    public partial class YuoWorld
    {
        /// <summary>
        /// 根据 ID 获取实体
        /// </summary>
        public YuoEntity GetEntity(long instanceId)
        {
            Entities.TryGetValue(instanceId, out YuoEntity entity);
            return entity;
        }

        /// <summary>
        /// 根据实体名称获取实体
        /// </summary>
        public YuoEntity GetEntity(string entityName)
        {
            Entities.TryGetValue(entityName.GetHashCode(), out YuoEntity entity);
            return entity;
        }

        /// <summary>
        /// 设置实体的父实体
        /// </summary>
        public void SetParent(YuoEntity entity, YuoEntity parent)
        {
            entity.SetParent(parent);
        }

        /// <summary>
        /// 根据 ID 获取组件
        /// </summary>
        public T GetComponent<T>(long instanceId) where T : YuoComponent
        {
            return GetEntity(instanceId)?.GetComponent<T>();
        }

        /// <summary>
        /// 获取指定类型的所有组件
        /// </summary>
        public HashSet<YuoComponent> GetAllComponents<T>() where T : YuoComponent
        {
            return components.ContainsKey(typeof(T)) ? components[typeof(T)] : null;
        }


        /// <summary>
        /// 根据类型名称获取组件类型
        /// </summary>
        public Type GetComponentType(string typeName)
        {
            return allComponentType.GetValueOrDefault(typeName);
        }

        /// <summary>
        /// 刷新世界状态
        /// </summary>
        public void Refresh()
        {
            CoverSystem();
            SystemSort();
            GetLinkComponent();
            GetSingleComponent();
        }

        /// <summary>
        /// 获取系统的排序顺序
        /// </summary>
        private short GetOrder(Type type)
        {
            return type.GetCustomAttribute<SystemOrderAttribute>()?.Order ?? 0;
        }

        /// <summary>
        /// 加载程序集中的类型
        /// </summary>
        public void LoadAssembly(Assembly assembly)
        {
            LoadTypes(assembly.GetTypes());
        }

        /// <summary>
        /// 创建系统实例
        /// </summary>
        void CreateSystem(SystemBase system)
        {
            if (!allSystem.Contains(system))
            {
                allSystem.Add(system);
            }
        }

        /// <summary>
        /// 创建组件类型
        /// </summary>
        void CreateComponent(Type type)
        {
            allComponentType.TryAdd(type.Name, type);
            if (!components.ContainsKey(type))
            {
                components.Add(type, new());
            }

            var baseType = type.BaseType;
            if (baseType != null && baseType.BaseType == typeof(YuoComponent) &&
                !baseType.Name.Contains("YuoComponentInstance") &&
                !baseType.Name.Contains("YuoComponentGet"))
            {
                baseComponents[type] = baseType;
            }
        }

        /// <summary>
        /// 加载类型
        /// </summary>
        public void LoadTypes(Type[] types)
        {
            try
            {
                List<SystemBase> systems = new();

                foreach (var type in types)
                {
                    var systemBase = CheckSystem<SystemBase>(type);
                    var yuoComponent = CheckComponent<YuoComponent>(type);

                    if (systemBase != null)
                    {
                        systems.Add(systemBase);
                        systemBase.Type = type;
                    }
                    else if (yuoComponent)
                    {
                        CreateComponent(type);
                    }
                }

                foreach (var system in systems)
                {
                    CreateSystem(system);
                }
            }
            catch (Exception e)
            {
                e.LogError();
            }
        }

        /// <summary>
        /// 对系统进行排序
        /// </summary>
        public void SystemSort()
        {
            allSystem.Sort((a, b) => GetOrder(a.Type) - GetOrder(b.Type));

            systemsOfComponent.Clear();
            systemsOfTag.Clear();

            foreach (var system in allSystem)
            {
                system.Clear();
                system.Init(this);
                foreach (var inter in system.Type.GetInterfaces())
                {
                    foreach (var iInter in inter.GetInterfaces())
                    {
                        if (iInter == typeof(ISystemTag))
                        {
                            if (!systemsOfTag.ContainsKey(inter))
                                systemsOfTag.Add(inter, new());
                            systemsOfTag[inter].Add(system);
                            system.systemTags.Add(inter);
                        }
                    }
                }
            }

            foreach (var systems in systemsOfComponent.Values)
            {
                systems.Sort((a, b) => GetOrder(a.Type) - GetOrder(b.Type));
            }

            foreach (var system in allSystem)
            {
                foreach (var entity in Entities.Values)
                {
                    system.AddComponent(entity);
                }
            }
        }

        /// <summary>
        /// 获取链接组件
        /// </summary>
        public void GetLinkComponent()
        {
            foreach (var type in GetAllComponentOfType().Values)
            {
                var link = type.GetCustomAttributes<LinkComponentOfAttribute>();

                foreach (var linkComponentAttribute in link)
                {
                    linkComponent.AddItem(linkComponentAttribute.LinkType, type);
                }
            }
        }

        /// <summary>
        /// 获取单例组件
        /// </summary>
        public void GetSingleComponent()
        {
            foreach (var type in GetAllComponentOfType().Values)
            {
                var single = type.GetCustomAttributes<SingleComponentAttribute>();
                if (single.Any())
                {
                    singleComponents.Add(type);
                }
            }
        }

        /// <summary>
        /// 获取所有组件类型
        /// </summary>
        public Dictionary<string, Type> GetAllComponentOfType()
        {
            return allComponentType;
        }

        public static void DisableSystem<T>() where T : SystemBase
        {
            var type = typeof(T);
            var system = Instance.systemDic[type];
            if (system != null)
                system.Enabled = false;
        }
    }
}