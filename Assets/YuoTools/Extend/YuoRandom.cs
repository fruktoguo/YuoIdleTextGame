using System;
using System.Collections.Generic;
using UnityEngine;
using YuoTools.Main.Ecs;
using Random = System.Random;

namespace YuoTools.Main.Ecs
{
    public static class RandomHelper
    {
        private static readonly Random random = new Random(Guid.NewGuid().GetHashCode());

        private static readonly byte[] byte8 = new byte[8];

        public static ulong RandUInt64()
        {
            random.NextBytes(byte8);
            return BitConverter.ToUInt64(byte8, 0);
        }

        public static int RandInt32()
        {
            return random.Next();
        }

        public static uint RandUInt32()
        {
            return (uint)random.Next();
        }

        public static long RandInt64()
        {
            random.NextBytes(byte8);
            return BitConverter.ToInt64(byte8, 0);
        }

        /// <summary>
        /// 获取lower与Upper之间的随机数,包含下限，不包含上限
        /// </summary>
        /// <param name="lower"></param>
        /// <param name="upper"></param>
        /// <returns></returns>
        public static int RandomNumber(int lower, int upper)
        {
            int value = random.Next(lower, upper);
            return value;
        }

        public static long NextLong(long minValue, long maxValue)
        {
            if (minValue > maxValue)
            {
                throw new ArgumentException("minValue is great than maxValue", nameof(minValue));
            }

            long num = maxValue - minValue;
            return minValue + (long)(random.NextDouble() * num);
        }

        public static bool RandomBool()
        {
            return random.Next(2) == 0;
        }

        public static T RandomArray<T>(this T[] array)
        {
            return array[RandomNumber(0, array.Length)];
        }

        public static T RandomArray<T>(this List<T> array)
        {
            return array[RandomNumber(0, array.Count)];
        }

        /// <summary>
        /// 打乱数组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr">要打乱的数组</param>
        public static void BreakRank<T>(this List<T> arr)
        {
            if (arr == null || arr.Count < 2)
            {
                return;
            }

            for (int i = 0; i < arr.Count; i++)
            {
                int index = random.Next(0, arr.Count);
                (arr[index], arr[i]) = (arr[i], arr[index]);
            }
        }

        /// <summary>
        ///  random float between 0 and 1
        /// </summary>
        /// <returns></returns>
        public static float RandFloat01()
        {
            int a = RandomNumber(0, 1000000);
            return a / 1000000f;
        }
        
        /// <summary>
        ///  random float between -1 and 1
        /// </summary>
        /// <returns></returns>
        public static float RandFloat11()
        {
            int a = RandomNumber(0, 1000000);
            return a / 500000f - 1;
        }
    }

    public class YuoRandom : YuoComponentGet<YuoRandom>
    {
        private static YuoRandom _instance;
        Random random;
        [SerializeField] private int index;

        public static int Range(int min, int max)
        {
            _instance.index++;
            return _instance.random.Next(min, max);
        }

        public void Init()
        {
            _instance = this;
            random = new Random();
        }

        public void OnLoad()
        {
            for (int i = 0; i < index; i++)
            {
                _instance.random.Next();
            }
        }
    }

    public class YuoRandomSystem : YuoSystem<YuoRandom>, IAwake, IOnLoad
    {
        public override string Group => SystemGroupConst.Main;

        public override void Run(YuoRandom component)
        {
            if (RunType == SystemTagType.Awake)
            {
                component.Init();
            }
            else if (RunType == typeof(IOnLoad))
            {
                component.OnLoad();
            }
        }
    }
}