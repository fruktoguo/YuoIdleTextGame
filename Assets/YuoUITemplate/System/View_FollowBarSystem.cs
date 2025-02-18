﻿using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Pool;
using YuoTools.Extend.Helper;
using YuoTools.Main.Ecs;

namespace YuoTools.UI
{
    public partial class View_FollowBarComponent
    {
        public ObjectPool<View_FollowBarItemComponent> barPool;

        public DicList<Transform, View_FollowBarItemComponent> itemDic = new();
        public float Space = 10;

        public View_FollowBarItemComponent Follow(Transform target,FloatAction maxValueGetter, FloatAction valueGetter)
        {
            var item = barPool.Get();
            item.ValueGetter = valueGetter;
            item.MaxValueGetter = maxValueGetter;
            item.target = target;
            itemDic[target].Add(item);
            return item;
        }

        public void Remove(View_FollowBarItemComponent item)
        {
            if (item.target)
            {
                itemDic[item.target].Remove(item);
            }

            foreach (var (key, value) in itemDic)
            {
                if (!key)
                {
                    foreach (var viewFollowBarItemComponent in value)
                    {
                        barPool.Release(viewFollowBarItemComponent);
                    }

                    value.Clear();
                }
            }

            //移除空的字典
            itemDic.RemoveLinq(x => x.Value.Count == 0);
        }
    }

    public class ViewFollowBarCreateSystem : YuoSystem<View_FollowBarComponent>, IUICreate
    {
        public override string Group => "UI/FollowBar";

        public override void Run(View_FollowBarComponent view)
        {
            view.FindAll();
            view.barPool = new ObjectPool<View_FollowBarItemComponent>(
                () => view.AddChildAndInstantiate(view.Child_FollowBarItem),
                actionOnRelease: item =>
                {
                    item.RunSystem<IUIClose>();
                    item.rectTransform.Hide();
                },
                actionOnGet: item => { }, actionOnDestroy: item => item.Entity.Destroy());
        }
    }

    public class ViewFollowBarUpdateSystem : YuoSystem<View_FollowBarComponent>, IUIUpdate
    {
        public override string Group => "UI/FollowBar";

        public override void Run(View_FollowBarComponent view)
        {
            foreach (var (tran, barList) in view.itemDic)
            {
                float offset = -50;
                foreach (var bar in barList)
                {
                    bar.targetPos = bar.rectTransform.parent.InverseTransformPoint(tran.position).xy() +
                                    Vector2.up * offset;
                    offset -= bar.rectTransform.rect.height + view.Space;
                }
            }
        }
    }
}