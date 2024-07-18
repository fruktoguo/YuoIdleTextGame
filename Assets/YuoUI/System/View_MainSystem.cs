using DG.Tweening;
using SimpleJSON;
using UnityEditor.ShaderGraph.Serialization;
using UnityEngine;
using UnityEngine.Events;
using YuoTools.Extend.Helper;
using YuoTools.Main.Ecs;

namespace YuoTools.UI
{
    public partial class View_MainComponent
    {
        public void Log(string message)
        {
            TextMeshProUGUI_Console.text += message + "\n";
        }
    }

    public class ViewMainCreateSystem : YuoSystem<View_MainComponent>, IUICreate
    {
        public override string Group => "UI/Main";

        protected override void Run(View_MainComponent view)
        {
            view.FindAll();
            //关闭窗口的事件注册,名字不同请自行更

            var text = DataHelper.LoadConfig("Build");

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
            YuoViewLogHelper.OnLog.AddListener(view.Log);
        }
    }

    public class ViewMainCloseSystem : YuoSystem<View_MainComponent>, IUIClose
    {
        public override string Group => "UI/Main";

        protected override void Run(View_MainComponent view)
        {
            YuoViewLogHelper.OnLog.RemoveListener(view.Log);
        }
    }

    public static class YuoViewLogHelper
    {
        public static UnityEvent<string> OnLog = new();

        public static void ViewLog(this string message)
        {
            OnLog?.Invoke(message);
        }
    }
}