using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace YuoTools.UI
{
	public static partial class ViewType
	{
		public const string FollowBar = "FollowBar";
	}

	public partial class View_FollowBarComponent : UIComponent 
	{

		public static View_FollowBarComponent GetView() => UIManagerComponent.Get.GetUIView<View_FollowBarComponent>();


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

		private View_FollowBarItemComponent mChild_FollowBarItem;

		public View_FollowBarItemComponent Child_FollowBarItem
		{
            get
            {
                if (mChild_FollowBarItem == null)
                {
                    mChild_FollowBarItem = Entity.AddChild<View_FollowBarItemComponent>();
                    mChild_FollowBarItem.Entity.EntityName = "FollowBarItem";
                    mChild_FollowBarItem.rectTransform = rectTransform.Find("D_FollowBarItem") as RectTransform;
                    mChild_FollowBarItem.RunSystem<IUICreate>();
                }
                return mChild_FollowBarItem;
            }
        }

		[FoldoutGroup("ALL")]
		public List<RectTransform> all_RectTransform = new();
		[FoldoutGroup("ALL")]
		public List<View_FollowBarItemComponent> all_View_FollowBarItemComponent = new();

		public void FindAll()
		{
				
			all_RectTransform.Add(MainRectTransform);
				
			all_View_FollowBarItemComponent.Add(Child_FollowBarItem);

		}
	}}
