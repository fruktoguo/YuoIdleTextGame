using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace YuoTools.UI
{

	public partial class View_YuoBarItemComponent : UIComponent 
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

		[FoldoutGroup("ALL")]
		public List<RectTransform> all_RectTransform = new();
		[FoldoutGroup("ALL")]
		public List<YuoBar> all_YuoBar = new();

		public void FindAll()
		{
				
			all_RectTransform.Add(MainRectTransform);
				
			all_YuoBar.Add(MainYuoBar);

		}
	}}
