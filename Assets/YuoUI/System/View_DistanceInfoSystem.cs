using DG.Tweening;
using UnityEngine;
using YuoTools.Extend.Helper;
using YuoTools.Main.Ecs;

namespace YuoTools.UI
{
    public partial class View_DistanceInfoComponent
    {
        public void ShowDistance(GameObject obj1, GameObject obj2)
        {
            if (obj1 == null || obj2 == null) return;
            var dis = AddChildAndInstantiate(Child_DistanceItem);
            dis.Entity.EntityName = $"{obj1.name}_{obj2.name}";
            dis.startPos = () => obj1.transform.position;
            dis.endPos = () => obj2.transform.position;
        }
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