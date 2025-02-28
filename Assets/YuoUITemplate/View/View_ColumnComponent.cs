using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace YuoTools.UI
{
	public static partial class ViewType
	{
		public const string Column = "Column";
	}

	public partial class View_ColumnComponent : UIComponent 
	{

		public static View_ColumnComponent GetView() => UIManagerComponent.Get.GetUIView<View_ColumnComponent>();


		private RectTransform mainRectTransform;

		public RectTransform MainRectTransform
		{
			get
			{
				if (mainRectTransform == null)
					mainRectTransform = rectTransform.GetComponent<RectTransform>();
				return mainRectTransform;
			}
		}

		private View_ColumnItemComponent mChild_ColumnItem;

		public View_ColumnItemComponent Child_ColumnItem
		{
            get
            {
                if (mChild_ColumnItem == null)
                {
                    mChild_ColumnItem = Entity.AddChild<View_ColumnItemComponent>();
                    mChild_ColumnItem.Entity.EntityName = "ColumnItem";
                    mChild_ColumnItem.rectTransform = rectTransform.Find("D_ColumnItem") as RectTransform;
                    mChild_ColumnItem.RunSystem<IUICreate>();
                }
                return mChild_ColumnItem;
            }
        }

		private View_ColumnPanelComponent mChild_ColumnPanel;

		public View_ColumnPanelComponent Child_ColumnPanel
		{
            get
            {
                if (mChild_ColumnPanel == null)
                {
                    mChild_ColumnPanel = Entity.AddChild<View_ColumnPanelComponent>();
                    mChild_ColumnPanel.Entity.EntityName = "ColumnPanel";
                    mChild_ColumnPanel.rectTransform = rectTransform.Find("D_ColumnPanel") as RectTransform;
                    mChild_ColumnPanel.RunSystem<IUICreate>();
                }
                return mChild_ColumnPanel;
            }
        }

		[FoldoutGroup("ALL")]
		public List<RectTransform> all_RectTransform = new();
		[FoldoutGroup("ALL")]
		public List<View_ColumnItemComponent> all_View_ColumnItemComponent = new();
		[FoldoutGroup("ALL")]
		public List<View_ColumnPanelComponent> all_View_ColumnPanelComponent = new();

		public void FindAll()
		{
				
			all_RectTransform.Add(MainRectTransform);
				
			all_View_ColumnItemComponent.Add(Child_ColumnItem);
				
			all_View_ColumnPanelComponent.Add(Child_ColumnPanel);

		}
	}}
