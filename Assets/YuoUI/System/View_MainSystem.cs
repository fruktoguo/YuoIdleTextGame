using System.Collections.Generic;
using SimpleJSON;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using YuoTools.Main.Ecs;

namespace YuoTools.UI
{
    public partial class View_MainComponent
    {
        private string oldMessage = string.Empty;
        public string nowMessage = string.Empty;
        public int nowMessageCount = 1;
        private const int MaxLogLines = 100; // 最大行数限制

        public void Log(string message)
        {
            if (message == nowMessage)
            {
                nowMessageCount++;
            }
            else
            {
                if (nowMessage != string.Empty)
                {
                    oldMessage += nowMessage;
                    if (nowMessageCount > 1) oldMessage += (" * " + nowMessageCount).ColorizeNum();
                    oldMessage += "\n";
                }

                nowMessage = message;
                nowMessageCount = 1;
            }

            TrimLog(); // 修剪日志
            UpdateLogDisplay(); // 更新显示

            (ScrollRect_Console.transform as RectTransform).ForceRebuildLayout();
            ScrollRect_Console.verticalNormalizedPosition = 0;
        }

        private void TrimLog()
        {
            var lines = oldMessage.Split('\n');
            if (lines.Length > MaxLogLines)
            {
                oldMessage = string.Join("\n", lines, lines.Length - MaxLogLines, MaxLogLines);
            }
        }

        private void UpdateLogDisplay()
        {
            TextMeshProUGUI_Console.text = nowMessageCount > 1
                ? $"{oldMessage}{nowMessage}{(" * " + nowMessageCount).ColorizeNum()}"
                : $"{oldMessage}{nowMessage}";
        }

        [Button]
        public void Load()
        {
            ItemHelper.Load();
        }

        [Button]
        public void Save()
        {
            ItemHelper.Save();
        }
    }

    public class ViewMainCreateSystem : YuoSystem<View_MainComponent>, IUICreate
    {
        public override string Group => "UI/Main";

        protected override async void Run(View_MainComponent view)
        {
            view.FindAll();
            //关闭窗口的事件注册,名字不同请自行更

            DataHelper.Init();

            var text = DataHelper.LoadConfig("Build");

            var build = JSONNode.Parse(text);

            if (build != null)
            {
                foreach (var (_, node) in build)
                {
                    var item = view.AddChildAndInstantiate(view.Child_Item);
                    item.rectTransform.SetParent(view.FlowLayout_Build.transform);
                    item.Init(node);
                }
            }

            text = DataHelper.LoadConfig("Behavior");

            var behavior = JSONNode.Parse(text);

            if (behavior != null)
            {
                foreach (var (_, node) in behavior)
                {
                    var item = view.AddChildAndInstantiate(view.Child_Item);
                    item.rectTransform.SetParent(view.FlowLayout_Behavior.transform);
                    item.Init(node);
                }
            }

            view.FlowLayout_Build.ArrangeChildren();
            view.FlowLayout_Behavior.ArrangeChildren();

            await YuoWait.WaitTimeAsync(0.1f);

            var result = View_TipWindowComponent.GetView().ShowTip("测试一下");
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
            message.Log();
            OnLog?.Invoke(message);
        }
    }
}