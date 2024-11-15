using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;

namespace YuoTools.Main.Ecs
{
    public partial class YuoWorld
    {
        /// <summary>
        /// 处理系统覆盖
        /// </summary>
        private void CoverSystem()
        {
            // 查找所有需要覆盖的系统类型
            var coverSystems = new List<Type>();
            foreach (var system in allSystem)
            {
                var coverSystem = system.GetType().GetCustomAttribute<CoverSystemAttribute>();
                if (coverSystem != null)
                {
                    coverSystems.Add(coverSystem.CoverType);
                }
            }

            // 在所有系统中查找需要覆盖的系统
            var tempSystem = new List<SystemBase>();
            foreach (var system in allSystem)
            {
                if (coverSystems.Contains(system.Type))
                    tempSystem.Add(system);
            }

            // 移除需要覆盖的系统
            foreach (var system in tempSystem)
            {
                RemoveSystem(system);
            }

            // 重新构建系统字典
            systemDic.Clear();
            foreach (var systemBase in allSystem)
            {
                systemDic.Add(systemBase.Type, systemBase);
            }
        }

        /// <summary>
        /// 从系统列表和映射中移除指定系统
        /// </summary>
        private void RemoveSystem(SystemBase system)
        {
            allSystem.Remove(system);
            systemsOfTag.Remove(system.GetType());
            systemsOfComponent.Remove(system.GetType());
        }

        [ShowInInspector]
        private List<YuoEntity> entityTrashTemp = new();
        private List<YuoComponent> componentsTrashTemp = new();

        /// <summary>
        /// 清理待销毁的实体和组件
        /// </summary>
        private void ClearTrash()
        {
            if (componentsTrash.Count == 0 && entityTrash.Count == 0) return;

            componentsTrashTemp.Clear();
            componentsTrashTemp.AddRange(componentsTrash);
            componentsTrash.Clear();
            
            foreach (var yuoComponent in componentsTrashTemp)
            {
                yuoComponent.Dispose();
            }

            entityTrashTemp.Clear();
            entityTrashTemp.AddRange(entityTrash);
            entityTrash.Clear();
            
            foreach (var entity in entityTrashTemp)
            {
                entity.Dispose();
            }
        }

        /// <summary>
        /// 注册系统
        /// </summary>
        internal void RegisterSystem(SystemBase system, Type type)
        {
            systemsOfComponent.AddItem(type, system);
        }

        /// <summary>
        /// 注册基础系统
        /// </summary>
        internal void RegisterBaseSystem(SystemBaseBase system, Type type)
        {
            systemsOfBaseComponent.AddItem(type, system);
        }

        /// <summary>
        /// 初始化主要数据
        /// </summary>
        private void Initialization()
        {
            var yuoTool = Assembly.Load("YuoTools");
            var assemblyCSharp = Assembly.Load("Assembly-CSharp");
            LoadAssembly(yuoTool);
            LoadAssembly(assemblyCSharp);
            Refresh();
        }

        #region 反射获取组件

        private static bool CheckComponent<TP>(Type find) where TP : class
        {
            var baseType = find.BaseType; //获取基类
            while (baseType != null) //获取所有基类
            {
                if (baseType.Name == typeof(TP).Name) return true;
                baseType = baseType.BaseType;
            }

            return false;
        }

        private static TP CheckSystem<TP>(Type find) where TP : class
        {
            var baseType = find.BaseType; //获取基类
            while (baseType != null) //获取所有基类
            {
                if (baseType.Name == typeof(TP).Name)
                {
                    try
                    {
                        object obj = Activator.CreateInstance(find);
                        if (obj != null)
                        {
                            TP info = obj as TP;
                            return info;
                        }
                    }
                    catch
                    {
                        // ignored
                    }

                    break;
                }
                else
                {
                    baseType = baseType.BaseType;
                }
            }

            return null;
        }

        #endregion
    }
}