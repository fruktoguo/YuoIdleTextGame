using UnityEngine;
using UnityEngine.Pool;
using YuoTools.Main.Ecs;

namespace YuoTools.UI
{
    public partial class View_DamageNumComponent
    {
        public ObjectPool<View_NumComponent> numPool;

        public async void ShowNum(long num, Vector3 pos)
        {
            var numView = numPool.Get();
            numView.MainTextMeshProUGUI.text = (-num).ToString();
            numView.rectTransform.position = pos;
            numView.rectTransform.anchoredPosition += new Vector2(Random.Range(-50, 50), Random.Range(0, 50));
            await YuoWait.WaitTimeAsync(0.5f);
            numPool.Release(numView);
        }
    }

    public class ViewDamageNumCreateSystem : YuoSystem<View_DamageNumComponent>, IUICreate
    {
        public override string Group => "UI/DamageNum";

        public override void Run(View_DamageNumComponent view)
        {
            view.FindAll();
            view.numPool = new ObjectPool<View_NumComponent>(() => view.AddChildAndInstantiate(view.Child_Num),
                YuoWorld.RunSystem<IUIOpen>, YuoWorld.RunSystem<IUIClose>);
        }
    }

    public class ViewDamageNumOpenSystem : YuoSystem<View_DamageNumComponent>, IUIOpen
    {
        public override string Group => "UI/DamageNum";

        public override void Run(View_DamageNumComponent view)
        {
        }
    }

    public class ViewDamageNumCloseSystem : YuoSystem<View_DamageNumComponent>, IUIClose
    {
        public override string Group => "UI/DamageNum";

        public override void Run(View_DamageNumComponent view)
        {
        }
    }
}