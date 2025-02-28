using DG.Tweening;
using YuoTools.Extend.Helper;
using YuoTools.Main.Ecs;
namespace YuoTools.UI
{
	public partial class View_ColumnItemComponent
	{
		public void Show()
		{
			MainRectTransform.Show();
		}

		public void Hide()
		{
			MainRectTransform.Hide();
		}
	}
	public class ViewColumnItemCreateSystem :YuoSystem<View_ColumnItemComponent>, IUICreate
	{
		public override string Group =>"UI/ColumnItem";

		public override void Run(View_ColumnItemComponent view)
		{
			view.FindAll();
		}
	}
	public class ViewColumnItemOpenSystem :YuoSystem<View_ColumnItemComponent>, IUIOpen
	{
		public override string Group =>"UI/ColumnItem";

		public override void Run(View_ColumnItemComponent view)
		{
		}
	}
	public class ViewColumnItemCloseSystem :YuoSystem<View_ColumnItemComponent>, IUIClose
	{
		public override string Group =>"UI/ColumnItem";

		public override void Run(View_ColumnItemComponent view)
		{
		}
	}
}
