using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace YuoTools.UI
{
    [Serializable]
    public class YuoViewPool<T> where T : UIComponent, new()
    {
        private T viewPrefab;
        private UIComponent parent;
        [ShowInInspector] private List<T> pool = new();
        [ShowInInspector] private List<T> activePool = new();

        public YuoViewPool(T viewPrefab, UIComponent parent)
        {
            this.viewPrefab = viewPrefab;
            this.parent = parent;
        }

        public T Get()
        {
            T view;
            if (pool.Count > 0)
            {
                view = pool[0];
                pool.RemoveAt(0);
            }
            else
            {
                view = Create();
            }
            activePool.Add(view);
            view.Entity.EntityName = viewPrefab.rectTransform.gameObject.name + "_" + activePool.Count;
            return view;
        }

        private T Create()
        {
            var view = parent.AddChildAndInstantiate(viewPrefab);
            return view;
        }

        public void Remove(T view)
        {
            if (activePool.Contains(view))
            {
                activePool.Remove(view);
                pool.Add(view);
            }
            else
            {
                $"要移除的物体 [{view}] 不属于该对象池".Log("#ff0000");
            }
        }
    }
}