using System;
using UnityEngine;

namespace YuoTools.Extend.Helper
{
    public static class MathHelper
    {
        /// <summary>
        ///  弧度转角度
        /// </summary>
        /// <param name="radians"></param>
        /// <returns></returns>
        public static float RadToDeg(float radians)
        {
            return (float)(radians * 180 / Math.PI);
        }

        /// <summary>
        /// 角度转弧度
        /// </summary>
        /// <param name="degrees"></param>
        /// <returns></returns>
        public static float DegToRad(float degrees)
        {
            return (float)(degrees * Math.PI / 180);
        }

        /// <summary>
        ///  勾股定理
        /// </summary>
        public static float PythagoreanTheorem(float a, float b)
        {
            return (float)Math.Sqrt(a * a + b * b);
        }

        /// <summary>
        ///  平方差公式
        /// </summary>
        public static float SquareDifference(float a, float b)
        {
            return (float)Math.Sqrt(Math.Pow(a, 2) - Math.Pow(b, 2));
        }

        /// <summary>
        /// 阶乘
        /// </summary>
        public static long Factorial(int a, int b = 1)
        {
            int sum = 1;
            for (; a > b; a--)
            {
                sum *= a;
            }

            return sum;
        }

        /// <summary>
        /// 巴比伦法求平方根
        /// </summary>
        public static double Sqrt(double number, double guess = 1.0)
        {
            double precision = 0.000001;
            while (Math.Abs(number - guess * guess) > precision)
            {
                guess = (guess + number / guess) / 2;
            }

            return guess;
        }

        /// <summary>
        /// 避免除0 异常
        /// </summary>
        public static float Division(float a, float b)
        {
            if (b == 0)
            {
                return float.MaxValue;
            }

            return a / b;
        }
    }
}