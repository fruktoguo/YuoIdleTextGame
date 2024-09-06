using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using YuoTools.Main.Ecs;
using Sirenix.OdinInspector;
using UnityEngine.UIElements;

namespace YuoTools.UI
{
	public static partial class ViewType
	{
		public const string UIDocument = "UIDocument";
	}

	public partial class View_UIDocumentComponent : UIComponent 
	{

		public static View_UIDocumentComponent GetView() => UIManagerComponent.Get.GetUIView<View_UIDocumentComponent>();


		private UIDocument mainUIDocument;

		public UIDocument MainUIDocument
		{
			get
			{
				if (mainUIDocument == null)
					mainUIDocument = rectTransform.GetComponent<UIDocument>();
				return mainUIDocument;
			}
		}


		[FoldoutGroup("ALL")]
		public List<UIDocument> all_UIDocument = new();

		public void FindAll()
		{
				
			all_UIDocument.Add(MainUIDocument);;

		}
	}}
