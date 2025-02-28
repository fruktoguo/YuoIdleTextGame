using System.Linq;
using ET;
using Sirenix.OdinInspector;
using UnityEngine;
using YuoTools.Extend;
using YuoTools.Main.Ecs;

namespace YuoTools.UI
{
    using System.Collections.Generic;

    public partial class UIManagerComponent
    {
        public async ETTask<T> OpenAsync<T>(T component) where T : UIComponent
        {
            // $"Open View {component}".Log();

            if (component == null) return null;

            if (!component.ModuleUI) TopView = component;

            //不能重复打开
            if (!openItems.Contains(component))
            {
                component.IsOpen = true;

                openItems.Add(component);
                var sort = TryGetSortComponent(component);
                sort?.openItems.Add(component);

                component.AddComponent<UIActiveComponent>();

                RunSystemAndChild<IUIOpen>(component);

                if (component.TryGetComponent<UIAnimaComponent>(out var anima))
                {
                    await anima.Open();
                    anima.RunSystem<IUIOpenAnima>();
                }
            }

            return component;
        }

        public async void Open<T>(T component) where T : UIComponent
        {
            await OpenAsync(component);
        }

        public async ETTask CloseAsync<T>(T component) where T : UIComponent
        {
            if (component == null)
            {
                $"找不到窗口{typeof(T).FullName}".LogError();
                return;
            }

            if (component.rectTransform.gameObject.activeSelf) RunSystemAndChild<IUIClose>(component);

            if (component.ModuleUI && moduleUiItems.Contains(component))
            {
                moduleUiItems.Remove(component);

                var sort = TryGetSortComponent(component);
                sort?.moduleUiItems.Remove(component);
            }

            if (openItems.Contains(component))
            {
                openItems.Remove(component);

                var sort = TryGetSortComponent(component);
                sort?.openItems.Remove(component);
            }

            component.IsOpen = false;

            component.Entity.RemoveComponent<UIActiveComponent>();

            if (!component.ModuleUI)
            {
                TopView = openItems.LastOrDefault(item => !item.ModuleUI);
            }

            if (component.TryGetComponent<UIAnimaComponent>(out var anima))
            {
                await anima.Close();
                YuoWorld.RunSystem<IUICloseAnima>(anima);
            }

            if (component.AutoHide) component.rectTransform.gameObject.Hide();
        }

        public async void Close<T>(T component) where T : UIComponent
        {
            await CloseAsync(component);
        }

        #region Open

        public async ETTask<UIComponent> OpenAsync(string winName, GameObject go = null)
        {
            UIComponent component = GetUIView(winName) ?? await AddWindow(winName, go);

            Open(component);

            return component;
        }

        public async void Open(string winName, GameObject go = null) => await OpenAsync(winName, go);

        public async void Open<T>() where T : UIComponent => await OpenAsync<T>();

        public async ETTask<T> OpenAsync<T>() where T : UIComponent
        {
            if (uiItemsType.ContainsKey(typeof(T)))
                return await OpenAsync(uiItemsType[typeof(T)].ViewName) as T;
            else
            {
                Debug.LogError($"UIManagerComponent Open error,not find {typeof(T)}");
                return null;
            }
        }

        #endregion

        #region Close

        public void Close<T>() where T : UIComponent
        {
            if (uiItemsType.ContainsKey(typeof(T)))
                Close(uiItemsType[typeof(T)].ViewName);
            else
            {
                Debug.LogError($"UIManagerComponent Close<T> error,not find {typeof(T)}");
            }
        }

        public void Close(string winName)
        {
            UIComponent component = GetUIView(winName);
            Close(component);
        }

        public void CloseAll(bool closeModule = false)
        {
            var openItemsCopy = openItems.ToArray();
            foreach (var item in openItemsCopy)
            {
                if (!closeModule && item.ModuleUI) continue;
                Close(item.ViewName);
            }
        }

        #endregion

        public static void ResetWindowLayer(UIComponent component)
        {
            var sort = Get.TryGetSortComponent(component);
            if (component.ModuleUI)
            {
                if (!Get.moduleUiItems.Contains(component))
                {
                    Get.moduleUiItems.Add(component);

                    sort?.moduleUiItems.Add(component);

                    component.AddComponent<TopViewComponent>();
                }

                SortModuleWindowLayer(sort, component);
                // component.rectTransform.SetAsLastSibling();
            }
            else
            {
                component.SetWindowLayer(sort.Root.childCount - 1 - sort.moduleUiItems.Count);
            }
        }

        public static void SortModuleWindowLayer(UIViewSort sort, UIComponent view)
        {
            var modules = sort.moduleUiItems;
            modules.Sort((x, y) => x.ModuleLayer - y.ModuleLayer);
            var viewLayer = view.ModuleLayer;
            var greaterCount = 0;
            foreach (var item in modules)
            {
                if (item.ModuleLayer > viewLayer) greaterCount++;
                item.rectTransform.SetAsLastSibling();
            }
            view.SetWindowLayer(sort.Root.childCount - 1 - greaterCount);
        }

