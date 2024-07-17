using DG.Tweening;
using SimpleJSON;
using UnityEditor.ShaderGraph.Serialization;
using UnityEngine;
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

            var text = FileHelper.ReadAllText($"{Application.dataPath}/Resources/Build.json");
            var build = JSONNode.Parse(text);
            if (build != null)
            {
                foreach (var (_, node) in build)
                {
                    var item = view.AddChildAndInstantiate(view.Child_Item);
                    item.Init(node);
                }
            }

            view.FlowLayout_Content.ArrangeChildren();
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