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
}