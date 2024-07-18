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


		private RectTransform mRectTransform_Top;

		public RectTransform RectTransform_Top
		{
			get
			{
				if (mRectTransform_Top == null)
					mRectTransform_Top = rectTransform.Find("C_Top").GetComponent<RectTransform>();
				return mRectTransform_Top;
			}
		}


		private FlowLayout mFlowLayout_Content;

		public FlowLayout FlowLayout_Content
		{
			get
			{
				if (mFlowLayout_Content == null)
					mFlowLayout_Content = rectTransform.Find("C_Content").GetComponent<FlowLayout>();
				return mFlowLayout_Content;
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


		private TextMeshProUGUI mTextMeshProUGUI_Console;

		public TextMeshProUGUI TextMeshProUGUI_Console
		{
			get
			{
				if (mTextMeshProUGUI_Console == null)
					mTextMeshProUGUI_Console = rectTransform.Find("C_Bottom/Scroll View/Viewport/C_Console").GetComponent<TextMeshProUGUI>();
				return mTextMeshProUGUI_Console;
			}
		}


		private ContentSizeFitter mContentSizeFitter_Console;

		public ContentSizeFitter ContentSizeFitter_Console
		{
			get
			{
				if (mContentSizeFitter_Console == null)
					mContentSizeFitter_Console = rectTransform.Find("C_Bottom/Scroll View/Viewport/C_Console").GetComponent<ContentSizeFitter>();
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
					mChild_Item.rectTransform = rectTransform.Find("C_Content/D_Item") as RectTransform;
					mChild_Item.RunSystem<IUICreate>();
				}
				return mChild_Item;
			}
		}


		[FoldoutGroup("ALL")]

		public List<RectTransform> all_RectTransform = new();

		[FoldoutGroup("ALL")]

		public List<FlowLayout> all_FlowLayout = new();

		[FoldoutGroup("ALL")]

		public List<TextMeshProUGUI> all_TextMeshProUGUI = new();

		[FoldoutGroup("ALL")]

		public List<ContentSizeFitter> all_ContentSizeFitter = new();

		[FoldoutGroup("ALL")]

		public List<View_ItemComponent> all_View_ItemComponent = new();

		public void FindAll()
		{
				
			all_RectTransform.Add(RectTransform_Top);
			all_RectTransform.Add(RectTransform_Bottom);;
				
			all_FlowLayout.Add(FlowLayout_Content);;
				
			all_TextMeshProUGUI.Add(TextMeshProUGUI_Console);;
				
			all_ContentSizeFitter.Add(ContentSizeFitter_Console);;
				
			all_View_ItemComponent.Add(Child_Item);;


		}
	}}
