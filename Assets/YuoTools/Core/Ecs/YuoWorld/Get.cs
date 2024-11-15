using System;
using System.Collections.Generic;

namespace YuoTools.Main.Ecs
{
    public partial class YuoWorld
    {
        /// <summary>
        /// 通过标签获取系统
        /// </summary>
        private bool GetSystemOfTag(Type type, out List<SystemBase> systems)
        {
            if (systemsOfTag.TryGetValue(type, out var value))
            {
                systems = value;
                return true;
            }

            systems = null;
            return false;
        }

        /// <summary>
        ///  通过组件获取系统
        /// </summary>
        private bool GetSystemOfComponent(Type type, out List<SystemBase> systems)
        {
            if (systemsOfComponent.TryGetValue(type, out var value))
            {
                systems = value;
                return true;
            }

            systems = null;
            return false;
        }

        private bool GetSystemOfBaseComponent(Type type, out List<SystemBaseBase> systems)
        {
            if (systemsOfBaseComponent.TryGetValue(type, out var list))
            {
                systems = list;
                return true;
            }

            systems = null;
            return false;
        }

        public MultiHashSetMap<Type, YuoComponent> GetAllComponents() => components;
        public MultiMap<Type, SystemBase> GetAllSystemOfComponent() => systemsOfComponent;
        public MultiMap<Type, SystemBase> GetAllSystemOfTag() => systemsOfTag;
        public List<SystemBase> GetAllSystem => allSystem;
    }
}