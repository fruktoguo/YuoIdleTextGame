using System.Collections.Generic;
using UnityEngine;

namespace YuoTools
{
    public static class YuoTransformExtension
    {
        public static T GetOrAddComponent<T>(this Transform transform) where T : Component
        {
            T component = transform.GetComponent<T>();
            if (component == null)
            {
                component = transform.gameObject.AddComponent<T>();
            }

            return component;
        }

        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            return GetOrAddComponent<T>(gameObject.transform);
        }

        public static void ResetTrans(this Transform tran)
        {
            tran.localPosition = Vector3.zero;
            tran.localEulerAngles = Vector3.zero;
            tran.localScale = Vector3.one;
        }

        public static void ResetTrans(this GameObject gameObject)
        {
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localEulerAngles = Vector3.zero;
            gameObject.transform.localScale = Vector3.one;
        }

        public static Transform Show(this Transform tran)
        {
            tran.gameObject.SetActive(true);
            return tran;
        }

        public static Transform Hide(this Transform tran)
        {
            tran.gameObject.SetActive(false);
            return tran;
        }

        #region Find

        /// <summary>
        /// 递归寻找所有子物体(包括孙子物体等)中符合名字的物体
        /// </summary>
        /// <param name="transform">要搜索的父物体</param>
        /// <param name="name">要查找的物体名称</param>
        /// <returns>找到的Transform，如果没找到返回null</returns>
        public static Transform FindAnyChild(this Transform transform, string name)
        {
            // 检查直接子物体
            foreach (Transform child in transform)
            {
                // 如果当前子物体名字匹配，直接返回
                if (child.name == name)
                {
                    return child;
                }

                // 递归搜索子物体的子物体
                Transform result = child.FindAnyChild(name);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        /// <summary>
        /// 在直接子物体中查找名称包含指定字符串的物体(返回第一个匹配的)
        /// </summary>
        /// <param name="transform">要搜索的父物体</param>
        /// <param name="containsName">名称中需要包含的字符串</param>
        /// <returns>找到的第一个Transform，如果没找到返回null</returns>
        public static Transform FindChildContains(this Transform transform, string containsName)
        {
            foreach (Transform child in transform)
            {
                if (child.name.Contains(containsName))
                {
                    return child;
                }
            }

            return null;
        }

        /// <summary>
        /// 递归查找所有子物体中名称包含指定字符串的物体(返回第一个匹配的)
        /// </summary>
        /// <param name="transform">要搜索的父物体</param>
        /// <param name="containsName">名称中需要包含的字符串</param>
        /// <returns>找到的第一个Transform，如果没找到返回null</returns>
        public static Transform FindAnyChildContains(this Transform transform, string containsName)
        {
            foreach (Transform child in transform)
            {
                if (child.name.Contains(containsName))
                {
                    return child;
                }

                Transform result = child.FindAnyChildContains(containsName);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        /// <summary>
        /// 在直接子物体中查找所有名称包含指定字符串的物体
        /// </summary>
        /// <param name="transform">要搜索的父物体</param>
        /// <param name="containsName">名称中需要包含的字符串</param>
        /// <returns>找到的所有Transform列表</returns>
        public static List<Transform> FindChildrenContains(this Transform transform, string containsName)
        {
            List<Transform> results = new List<Transform>();
            foreach (Transform child in transform)
            {
                if (child.name.Contains(containsName))
                {
                    results.Add(child);
                }
            }

            return results;
        }

        /// <summary>
        /// 递归查找所有子物体中名称包含指定字符串的所有物体
        /// </summary>
        /// <param name="transform">要搜索的父物体</param>
        /// <param name="containsName">名称中需要包含的字符串</param>
        /// <returns>找到的所有Transform列表</returns>
        public static List<Transform> FindAnyChildrenContains(this Transform transform, string containsName)
        {
            List<Transform> results = new List<Transform>();
            foreach (Transform child in transform)
            {
                if (child.name.Contains(containsName))
                {
                    results.Add(child);
                }

                results.AddRange(child.FindAnyChildrenContains(containsName));
            }

            return results;
        }

        /// <summary>
        /// 获取第一个直接子物体
        /// </summary>
        /// <param name="transform">要获取子物体的Transform</param>
        /// <returns>第一个子物体，如果没有则返回null</returns>
        public static Transform GetChild(this Transform transform)
        {
            return transform.childCount > 0 ? transform.GetChild(0) : null;
        }

        /// <summary>
        /// 获取所有直接子物体
        /// </summary>
        /// <param name="transform">要获取子物体的Transform</param>
        /// <returns>所有直接子物体列表</returns>
        public static List<Transform> GetChildren(this Transform transform)
        {
            List<Transform> children = new List<Transform>();
            foreach (Transform child in transform)
            {
                children.Add(child);
            }

            return children;
        }

        /// <summary>
        /// 递归获取所有子物体（包括孙子物体等）
        /// </summary>
        /// <param name="transform">要获取子物体的Transform</param>
        /// <returns>所有子物体列表</returns>
        public static List<Transform> GetAllChildren(this Transform transform)
        {
            List<Transform> children = new List<Transform>();
            foreach (Transform child in transform)
            {
                children.Add(child);
                children.AddRange(child.GetAllChildren());
            }

            return children;
        }

        #endregion

        #region Find Regex

        /// <summary>
        /// 在直接子物体中使用正则表达式查找匹配的物体
        /// </summary>
        /// <param name="transform">要搜索的父物体</param>
        /// <param name="pattern">正则表达式模式</param>
        /// <returns>找到的第一个Transform，如果没找到返回null</returns>
        public static Transform FindChildRegex(this Transform transform, string pattern)
        {
            var regex = new System.Text.RegularExpressions.Regex(pattern);
            foreach (Transform child in transform)
            {
                if (regex.IsMatch(child.name))
                {
                    return child;
                }
            }

            return null;
        }

        /// <summary>
        /// 递归查找所有子物体中使用正则表达式匹配的物体
        /// </summary>
        /// <param name="transform">要搜索的父物体</param>
        /// <param name="pattern">正则表达式模式</param>
        /// <returns>找到的第一个Transform，如果没找到返回null</returns>
        public static Transform FindAnyChildRegex(this Transform transform, string pattern)
        {
            var regex = new System.Text.RegularExpressions.Regex(pattern);
            foreach (Transform child in transform)
            {
                if (regex.IsMatch(child.name))
                {
                    return child;
                }

                Transform result = child.FindAnyChildRegex(pattern);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        /// <summary>
        /// 在直接子物体中使用正则表达式查找所有匹配的物体
        /// </summary>
        /// <param name="transform">要搜索的父物体</param>
        /// <param name="pattern">正则表达式模式</param>
        /// <returns>找到的所有Transform列表</returns>
        public static List<Transform> FindChildrenRegex(this Transform transform, string pattern)
        {
            var regex = new System.Text.RegularExpressions.Regex(pattern);
            List<Transform> results = new List<Transform>();
            foreach (Transform child in transform)
            {
                if (regex.IsMatch(child.name))
                {
                    results.Add(child);
                }
            }

            return results;
        }

        /// <summary>
        /// 递归查找所有子物体中使用正则表达式匹配的所有物体
        /// </summary>
        /// <param name="transform">要搜索的父物体</param>
        /// <param name="pattern">正则表达式模式</param>
        /// <returns>找到的所有Transform列表</returns>
        public static List<Transform> FindAnyChildrenRegex(this Transform transform, string pattern)
        {
            var regex = new System.Text.RegularExpressions.Regex(pattern);
            List<Transform> results = new List<Transform>();
            foreach (Transform child in transform)
            {
                if (regex.IsMatch(child.name))
                {
                    results.Add(child);
                }

                results.AddRange(child.FindAnyChildrenRegex(pattern));
            }

            return results;
        }

        #endregion

        public static Vector2 GetSquarePointByAngle_Round(this Rect rect, float angle)
        {
            angle = angle.Residual(360);
            float x = rect.width / 2;
            float y = rect.height / 2;
            float widthRatio = x > y ? 1 : x / y;
            float heightRatio = y > x ? 1 : y / x;

            float radius = x > y ? x : y;
            //总共有八个区域
            if (angle is >= 0 and < 45)
            {
                y = radius * Mathf.Tan(angle * Mathf.Deg2Rad);
                y *= heightRatio;
            }
            else if (angle is >= 45 and < 90)
            {
                angle = 90 - angle;
                x = radius * Mathf.Tan(angle * Mathf.Deg2Rad);
                x *= widthRatio;
            }
            else if (angle is >= 90 and < 135)
            {
                angle = angle - 90;
                x = -radius * Mathf.Tan(angle * Mathf.Deg2Rad);
                x *= widthRatio;
            }
            else if (angle is >= 135 and < 180)
            {
                angle = (180 - angle);
                y = radius * Mathf.Tan(angle * Mathf.Deg2Rad);
                x = -x;
                y *= heightRatio;
            }
            else if (angle is >= 180 and < 225)
            {
                angle = angle - 180;
                y = -radius * Mathf.Tan(angle * Mathf.Deg2Rad);
                x = -x;
                y *= heightRatio;
            }
            else if (angle is >= 225 and < 270)
            {
                angle = 270 - angle;
                x = -radius * Mathf.Tan(angle * Mathf.Deg2Rad);
                y = -y;
                x *= widthRatio;
            }
            else if (angle is >= 270 and < 315)
            {
                angle = angle - 270;
                x = radius * Mathf.Tan(angle * Mathf.Deg2Rad);
                y = -y;
                x *= widthRatio;
            }
            else if (angle is >= 315 and < 360)
            {
                angle = 360 - angle;
                y = -radius * Mathf.Tan(angle * Mathf.Deg2Rad);
                y *= heightRatio;
            }


            return new Vector2(x, y);
        }

        public static Vector2 GetSquarePointByAngle_Uniform(this Rect rect, float ratio)
        {
            ratio = ratio.Residual(1);

            float allLength = rect.width * 2 + rect.height * 2;
            float widthRatio = rect.width / allLength;
            float heightRatio = rect.height / allLength;

            float x = rect.width / 2;
            float y = rect.height / 2;

            if (ratio < widthRatio)
            {
                x = (ratio - widthRatio / 2) / widthRatio * rect.width;
            }
            else if (ratio < widthRatio + heightRatio)
            {
                y = (-ratio + widthRatio + heightRatio / 2) / heightRatio * rect.height;
            }
            else if (ratio < widthRatio * 2 + heightRatio)
            {
                x = (-ratio + widthRatio * 1.5f + heightRatio) / widthRatio * rect.width;
                y = -y;
            }
            else
            {
                y = (ratio - widthRatio * 2 - heightRatio * 1.5f) / heightRatio * rect.height;
                x = -x;
            }

            Temp.V2.Set(x, y);
            return Temp.V2;
        }

        public static void SetPos(this RectTransform tran, float x, float y)
        {
            Temp.V2.Set(x, y);
            tran.anchoredPosition = Temp.V2;
        }

        public static void SetPos(this Transform tran, float x, float y, float z)
        {
            Temp.V3.Set(x, y, z);
            tran.position = Temp.V3;
        }

        public static Vector3 RSet(this Vector3 pos, float x, float y, float z)
        {
            pos.x = x;
            pos.y = y;
            pos.z = z;
            return pos;
        }

        public static Vector2 RSet(this Vector2 pos, float x, float y)
        {
            pos.x = x;
            pos.y = y;
            return pos;
        }

        public static Vector3 SetX(this ref Vector3 v3, float x)
        {
            Temp.V3.Set(x, v3.y, v3.z);
            v3 = Temp.V3;
            return v3;
        }

        public static Vector3 SetY(this ref Vector3 v3, float y)
        {
            Temp.V3.Set(v3.x, y, v3.z);
            v3 = Temp.V3;
            return v3;
        }

        public static Vector3 SetZ(this ref Vector3 v3, float z)
        {
            Temp.V3.Set(v3.x, v3.y, z);
            v3 = Temp.V3;
            return v3;
        }

        public static Vector2 SetX(this ref Vector2 v2, float x)
        {
            Temp.V2.Set(x, v2.y);
            v2 = Temp.V2;
            return v2;
        }

        public static Vector2 SetY(this ref Vector2 v2, float y)
        {
            Temp.V2.Set(v2.x, y);
            v2 = Temp.V2;
            return v2;
        }

        public static Vector2 RSetX(this Vector2 v2, float x)
        {
            Temp.V2.Set(x, v2.y);
            v2 = Temp.V2;
            return v2;
        }

        public static Vector2 RSetY(this Vector2 v2, float y)
        {
            Temp.V2.Set(v2.x, y);
            v2 = Temp.V2;
            return v2;
        }

        public static Vector2 AddX(this ref Vector2 v2, float x)
        {
            return v2.SetX(v2.x + x);
        }

        public static Vector2 AddY(this ref Vector2 v2, float y)
        {
            return v2.SetY(v2.y + y);
        }

        public static Vector2 RAddX(this Vector2 v2, float x)
        {
            return v2.SetX(v2.x + x);
        }

        public static Vector2 RAddY(this Vector2 v2, float y)
        {
            return v2.SetY(v2.y + y);
        }

        public static Vector3 AddX(this ref Vector3 v3, float x)
        {
            return v3.SetX(v3.x + x);
        }

        public static Vector3 AddY(this ref Vector3 v3, float y)
        {
            return v3.SetY(v3.y + y);
        }

        public static Vector3 AddZ(this ref Vector3 v3, float z)
        {
            return v3.SetZ(v3.z + z);
        }

        public static Vector3 RAddX(this Vector3 v3, float x)
        {
            return v3.SetX(v3.x + x);
        }

        public static Vector3 RAddY(this Vector3 v3, float y)
        {
            return v3.SetY(v3.y + y);
        }

        public static Vector3 RAddZ(this Vector3 v3, float z)
        {
            return v3.SetZ(v3.z + z);
        }

        public static Vector3 RSetX(this Vector3 v3, float x)
        {
            Temp.V3.Set(x, v3.y, v3.z);
            v3 = Temp.V3;
            return v3;
        }

        public static Vector3 RSetY(this Vector3 v3, float y)
        {
            Temp.V3.Set(v3.x, y, v3.z);
            v3 = Temp.V3;
            return v3;
        }

        public static Vector3 RSetZ(this Vector3 v3, float z)
        {
            Temp.V3.Set(v3.x, v3.y, z);
            v3 = Temp.V3;
            return v3;
        }

        public static Vector3 SetPos(this ref Vector3 v3, float x, float y, float z)
        {
            Temp.V3.Set(x, y, z);
            v3 = Temp.V3;
            return v3;
        }

        public static Vector3 SetPosX(this Transform tran, float posX)
        {
            Temp.V3.Set(posX, tran.position.y, tran.position.z);
            tran.position = Temp.V3;
            return tran.position;
        }

        public static Vector3 SetPosY(this Transform tran, float posY)
        {
            Temp.V3.Set(tran.position.x, posY, tran.position.z);
            tran.position = Temp.V3;
            return tran.position;
        }

        public static Vector3 SetPosZ(this Transform tran, float PosZ)
        {
            Temp.V3.Set(tran.position.x, tran.position.y, PosZ);
            tran.position = Temp.V3;
            return tran.position;
        }

        public static Vector3 SetLocalPosX(this Transform tran, float PosX)
        {
            Temp.V3.Set(PosX, tran.localPosition.y, tran.localPosition.z);
            tran.localPosition = Temp.V3;
            return tran.localPosition;
        }

        public static Vector3 SetLocalPosY(this Transform tran, float PosY)
        {
            Temp.V3.Set(tran.localPosition.x, PosY, tran.localPosition.z);
            tran.localPosition = Temp.V3;
            return tran.localPosition;
        }

        public static Vector3 SetLocalPosZ(this Transform tran, float PosZ)
        {
            Temp.V3.Set(tran.localPosition.x, tran.localPosition.y, PosZ);
            tran.localPosition = Temp.V3;
            return tran.localPosition;
        }

        public static Vector3 AddPosX(this Transform tran, float posX)
        {
            Temp.V3.Set(tran.position.x + posX, tran.position.y, tran.position.z);
            tran.position = Temp.V3;
            return tran.position;
        }

        public static Vector3 AddPosY(this Transform tran, float posY)
        {
            Temp.V3.Set(tran.position.x, tran.position.y + posY, tran.position.z);
            tran.position = Temp.V3;
            return tran.position;
        }

        public static Vector3 AddPosZ(this Transform tran, float posZ)
        {
            Temp.V3.Set(tran.position.x, tran.position.y, tran.position.z + posZ);
            tran.position = Temp.V3;
            return tran.position;
        }

        public static Vector3 AddLocalPosX(this Transform tran, float posX)
        {
            Temp.V3.Set(tran.localPosition.x + posX, tran.localPosition.y, tran.localPosition.z);
            tran.localPosition = Temp.V3;
            return tran.localPosition;
        }

        public static Vector3 AddLocalPosY(this Transform tran, float posY)
        {
            Temp.V3.Set(tran.localPosition.x, tran.localPosition.y + posY, tran.localPosition.z);
            tran.localPosition = Temp.V3;
            return tran.localPosition;
        }

        public static Vector3 AddLocalPosZ(this Transform tran, float posZ)
        {
            Temp.V3.Set(tran.localPosition.x, tran.localPosition.y, tran.localPosition.z + posZ);
            tran.localPosition = Temp.V3;
            return tran.localPosition;
        }

        public static bool InRange(this Vector2Int pos, Vector2Int zero, int MaxWidth, int MinWidth, int MaxHeight,
            int MinHeight)
        {
            if (pos.x >= zero.x + MinWidth && pos.x < zero.x + MaxWidth && pos.y >= zero.y + MinHeight &&
                pos.y < zero.y + MaxHeight / 2)
                return true;
            return false;
        }

        public static bool InRange(this Vector2Int pos, Vector2Int zero, int width, int height)
        {
            if ((pos.x >= zero.x - width / 2 && pos.x < zero.x + width / 2) &&
                (pos.y >= zero.y - height / 2 && pos.y < zero.y + height / 2))
                return true;
            return false;
        }
    }
}