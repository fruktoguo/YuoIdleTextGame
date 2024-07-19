using DG.Tweening;
using UnityEngine;
using UnityEngine.Pool;
using YuoTools.Extend.Helper;
using YuoTools.Main.Ecs;

namespace YuoTools.UI
{
    public partial class View_DamageNumComponent
    {
        public ObjectPool<View_NumComponent> numPool;

        public void ShowNum(long num, Vector3 pos)
        {
        }
    }

    public class ViewDamageNumCreateSystem : YuoSystem<View_DamageNumComponent>, IUICreate
    {
        public override string Group => "UI/DamageNum";

        protected override void Run(View_DamageNumComponent view)
        {
            view.FindAll();
            view.numPool = new ObjectPool<View_NumComponent>(() => view.AddChildAndInstantiate(view.Child_Num));
        }
    }

    public class ViewDamageNumOpenSystem : YuoSystem<View_DamageNumComponent>, IUIOpen
    {
        public override string Group => "UI/DamageNum";

        protected override void Run(View_DamageNumComponent view)
        {
        }
    }

    public class ViewDamageNumCloseSystem : YuoSystem<View_DamageNumComponent>, IUIClose
    {
        public override string Group => "UI/DamageNum";

        protected override void Run(View_DamageNumComponent view)
        {
        }
    }
}