using DG.Tweening;
using YuoTools.Extend.Helper;
using YuoTools.Main.Ecs;

namespace YuoTools.UI
{
    public partial class View_DistanceInfoComponent
    {
    }

    public class ViewDistanceInfoCreateSystem : YuoSystem<View_DistanceInfoComponent>, IUICreate
    {
        public override string Group => "UI/DistanceInfo";

        public override void Run(View_DistanceInfoComponent view)
        {
            view.FindAll();
        }
    }
}