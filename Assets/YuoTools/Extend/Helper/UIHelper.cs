using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using YuoTools.Main.Ecs;
using YuoTools.UI;

namespace YuoTools.Extend.Helper
{
    /// <summary>
    /// UI辅助类，提供了一系列用于UI操作的静态方法
    /// </summary>
    public static class UIHelper
    {
        /// <summary>
        /// 设置按钮的点击事件
        /// </summary>
        /// <param name="btn">要设置的按钮</param>
        /// <param name="action">点击时要执行的动作</param>
        public static void SetBtnClick(this Button btn, UnityAction action)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(action);
        }

        /// <summary>
        /// 为按钮添加关闭指定UI窗口的事件
        /// </summary>
        /// <param name="btn">要设置的按钮</param>
        /// <param name="windowName">要关闭的UI窗口名称</param>
        public static void AddUIClose(this Button btn, string windowName)
        {
            btn.onClick.AddListener(() => UIManagerComponent.Get.Close(windowName));
        }

        /// <summary>
        /// 设置按钮的点击事件为关闭指定UI窗口
        /// </summary>
        /// <param name="btn">要设置的按钮</param>
        /// <param name="windowName">要关闭的UI窗口名称</param>
        public static void SetUIClose(this Button btn, string windowName)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => UIManagerComponent.Get.Close(windowName));
        }

        /// <summary>
        /// 设置按钮的点击事件为关闭当前UI窗口并打开新的UI窗口
        /// </summary>
        /// <param name="btn">要设置的按钮</param>
        /// <param name="windowName">要关闭的UI窗口名称</param>
        /// <param name="openWindowName">要打开的UI窗口名称</param>
        public static void SetUICloseAndOpen(this Button btn, string windowName, string openWindowName)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                UIManagerComponent.Get.Close(windowName);
                UIManagerComponent.Get.Open(openWindowName);
            });
        }

        /// <summary>
        /// 设置按钮的点击事件为等待动画完成后关闭指定UI窗口
        /// </summary>
        /// <param name="btn">要设置的按钮</param>
        /// <param name="windowName">要关闭的UI窗口名称</param>
        public static void SetUICloseWaitAnima(this Button btn, string windowName)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                var view = UIManagerComponent.Get.GetUIView(windowName);
                var viewAnima = view.GetComponent<UIAnimaComponent>();
                if (viewAnima != null && viewAnima.Sate != UISetting.UISate.Show) return;
                UIManagerComponent.Get.Close(windowName);
            });
        }

        /// <summary>
        /// 为按钮添加打开指定UI窗口的事件
        /// </summary>
        /// <param name="btn">要设置的按钮</param>
        /// <param name="windowName">要打开的UI窗口名称</param>
        public static void AddUIOpen(this Button btn, string windowName)
        {
            btn.onClick.AddListener(Call);

            async void Call() => await UIManagerComponent.Get.OpenAsync(windowName);
        }

        /// <summary>
        /// 设置按钮的点击事件为打开指定UI窗口
        /// </summary>
        /// <param name="btn">要设置的按钮</param>
        /// <param name="windowName">要打开的UI窗口名称</param>
        public static void SetUIOpen(this Button btn, string windowName)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(Call);

            async void Call() => await UIManagerComponent.Get.OpenAsync(windowName);
        }

        /// <summary>
        /// 为Toggle添加开启时的事件
        /// </summary>
        /// <param name="toggle">要设置的Toggle</param>
        /// <param name="action">开启时要执行的动作</param>
        public static void AddOnOpen(this Toggle toggle, UnityAction action)
        {
            toggle.onValueChanged.AddListener(x =>
            {
                if (x) action();
            });
        }

        /// <summary>
        /// 为Toggle添加关闭时的事件
        /// </summary>
        /// <param name="toggle">要设置的Toggle</param>
        /// <param name="action">关闭时要执行的动作</param>
        public static void AddOnClose(this Toggle toggle, UnityAction action)
        {
            toggle.onValueChanged.AddListener(x =>
            {
                if (!x) action();
            });
        }
        
        /// <summary>
        /// 暂时禁用可选择对象的交互能力
        /// </summary>
        /// <param name="selectable">要禁用的可选择对象</param>
        /// <param name="time">禁用的时间（秒）</param>
        public static async void DisableInteractable(this Selectable selectable,float time)
        {
            selectable.interactable = false;
            await YuoWait.WaitUnscaledTimeAsync(time);
            selectable.interactable = true;
        }

        /// <summary>
        /// 将世界坐标转换为Canvas上的坐标
        /// </summary>
        /// <param name="canvas">目标Canvas</param>
        /// <param name="worldPosition">世界坐标</param>
        /// <returns>Canvas上的坐标</returns>
        public static Vector2 WorldPosToCanvas(this Canvas canvas, Vector3 worldPosition)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform,
                    canvas.worldCamera.WorldToScreenPoint(worldPosition), canvas.worldCamera, out var position))
            {
                return position;
            }

            return Vector2.zero;
        }

        /// <summary>
        /// 将世界坐标转换为Canvas上的坐标（使用指定的相机）
        /// </summary>
        /// <param name="canvas">目标Canvas</param>
        /// <param name="worldPosition">世界坐标</param>
        /// <param name="camera">用于转换的相机</param>
        /// <returns>Canvas上的坐标</returns>
        public static Vector2 WorldPosToCanvas(this Canvas canvas, Vector2 worldPosition, Camera camera)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform,
                    camera.WorldToScreenPoint(new Vector3(worldPosition.x, worldPosition.y, 0)),
                    camera, out var position))
            {
                return position;
            }

            return Vector2.zero;
        }

        /// <summary>
        /// 将视口坐标转换为Canvas上的坐标
        /// </summary>
        /// <param name="canvas">目标Canvas</param>
        /// <param name="viewPos">视口坐标</param>
        /// <returns>Canvas上的坐标</returns>
        public static Vector2 ViewPosToCanvas(this Canvas canvas, Vector2 viewPos)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform,
                    canvas.worldCamera.ViewportToScreenPoint(viewPos), canvas.worldCamera, out var position))
            {
                return position;
            }

            return Vector2.zero;
        }
    }
}