using System;
using System.Collections.Generic;
using ET;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace YuoTools
{
    /// <summary>
    /// 提供各种等待和异步操作的静态工具类
    /// </summary>
    public static class YuoWait
    {
        #region 协程相关方法
        
        // // 存储WaitForSeconds对象的字典
        // private static Dictionary<int, WaitForSeconds> waits = new Dictionary<int, WaitForSeconds>();
        //
        // // 存储WaitForSecondsRealtime对象的字典
        // private static Dictionary<int, WaitForSecondsRealtime> waitsRealtime = new Dictionary<int, WaitForSecondsRealtime>();
        //
        // /// <summary>
        // /// 返回一个WaitForSeconds对象
        // /// </summary>
        // /// <param name="time">等待时间（秒）</param>
        // /// <returns>WaitForSeconds对象</returns>
        // public static WaitForSeconds WaitTime(float time = 1)
        // {
        //     int key = (int)(time * 1000);
        //     if (!waits.TryGetValue(key, out var wait))
        //     {
        //         wait = new WaitForSeconds(time);
        //         waits[key] = wait;
        //     }
        //     return wait;
        // }
        //
        // /// <summary>
        // /// 返回一个不受TimeScale影响的WaitForSecondsRealtime对象
        // /// </summary>
        // /// <param name="time">等待时间（秒）</param>
        // /// <returns>WaitForSecondsRealtime对象</returns>
        // public static WaitForSecondsRealtime WaitUnscaledTime(float time = 1)
        // {
        //     int key = (int)(time * 1000);
        //     if (!waitsRealtime.TryGetValue(key, out var wait))
        //     {
        //         wait = new WaitForSecondsRealtime(time);
        //         waitsRealtime[key] = wait;
        //     }
        //     return wait;
        // }

        #endregion

        #region 异步任务相关方法

        /// <summary>
        /// 异步等待指定时间（受TimeScale影响）
        /// </summary>
        /// <param name="waitTime">等待时间（秒）</param>
        /// <param name="cancelToken">取消令牌</param>
        /// <returns>ETTask</returns>
        public static async ETTask WaitTimeAsync(float waitTime, ETCancellationToken cancelToken = null) 
            => await YuoAwait_Mono.Instance.WaitTimeAsync(waitTime, cancelToken);

        /// <summary>
        /// 异步等待指定帧数
        /// </summary>
        /// <param name="frame">等待的帧数</param>
        /// <param name="cancelToken">取消令牌</param>
        /// <returns>ETTask</returns>
        public static async ETTask WaitFrameAsync(int frame = 1, ETCancellationToken cancelToken = null) 
            => await YuoAwait_Mono.Instance.WaitFrameAsync(frame, cancelToken);

        /// <summary>
        /// 异步等待指定时间（不受TimeScale影响）
        /// </summary>
        /// <param name="waitTime">等待时间（秒）</param>
        /// <param name="cancelToken">取消令牌</param>
        /// <returns>ETTask</returns>
        public static async ETTask WaitUnscaledTimeAsync(float waitTime, ETCancellationToken cancelToken = null) 
            => await YuoAwait_Mono.Instance.WaitUnscaledTimeAsync(waitTime, cancelToken);

        /// <summary>
        /// 在指定时间内每帧执行操作
        /// </summary>
        /// <param name="time">持续时间（秒）</param>
        /// <param name="action">每帧执行的操作</param>
        /// <param name="cancelToken">取消令牌</param>
        /// <returns>ETTask</returns>
        public static async ETTask InvokeEveryTimeAsync(float time, UnityAction action, ETCancellationToken cancelToken = null)
        {
            float startTime = Time.time;
            while (Time.time - startTime < time)
            {
                if (cancelToken != null && cancelToken.IsCancel())
                {
                    throw new OperationCanceledException();
                }
                action?.Invoke();
                await WaitFrameAsync(1, cancelToken);
            }
        }

        /// <summary>
        /// 在指定时间内每帧执行操作（不受TimeScale影响）
        /// </summary>
        /// <param name="time">持续时间（秒）</param>
        /// <param name="action">每帧执行的操作</param>
        /// <param name="cancelToken">取消令牌</param>
        /// <returns>ETTask</returns>
        public static async ETTask InvokeEveryUnscaledTimeAsync(float time, UnityAction action, ETCancellationToken cancelToken = null)
        {
            float startTime = Time.unscaledTime;
            while (Time.unscaledTime - startTime < time)
            {
                if (cancelToken != null && cancelToken.IsCancel())
                {
                    throw new OperationCanceledException();
                }
                action?.Invoke();
                await WaitFrameAsync(1, cancelToken);
            }
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="path">资源路径</param>
        /// <param name="cancelToken">取消令牌</param>
        /// <returns>加载的资源</returns>
        public static async ETTask<T> ResourcesLoadAsync<T>(string path, ETCancellationToken cancelToken = null) where T : Object
            => await YuoAwait_Mono.Instance.ResourcesLoadAsync<T>(path, cancelToken);

        #endregion

        #region 输入等待方法

        /// <summary>
        /// 等待指定按键被按下
        /// </summary>
        /// <param name="key">按键</param>
        /// <param name="cancelToken">取消令牌</param>
        /// <returns>ETTask</returns>
        public static async ETTask WaitInputKeyDownAsync(KeyCode key, ETCancellationToken cancelToken = null)
        {
            while (!Input.GetKeyDown(key))
            {
                if (cancelToken != null && cancelToken.IsCancel())
                {
                    throw new OperationCanceledException();
                }
                await WaitFrameAsync(1, cancelToken);
            }
        }

        /// <summary>
        /// 等待任意按键被按下
        /// </summary>
        /// <param name="cancelToken">取消令牌</param>
        /// <returns>ETTask</returns>
        public static async ETTask WaitInputAnyKeyDownAsync(ETCancellationToken cancelToken = null)
        {
            while (!Input.anyKeyDown)
            {
                if (cancelToken != null && cancelToken.IsCancel())
                {
                    throw new OperationCanceledException();
                }
                await WaitFrameAsync(1, cancelToken);
            }
        }

        /// <summary>
        /// 等待鼠标按键被按下
        /// </summary>
        /// <param name="mouseButton">鼠标按键</param>
        /// <param name="cancelToken">取消令牌</param>
        /// <returns>ETTask</returns>
        public static async ETTask WaitInputMouseDownAsync(int mouseButton, ETCancellationToken cancelToken = null)
        {
            while (!Input.GetMouseButtonDown(mouseButton))
            {
                if (cancelToken != null && cancelToken.IsCancel())
                {
                    throw new OperationCanceledException();
                }
                await WaitFrameAsync(1, cancelToken);
            }
        }

        /// <summary>
        /// 等待指定按键被按住
        /// </summary>
        /// <param name="key">按键</param>
        /// <param name="cancelToken">取消令牌</param>
        /// <returns>ETTask</returns>
        public static async ETTask WaitInputKeyAsync(KeyCode key, ETCancellationToken cancelToken = null)
        {
            while (!Input.GetKey(key))
            {
                if (cancelToken != null && cancelToken.IsCancel())
                {
                    throw new OperationCanceledException();
                }
                await WaitFrameAsync(1, cancelToken);
            }
        }

        #endregion

        #region 条件等待方法

        /// <summary>
        /// 等待直到条件满足
        /// </summary>
        /// <param name="condition">条件函数</param>
        /// <param name="cancelToken">取消令牌</param>
        /// <returns>ETTask</returns>
        public static async ETTask WaitUntilAsync(Func<bool> condition, ETCancellationToken cancelToken = null)
        {
            while (!condition())
            {
                if (cancelToken != null && cancelToken.IsCancel())
                {
                    throw new OperationCanceledException();
                }
                await WaitFrameAsync(1, cancelToken);
            }
        }

        #endregion
    }
}