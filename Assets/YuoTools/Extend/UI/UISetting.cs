using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using YuoTools.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace YuoTools.UI
{
    using DG.Tweening;

    public class UISetting : MonoBehaviour
    {
        [HideInInspector] [HorizontalGroup] [Header("默认显示状态")]
        public bool Active = true;

        [HorizontalGroup("2")] [LabelText("模块UI")]
        public bool ModuleUI = false;

        [ShowIf(nameof(hasAnimator))] public float AnimatorLength = 0.5f;

        bool hasAnimator => GetComponent<Animator>() != null;

#if UNITY_EDITOR

        [HorizontalGroup("2")]
        [Button("生成UI代码")]
        public void SpawnCode()
        {
            SpawnUICode.SpawnCode(gameObject);
        }

        [HorizontalGroup("2")]
        [Button("打开Component脚本")]
        public void SelectSystemScript()
        {
            var fileName = $"View_{gameObject.name}Component";
            var result = AssetDatabase.FindAssets(fileName);
            if (result.Length > 0)
            {
                var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(result[0]));
                AssetDatabase.OpenAsset(asset);
            }
            else
            {
                Debug.LogError($"{fileName} 没有找到");
            }
        }

        [HorizontalGroup("2")]
        [Button("打开System脚本")]
        public void OpenSystemScript()
        {
            var fileName = $"View_{gameObject.name}System";
            var result = AssetDatabase.FindAssets(fileName);
            if (result.Length > 0)
            {
                var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(result[0]));
                AssetDatabase.OpenAsset(asset);
            }
            else
            {
                Debug.LogError($"{fileName} 没有找到");
            }
        }

        [ShowInInspector] public bool OpenTools { get; set; } = false;


        [ShowIf("OpenTools", true)] [FoldoutGroup("Raycast")]
        public List<MaskableGraphic> maskableGraphics = new List<MaskableGraphic>();

        [ShowIf("OpenTools", true)]
        [FoldoutGroup("Raycast")]
        [Button(ButtonHeight = 30, Name = "获取所有开启了Raycast的物体")]
        public void FindAllRaycast()
        {
            maskableGraphics.Clear();
            foreach (var item in transform.GetComponentsInChildren<MaskableGraphic>())
            {
                if (item.raycastTarget)
                {
                    maskableGraphics.Add(item);
                }
            }
        }

        [ShowIf("OpenTools", true)]
        [FoldoutGroup("Raycast")]
        [Button(ButtonHeight = 30, Name = "清除剩余Raycast")]
        public void CloseRaycast()
        {
            foreach (var item in maskableGraphics)
            {
                item.raycastTarget = false;
            }
        }

#endif

        public async void Init()
        {
            if (!enabled) return;
            var o = gameObject;
            Active = o.activeSelf;
            if (Active)
                await UIManagerComponent.Get.OpenAsync(o.name, o);
            else
            {
                var uiComponent = await UIManagerComponent.Get.AddWindow(o.name, gameObject);
                if (uiComponent == null) $"创建View失败_{o.name}".LogError();
            }
        }

        private Animator animator;

        public Animator Animator
        {
            get
            {
                if (!animator) animator = GetComponent<Animator>();
                return animator;
            }
        }


        public bool HasAnima()
        {
            return gameObject.TryGetComponent<DOTweenAnimation>(out var _) ||
                   gameObject.TryGetComponent<Animator>(out _);
        }

        public enum UISate
        {
            Hide = 0,
            Show = 1,
            ShowAnima = 2,
            HideAnima = 3,
        }
    }
}