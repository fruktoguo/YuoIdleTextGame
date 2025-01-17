using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace YuoTools.UI
{
	public static partial class ViewType
	{
		public const string DynamicIsland = "DynamicIsland";
	}

	public partial class View_DynamicIslandComponent : UIComponent 
	{

		public static View_DynamicIslandComponent GetView() => UIManagerComponent.Get.GetUIView<View_DynamicIslandComponent>();


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

		private RectTransform mRectTransform_Tip;

		public RectTransform RectTransform_Tip => mRectTransform_Tip ??= rectTransform.Find("C_Tip").GetComponent<RectTransform>();


		private UILerp mUILerp_Tip;

		public UILerp UILerp_Tip => mUILerp_Tip ??= rectTransform.Find("C_Tip").GetComponent<UILerp>();


		private Image mImage_Tip;

		public Image Image_Tip => mImage_Tip ??= rectTransform.Find("C_Tip").GetComponent<Image>();


		[FoldoutGroup("ALL")]
		public List<RectTransform> all_RectTransform = new();
		[FoldoutGroup("ALL")]
		public List<UILerp> all_UILerp = new();
		[FoldoutGroup("ALL")]
		public List<Image> all_Image = new();

		public void FindAll()
		{
				
			all_RectTransform.Add(MainRectTransform);
			all_RectTransform.Add(RectTransform_Tip);
				
			all_UILerp.Add(UILerp_Tip);
				
			all_Image.Add(Image_Tip);

		}
	}}
