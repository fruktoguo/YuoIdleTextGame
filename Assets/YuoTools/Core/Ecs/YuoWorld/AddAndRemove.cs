using System;

namespace YuoTools.Main.Ecs
{
    public partial class YuoWorld
    {
        /// <summary>
        /// 将实体标记为待销毁
        /// </summary>
        public static void DestroyEntity(YuoEntity entity)
        {
            Instance.entityTrash.Add(entity);
        }

        /// <summary>
        /// 将组件标记为待销毁
        /// </summary>
        public static void DestroyComponent(YuoComponent component)
        {
            Instance.componentsTrash.Add(component);
        }

        /// <summary>
        /// 立即强制销毁实体
        /// </summary>
        public static void DestroyEntityForce(YuoEntity entity)
        {
            entity?.Dispose();
        }

        /// <summary>
        /// 立即强制销毁组件
        /// </summary>
        public static void DestroyComponentForce(YuoComponent component)
        {
            component?.Dispose();
        }

        /// <summary>
        /// 注册实体到世界
        /// </summary>
        public void RegisterEntity(YuoEntity entity)
        {
            if (!Entities.TryAdd(entity.EntityData.Id, entity))
                $"实体ID重复，请检查：{entity.EntityData.Id}".LogError();
        }

        /// <summary>
        /// 从世界中注销实体
        /// </summary>
        public void UnRegisterEntity(YuoEntity entity)
        {
            if (Entities.ContainsKey(entity.EntityData.Id))
            {
                Entities.Remove(entity.EntityData.Id);
            }
        }

        /// <summary>
        /// 从实体中移除组件
        /// </summary>
        public void RemoveComponent(YuoEntity entity, YuoComponent component)
        {
            components[component.Type].Remove(component);
            // 世界销毁时，不调用 Destroy，只调用 ExitGame
            if (!_worldIsDestroy)
            {
                if (baseComponents.TryGetValue(component.Type, out var baseType))
                {
                    RunSystemOfBase<IDestroy>(entity, baseType);
                }

                foreach (var system in systemsOfComponent[component.Type])
                {
                    if (system is IDestroy)
                    {
                        system.RunType = SystemType.Destroy;
                        system.m_Run(entity);
                    }

                    system.RemoveComponent(entity);
                }
            }

            component.Dispose();
        }

        /// <summary>
        /// 将组件添加到实体
        /// </summary>
        public void AddComponent(YuoEntity entity, YuoComponent component)
        {
            AddComponent(entity, component, component.Type);
        }

        /// <summary>
        /// 将组件添加到实体，指定组件类型
        /// </summary>
        public void AddComponent(YuoEntity entity, YuoComponent component, Type componentType)
        {
            if (!components.ContainsKey(componentType))
            {
                components.Add(componentType, new());
            }

            if (!components[componentType].Contains(component))
            {
                if (baseComponents.TryGetValue(componentType, out var baseType))
                {
                    component.BaseComponentType = baseType;
                    entity.BaseComponents.AddItem(baseType, component);
                    RunAwakeSystemOfBase(entity, baseType);
                }

                components[componentType].Add(component);
                // 运行 Awake 系统
                RunAwakeSystem(entity, componentType);

                startSystems.Add(component);

                if (singleComponents.Contains(componentType))
                {
                    foreach (var yuoComponent in components[componentType])
                    {
                        yuoComponent.DestroyComponent();
                    }
                }
            }
        }

        /// <summary>
        /// 替换实体上的组件
        /// </summary>
        public void SetComponent(YuoComponent component1, YuoComponent component2)
        {
            YuoEntity entity = component1.Entity;
            var list = components[component1.Type];
            list.Remove(component1);
            list.Add(component2);

            foreach (var system in systemsOfComponent[component1.Type])
            {
                system.SetComponent(entity, component1.Type, component2);
            }

            RunSystemOfTag<ISwitchComponent>(component2);
        }

    }
}