using DG.Tweening;
using YuoTools.Extend.Helper;
using YuoTools.Main.Ecs;
namespace YuoTools.UI
{
	public partial class View_NumComponent
	{
	}
	public class ViewNumCreateSystem :YuoSystem<View_NumComponent>, IUICreate
	{
		public override string Group =>"UI/Num";

		protected override void Run(View_NumComponent view)
		{
			view.FindAll();
		}
	}
	public class ViewNumOpenSystem :YuoSystem<View_NumComponent>, IUIOpen
	{
		public override string Group =>"UI/Num";

		protected override void Run(View_NumComponent view)
		{
		}
	}
	public class ViewNumCloseSystem :YuoSystem<View_NumComponent>, IUIClose
	{
		public override string Group =>"UI/Num";

		protected override void Run(View_NumComponent view)
		{
		}
	}
}
