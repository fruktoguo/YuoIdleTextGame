using System;
using System.Collections.Generic;
using ET;
using UnityEngine;
using Object = UnityEngine.Object;

namespace YuoTools
{
    /// <summary>
    /// 管理异步等待操作的单例类
    /// </summary>
    public class YuoAwait_Mono : SingletonMono<YuoAwait_Mono>
    {
        [SerializeField] private YuoObjectPool<AwaitItem> awaitPools;
        private List<AwaitItem> removeList = new List<AwaitItem>();
        private Dictionary<int, AwaitItem> _frameAwaits = new Dictionary<int, AwaitItem>();

        public override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
            gameObject.name = "YuoAwait";
            InitializeObjectPool();
        }

        private void InitializeObjectPool()
        {
            awaitPools = new YuoObjectPool<AwaitItem>(
                createFunc: () => new AwaitItem(),
                resetAction: item =>
                {
                    item.timeType = TimeType.Normal;
                    item.tcs = ETTask.Create(true);
                    item.CancelToken = null;
                });
        }

        private void Update()
        {
            CheckTimerOut();
        }

        /// <summary>
        /// 检查并处理超时的AwaitItem
        /// </summary>
        private void CheckTimerOut()
        {
            if (awaitPools.Actives.Count > 0)
            {
                foreach (var item in awaitPools.Actives)
                {
                    if (item.CancelToken != null && item.CancelToken.IsCancel())
                    {
                        removeList.Add(item);
                        continue;
                    }

                    switch (item.timeType)
                    {
                        case TimeType.Normal:
                            if (Time.time < item.TargetTime) continue;
                            break;
                        case TimeType.Unscaled:
                            if (Time.unscaledTime < item.TargetTime) continue;
                            break;
                        case TimeType.Frame:
                            if (Time.frameCount < item.TargetFrame) continue;
                            break;
                    }

                    removeList.Add(item);
                }

                ProcessRemoveList();
            }

            _frameAwaits.Clear();
        }

        private void ProcessRemoveList()
        {
            if (removeList.Count > 0)
            {
                foreach (var item in removeList)
                {
                    if (item.CancelToken == null || !item.CancelToken.IsCancel())
                    {
                        item.tcs.SetResult();
                    }
                    else
                    {
                        item.tcs.SetException(new OperationCanceledException());
                    }

                    awaitPools.Remove(item);
                }

                removeList.Clear();
            }
        }

        /// <summary>
        /// 等待指定的无缩放时间
        /// </summary>
        /// <param name="waitTime">等待时间</param>
        /// <param name="cancelToken">取消令牌</param>
        /// <returns>ETTask</returns>
        public async ETTask WaitUnscaledTimeAsync(float waitTime, ETCancellationToken cancelToken = null)
        {
            AwaitItem item = awaitPools.GetItem();
            item.timeType = TimeType.Unscaled;
            item.CreatTime = Time.unscaledTime;
            item.TargetTime = Time.unscaledTime + waitTime;
            item.CancelToken = cancelToken;
            await item.tcs;
        }

        /// <summary>
        /// 等待指定的时间
        /// </summary>
        /// <param name="waitTime">等待时间</param>
        /// <param name="cancelToken">取消令牌</param>
        /// <returns>ETTask</returns>
        public async ETTask WaitTimeAsync(float waitTime, ETCancellationToken cancelToken = null)
        {
            AwaitItem item = awaitPools.GetItem();
            item.timeType = TimeType.Normal;
            item.CreatTime = Time.time;
            waitTime = Mathf.Clamp(waitTime, 0.00001f, float.MaxValue);
            item.TargetTime = Time.time + waitTime;
            item.CancelToken = cancelToken;
            await item.tcs;
        }

        /// <summary>
        /// 等待指定的帧数
        /// </summary>
        /// <param name="frame">等待帧数</param>
        /// <param name="cancelToken">取消令牌</param>
        /// <returns>ETTask</returns>
        public async ETTask WaitFrameAsync(int frame, ETCancellationToken cancelToken = null)
        {
            var item = CreateFrameAwait(frame);
            item.CancelToken = cancelToken;
            await item.tcs;
        }

        private AwaitItem CreateFrameAwait(int frame)
        {
            AwaitItem item = awaitPools.GetItem();
            item.timeType = TimeType.Frame;
            item.CreatFrame = Time.frameCount;
            item.TargetFrame = Time.frameCount + frame;
            _frameAwaits[item.TargetFrame] = item;
            return item;
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="path">资源路径</param>
        /// <param name="cancelToken">取消令牌</param>
        /// <returns>加载的资源</returns>
        public async ETTask<T> ResourcesLoadAsync<T>(string path, ETCancellationToken cancelToken = null)
            where T : Object
        {
            ResourceRequest request = Resources.LoadAsync<T>(path);
            while (!request.isDone)
            {
                if (cancelToken != null && cancelToken.IsCancel())
                {
                    throw new OperationCanceledException();
                }

                await WaitFrameAsync(1, cancelToken);
            }

            return request.asset as T;
        }

        /// <summary>
        /// 表示一个等待项
        /// </summary>
        private class AwaitItem
        {
            public ETTask tcs = ETTask.Create(true);
            public float CreatTime = 0;
            public float TargetTime = 0;
            public int TargetFrame = 0;
            public float CreatFrame = 0;
            public TimeType timeType = TimeType.Normal;
            public ETCancellationToken CancelToken;
        }

        /// <summary>
        /// 时间类型枚举
        /// </summary>
        public enum TimeType
        {
            Normal = 0,
            Unscaled = 1,
            Frame = 2
        }
    }
}