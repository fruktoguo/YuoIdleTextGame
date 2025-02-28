using Sirenix.OdinInspector;
using UnityEngine;

namespace YuoTools.Extend
{
    [RequireComponent(typeof(MouseInteractable))]
    public class DragMoveHandler : MonoBehaviour
    {
        #region 编辑器方法

#if UNITY_EDITOR
        [PropertySpace(10)]
        [Button("可视化拖拽边界")]
        private void VisualizeBounds()
        {
            if (useBounds)
            {
                Debug.DrawLine(dragBounds.min, new Vector3(dragBounds.max.x, dragBounds.min.y, dragBounds.min.z), Color.red,
                    5f);
                Debug.DrawLine(dragBounds.min, new Vector3(dragBounds.min.x, dragBounds.max.y, dragBounds.min.z),
                    Color.green, 5f);
                Debug.DrawLine(dragBounds.min, new Vector3(dragBounds.min.x, dragBounds.min.y, dragBounds.max.z),
                    Color.blue, 5f);

                Debug.DrawLine(dragBounds.max, new Vector3(dragBounds.min.x, dragBounds.max.y, dragBounds.max.z), Color.red,
                    5f);
                Debug.DrawLine(dragBounds.max, new Vector3(dragBounds.max.x, dragBounds.min.y, dragBounds.max.z),
                    Color.green, 5f);
                Debug.DrawLine(dragBounds.max, new Vector3(dragBounds.max.x, dragBounds.max.y, dragBounds.min.z),
                    Color.blue, 5f);

                Debug.DrawLine(new Vector3(dragBounds.min.x, dragBounds.min.y, dragBounds.max.z),
                    new Vector3(dragBounds.min.x, dragBounds.max.y, dragBounds.max.z), Color.cyan, 5f);
                Debug.DrawLine(new Vector3(dragBounds.min.x, dragBounds.min.y, dragBounds.max.z),
                    new Vector3(dragBounds.max.x, dragBounds.min.y, dragBounds.max.z), Color.cyan, 5f);

                Debug.DrawLine(new Vector3(dragBounds.max.x, dragBounds.max.y, dragBounds.min.z),
                    new Vector3(dragBounds.max.x, dragBounds.min.y, dragBounds.min.z), Color.yellow, 5f);
                Debug.DrawLine(new Vector3(dragBounds.max.x, dragBounds.max.y, dragBounds.min.z),
                    new Vector3(dragBounds.min.x, dragBounds.max.y, dragBounds.min.z), Color.yellow, 5f);

                Debug.DrawLine(new Vector3(dragBounds.max.x, dragBounds.min.y, dragBounds.max.z),
                    new Vector3(dragBounds.max.x, dragBounds.max.y, dragBounds.max.z), Color.magenta, 5f);
                Debug.DrawLine(new Vector3(dragBounds.min.x, dragBounds.max.y, dragBounds.max.z),
                    new Vector3(dragBounds.max.x, dragBounds.max.y, dragBounds.max.z), Color.magenta, 5f);

                Debug.Log("拖拽边界已可视化，将在Scene视图中显示5秒");
            }
            else
            {
                Debug.LogWarning("请先启用边界限制(useBounds)");
            }
        }
#endif

        #endregion

        #region 公共属性

#if UNITY_EDITOR
        [TitleGroup("拖拽设置")] [Tooltip("拖拽速度，值越大移动越平滑")] [Range(0.1f, 10f)]
#endif
        public float dragSpeed = 1.0f;

#if UNITY_EDITOR
        [TitleGroup("拖拽设置")] [Tooltip("是否锁定X轴移动")] [ToggleLeft]
#endif
        public bool lockX;

#if UNITY_EDITOR
        [TitleGroup("拖拽设置")] [Tooltip("是否锁定Y轴移动")] [ToggleLeft]
#endif
        public bool lockY;

#if UNITY_EDITOR
        [TitleGroup("拖拽设置")] [Tooltip("是否锁定Z轴移动")] [ToggleLeft]
#endif
        public bool lockZ = true;

#if UNITY_EDITOR
        [TitleGroup("拖拽设置")] [Tooltip("拖拽时是否使用屏幕空间移动")] [ToggleLeft]
#endif
        public bool useScreenSpace;

#if UNITY_EDITOR
        [TitleGroup("拖拽设置")] [Tooltip("拖拽时是否保持原始深度")] [ToggleLeft]
#endif
        public bool maintainDepth = true;

#if UNITY_EDITOR
        [FoldoutGroup("高级设置", false)] [Tooltip("拖拽时的移动平面法线")]
#endif
        public Vector3 dragPlaneNormal = Vector3.up;

#if UNITY_EDITOR
        [FoldoutGroup("高级设置", false)] [Tooltip("拖拽时是否使用物体自身坐标系")] [ToggleLeft]
#endif
        public bool useLocalSpace;

#if UNITY_EDITOR
        [FoldoutGroup("高级设置", false)] [Tooltip("拖拽时是否启用边界限制")] [ToggleLeft]
#endif
        public bool useBounds;

#if UNITY_EDITOR
        [FoldoutGroup("高级设置", false)] [ShowIf("useBounds")] [Tooltip("拖拽边界")]
#endif
        public Bounds dragBounds = new(Vector3.zero, new Vector3(10, 10, 10));

