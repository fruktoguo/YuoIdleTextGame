using DG.Tweening;
using YuoTools.Extend.Helper;
using YuoTools.Main.Ecs;

namespace YuoTools.UI
{
    public partial class View_BattleComponent
    {
    }

    public class ViewBattleCreateSystem : YuoSystem<View_BattleComponent>, IUICreate
    {
        public override string Group => "UI/Battle";

        protected override void Run(View_BattleComponent view)
        {
            view.FindAll();
        }
    }

    public class ViewBattleOpenSystem : YuoSystem<View_BattleComponent>, IUIOpen
    {
        public override string Group => "UI/Battle";

        protected override async void Run(View_BattleComponent view)
        {
            var role1 = view.AddChildAndInstantiate(view.Child_Role);
            role1.rectTransform.Hide();
            role1.rectTransform.SetPosRatioInParent(0.2f, 0.1f);
            
            role1.Data.Speed.AddBaseValue("Test".GetHashCode(), 500);

            role1.Rig.mass = 100;
            
            role1.Entity.EntityName = "力量型";

            var role2 = view.AddChildAndInstantiate(view.Child_Role);
            role2.rectTransform.Hide();
            role2.rectTransform.SetPosRatioInParent(0.8f, 0.9f);
            
            role2.Data.Speed.AddBaseValue("Test".GetHashCode(), 250);
            role2.Data.Def.AddBaseValue("Test".GetHashCode(), 10);
            
            role2.Entity.EntityName = "敏捷型";
            

            role1.Target = role2;
            role2.Target = role1;

            await YuoWait.WaitFrameAsync();
            role2.rectTransform.Show();
            role1.rectTransform.Show();
        }
    }

    public class ViewBattleCloseSystem : YuoSystem<View_BattleComponent>, IUIClose
    {
        public override string Group => "UI/Battle";

        protected override void Run(View_BattleComponent view)
        {
        }
    }
}