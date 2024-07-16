using DG.Tweening;
using YuoTools.Extend.Helper;
using YuoTools.Main.Ecs;

namespace YuoTools.UI
{
    public partial class View_MainComponent
    {
    }

    public class ViewMainCreateSystem : YuoSystem<View_MainComponent>, IUICreate
    {
        public override string Group => "UI/Main";

        protected override void Run(View_MainComponent view)
        {
            view.FindAll();
            //关闭窗口的事件注册,名字不同请自行更
        }
    }

    public class ViewMainOpenSystem : YuoSystem<View_MainComponent>, IUIOpen
    {
        public override string Group => "UI/Main";

        protected override void Run(View_MainComponent view)
        {
        }
    }

    public class ViewMainCloseSystem : YuoSystem<View_MainComponent>, IUIClose
    {
        public override string Group => "UI/Main";

        protected override void Run(View_MainComponent view)
        {
        }
    }
}