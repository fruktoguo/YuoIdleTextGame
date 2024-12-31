using DG.Tweening;
using YuoTools.Extend.Helper;
using YuoTools.Main.Ecs;

namespace YuoTools.UI
{
    public partial class View_DynamicIslandComponent
    {
    }

    public class ViewDynamicIslandCreateSystem : YuoSystem<View_DynamicIslandComponent>, IUICreate
    {
        public override string Group => "UI/DynamicIsland";

        public override void Run(View_DynamicIslandComponent view)
        {
            view.FindAll();
        }
    }

    public class ViewDynamicIslandOpenSystem : YuoSystem<View_DynamicIslandComponent>, IUIOpen
    {
        public override string Group => "UI/DynamicIsland";

        public override void Run(View_DynamicIslandComponent view)
        {
        }
    }

    public class ViewDynamicIslandCloseSystem : YuoSystem<View_DynamicIslandComponent>, IUIClose
    {
        public override string Group => "UI/DynamicIsland";

        public override void Run(View_DynamicIslandComponent view)
        {
        }
    }
}