        #endregion

        #region 私有变量

        private MouseInteractable mouseInteractable;
        private Vector3 dragOffset;
        private float initialDistance;
        private Plane dragPlane;
        private bool isDragging;

        #endregion

        #region Unity生命周期

        private void Awake()
        {
            mouseInteractable = GetComponent<MouseInteractable>();
        }

        private void Start()
        {
            if (mouseInteractable != null)
            {
                mouseInteractable.OnBeginDragEvent.AddListener(StartDrag);
                mouseInteractable.OnMouseDragEvent.AddListener(OnDrag);
                mouseInteractable.OnEndDragEvent.AddListener(EndDrag);
            }
            else
            {
                Debug.LogError("MouseInteractable组件未找到!");
                enabled = false;
            }
        }

        private void OnDestroy()
        {
            if (mouseInteractable != null)
            {
                mouseInteractable.OnBeginDragEvent.RemoveListener(StartDrag);
                mouseInteractable.OnMouseDragEvent.RemoveListener(OnDrag);
                mouseInteractable.OnEndDragEvent.RemoveListener(EndDrag);
            }
        }

        #endregion

        #region 拖拽处理

        private void StartDrag(MouseInteractable.MouseEventData data)
        {
            isDragging = true;

            // 确定拖拽平面
            var planeNormal = useLocalSpace
                ? transform.TransformDirection(dragPlaneNormal).normalized
                : dragPlaneNormal.normalized;
            if (planeNormal == Vector3.zero) planeNormal = Camera.main.transform.forward;

            // 创建拖拽平面
            if (maintainDepth)
                dragPlane = new Plane(Camera.main.transform.forward, transform.position);
            else
                dragPlane = new Plane(planeNormal, transform.position);

            // 计算拖拽偏移量
            var ray = Camera.main.ScreenPointToRay(data.mousePosition);
            float distance;
            if (dragPlane.Raycast(ray, out distance))
            {
                var hitPoint = ray.GetPoint(distance);
                dragOffset = transform.position - hitPoint;
                initialDistance = distance;
            }
        }

        private void OnDrag(MouseInteractable.MouseEventData data)
        {
            if (!isDragging) return;

            Vector3 newPosition;

            if (useScreenSpace)
            {
                // 屏幕空间拖拽
                var screenPos = Camera.main.WorldToScreenPoint(transform.position);
                screenPos += new Vector3(data.dragDelta.x, data.dragDelta.y, 0);
                newPosition = Camera.main.ScreenToWorldPoint(screenPos);
            }
            else
            {
                // 世界空间拖拽
                var ray = Camera.main.ScreenPointToRay(data.mousePosition);
                float distance;
                if (dragPlane.Raycast(ray, out distance))
                    newPosition = ray.GetPoint(distance) + dragOffset;
                else
                    return;
            }

            // 应用位置锁定
            var targetPosition = transform.position;
            if (!lockX) targetPosition.x = newPosition.x;
            if (!lockY) targetPosition.y = newPosition.y;
            if (!lockZ) targetPosition.z = newPosition.z;

            // 应用边界限制
            if (useBounds)
            {
                targetPosition.x = Mathf.Clamp(targetPosition.x, dragBounds.min.x, dragBounds.max.x);
                targetPosition.y = Mathf.Clamp(targetPosition.y, dragBounds.min.y, dragBounds.max.y);
                targetPosition.z = Mathf.Clamp(targetPosition.z, dragBounds.min.z, dragBounds.max.z);
            }

            // 平滑移动
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * dragSpeed * 10);
        }

        private void EndDrag(MouseInteractable.MouseEventData data)
        {
            isDragging = false;
        }

        #endregion
    }
}