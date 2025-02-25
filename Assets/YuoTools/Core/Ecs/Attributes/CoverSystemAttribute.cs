using System;

namespace YuoTools.Main.Ecs
{
    public class CoverSystemAttribute : Attribute
    {
        public Type coverSystemType { get; }
        public Type[] componentTypes { get; }

        /// <summary>
        /// 当Entity包含了全部的组件时，会让指定系统失效
        /// </summary>
        /// <param name="coverSystemType">要覆盖的系统类型</param>
        /// <param name="componentTypes">要求的组件类型</param>
        public CoverSystemAttribute(Type coverSystemType, params Type[] componentTypes)
        {
            this.coverSystemType = coverSystemType;
            this.componentTypes = componentTypes;
        }
    }
}