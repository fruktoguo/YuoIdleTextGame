﻿using System;
using System.Collections.Generic;

namespace YuoTools.Main.Ecs
{
    public abstract class SystemBase
    {
        public bool Enabled { get; set; } = true;

        public readonly List<YuoEntity> Entities = new();

        public int EntityCount => Entities.Count;

        public virtual string Name => Type.Name;

        public virtual string Group => "";

#if UNITY_EDITOR

        public double TimeConsuming { get; private set; }

        public double TotalTimeConsuming { get; private set; }

        public long TotalRunCount { get; private set; }

#endif

        protected SystemBase()
        {
#if UNITY_EDITOR
            _sw = new System.Diagnostics.Stopwatch();
            TimeConsuming = 0;
            TotalTimeConsuming = 0;
            TotalRunCount = 0;
#endif
        }

#if UNITY_EDITOR

        internal void StartClock()
        {
            _sw.Restart();
        }

        internal void StopClock()
        {
            _sw.Stop();
            SetTimeConsuming(_sw.Elapsed.TotalMilliseconds);
        }


        private void SetTimeConsuming(double time)
        {
            TimeConsuming = time;
            TotalTimeConsuming += time;
            TotalRunCount++;
        }

        private readonly System.Diagnostics.Stopwatch _sw;

#endif

        public abstract void Init(YuoWorld yuoWorld);

        public abstract List<Type> InfluenceTypes();

        public Type Type { get; internal set; }

        public Type RunType;

        internal bool hasCoverSystem;

        internal bool CheckCoverSystem(YuoEntity entity)
        {
            if (!hasCoverSystem) return false;
            if (YuoWorld.Instance is { } world && world.coverSystemDic.TryGetValue(RunType, out var cover))
            {
                foreach (var component in cover.componentTypes)
                {
                    if (world.baseComponents.TryGetValue(component, out var baseComponent))
                    {
                        if (!entity.HasBaseComponent(baseComponent))
                            return false;
                    }
                    else
                    {
                        if (!entity.HasComponent(component))
                            return false;
                    }
                }
            }

            return false;
        }

        internal bool CheckCoverSystem(int entityIndex) => entityIndex >= 0 && entityIndex < Entities.Count &&
                                                           CheckCoverSystem(Entities[entityIndex]);


        internal void m_Run()
        {
            if (!Enabled) return;
            if (Entities.Count == 0)
            {
                return;
            }

#if UNITY_EDITOR
            StartClock();
#endif
            if (YuoWorld.CloseSystemTry)
            {
                for (int i = 0; i < Entities.Count; i++)
                {
                    RunForIndex(i);
                }
#if UNITY_EDITOR
                StopClock();
#endif
                return;
            }

            for (int i = 0; i < Entities.Count; i++)
            {
                try
                {
                    RunForIndex(i);
                }
                catch (Exception e)
                {
                    e.LogError();
                }
            }

#if UNITY_EDITOR
            StopClock();
#endif
        }

        internal virtual void Clear()
        {
            Entities.Clear();
            systemTags.Clear();
            InfluenceTypes().Clear();
        }

        internal void RunForIndex(int entityIndex)
        {
            if (CheckCoverSystem(entityIndex)) return;
            m_Run(entityIndex);
        }

        protected internal abstract void m_Run(int entityIndex);

        internal bool m_Run(YuoEntity entity)
        {
            if (!Enabled) return false;
            var entityIndex = Entities.IndexOf(entity);
            if (entityIndex == -1) return false;
            if (CheckCoverSystem(entity)) return false;

#if UNITY_EDITOR
            StartClock();
#endif
            if (YuoWorld.CloseSystemTry)
            {
                RunForIndex(entityIndex);
#if UNITY_EDITOR
                StopClock();
#endif
                return true;
            }

            try
            {
                RunForIndex(entityIndex);
            }
            catch (Exception e)
            {
                e.LogError();
            }
#if UNITY_EDITOR
            StopClock();
#endif
            return true;
        }

        internal abstract void SetComponent(YuoEntity entity, Type type, YuoComponent component2);

        internal abstract bool AddComponent(YuoEntity entity);

        internal abstract void RemoveComponent(YuoEntity entity);

        internal List<Type> systemTags = new List<Type>();

        public bool HasTag<T>() where T : ISystemTag
        {
            return systemTags.Contains(typeof(T));
        }

        public bool HasTag(Type type)
        {
            return systemTags.Contains(type);
        }
    }

    public abstract class SystemBaseBase : SystemBase
    {
        internal abstract bool CheckIsBaseComponent();
        internal abstract void m_Run(YuoComponent baseComponent);
    }
}