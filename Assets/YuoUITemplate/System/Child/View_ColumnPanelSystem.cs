using System.Collections.Generic;
using UnityEngine;
using YuoTools.Extend;
using YuoTools.Main.Ecs;

namespace YuoTools.UI
{
    public partial class View_ColumnPanelComponent
    {
        public int maxColumnNum = 8;
        public RectOffset padding = new(10, 10, 10, 10);
        public float space = 10;

        [HideInInspector] public List<View_ColumnItemComponent> items = new();

        public void Layout()
        {
            var parent = View_ColumnComponent.GetView();
            var rect = MainRectTransform;
            var count = items.Count;
            var itemSize = parent.Child_ColumnItem.MainRectTransform.sizeDelta;

            // 计算行数
            var columnNum = Mathf.Min(maxColumnNum, count);
            var rowNum = Mathf.CeilToInt((float)count / maxColumnNum);

            // 计算总高度并设置容器大小
            var height = rowNum * itemSize.y + padding.top + padding.bottom + space * (rowNum - 1);
            var width = padding.left + padding.right + columnNum * itemSize.x + space * (columnNum - 1);
            rect.sizeDelta = new Vector2(width, height);

            // 布局每个item
            for (int i = 0; i < count; i++)
            {
                var item = items[i];
                item.MainRectTransform.SetParent(MainRectTransform);
                var row = i / maxColumnNum;
                var column = i % maxColumnNum;

                var x = padding.left + column * space + column * itemSize.x;
                var y = padding.top + row * space + row * itemSize.y;

                item.MainRectTransform.anchoredPosition = new Vector2(x, -y);
            }
        }

        public void SetPosFormMouse()
        {
            MainRectTransform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            MainRectTransform.SetLocalPosZ(0);
        }
        
        public void SetPosFormTransform(Transform tran)
        {
            MainRectTransform.position = tran.position;
            MainRectTransform.SetLocalPosZ(0);
        }

        public View_ColumnItemComponent AddItem()
        {
            var parent = View_ColumnComponent.GetView();
            var item = parent.GetItem();
            items.Add(item);
            Layout();
            return item;
        }

        public void Show()
        {
            isShow = true;
            MainRectTransform.Show();
        }

        public void Hide()
        {
            isShow = false;
            MainRectTransform.Hide();
            var parent = View_ColumnComponent.GetView();
            foreach (var item in items)
            {
                parent.RemoveItem(item);
            }

            items.Clear();
        }

        public bool isShow { get; private set; }


        public bool inRange;
        public bool AutonomousHiding;
    }

    public class ViewColumnPanelCreateSystem : YuoSystem<View_ColumnPanelComponent>, IUICreate
    {
        public override string Group => "UI/ColumnPanel";

        public override void Run(View_ColumnPanelComponent view)
        {
            view.FindAll();


            var callback = view.Entity.AddComponent<MouseCallbackComponent, Transform>(view.MainRectTransform);
        }
    }

    public class ViewColumnPanelCallbackSystem : YuoSystem<View_ColumnPanelComponent, MouseCallbackComponent>,
        IMouseEnter,
        IMouseExit
    {
        public override string Group => "UI/Column";

        public override void Run(View_ColumnPanelComponent view, MouseCallbackComponent callback)
        {
            if (RunType == typeof(IMouseEnter))
            {
                view.inRange = true;
                view.AutonomousHiding = false;
            }
            else if (RunType == typeof(IMouseExit))
            {
                view.inRange = false;
                if (view.AutonomousHiding)
                {
                    view.AutonomousHiding = false;
                    view.Hide();
                }
            }
        }
    }

    public class ViewColumnPanelOpenSystem : YuoSystem<View_ColumnPanelComponent>, IUIOpen
    {
        public override string Group => "UI/ColumnPanel";

        public override void Run(View_ColumnPanelComponent view)
        {
            view.Layout();
        }
    }

    public class ViewColumnPanelCloseSystem : YuoSystem<View_ColumnPanelComponent>, IUIClose
    {
        public override string Group => "UI/ColumnPanel";

        public override void Run(View_ColumnPanelComponent view)
        {
            view.Hide();
        }
    }
}