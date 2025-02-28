using UnityEngine;
using YuoTools.Extend;
using YuoTools.Main.Ecs;

namespace YuoTools.UI
{
    public partial class View_ColumnComponent
    {
        public YuoViewPool<View_ColumnItemComponent> itemPool;
        public YuoViewPool<View_ColumnPanelComponent> panelPool;

        public View_ColumnPanelComponent GetPanel()
        {
            var panel = panelPool.Get();
            panel.Show();
            return panel;
        }

        public View_ColumnItemComponent GetItem()
        {
            var item = itemPool.Get();
            item.Show();
            return item;
        }

        public void RemovePanel(View_ColumnPanelComponent panel)
        {
            panel.Hide();
            panelPool.Remove(panel);
        }

        public void RemoveItem(View_ColumnItemComponent item)
        {
            item.Hide();
            itemPool.Remove(item);
            item.MainRectTransform.SetParent(MainRectTransform);
        }
    }

    public class ViewColumnCreateSystem : YuoSystem<View_ColumnComponent>, IUICreate
    {
        public override string Group => "UI/Column";

        public override void Run(View_ColumnComponent view)
        {
            view.FindAll();
            view.itemPool = new(view.Child_ColumnItem, view);
            view.panelPool = new(view.Child_ColumnPanel, view);
        }
    }

    public class ViewColumnOpenSystem : YuoSystem<View_ColumnComponent>, IUIOpen
    {
        public override string Group => "UI/Column";

        public override void Run(View_ColumnComponent view)
        {
        }
    }

    public class ViewColumnCloseSystem : YuoSystem<View_ColumnComponent>, IUIClose
    {
        public override string Group => "UI/Column";

        public override void Run(View_ColumnComponent view)
        {
        }
    }
}