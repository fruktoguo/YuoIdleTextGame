using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using DG.Tweening;

namespace YuoTools.UI
{
	public static partial class ViewType
	{
		public const string DistanceInfo = "DistanceInfo";
	}

	public partial class View_DistanceInfoComponent : UIComponent 
	{

		public static View_DistanceInfoComponent GetView() => UIManagerComponent.Get.GetUIView<View_DistanceInfoComponent>();


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

		private DOTweenAnimation mainDOTweenAnimation;

		public DOTweenAnimation MainDOTweenAnimation
		{
			get
			{
				if (mainDOTweenAnimation == null)
					mainDOTweenAnimation = rectTransform.GetComponent<DOTweenAnimation>();
				return mainDOTweenAnimation;
			}
		}

		[FoldoutGroup("ALL")]
		public List<RectTransform> all_RectTransform = new();
		[FoldoutGroup("ALL")]
		public List<DOTweenAnimation> all_DOTweenAnimation = new();

		public void FindAll()
		{
				
			all_RectTransform.Add(MainRectTransform);
				
			all_DOTweenAnimation.Add(MainDOTweenAnimation);

		}
	}}
