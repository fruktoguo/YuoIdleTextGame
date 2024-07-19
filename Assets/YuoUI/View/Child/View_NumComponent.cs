using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using YuoTools.Main.Ecs;
using Sirenix.OdinInspector;
using TMPro;

namespace YuoTools.UI
{

	public partial class View_NumComponent : UIComponent 
	{

		private TextMeshProUGUI mainTextMeshProUGUI;

		public TextMeshProUGUI MainTextMeshProUGUI
		{
			get
			{
				if (mainTextMeshProUGUI == null)
					mainTextMeshProUGUI = rectTransform.GetComponent<TextMeshProUGUI>();
				return mainTextMeshProUGUI;
			}
		}


		[FoldoutGroup("ALL")]

		public List<TextMeshProUGUI> all_TextMeshProUGUI = new();

		public void FindAll()
		{
				
			all_TextMeshProUGUI.Add(MainTextMeshProUGUI);;


		}
	}}
