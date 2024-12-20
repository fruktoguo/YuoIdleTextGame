using DG.Tweening;
using UnityEngine;
using YuoTools.Extend.Helper;
using YuoTools.Main.Ecs;

namespace YuoTools.UI
{
    public partial class View_FollowBarItemComponent
    {
        public Transform target;
    }

    public class ViewFollowBarItemCreateSystem : YuoSystem<View_FollowBarItemComponent>, IUICreate
    {
        public override string Group => "UI/FollowBarItem";

        protected override void Run(View_FollowBarItemComponent view)
        {
            view.FindAll();
        }
    }

    public class ViewFollowBarItemOpenSystem : YuoSystem<View_FollowBarItemComponent>, IUIOpen
    {
        public override string Group => "UI/FollowBarItem";

        protected override void Run(View_FollowBarItemComponent view)
        {
        }
    }

    public class ViewFollowBarItemCloseSystem : YuoSystem<View_FollowBarItemComponent>, IUIClose
    {
        public override string Group => "UI/FollowBarItem";

        protected override void Run(View_FollowBarItemComponent view)
        {
        }
    }
}