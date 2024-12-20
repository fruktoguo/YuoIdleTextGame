using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace YuoTools.UI
{
	public static partial class ViewType
	{
		public const string Bar = "Bar";
	}

	public partial class View_BarComponent : UIComponent 
	{

		public static View_BarComponent GetView() => UIManagerComponent.Get.GetUIView<View_BarComponent>();


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

		private View_YuoBarItemComponent mChild_YuoBarItem;

		public View_YuoBarItemComponent Child_YuoBarItem
		{
            get
            {
                if (mChild_YuoBarItem == null)
                {
                    mChild_YuoBarItem = Entity.AddChild<View_YuoBarItemComponent>();
                    mChild_YuoBarItem.Entity.EntityName = "YuoBarItem";
                    mChild_YuoBarItem.rectTransform = rectTransform.Find("D_YuoBarItem") as RectTransform;
                    mChild_YuoBarItem.RunSystem<IUICreate>();
                }
                return mChild_YuoBarItem;
            }
        }

		[FoldoutGroup("ALL")]
		public List<RectTransform> all_RectTransform = new();
		[FoldoutGroup("ALL")]
		public List<View_YuoBarItemComponent> all_View_YuoBarItemComponent = new();

		public void FindAll()
		{
				
			all_RectTransform.Add(MainRectTransform);
				
			all_View_YuoBarItemComponent.Add(Child_YuoBarItem);

		}
	}}
