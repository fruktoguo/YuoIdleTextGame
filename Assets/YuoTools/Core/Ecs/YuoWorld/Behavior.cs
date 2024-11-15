using System;
using System.Collections.Generic;
using System.Linq;

namespace YuoTools.Main.Ecs
{
    public partial class YuoWorld
    {
        #region 基础生命周期

        /// <summary>
        /// 初始化 YuoWorld
        /// </summary>
        public void OnInit()
        {
            allSystem = new();
            Entities = new();
            components = new();
            systemsOfTag = new();
            systemsOfComponent = new();
            systemsOfBaseComponent = new();

            Instance = this;

            // 系统初始化必须放在所有初始化之前
            Initialization();

            // 创建主实体并添加核心组件
            Main = new YuoEntity(0);
            Main.EntityName = "核心组件";
            Main.AddComponent<YuoWorldMainComponent>();

            // 添加一个默认场景实体
            var scene = new YuoEntity(IDGenerate.GetID(IDGenerate.IDType.Scene, 0));
            scene.EntityName = "默认场景";
            AllScenes.Add(scene);
            scene.AddComponent<SceneComponent>();
        }

        /// <summary>
        /// 销毁 YuoWorld
        /// </summary>
        public void OnDestroy()
        {
            _worldIsDestroy = true;
            try
            {
                // 退出前执行 ExitGame 系统
                RunSystemForAllEntity<IExitGame>();

                // 销毁所有实体
                foreach (var entity in Entities.Values.ToList())
                {
                    entity?.Dispose();
                }
            }
            catch (Exception e)
            {
                $"Game Exit Error {e}".LogError();
            }

            // 清理所有集合
            Entities.Clear();
            components.Clear();
            systemsOfTag.Clear();
            systemsOfComponent.Clear();
            systemsOfBaseComponent.Clear();
            allSystem.Clear();

            // 清除所有场景
            AllScenes.Clear();
            Instance = null;
            // 手动触发垃圾回收
            GC.Collect();
            "Game Exit".Log();
        }

        /// <summary>
        /// 更新函数，每帧调用
        /// </summary>
        public void Update()
        {
            ClearTrash();
            if (startSystems.Count > 0)
            {
                List<YuoComponent> startTemps = new List<YuoComponent>();
                startTemps.AddRange(startSystems);
                foreach (var component in startTemps)
                {
                    // 添加链接组件
                    if (linkComponent.TryGetValue(component.Type, out var linkTypes))
                    {
                        foreach (var linkType in linkTypes)
                        {
                            component.Entity.AddComponent(linkType);
                        }
                    }
                }

                var startType = typeof(IStart);
                foreach (var component in startTemps)
                {
                    RunSystem(startType, component);
                }
            }

            ClearTrash();
            startSystems.Clear();

            RunSystemForAllEntity(SystemType.Update);
        }

        /// <summary>
        /// 固定更新函数，固定时间间隔调用
        /// </summary>
        public void FixedUpdate()
        {
            ClearTrash();
            RunSystemForAllEntity(SystemType.FixedUpdate);
            ClearTrash();
        }

        /// <summary>
        /// 延迟更新函数，在 Update 之后调用
        /// </summary>
        public void LateUpdate()
        {
            ClearTrash();
            RunSystemForAllEntity(SystemType.LateUpdate);
            ClearTrash();
        }

        #endregion
    }
}