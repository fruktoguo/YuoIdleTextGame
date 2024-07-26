using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using YuoTools.Main.Ecs;
using Sirenix.OdinInspector;
using TMPro;

namespace YuoTools.UI
{

	public static partial class ViewType
	{
		public const string Main = "Main";
	}
	public partial class View_MainComponent : UIComponent 
	{

		public static View_MainComponent GetView() => UIManagerComponent.Get.GetUIView<View_MainComponent>();


		private FlowLayout mFlowLayout_Behavior;

		public FlowLayout FlowLayout_Behavior
		{
			get
			{
				if (mFlowLayout_Behavior == null)
					mFlowLayout_Behavior = rectTransform.Find("C_Behavior").GetComponent<FlowLayout>();
				return mFlowLayout_Behavior;
			}
		}


		private VerticalLayoutGroup mVerticalLayoutGroup_PlayerState;

		public VerticalLayoutGroup VerticalLayoutGroup_PlayerState
		{
			get
			{
				if (mVerticalLayoutGroup_PlayerState == null)
					mVerticalLayoutGroup_PlayerState = rectTransform.Find("C_PlayerState").GetComponent<VerticalLayoutGroup>();
				return mVerticalLayoutGroup_PlayerState;
			}
		}


		private FlowLayout mFlowLayout_Build;

		public FlowLayout FlowLayout_Build
		{
			get
			{
				if (mFlowLayout_Build == null)
					mFlowLayout_Build = rectTransform.Find("C_Build").GetComponent<FlowLayout>();
				return mFlowLayout_Build;
			}
		}


		private RectTransform mRectTransform_Bottom;

		public RectTransform RectTransform_Bottom
		{
			get
			{
				if (mRectTransform_Bottom == null)
					mRectTransform_Bottom = rectTransform.Find("C_Bottom").GetComponent<RectTransform>();
				return mRectTransform_Bottom;
			}
		}


		private ScrollRect mScrollRect_Console;

		public ScrollRect ScrollRect_Console
		{
			get
			{
				if (mScrollRect_Console == null)
					mScrollRect_Console = rectTransform.Find("C_Bottom/C_Console").GetComponent<ScrollRect>();
				return mScrollRect_Console;
			}
		}


		private TextMeshProUGUI mTextMeshProUGUI_Console;

		public TextMeshProUGUI TextMeshProUGUI_Console
		{
			get
			{
				if (mTextMeshProUGUI_Console == null)
					mTextMeshProUGUI_Console = rectTransform.Find("C_Bottom/C_Console/Viewport/C_Console").GetComponent<TextMeshProUGUI>();
				return mTextMeshProUGUI_Console;
			}
		}


		private ContentSizeFitter mContentSizeFitter_Console;

		public ContentSizeFitter ContentSizeFitter_Console
		{
			get
			{
				if (mContentSizeFitter_Console == null)
					mContentSizeFitter_Console = rectTransform.Find("C_Bottom/C_Console/Viewport/C_Console").GetComponent<ContentSizeFitter>();
				return mContentSizeFitter_Console;
			}
		}


		private View_ItemComponent mChild_Item;

		public View_ItemComponent Child_Item
		{
			get
			{
				if (mChild_Item == null)
				{
					mChild_Item = Entity.AddChild<View_ItemComponent>();
					mChild_Item.Entity.EntityName = "Item";
					mChild_Item.rectTransform = rectTransform.Find("D_Item") as RectTransform;
					mChild_Item.RunSystem<IUICreate>();
				}
				return mChild_Item;
			}
		}

		private View_PropertyComponent mChild_Property;

		public View_PropertyComponent Child_Property
		{
			get
			{
				if (mChild_Property == null)
				{
					mChild_Property = Entity.AddChild<View_PropertyComponent>();
					mChild_Property.Entity.EntityName = "Property";
					mChild_Property.rectTransform = rectTransform.Find("D_Property") as RectTransform;
					mChild_Property.RunSystem<IUICreate>();
				}
				return mChild_Property;
			}
		}


		[FoldoutGroup("ALL")]

		public List<FlowLayout> all_FlowLayout = new();

		[FoldoutGroup("ALL")]

		public List<VerticalLayoutGroup> all_VerticalLayoutGroup = new();

		[FoldoutGroup("ALL")]

		public List<RectTransform> all_RectTransform = new();

		[FoldoutGroup("ALL")]

		public List<ScrollRect> all_ScrollRect = new();

		[FoldoutGroup("ALL")]

		public List<TextMeshProUGUI> all_TextMeshProUGUI = new();

		[FoldoutGroup("ALL")]

		public List<ContentSizeFitter> all_ContentSizeFitter = new();

		[FoldoutGroup("ALL")]

		public List<View_ItemComponent> all_View_ItemComponent = new();

		[FoldoutGroup("ALL")]

		public List<View_PropertyComponent> all_View_PropertyComponent = new();

		public void FindAll()
		{
				
			all_FlowLayout.Add(FlowLayout_Behavior);
			all_FlowLayout.Add(FlowLayout_Build);;
				
			all_VerticalLayoutGroup.Add(VerticalLayoutGroup_PlayerState);;
				
			all_RectTransform.Add(RectTransform_Bottom);;
				
			all_ScrollRect.Add(ScrollRect_Console);;
				
			all_TextMeshProUGUI.Add(TextMeshProUGUI_Console);;
				
			all_ContentSizeFitter.Add(ContentSizeFitter_Console);;
				
			all_View_ItemComponent.Add(Child_Item);;
				
			all_View_PropertyComponent.Add(Child_Property);;


		}
	}}
