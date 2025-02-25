using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using YuoTools.Extend;
using YuoTools.Main.Ecs;

namespace YuoTools.Editor.Ecs
{
    public partial class YuoWorldWindow
    {
        public class ComponentView
        {
            [ReadOnly] [HorizontalGroup("info", 300, LabelWidth = 100)] [GUIColor(0.8f, 0.5f, 0.3f)]
            public long EntityID;

            [ShowInInspector]
            [HorizontalGroup("info", 200, LabelWidth = 100)]
            [GUIColor(0.8f, 0.5f, 0.3f)]
            public int ChildCount => Entity.Children.Count;

            [ShowInInspector]
            [HorizontalGroup("info", 200, LabelWidth = 150)]
            [GUIColor(0.8f, 0.5f, 0.3f)]
            public int ComponentCount => Components.Count;

            [ListDrawerSettings(ShowFoldout = true, HideAddButton = true, HideRemoveButton = true,
                DraggableItems = false,
                NumberOfItemsPerPage = 99,
                ShowItemCount = false, ListElementLabelName = "@Name",
                ElementColor = nameof(ElementColor))]
            [ShowInInspector]
            [HideIf("_count", 0)]
            public List<YuoComponent> Components = new();

            [HideInInspector] public YuoEntity Entity;

            private static YuoEntity _currentEntity;

            private GameObject _gameObject;

            [OnInspectorGUI]
            public void OnSelect()
            {
                // 检查当前Entity是否与静态变量存储的Entity不同
                if (_currentEntity != Entity)
                {
                    _currentEntity = Entity;

                    // 尝试从Entity中获取EntitySelectComponent组件
                    if (Entity.TryGetComponent<EntitySelectComponent>(out var select))
                    {
                        // 将当前选中的GameObject设置为select.SelectGameObject
                        Selection.activeGameObject = select.SelectGameObject;

                        _gameObject = select.SelectGameObject;
                    }
                }
            }

            private Color ElementColor(int index)
            {
                return Components[index].CustomEditorElementColor();
            }

            int _count;

            public void Update()
            {
                _count = Components.Count;
                if (Entity == null) return;
                Components.Clear();
                if (Entity.TryGetComponent<EntitySelectComponent>(out var select))
                {
                    _gameObject = select.SelectGameObject;
                }

                foreach (var component in Entity.Components.Values)
                {
                    if (!Components.Contains(component))
                    {
                        Components.Add(component);
                    }
                }
            }

            private bool crash_add;

            [HorizontalGroup("fun")]
            [Button("添加组件", ButtonStyle.CompactBox), GUIColor(0.5f, 0.7f, 1f)]
            public void AddComponent()
            {
                crash_componentToAdd = null;
                crash_add = !crash_add;
            }

            [PropertyOrder(100)]
            [BoxGroup("ComponentSelection", ShowLabel = false)]
            [SerializeReference]
            [OnValueChanged(nameof(OnComponentChanged))]
            [ShowIf(nameof(crash_add))]
            [ShowInInspector]
            [LabelText("缓存组件")]
            YuoComponent crash_componentToAdd;

            private void OnComponentChanged()
            {
                if (crash_componentToAdd != null)
                {
                    // 强制重绘 Inspector
                    UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                }
            }

            [PropertyOrder(100)]
            [BoxGroup("ComponentSelection")]
            [HorizontalGroup("ComponentSelection/Buttons")]
            [Button("确认", ButtonStyle.CompactBox), GUIColor(0.3f, 0.8f, 0.3f)]
            [ShowIf("@crash_add")]
            public void ConfirmAdd()
            {
                if (crash_componentToAdd != null)
                {
                    Entity.SetComponent(crash_componentToAdd.Type, crash_componentToAdd);
                    crash_componentToAdd = null;
                }

                crash_add = false;
            }

            [PropertyOrder(100)]
            [BoxGroup("ComponentSelection")]
            [HorizontalGroup("ComponentSelection/Buttons")]
            [Button("取消", ButtonStyle.CompactBox), GUIColor(0.7f, 0.7f, 0.7f)]
            [ShowIf("@crash_add")]
            public void Cancel()
            {
                crash_componentToAdd = null;
                crash_add = false;
            }

            [HorizontalGroup("fun")]
            [Button("删除实体")]
            void DestroyEntity()
            {
                Entity.Destroy();
            }
        }

        public class SystemView
        {
            [PropertyOrder(-99)]
            [ShowInInspector]
            [LabelText("系统启用")]
            public bool Enabled
            {
                get => System.Enabled;
                set => System.Enabled = value;
            }

            [HorizontalGroup()]
            [ListDrawerSettings(ShowFoldout = true, HideAddButton = true, HideRemoveButton = true,
                DraggableItems = false,
                ShowItemCount = false)]
            [ShowInInspector]
            [ReadOnly]
            [LabelText("支持的类型")]
            public List<string> InfluenceType = new();

            [HorizontalGroup()]
            [ListDrawerSettings(ShowFoldout = true, HideAddButton = true, HideRemoveButton = true,
                DraggableItems = false,
                ShowItemCount = false)]
            [ShowInInspector]
            [ReadOnly]
            [LabelText("触发的类型")]
            public List<string> InterfaceType = new();

            [ReadOnly] public short SystemOrder;

            [BoxGroup("Time")] [LabelWidth(100)] [LabelText("执行耗时")] [SuffixLabel("毫秒")] [ReadOnly]
            public double TimeConsuming;

            [BoxGroup("Time")] [LabelWidth(100)] [LabelText("总耗时")] [SuffixLabel("毫秒")] [ReadOnly]
            public double TotalTimeConsuming;

            [LabelWidth(100)] [BoxGroup("Time")] [LabelText("执行")] [SuffixLabel("次")] [ReadOnly]
            public long RunCount;

            [BoxGroup("Time")]
            [LabelText("平均耗时")]
            [SuffixLabel("毫秒")]
            [LabelWidth(100)]
            [ReadOnly]
            [ShowInInspector]
            public double AverageTimeConsuming => RunCount == 0 ? 0 : TotalTimeConsuming / RunCount;

            [BoxGroup("Time")]
            [LabelText("总耗时占比")]
            [LabelWidth(100)]
            [SuffixLabel("万分之一")]
            [ReadOnly]
            [ShowInInspector]
            public string RatioForMain => (TotalTimeConsuming / Time.unscaledTime * 10).ToString("F4");

            public SystemBase System;
            public List<YuoEntity> Entities = new();

            [ListDrawerSettings(ShowFoldout = true, HideAddButton = true, HideRemoveButton = true,
                DraggableItems = false,
                ShowItemCount = false)]
            [ShowInInspector]
            [ReadOnly]
            [LabelText("Entities")]
            public List<string> Entity = new();

            public void Update()
            {
                TimeConsuming = System.TimeConsuming;
                TotalTimeConsuming = System.TotalTimeConsuming;
                RunCount = System.TotalRunCount;
                Entities = System.Entities;
                Entity.Clear();
                Entity.AddRange(Entities.Select(e => e.EntityName));
            }
        }
    }
}