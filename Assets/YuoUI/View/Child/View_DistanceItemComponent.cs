using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace YuoTools.UI
{

	public partial class View_DistanceItemComponent : UIComponent 
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

		private Image mainImage;

		public Image MainImage
		{
			get
			{
				if (mainImage == null)
					mainImage = rectTransform.GetComponent<Image>();
				return mainImage;
			}
		}

		private RectTransform mRectTransform_Line;

		public RectTransform RectTransform_Line => mRectTransform_Line ??= rectTransform.Find("C_Line").GetComponent<RectTransform>();


		private Image mImage_Line;

		public Image Image_Line => mImage_Line ??= rectTransform.Find("C_Line").GetComponent<Image>();


		private RectTransform mRectTransform_Board;

		public RectTransform RectTransform_Board => mRectTransform_Board ??= rectTransform.Find("C_Board").GetComponent<RectTransform>();


		private Image mImage_Board;

		public Image Image_Board => mImage_Board ??= rectTransform.Find("C_Board").GetComponent<Image>();


		private RectTransform mRectTransform_DistanceText;

		public RectTransform RectTransform_DistanceText => mRectTransform_DistanceText ??= rectTransform.Find("C_Board/C_DistanceText").GetComponent<RectTransform>();


		private TextMeshProUGUI mTextMeshProUGUI_DistanceText;

		public TextMeshProUGUI TextMeshProUGUI_DistanceText => mTextMeshProUGUI_DistanceText ??= rectTransform.Find("C_Board/C_DistanceText").GetComponent<TextMeshProUGUI>();


		[FoldoutGroup("ALL")]
		public List<RectTransform> all_RectTransform = new();
		[FoldoutGroup("ALL")]
		public List<Image> all_Image = new();
		[FoldoutGroup("ALL")]
		public List<TextMeshProUGUI> all_TextMeshProUGUI = new();

		public void FindAll()
		{
				
			all_RectTransform.Add(MainRectTransform);
			all_RectTransform.Add(RectTransform_Line);
			all_RectTransform.Add(RectTransform_Board);
			all_RectTransform.Add(RectTransform_DistanceText);
				
			all_Image.Add(MainImage);
			all_Image.Add(Image_Line);
			all_Image.Add(Image_Board);
				
			all_TextMeshProUGUI.Add(TextMeshProUGUI_DistanceText);

		}
	}}
