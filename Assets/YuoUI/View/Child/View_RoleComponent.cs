using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using YuoTools.Main.Ecs;
using Sirenix.OdinInspector;
using TMPro;

namespace YuoTools.UI
{

	public partial class View_RoleComponent : UIComponent 
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

		private TextMeshProUGUI mTextMeshProUGUI_Name;

		public TextMeshProUGUI TextMeshProUGUI_Name
		{
			get
			{
				if (mTextMeshProUGUI_Name == null)
					mTextMeshProUGUI_Name = rectTransform.Find("C_Name").GetComponent<TextMeshProUGUI>();
				return mTextMeshProUGUI_Name;
			}
		}



		[FoldoutGroup("ALL")]

		public List<Image> all_Image = new();

		[FoldoutGroup("ALL")]

		public List<TextMeshProUGUI> all_TextMeshProUGUI = new();

		public void FindAll()
		{
				
			all_Image.Add(MainImage);;
				
			all_TextMeshProUGUI.Add(TextMeshProUGUI_Name);;


		}
	}}
