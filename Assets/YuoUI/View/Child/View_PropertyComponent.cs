﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using YuoTools.Main.Ecs;
using Sirenix.OdinInspector;
using TMPro;

namespace YuoTools.UI
{

	public partial class View_PropertyComponent : UIComponent 
	{

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

		private Image mImage_Mask;

		public Image Image_Mask
		{
			get
			{
				if (mImage_Mask == null)
					mImage_Mask = rectTransform.Find("C_Mask").GetComponent<Image>();
				return mImage_Mask;
			}
		}


		private TextMeshProUGUI mTextMeshProUGUI_Text;

		public TextMeshProUGUI TextMeshProUGUI_Text
		{
			get
			{
				if (mTextMeshProUGUI_Text == null)
					mTextMeshProUGUI_Text = rectTransform.Find("C_Text").GetComponent<TextMeshProUGUI>();
				return mTextMeshProUGUI_Text;
			}
		}


		private TextMeshProUGUI mTextMeshProUGUI_lv;

		public TextMeshProUGUI TextMeshProUGUI_lv
		{
			get
			{
				if (mTextMeshProUGUI_lv == null)
					mTextMeshProUGUI_lv = rectTransform.Find("C_lv").GetComponent<TextMeshProUGUI>();
				return mTextMeshProUGUI_lv;
			}
		}



		[FoldoutGroup("ALL")]
		public List<Image> all_Image = new();

		[FoldoutGroup("ALL")]
		public List<TextMeshProUGUI> all_TextMeshProUGUI = new();

		public void FindAll()
		{
				
			all_Image.Add(MainImage);
			all_Image.Add(Image_Mask);;
				
			all_TextMeshProUGUI.Add(TextMeshProUGUI_Text);
			all_TextMeshProUGUI.Add(TextMeshProUGUI_lv);;

		}
	}}
