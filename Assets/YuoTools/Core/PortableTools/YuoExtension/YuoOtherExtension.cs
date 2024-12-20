using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace YuoTools
{
    public static class YuoOtherExtension
    {
        #region Color

        public static Color UpdateColorFade(this Color color, float fade)
        {
            fade = Mathf.Clamp(fade, 0, 1);
            color = new Color(color.r, color.g, color.b, fade);
            return color;
        }

        public static Color AddColorFade(this Color color, float fade)
        {
            float temp = fade + color.a;
            temp = Mathf.Clamp(temp, 0, 1);
            color = new Color(color.r, color.g, color.b, temp);
            return color;
        }

        public static Color Set(this ref Color color, float r, float g, float b, float a)
        {
            color.r = r;
            color.g = g;
            color.b = b;
            color.a = a;
            return color;
        }

        public static Color SetR(this ref Color color, float r)
        {
            color.r = r;
            return color;
        }

        public static Color SetG(this ref Color color, float g)
        {
            color.g = g;
            return color;
        }

        public static Color SetB(this ref Color color, float b)
        {
            color.b = b;
            return color;
        }

        public static Color RSetR(this Color color, float r)
        {
            color.r = r;
            return color;
        }

        public static Color RSetG(this Color color, float g)
        {
            color.g = g;
            return color;
        }

        public static Color RSetB(this Color color, float b)
        {
            color.b = b;
            return color;
        }

        public static Color RSetA(this Color color, float a)
        {
            color.a = a;
            return color;
        }

        public static void SetColorR(this Graphic image, float r)
        {
            image.color = image.color.RSetR(r);
        }

        public static void SetColorG(this Graphic image, float g)
        {
            image.color = image.color.RSetG(g);
        }

        public static void SetColorB(this Graphic image, float b)
        {
            image.color = image.color.RSetB(b);
        }

        public static void SetColorA(this Graphic image, float a)
        {
            image.color = image.color.RSetA(a);
        }

        public static void SetColorR(this SpriteRenderer renderer, float r)
        {
            renderer.color = renderer.color.RSetR(r);
        }

        public static void SetColorG(this SpriteRenderer renderer, float g)
        {
            renderer.color = renderer.color.RSetG(g);
        }

        public static void SetColorB(this SpriteRenderer renderer, float b)
        {
            renderer.color = renderer.color.RSetB(b);
        }

        public static void SetColorA(this SpriteRenderer renderer, float a)
        {
            renderer.color = renderer.color.RSetA(a);
        }

        #endregion Color

        #region Animator

        public static float GetClipLength(this Animator animator, string clip)
        {
            if (null == animator || string.IsNullOrEmpty(clip) || null == animator.runtimeAnimatorController)
                return 0;
            RuntimeAnimatorController ac = animator.runtimeAnimatorController;
            AnimationClip[] tAnimationClips = ac.animationClips;
            if (null == tAnimationClips || tAnimationClips.Length <= 0) return 0;
            AnimationClip tAnimationClip;
            for (int tCounter = 0, tLen = tAnimationClips.Length; tCounter < tLen; tCounter++)
            {
                tAnimationClip = ac.animationClips[tCounter];
                if (null != tAnimationClip && tAnimationClip.name == clip)
                    return tAnimationClip.length;
            }

            return 0F;
        }

        #endregion Animator

        #region Enum

        // public static System.Array GetAll<T>(this T _enum) where T : System.Enum
        // {
        //     return System.Enum.GetValues(typeof(T));
        // }

        public static T[] GetAll<T>(this T @enum) where T : System.Enum
        {
            var array = System.Enum.GetValues(typeof(T));
            T[] newArray = new T[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                newArray[i] = (T)array.GetValue(i);
            }

            return newArray;
        }

        #endregion Enum

        #region Other

        public static void Adds<T>(this List<T> list, params T[] t)
        {
            list.AddRange(t);
        }

        public static void DisposeAll(this ICollection<IDisposable> array)
        {
            foreach (var t in array)
            {
                t.Dispose();
            }
        }

        #endregion
    }
}