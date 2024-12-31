using DG.Tweening;
using UnityEngine;
using YuoTools.Extend.Helper;
using YuoTools.Main.Ecs;

namespace YuoTools.UI
{
    public partial class View_PropertyComponent
    {
        public float value;
        public float maxValue;
    }

    public class ViewPropertyCreateSystem : YuoSystem<View_PropertyComponent>, IUICreate
    {
        public override string Group => "UI/Property";

        public override void Run(View_PropertyComponent view)
        {
            view.FindAll();
        }
    }

    public class ViewPropertyUpdateSystem : YuoSystem<View_PropertyComponent>, IUIUpdate
    {
        public override void Run(View_PropertyComponent view)
        {
            view.Image_Mask.fillAmount = view.value / view.maxValue;
        }
    }
}