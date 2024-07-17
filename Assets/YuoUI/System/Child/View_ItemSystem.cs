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

        public void Init(JSONNode node)
        {
            TextMeshProUGUI_Text.text = node[a.Name];

            var produce = node[a.Produce];

            foreach (var (_, p) in produce)
            {
                // $"检索到配方{p[a.Name]}_需要时间{p[a.Time]}s".Log();
                MainButton.SetBtnClick(() => Produce(p));
            }

            rectTransform.sizeDelta = TextMeshProUGUI_Text.rectTransform.GetPreferredSize() + new Vector2(30, 30);
        }

        public void Produce(JSONNode node)
        {
            if (node.HasKey(a.In))
            {
                var input = node[a.In];

                string result = "";
                bool condition = true;
                foreach (var (_, item) in input)
                {
                    if (!ConditionHelper.Check(item, out var error))
                    {
                        result += error;
                        condition = false;
                    }
                }

                if (!condition)
                {
                    $"不满足条件,{result}".Log();
                    return;
                }

                foreach (var (_, item) in input)
                {
                    if (item[a.Type].Value == a.Item)
                    {
                        ItemHelper.RemoveItem(item[a.Name], item[a.Num].AsInt);
                    }
                }
            }
            //没有输入就直接输出

            var output = node[a.Out];

            foreach (var (_, item) in output)
            {
                ItemHelper.AddItem(item[a.Name], item[a.Num].AsInt);
            }

            countMax = node[a.Time].AsFloat;
            countTimer = countMax;
        }
    }

    public class ViewItemComponentUpdateSystem : YuoSystem<View_ItemComponent>, IUpdate
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