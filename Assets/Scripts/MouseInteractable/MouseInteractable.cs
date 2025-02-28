using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace YuoTools.Extend
{
    public class MouseInteractable : MonoBehaviour
    {
        private bool HasDisabledCollider =>
            (GetComponent<Collider>() != null && !GetComponent<Collider>().enabled) ||
            (GetComponent<Collider2D>() != null && !GetComponent<Collider2D>().enabled);

        private bool NeedMainCamera => Camera.main == null;

        private bool NeedCollider => GetComponent<Collider>() == null && GetComponent<Collider2D>() == null;

        private string GetColliderWarning =>
            $"[MouseInteractable] {gameObject.name} 的{(GetComponent<Collider>() != null ? "Collider" : "Collider2D")}被禁用!";

        #region 事件数据定义

        [Serializable]
        public class MouseEventData
        {
            public Vector3 mousePosition; // 屏幕坐标
            public Vector3 worldPosition; // 世界坐标
            public Vector3 localPosition; // 局部坐标
            public Vector3 dragDelta; // 拖拽增量
            public Vector3 dragStartPosition; // 拖拽起始位置
            public float clickTime; // 点击时间
            public int clickCount; // 点击次数(用于双击检测)
        }

        [Serializable]
        public class MouseInteractionEvent : UnityEvent<MouseEventData>
        {
        }

        #endregion

        #region 公共属性

        [InfoBox("此组件需要Collider或Collider2D组件!", InfoMessageType.Error, "NeedCollider")]
        [InfoBox("[MouseInteractable] 场景中没有标记为MainCamera的相机!", InfoMessageType.Warning, "NeedMainCamera")]
        [InfoBox("$GetColliderWarning", InfoMessageType.Warning, "HasDisabledCollider")]
        [TitleGroup("交互设置")]
        [ToggleLeft]
        public bool interactable = true;

        [TitleGroup("交互设置")] [Tooltip("如果启用，只有当相机射线击中此物体的碰撞器时才触发事件")] [ToggleLeft]
        public bool requireDirectHit = true;

        [TitleGroup("交互设置")] [Tooltip("如果启用，会在控制台输出交互事件信息")] [ToggleLeft]
        public bool debugMode;

        [FoldoutGroup("高级设置", false)] [Tooltip("点击的最大时间阈值，超过此时间不算点击")] [Range(0.1f, 1.0f)]
        public float clickThreshold = 0.5f;

        [FoldoutGroup("高级设置", false)] [Tooltip("移动阈值，超过此距离不算点击")] [Range(1f, 20f)]
        public float moveThreshold = 5.0f;

        [FoldoutGroup("高级设置", false)] [Tooltip("双击的最大间隔时间")] [Range(0.1f, 0.5f)]
        public float doubleClickTime = 0.3f;

        [FoldoutGroup("高级设置", false)] [Tooltip("长按触发的时间")] [Range(0.5f, 2.0f)]
        public float longPressTime = 0.8f;

        [FoldoutGroup("高级设置", false)] [Tooltip("拖拽开始的最小距离")] [Range(1f, 10f)]
        public float dragThreshold = 3.0f;

        [TitleGroup("交互设置")] [Tooltip("指定触发交互的鼠标按键，默认为左键")]
        public int mouseButton; // 新增：自定义鼠标按键

        #endregion

        #region 事件声明

        [FoldoutGroup("基础事件", true)] [LabelText("鼠标进入")]
        public MouseInteractionEvent OnMouseEnterEvent = new();


        [FoldoutGroup("基础事件")] [LabelText("鼠标离开")]
        public MouseInteractionEvent OnMouseExitEvent = new();


        [FoldoutGroup("基础事件")] [LabelText("鼠标悬停")]
        public MouseInteractionEvent OnMouseOverEvent = new();


        [FoldoutGroup("基础事件")] [LabelText("鼠标按下")]
        public MouseInteractionEvent OnMouseDownEvent = new();


        [FoldoutGroup("基础事件")] [LabelText("鼠标抬起")]
        public MouseInteractionEvent OnMouseUpEvent = new();


        [FoldoutGroup("基础事件")] [LabelText("鼠标点击")]
        public MouseInteractionEvent OnMouseClickEvent = new();


        [FoldoutGroup("高级事件", false)] [LabelText("鼠标拖拽")]
        public MouseInteractionEvent OnMouseDragEvent = new();


        [FoldoutGroup("高级事件", false)] [LabelText("开始拖拽")]
        public MouseInteractionEvent OnBeginDragEvent = new();


        [FoldoutGroup("高级事件", false)] [LabelText("结束拖拽")]
        public MouseInteractionEvent OnEndDragEvent = new();


        [FoldoutGroup("高级事件", false)] [LabelText("长按")]
        public MouseInteractionEvent OnLongPressEvent = new();

        #endregion

        #region 私有变量

        // 状态追踪
        private bool isMouseOver;
        private bool isMouseDown;
        private bool isDragging;
        private bool longPressTriggered;
        private Vector3 mouseDownPosition;
        private Vector3 lastMousePosition;
        private float mouseDownTime;
        private Coroutine longPressCoroutine;

        // 当前鼠标数据
        private MouseEventData currentEventData = new();

        // 组件缓存
        private Collider myCollider;
        private Collider2D myCollider2D;
        private bool is3DMode = true; // 用于标记当前使用的是3D还是2D模式

        #endregion

        #region Unity生命周期

        private void Awake()
        {
            // 检查是否有3D碰撞器
            myCollider = GetComponent<Collider>();

            // 检查是否有2D碰撞器
            myCollider2D = GetComponent<Collider2D>();

            // 确定工作模式
            if (myCollider != null)
            {
                is3DMode = true;
            }
            else if (myCollider2D != null)
            {
                is3DMode = false;
            }
            else
            {
                // 两种碰撞器都没有
                Debug.LogError($"[MouseInteractable] {gameObject.name} 需要一个Collider或Collider2D组件!");
                enabled = false;
            }
        }

        private void Update()
        {
            // 处理持续悬停事件
            if (isMouseOver && interactable)
            {
                UpdateMouseEventData();
                OnMouseOverEvent.Invoke(currentEventData);
            }

            // 处理拖拽检测
            if (isMouseDown && !isDragging && interactable)
            {
                var dragDistance = Vector3.Distance(mouseDownPosition, Input.mousePosition);
                if (dragDistance > dragThreshold)
                {
                    isDragging = true;
                    UpdateMouseEventData();
                    currentEventData.dragStartPosition = mouseDownPosition;
                    OnBeginDragEvent.Invoke(currentEventData);
                }
            }
        }

        private void OnDisable()
        {
            // 清理状态
            if (isMouseOver)
            {
                isMouseOver = false;
                UpdateMouseEventData();
                OnMouseExitEvent.Invoke(currentEventData);
            }

            if (isMouseDown)
            {
                isMouseDown = false;
                UpdateMouseEventData();
                OnMouseUpEvent.Invoke(currentEventData);
            }

            if (isDragging)
            {
                isDragging = false;
                UpdateMouseEventData();
                OnEndDragEvent.Invoke(currentEventData);
            }

            StopLongPressCoroutine();
        }

        #endregion

        #region Unity鼠标回调

        private void OnMouseEnter()
        {
            if (!interactable || (requireDirectHit && !IsDirectHit())) return;

            isMouseOver = true;
            UpdateMouseEventData();
            OnMouseEnterEvent.Invoke(currentEventData);
            DebugLog("MouseEnter");
        }

        private void OnMouseExit()
        {
            if (!interactable) return;

            isMouseOver = false;
            UpdateMouseEventData();
            OnMouseExitEvent.Invoke(currentEventData);
            DebugLog("MouseExit");
        }

        private void OnMouseDown()
        {
            // 检查是否是指定的鼠标按键
            if (!interactable || (requireDirectHit && !IsDirectHit()) ||
                Input.GetMouseButtonDown(mouseButton) == false) return;

            isMouseDown = true;
            UpdateMouseEventData();
            mouseDownPosition = Input.mousePosition;
            lastMousePosition = mouseDownPosition;
            mouseDownTime = Time.time;
            currentEventData.clickTime = 0;
            OnMouseDownEvent.Invoke(currentEventData);
            DebugLog("MouseDown");

            // 启动长按检测
            StopLongPressCoroutine();
            longPressCoroutine = StartCoroutine(LongPressDetection());
        }

        private void OnMouseUp()
        {
            if (!interactable || !isMouseDown || Input.GetMouseButtonUp(mouseButton) == false) return;

            isMouseDown = false;
            UpdateMouseEventData();
            currentEventData.clickTime = Time.time - mouseDownTime;
            OnMouseUpEvent.Invoke(currentEventData);
            DebugLog("MouseUp");

            // 停止长按检测
            StopLongPressCoroutine();

            // 检查是否符合点击条件
            var mouseMoveDist = Vector3.Distance(mouseDownPosition, Input.mousePosition);
            if (currentEventData.clickTime < clickThreshold && mouseMoveDist < moveThreshold)
            {
                currentEventData.clickCount = 1;
                OnMouseClickEvent.Invoke(currentEventData);
                DebugLog("Click");
            }

            // 如果正在拖拽，触发结束拖拽事件
            if (isDragging)
            {
                isDragging = false;
                OnEndDragEvent.Invoke(currentEventData);
                DebugLog("EndDrag");
            }

            StopLongPressCoroutine();
        }

        private void OnMouseDrag()
        {
            if (!interactable || !isMouseDown) return;

            UpdateMouseEventData();

            // 计算拖拽增量
            currentEventData.dragDelta = Input.mousePosition - lastMousePosition;
            lastMousePosition = Input.mousePosition;

            if (isDragging)
            {
                OnMouseDragEvent.Invoke(currentEventData);
                DebugLog("Drag");
            }
        }

        #endregion

        #region 辅助方法

        // 更新鼠标事件数据
        private void UpdateMouseEventData()
        {
            currentEventData.mousePosition = Input.mousePosition;

            if (is3DMode)
            {
                // 3D世界坐标计算
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    currentEventData.worldPosition = hit.point;
                    if (hit.collider.gameObject == gameObject)
                        currentEventData.localPosition = transform.InverseTransformPoint(hit.point);
                }
            }
            else
            {
                // 2D世界坐标计算
                Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                currentEventData.worldPosition = new Vector3(mousePosition.x, mousePosition.y, transform.position.z);

                var hit = Physics2D.Raycast(mousePosition, Vector2.zero);
                if (hit.collider != null && hit.collider.gameObject == gameObject)
                    currentEventData.localPosition = transform.InverseTransformPoint(currentEventData.worldPosition);
            }
        }

        // 检查是否直接命中此物体
        private bool IsDirectHit()
        {
            if (is3DMode)
            {
                // 3D碰撞检测
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit)) return hit.collider.gameObject == gameObject;
            }
            else
            {
                // 2D碰撞检测
                Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var hit = Physics2D.Raycast(mousePosition, Vector2.zero);
                if (hit.collider != null) return hit.collider.gameObject == gameObject;
            }

            return false;
        }

        // 长按检测协程
        private IEnumerator LongPressDetection()
        {
            longPressTriggered = false;
            yield return new WaitForSeconds(longPressTime);

            if (isMouseDown && !isDragging)
            {
                longPressTriggered = true;
                UpdateMouseEventData();
                OnLongPressEvent.Invoke(currentEventData);
                DebugLog("LongPress");
            }
        }

        // 停止长按检测协程
        private void StopLongPressCoroutine()
        {
            if (longPressCoroutine != null)
            {
                StopCoroutine(longPressCoroutine);
                longPressCoroutine = null;
            }
        }

        // 调试日志
        private void DebugLog(string eventName)
        {
            if (debugMode) Debug.Log($"[MouseInteractable] {gameObject.name} - {eventName}");
        }

        // 公共方法：手动设置交互状态
        public void SetInteractable(bool state)
        {
            interactable = state;

            // 如果禁用交互，重置所有状态
            if (!interactable)
            {
                if (isMouseOver)
                {
                    isMouseOver = false;
                    UpdateMouseEventData();
                    OnMouseExitEvent.Invoke(currentEventData);
                    DebugLog("MouseExit (因禁用)");
                }

                if (isMouseDown)
                {
                    isMouseDown = false;
                    UpdateMouseEventData();
                    OnMouseUpEvent.Invoke(currentEventData);
                    DebugLog("MouseUp (因禁用)");

                    if (isDragging)
                    {
                        isDragging = false;
                        OnEndDragEvent.Invoke(currentEventData);
                        DebugLog("EndDrag (因禁用)");
                    }

                    StopLongPressCoroutine();
                }
            }
        }

        // 获取当前鼠标事件数据
        public MouseEventData GetCurrentEventData()
        {
            UpdateMouseEventData();
            return currentEventData;
        }

        // 检查鼠标是否悬停在物体上
        public bool IsMouseOver()
        {
            return isMouseOver;
        }

        // 检查鼠标是否按下在物体上
        public bool IsMouseDown()
        {
            return isMouseDown;
        }

        // 检查是否正在拖拽
        public bool IsDragging()
        {
            return isDragging;
        }

        #endregion
    }
}