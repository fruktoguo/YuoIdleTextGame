using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace YuoTools.UI
{

	public partial class View_ColumnPanelComponent : UIComponent 
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

		[FoldoutGroup("ALL")]
		public List<RectTransform> all_RectTransform = new();
		[FoldoutGroup("ALL")]
		public List<Image> all_Image = new();

		public void FindAll()
		{
				
			all_RectTransform.Add(MainRectTransform);
				
			all_Image.Add(MainImage);

		}
	}}