        [ShowInInspector] Dictionary<Transform, UIViewSort> uiSortDic = new();

        public UIViewSort TryGetSortComponent(UIComponent view)
        {
            var parent = view.rectTransform.parent;
            if (!uiSortDic.TryGetValue(parent, out var sort))
            {
                sort = new UIViewSort
                {
                    Root = parent
                };
                uiSortDic.Add(parent, sort);
            }

            return sort;
        }

        public async ETTask<UIComponent> AddWindow(string winName, GameObject go = null)
        {
            var type = YuoWorld.Instance.GetComponentType($"View_{winName}Component");
            if (type == null)
            {
                Debug.LogError($"找不到对应类型---View_{winName}Component");
                return null;
            }

            if (go == null) go = await Create(winName);
            if (go == null) return null;

            var rect = go.transform as RectTransform;

            //生成窗口
            var component =
                YuoWorld.Main.GetComponent<UIManagerComponent>().Entity
                    .AddChild(type, rect, IDGenerate.GetID(winName)) as UIComponent;
            if (component == null) return null;

            component.AddComponent<UIAutoExitComponent>();

            //如果有动画组件就挂载动画组件
            if (go.TryGetComponent<UISetting>(out var uiSetting))
            {
                if (uiSetting.HasAnima())
                    component.Entity.AddComponent<UIAnimaComponent>().From(uiSetting);
                component.ModuleUI = uiSetting.ModuleUI;
                component.ModuleLayer = uiSetting.ModuleLayer;
                go.SetActive(uiSetting.Active);
                Object.Destroy(uiSetting);
            }

            component.ViewName = winName;

            var sort = TryGetSortComponent(component);

            uiItems.Add(winName, component);
            uiItemsType.Add(type, component);

            sort.uiItems.Add(winName, component);

            if (BaseIndex == -1) BaseIndex = go.transform.GetSiblingIndex();

            component.Entity.EntityName = "View_" + component.ViewName;

            //调用这个窗口的初始化系统
            YuoWorld.RunSystem<IUICreate>(component.Entity);

            //模块UI永远在最上面
            if (component.ModuleUI)
            {
                component.Entity.AddComponent<TopViewComponent>();
                moduleUiItems.Add(component);

                sort.moduleUiItems.Add(component);
            }

            return component;
        }

        public void Remove(string winName)
        {
            var win = GetUIView(winName);
            if (win != null)
            {
                uiItems.Remove(winName);
                uiItemsType.Remove(win.Type);

                var sort = TryGetSortComponent(win);
                sort.uiItems.Add(winName, win);


                win.Entity.Dispose();
            }
        }

        public bool IsOpen(string winName)
        {
            return openItems.Contains(GetUIView(winName));
        }

        public UIComponent GetUIView(string winName)
        {
            return uiItems.GetValueOrDefault(winName);
        }

        public T GetUIView<T>() where T : UIComponent
        {
            if (uiItemsType.ContainsKey(typeof(T)))
                return uiItemsType[typeof(T)] as T;
            else
            {
                // Debug.LogError($"没有找到对应的窗口---{typeof(T)}");
                return null;
            }
        }

        public async ETTask<UIComponent> GetOrAddView(string winName)
        {
            return uiItems.ContainsKey(winName) ? uiItems[winName] : await AddWindow(winName);
        }

        private async ETTask<GameObject> Create(string winName)
        {
            return (await YuoWorld.Main.GetBaseComponent<AssetsLoadComponent>().LoadPrefabAsync(LoadPath + winName))
                .Instantiate(Transform);
        }

        public bool HasViewOfName(string viewName)
        {
            return uiItems.ContainsKey(viewName);
        }

        private UIComponent _topView;

        public UIComponent TopView
        {
            get => _topView;
            private set
            {
                if (_topView != value)
                {
                    if (_topView != null)
                    {
                        _topView.GetComponent<TopViewComponent>().DestroyComponent();
                        // $"{_topView.Entity}不在顶部了".Log();
                    }

                    if (value != null)
                    {
                        value.Entity.AddComponent<TopViewComponent>();
                        // $"{value.Entity}在顶部了".Log();

                        RunSystemAndChild<IUIActive>(value);
                    }
                    else
                    {
                        // "没有窗口在顶部".Log();
                    }

                    _topView = value;
                }
            }
        }

        public int WindowCount => uiItems.Count + BaseIndex;

        public static Vector2 CanvasSize;

        public void RunSystemAndChild<T>(UIComponent component) where T : ISystemTag
        {
            component.Entity.RunSystem<T>();
            var children = component.Entity.GetAllChildren();
            YuoWorld.RunSystem<T>(children);
        }

        public List<UIComponent> GetOpenItems() => openItems;
    }

    public partial class UIComponent
    {
        public void SetWindowLayer(int windowLayer)
        {
            rectTransform.SetSiblingIndex(windowLayer);
        }

