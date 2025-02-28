using System;
using System.Collections.Generic;

namespace YuoTools.Main.Ecs
{
    public partial class YuoEntity : IDisposable
    {
        public YuoEntity Parent { get; private set; }
        public EntityComponent EntityData { get; private set; }
        public Dictionary<Type, YuoComponent> Components { get; } = new();
        public HashSet<YuoEntity> Children { get; } = new();
        public MultiHashSetMap<Type, YuoComponent> ChildComponents { get; private set; } = new();
        public MultiMap<Type, YuoComponent> BaseComponents { get; private set; } = new();
        
        public YuoEntity()
        {
            //EntityComponent为基础组件,无法移除,不会显示在组件列表中,但当销毁时会自动移除
            //可以通过获取EntityComponent是否为null来判断Entity是否释放
            EntityData = new EntityComponent();
            EntityData.Entity = this;

            EntityData.Id = IDGenerate.GetID(this);

            YuoWorld.Instance.AddComponent(this, EntityData);
            YuoWorld.Instance.RegisterEntity(this);
            // $"Add Entity{EntityData.Id}".Log();
        }

        public YuoEntity(long id)
        {
            //EntityComponent为基础组件,无法移除,不会显示在组件列表中,但当销毁时会自动移除
            //可以通过获取EntityComponent是否为null来判断Entity是否释放
            EntityData = new EntityComponent();
            EntityData.Entity = this;

            EntityData.Id = id;

            YuoWorld.Instance.AddComponent(this, EntityData);
            YuoWorld.Instance.RegisterEntity(this);
            // $"Add Entity{EntityData.Id}".Log();
        }

        public YuoEntity(string name) : this(name.GetHashCode())
        {
            EntityName = name;
        }
    }
}