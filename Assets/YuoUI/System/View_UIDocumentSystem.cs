using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;
using YuoTools.Extend.Helper;
using YuoTools.Main.Ecs;

namespace YuoTools.UI
{
    public partial class View_UIDocumentComponent
    {
        // 省略其他代码
    }

    public class ViewUIDocumentCreateSystem : YuoSystem<View_UIDocumentComponent>, IUICreate
    {
        public override string Group => "UI/UIDocument";

        protected override async void Run(View_UIDocumentComponent view)
        {
            view.FindAll();

            await YuoWait.WaitUntilAsync(() => UIManagerComponent.Get.GetUIView<View_TipWindowComponent>() != null);
            var result = await View_TipWindowComponent.GetView().ShowTip("测试一下");
            view.OpenView();
            var root = view.MainUIDocument.rootVisualElement;
            root.Q<Button>("ConfirmButton").clicked += () =>
            {
                view.MainUIDocument.rootVisualElement.Q<VisualElement>("DialogContainer")
                    .RemoveFromClassList("show");
            };
            view.MainUIDocument.rootVisualElement.Q<VisualElement>("DialogContainer").AddToClassList("show");

            while (true)
            {
                await YuoWait.WaitInputKeyAsync(KeyCode.A);
            }
            // 遍历root下的所有子元素
        }
    }

    public class ViewUIDocumentOpenSystem : YuoSystem<View_UIDocumentComponent>, IUIOpen
    {
        public override string Group => "UI/UIDocument";

        protected override void Run(View_UIDocumentComponent view)
        {
        }
    }

    public class ViewUIDocumentCloseSystem : YuoSystem<View_UIDocumentComponent>, IUIClose
    {
        public override string Group => "UI/UIDocument";

        protected override void Run(View_UIDocumentComponent view)
        {
            view.MainUIDocument.rootVisualElement.RemoveFromClassList("show");
        }
    }
}