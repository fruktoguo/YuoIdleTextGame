﻿using Sirenix.OdinInspector;
using UnityEngine;
using YuoTools.Main.Ecs;

namespace YuoTools.Extend
{
    [DefaultExecutionOrder(int.MinValue)]
    public sealed class WorldMono : SerializedMonoBehaviour
    {
        public YuoWorld World;

        public static void WorldInit()
        {
            if (YuoWorld.Instance != null)
            {
                Debug.LogError("WorldInit 错误，请检查是否多次调用了该函数");
                return;
            }
            var worldMono = new GameObject("World").AddComponent<WorldMono>();
            worldMono.World = new YuoWorld();
            worldMono.World.OnInit();
            DontDestroyOnLoad(worldMono);
        }

        private void OnDestroy()
        {
            if (YuoWorld.Instance == World)
                World.OnDestroy();
        }

        private float time;
        private void Update()
        {
            World.Update();
            if (time < Time.time)
            {
                time += 1;
                YuoWorld.RunSystemForAll<IUpdateEverySecond>();
            }
        }

        private void FixedUpdate()
        {
            World.FixedUpdate();
        }
        
        private void LateUpdate()
        {
            World.LateUpdate();
        }
    }
}