using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace YuoTools.UI
{

	public partial class View_FollowBarItemComponent : UIComponent 
	{

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

		private YuoBar mainYuoBar;

		public YuoBar MainYuoBar
		{
			get
			{
				if (mainYuoBar == null)
					mainYuoBar = rectTransform.GetComponent<YuoBar>();
				return mainYuoBar;
			}
		}

		private RectTransform mRectTransform_BG;

		public RectTransform RectTransform_BG => mRectTransform_BG ??= rectTransform.Find("C_BG").GetComponent<RectTransform>();


		private Image mImage_BG;

		public Image Image_BG => mImage_BG ??= rectTransform.Find("C_BG").GetComponent<Image>();


		private RectTransform mRectTransform_Bar;

		public RectTransform RectTransform_Bar => mRectTransform_Bar ??= rectTransform.Find("C_Bar").GetComponent<RectTransform>();


		private Image mImage_Bar;

		public Image Image_Bar => mImage_Bar ??= rectTransform.Find("C_Bar").GetComponent<Image>();


		private RectTransform mRectTransform_Text;

		public RectTransform RectTransform_Text => mRectTransform_Text ??= rectTransform.Find("C_Text").GetComponent<RectTransform>();


		private TextMeshProUGUI mTextMeshProUGUI_Text;

		public TextMeshProUGUI TextMeshProUGUI_Text => mTextMeshProUGUI_Text ??= rectTransform.Find("C_Text").GetComponent<TextMeshProUGUI>();


		[FoldoutGroup("ALL")]
		public List<RectTransform> all_RectTransform = new();
		[FoldoutGroup("ALL")]
		public List<YuoBar> all_YuoBar = new();
		[FoldoutGroup("ALL")]
		public List<Image> all_Image = new();
		[FoldoutGroup("ALL")]
		public List<TextMeshProUGUI> all_TextMeshProUGUI = new();

		public void FindAll()
		{
				
			all_RectTransform.Add(MainRectTransform);
			all_RectTransform.Add(RectTransform_BG);
			all_RectTransform.Add(RectTransform_Bar);
			all_RectTransform.Add(RectTransform_Text);
				
			all_YuoBar.Add(MainYuoBar);
				
			all_Image.Add(Image_BG);
			all_Image.Add(Image_Bar);
				
			all_TextMeshProUGUI.Add(TextMeshProUGUI_Text);

		}
	}}