        public T AddChildAndInstantiate<T>(T template) where T : UIComponent, new() =>
            AddChildAndInstantiate(template, Entity);

        public T AddChildAndInstantiate<T>(T template, YuoEntity parent) where T : UIComponent, new()
        {
            var go = Object.Instantiate(template.rectTransform, template.rectTransform.parent);
            var child = parent.AddChild<T>();
            child.rectTransform = go.transform as RectTransform;
            child.RunSystem<IUICreate>();
            go.Show();
            return child;
        }
    }

    public static class YuoUIExtensions
    {
        public static Vector2 GetLossyScale(this RectTransform rectTransform)
        {
            var canvasScale = UIManagerComponent.Get?.Canvas.transform.lossyScale ?? Vector3.one;
            var lossyScale = rectTransform.lossyScale;
            return new Vector2(lossyScale.x / canvasScale.x, lossyScale.y / canvasScale.y);
        }
    }

    public class UIViewSort
    {
        public Transform Root;

        public Dictionary<string, UIComponent> uiItems = new();

        public List<UIComponent> moduleUiItems = new();

        public List<UIComponent> openItems = new();

        public bool Equal(UIViewSort other)
        {
            return Root == other.Root;
        }
    }

    public class UIManagerAwakeSystem : YuoSystem<UIManagerComponent>, IAwake
    {
        public override string Group => SystemGroupConst.MainUI;

        public override void Run(UIManagerComponent component)
        {
            if (component != UIManagerComponent.Get) return;
            if (component.Transform == null)
            {
                component.DestroyComponent();
                return;
            }

            //初始化所有挂载在根节点的UI
            var uiSettings = Object.FindObjectsByType<UISetting>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var uiSetting in uiSettings)
            {
                uiSetting.Init();
            }

            //初始化自动加载的UI
            var uiAutoLoadConfigs = Resources.LoadAll<UIAutoLoadConfig>("");
            foreach (var uiAutoLoadConfig in uiAutoLoadConfigs)
            {
                foreach (var uiWindow in uiAutoLoadConfig.AutoLoadList)
                {
                    if (component.HasViewOfName(uiWindow.name)) continue;
                    var uiObject = uiWindow.Instantiate(component.Transform);
                    uiObject.name = uiWindow.name;
                    var uiSetting = uiObject.GetComponent<UISetting>();
                    if (uiSetting != null)
                    {
                        uiSetting.Init();
                    }
                }
            }
        }
    }

    public class UIItemOpenSystem : YuoSystemOfBase<UIComponent, UIActiveComponent>, IUIOpen
    {
        public override string Group => SystemGroupConst.MainUI;

        public override void Run(UIComponent component, UIActiveComponent _)
        {
            // component.SetWindowLayer(YuoWorld.Main.GetComponent<UIManagerComponent>().WindowCount);
            UIManagerComponent.ResetWindowLayer(component);
            // $"{component.ViewName} open {component.AutoShow}".Log();
            if (component.AutoShow) component.rectTransform.Show();
        }
    }

    public class UIItemSelectSystem : YuoSystemOfBase<UIComponent>, IStart, IUICreate
    {
        public override string Group => SystemGroupConst.MainUI;

        public override void Run(UIComponent baseComponent)
        {
            if (baseComponent.rectTransform != null)
            {
                baseComponent.AddComponent<EntitySelectComponent>().SelectGameObject =
                    baseComponent.rectTransform.gameObject;
            }
            else
            {
                Debug.LogError($"UIItemSelectSystem error,not find rectTransform {baseComponent.ViewName}");
            }
        }
    }

    public class UICanvasSizeResetSystem : YuoSystem<UIManagerComponent>, IAwake, IUIAdaption
    {
        public override string Group => SystemGroupConst.MainUI;

        public override void Run(UIManagerComponent component)
        {
        }
    }

    public class UIAutoExitComponent : YuoComponent
    {
        public override string Name => "场景切换时自动销毁";
    }

    /// <summary>
    /// 切换场景时清理UI
    /// </summary>
    public class UIAutoExitSystem : YuoSystem<UIAutoExitComponent>, ISceneExit
    {
        public override string Group => SystemGroupConst.MainUI;

        public override void Run(UIAutoExitComponent component)
        {
            component.Entity.Destroy();
        }
    }

    public class UIDestroySystem : YuoSystemOfBase<UIComponent>, IDestroy
    {
        public override string Group => SystemGroupConst.MainUI;

        public override void Run(UIComponent baseComponent)
        {
            if (baseComponent.rectTransform != null)
            {
                Object.Destroy(baseComponent.rectTransform.gameObject);
            }
        }
    }

    public class UIUpdateSystem : YuoSystem<UIManagerComponent>, IUpdate
    {
        public override string Group => SystemGroupConst.MainUI;

        public override void Run(UIManagerComponent manager)
        {
            foreach (var uiComponent in manager.GetOpenItems())
            {
                manager.RunSystemAndChild<IUIUpdate>(uiComponent);
            }
        }
    }
}