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
		public const string Main = "Main";
	}
	public partial class View_MainComponent : UIComponent 
	{

		public static View_MainComponent GetView() => UIManagerComponent.Get.GetUIView<View_MainComponent>();


		private Button mButton_StartGame;

		public Button Button_StartGame
		{
			get
			{
				if (mButton_StartGame == null)
					mButton_StartGame = rectTransform.Find("Content/C_StartGame").GetComponent<Button>();
				return mButton_StartGame;
			}
		}



		[FoldoutGroup("ALL")]

		public List<Button> all_Button = new();

		public void FindAll()
		{
				
			all_Button.Add(Button_StartGame);;


		}
	}}
