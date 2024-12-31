using DG.Tweening;
using UnityEngine;
using YuoTools.Extend.Helper;
using YuoTools.Main.Ecs;

namespace YuoTools.UI
{
    public partial class View_NumComponent
    {
        public bool IsShow;

        public float timer;

        public void Show()
        {
            IsShow = true;
            rectTransform.Show();
            MainTextMeshProUGUI.SetColorA(1);
        }

        public void Hide()
        {
            IsShow = false;
            rectTransform.Hide();
        }
    }

    public class ViewNumCreateSystem : YuoSystem<View_NumComponent>, IUICreate
    {
        public override string Group => "UI/Num";

        public override void Run(View_NumComponent view)
        {
            view.FindAll();
        }
    }

    public class ViewNumOpenSystem : YuoSystem<View_NumComponent>, IUIOpen, IUIClose
    {
        public override string Group => "UI/Num";

        public override void Run(View_NumComponent view)
        {
            if (RunType == SystemTagType.UIOpen)
            {
                view.Show();
            }

            if (RunType == SystemTagType.UIClose)
            {
                view.Hide();
            }
        }
    }

    public class ViewNumUpdateSystem : YuoSystem<View_NumComponent>, IUpdate
    {
        public override string Group => "UI/Num";

        public override void Run(View_NumComponent view)
        {
            if (view.IsShow)
            {
                view.rectTransform.AddAnchoredPosY(500 * Time.deltaTime);
                view.MainTextMeshProUGUI.SetColorA(view.MainTextMeshProUGUI.color.a - Time.deltaTime);
            }
        }
    }
}