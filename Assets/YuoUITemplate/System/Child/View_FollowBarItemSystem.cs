using DG.Tweening;
using UnityEngine;
using YuoTools.Extend.Helper;
using YuoTools.Main.Ecs;

namespace YuoTools.UI
{
    public partial class View_FollowBarItemComponent
    {
        public Transform target;
        public Vector2 targetPos;

        public OneEuroFilter<Vector2> filter;

        public void Open()
        {
            this.RunSystem<IUIOpen>();
            this.rectTransform.Show();
        }

        public FloatAction MaxValueGetter;
        public FloatAction ValueGetter;

        public bool inAnima;
    }

    public class ViewFollowBarItemCreateSystem : YuoSystem<View_FollowBarItemComponent>, IUICreate
    {
        public override string Group => "UI/FollowBarItem";

        public override void Run(View_FollowBarItemComponent view)
        {
            view.FindAll();
            view.filter = new();
            view.filter.SetParameter(0.1f, 1, 4);
        }
    }

    public class ViewFollowBarItemOpenSystem : YuoSystem<View_FollowBarItemComponent>, IUIOpen
    {
        public override string Group => "UI/FollowBarItem";

        public override void Run(View_FollowBarItemComponent view)
        {
            if (view.ValueGetter == null || view.MaxValueGetter == null) return;
            view.inAnima = true;
            //播放一个展开的动画
            var rect = view.rectTransform;
            var text = view.MainYuoBar.text;
            var bg = view.MainYuoBar.background;
            var bar = view.MainYuoBar;

            // 重置初始状态
            bg.transform.localScale = Vector3.one.x0z();

            var currentWidth = rect.rect.width;
            var startWidth = 100f;
            rect.SetSizeX(startWidth);
            text.alpha = 0;
            var currentBarValue = view.ValueGetter.Invoke();
            bar.SliderValue = 0;

            // 创建动画序列
            Sequence sequence = DOTween.Sequence();

            // 1. 底板纵向展开动画 (0.5秒)
            sequence.Append(
                bg.rectTransform.DOScale(Vector3.one, 0.3f)
                    .SetEase(Ease.OutBack)
            );

            // 2. 文字渐现动画 (0.3秒)
            sequence.Append(
                text.DOFade(1, 0.3f)
                    .SetEase(Ease.InOutSine)
            );

            // 3. 进度条填充动画 (0.4秒)
            sequence.Append(
                DOTween.To(() => bar.SliderValue, x =>
                    {
                        bar.SliderValue = x;
                        rect.SetSizeX(Mathf.Lerp(startWidth, currentWidth, x / currentBarValue));
                    }, currentBarValue, 1f)
                    .SetEase(Ease.InOutQuad)
            );

            // 播放整个序列
            sequence.Play().OnComplete(() => view.inAnima = false);
        }
    }

    public class ViewFollowBarItemCloseSystem : YuoSystem<View_FollowBarItemComponent>, IUIClose
    {
        public override string Group => "UI/FollowBarItem";

        public override void Run(View_FollowBarItemComponent view)
        {
        }
    }

    public class ViewFollowBarItemUpdateSystem : YuoSystem<View_FollowBarItemComponent>, IUIUpdate
    {
        public override void Run(View_FollowBarItemComponent view)
        {
            view.rectTransform.anchoredPosition = view.filter.Step(view.targetPos, Time.deltaTime);
            if (!view.inAnima && view.ValueGetter != null && view.MaxValueGetter != null)
            {
                view.MainYuoBar.maxValue = view.MaxValueGetter.Invoke();
                view.MainYuoBar.SliderValue = view.ValueGetter.Invoke();
            }
        }
    }
}