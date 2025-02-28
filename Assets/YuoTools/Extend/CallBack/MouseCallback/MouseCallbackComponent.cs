using UnityEngine;
using YuoTools.Main.Ecs;

namespace YuoTools.Extend
{
    /// <summary>
    ///  鼠标回调 
    ///  如果不是UI,则Camera需要挂载PhysicsRaycaster或者Physics2DRaycaster,且物体需要有Collider或者Collider2D
    /// <returns></returns>
    /// </summary>
    public class MouseCallbackComponent : YuoComponent, IComponentInit<Transform>
    {
        public Transform transform;
        public MouseCallback mouseCallback;

        public void ComponentInit(Transform tran)
        {
            transform = tran;
        }
    }

    public class MouseCallbackComponentAwakeSystem : YuoSystem<MouseCallbackComponent>, IAwake
    {
        public override string Group => SystemGroupConst.CallBack;

        public override void Run(MouseCallbackComponent component)
        {
            //检查是否有鼠标回调组件
            component.mouseCallback ??= component.transform.GetOrAddComponent<MouseCallback>();
            component.mouseCallback.callbackComponent = component;
        }
    }

    public class MouseCallbackComponentDestroySystem : YuoSystem<MouseCallbackComponent>, IDestroy
    {
        public override string Group => SystemGroupConst.CallBack;

        public override void Run(MouseCallbackComponent component)
        {
            component.mouseCallback.TryDestroy();
        }
    }

    public interface IMouseEnter : ISystemTag
    {
    }

    public interface IMouseExit : ISystemTag
    {
    }

    public interface IMouseDown : ISystemTag
    {
    }

    public interface IMouseUp : ISystemTag
    {
    }

    public interface IMouseClick : ISystemTag
    {
    }

    // 没法用
    public interface IMouseRightClick : ISystemTag
    {
    }

    public interface IMouseRightDown : ISystemTag
    {
    }

    public interface IMouseRightUp : ISystemTag
    {
    }
}