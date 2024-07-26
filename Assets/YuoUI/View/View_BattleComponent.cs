using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using YuoTools.Main.Ecs;
using Sirenix.OdinInspector;
using TMPro;

namespace YuoTools.UI
{

	public static partial class ViewType
	{
		public const string Battle = "Battle";
	}
	public partial class View_BattleComponent : UIComponent 
	{

		public static View_BattleComponent GetView() => UIManagerComponent.Get.GetUIView<View_BattleComponent>();


		private Image mImage_Wall;

		public Image Image_Wall
		{
			get
			{
				if (mImage_Wall == null)
					mImage_Wall = rectTransform.Find("Item/C_Wall").GetComponent<Image>();
				return mImage_Wall;
			}
		}


		private TextMeshProUGUI mTextMeshProUGUI_Role1;

		public TextMeshProUGUI TextMeshProUGUI_Role1
		{
			get
			{
				if (mTextMeshProUGUI_Role1 == null)
					mTextMeshProUGUI_Role1 = rectTransform.Find("Item/C_Role1").GetComponent<TextMeshProUGUI>();
				return mTextMeshProUGUI_Role1;
			}
		}


		private TextMeshProUGUI mTextMeshProUGUI_Role2;

		public TextMeshProUGUI TextMeshProUGUI_Role2
		{
			get
			{
				if (mTextMeshProUGUI_Role2 == null)
					mTextMeshProUGUI_Role2 = rectTransform.Find("Item/C_Role2").GetComponent<TextMeshProUGUI>();
				return mTextMeshProUGUI_Role2;
			}
		}


		private View_RoleComponent mChild_Role;

		public View_RoleComponent Child_Role
		{
			get
			{
				if (mChild_Role == null)
				{
					mChild_Role = Entity.AddChild<View_RoleComponent>();
					mChild_Role.Entity.EntityName = "Role";
					mChild_Role.rectTransform = rectTransform.Find("Item/D_Role") as RectTransform;
					mChild_Role.RunSystem<IUICreate>();
				}
				return mChild_Role;
			}
		}


		[FoldoutGroup("ALL")]

		public List<Image> all_Image = new();

		[FoldoutGroup("ALL")]

		public List<TextMeshProUGUI> all_TextMeshProUGUI = new();

		[FoldoutGroup("ALL")]

		public List<View_RoleComponent> all_View_RoleComponent = new();

		public void FindAll()
		{
				
			all_Image.Add(Image_Wall);;
				
			all_TextMeshProUGUI.Add(TextMeshProUGUI_Role1);
			all_TextMeshProUGUI.Add(TextMeshProUGUI_Role2);;
				
			all_View_RoleComponent.Add(Child_Role);;


		}
	}}
