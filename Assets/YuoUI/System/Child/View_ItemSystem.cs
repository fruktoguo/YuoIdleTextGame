using SimpleJSON;
using UnityEngine;
using YuoTools.Extend.Helper;
using YuoTools.Main.Ecs;

namespace YuoTools.UI
{
    public partial class View_ItemComponent
    {
        public float countTimer;
        public float countMax;

        public JSONNode produce;

        public void Init(JSONNode node)
        {
            TextMeshProUGUI_Text.text = node[a.Name];

            produce = node[a.Produce];

            MainButton.SetBtnClick(() => Produce(produce));

            rectTransform.AutoPreferredSize(TextMeshProUGUI_Text.rectTransform, new Vector2(30, 30));
        }

        public void Produce(JSONNode node)
        {
            if (countTimer > 0)
            {
                $"正在冷却,还剩 {countTimer:f1} s".ViewLog();
                return;
            }

            if (InOutHelper.HasIn(node))
            {
                var input = node[a.In];

                var condition = InOutHelper.TryCheckIn(input, out var result);

                if (!condition)
                {
                    $"不满足条件,{result}".ViewLog();
                    return;
                }

                InOutHelper.GenerateInList(input);
            }
            
            //没有输入就直接输出
            var output = node[a.Out];

            InOutHelper.GenerateOutList(output);

            countMax = node[a.Time].AsFloat;
            countTimer = countMax;
        }
    }

    public class ViewItemComponentUpdateSystem : YuoSystem<View_ItemComponent>, IUIUpdate
    {
        protected override void Run(View_ItemComponent view)
        {
            if (view.countTimer > 0)
            {
                view.countTimer -= Time.deltaTime;
                view.Image_Mask.fillAmount = view.countTimer / view.countMax;
            }
        }
    }
}