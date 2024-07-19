using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using YuoTools.Main.Ecs;
using Sirenix.OdinInspector;

namespace YuoTools.UI
{

	public static partial class ViewType
	{
		public const string DamageNum = "DamageNum";
	}
	public partial class View_DamageNumComponent : UIComponent 
	{

		public static View_DamageNumComponent GetView() => UIManagerComponent.Get.GetUIView<View_DamageNumComponent>();


		private View_NumComponent mChild_Num;

		public View_NumComponent Child_Num
		{
			get
			{
				if (mChild_Num == null)
				{
					mChild_Num = Entity.AddChild<View_NumComponent>();
					mChild_Num.Entity.EntityName = "Num";
					mChild_Num.rectTransform = rectTransform.Find("D_Num") as RectTransform;
					mChild_Num.RunSystem<IUICreate>();
				}
				return mChild_Num;
			}
		}


		[FoldoutGroup("ALL")]

		public List<View_NumComponent> all_View_NumComponent = new();

		public void FindAll()
		{
				
			all_View_NumComponent.Add(Child_Num);;


		}
	}
}
