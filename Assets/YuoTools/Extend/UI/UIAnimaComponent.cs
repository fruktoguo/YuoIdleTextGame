﻿using DG.Tweening;
using ET;
using UnityEngine;
using YuoTools.Main.Ecs;

namespace YuoTools.UI
{
    public partial class UIAnimaComponent : YuoComponent
    {
        public override string Name => "动画信息";

        public RectTransform rectTransform;

        public UISetting.UISate Sate;

        private float animatorDuration;

        public float AnimaDuration
        {
            get
            {
                if (doTweenAnimation)
                {
                    return doTweenAnimation.duration;
                }

                if (animator)
                {
                    return animatorDuration;
                }

                return 0;
            }
        }

        private DOTweenAnimation doTweenAnimation;

        private Animator animator;

        public async ETTask Open()
        {
            if (doTweenAnimation)
            {
                doTweenAnimation.DOPlayForward();

                Sate = UISetting.UISate.ShowAnima;
                await YuoWait.WaitTimeAsync(AnimaDuration);
            }
            else if (animator)
            {
                animator.Play("In");

                Sate = UISetting.UISate.ShowAnima;
                await YuoWait.WaitTimeAsync(AnimaDuration);
            }

            Sate = UISetting.UISate.Show;
        }

        public async ETTask Close()
        {
            if (doTweenAnimation)
            {
                doTweenAnimation.DOPlayBackwards();

                Sate = UISetting.UISate.HideAnima;
                await YuoWait.WaitTimeAsync(AnimaDuration);
            }
            else if (animator)
            {
                animator.Play("Out");

                Sate = UISetting.UISate.HideAnima;
                await YuoWait.WaitTimeAsync(AnimaDuration);
            }

            Sate = UISetting.UISate.Hide;
        }

        public void From(UISetting setting)
        {
            rectTransform = setting.transform as RectTransform;
            doTweenAnimation = setting.GetComponent<DOTweenAnimation>();
            animator = setting.GetComponent<Animator>();
            animatorDuration = setting.AnimatorLength;
        }
    }
}