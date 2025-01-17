using System.Collections.Generic;
using UnityEngine;

namespace YuoTools
{
    public static class YuoGameObjectExtension
    {
        public static GameObject Show(this GameObject gameObject)
        {
            if (!gameObject)
            {
                Debug.LogError("物体不存在");
                return gameObject;
            }

            if (!gameObject.activeSelf) gameObject.SetActive(true);
            return gameObject;
        }

        public static GameObject Hide(this GameObject gameObject)
        {
            if (!gameObject)
            {
                Debug.LogError("物体不存在");
                return gameObject;
            }

            if (gameObject.activeSelf) gameObject.SetActive(false);
            return gameObject;
        }

        public static T Hide<T>(this T gameObject) where T : Component
        {
            gameObject.gameObject.Hide();
            return gameObject;
        }

        public static IEnumerable<T> HideAll<T>(this IEnumerable<T> collection) where T : Component
        {
            foreach (var item in collection)
            {
                item.gameObject.Hide();
            }

            return collection;
        }

        public static IEnumerable<GameObject> HideAll(this IEnumerable<GameObject> collection)
        {
            foreach (var item in collection)
            {
                item.Hide();
            }

            return collection;
        }

        public static T Show<T>(this T component) where T : Component
        {
            component.gameObject.Show();
            return component;
        }
        
        public static T FlipActiveState<T>(this T component) where T : Component
        {
            component.gameObject.SetActive(!component.gameObject.activeSelf);
            return component;
        }

        public static IEnumerable<T> ShowAll<T>(this IEnumerable<T> collection) where T : Component
        {
            foreach (var item in collection)
            {
                item.Show();
            }

            return collection;
        }

        public static IEnumerable<GameObject> ShowAll(this IEnumerable<GameObject> collection)
        {
            foreach (var item in collection)
            {
                item.Show();
            }

            return collection;
        }

        public static IEnumerable<T> EnableAll<T>(this IEnumerable<T> collection) where T : Behaviour
        {
            foreach (var item in collection)
            {
                item.enabled = true;
            }

            return collection;
        }

        public static IEnumerable<T> DisableAll<T>(this IEnumerable<T> collection) where T : Behaviour
        {
            foreach (var item in collection)
            {
                item.enabled = false;
            }

            return collection;
        }

        public static void DestroyAll<T>(this IEnumerable<T> collection) where T : Behaviour
        {
            foreach (var item in collection)
            {
                Object.Destroy(item);
            }
        }

        public static void DestroyAll(this IEnumerable<GameObject> collection)
        {
            foreach (var item in collection)
            {
                Object.Destroy(item);
            }
        }

        public static void DestroyAllObject<T>(this IEnumerable<T> collection) where T : Behaviour
        {
            foreach (var item in collection)
            {
                Object.Destroy(item.gameObject);
            }
        }

        public static GameObject ReShow(this GameObject gameObject)
        {
            if (!gameObject)
            {
                Debug.LogError("物体不存在");
                return gameObject;
            }

            gameObject.SetActive(false);
            gameObject.SetActive(true);
            return gameObject;
        }
        
        public static GameObject FlipActiveState(this GameObject gameObject)
        {
            gameObject.SetActive(!gameObject.activeSelf);
            return gameObject;
        }

        public static List<Transform> GetChildren(this Transform transform)
        {
            List<Transform> list = new List<Transform>();
            for (int i = 0; i < transform.childCount; i++)
            {
                list.Add(transform.GetChild(i));
            }

            return list;
        }

        public static bool TryDestroy(this Component component)
        {
            if (component == null)
            {
                return false;
            }

            Object.Destroy(component);
            return true;
        }

        public static GameObject Instantiate(this GameObject gameObject, Transform parent)
        {
            return Object.Instantiate(gameObject, parent);
        }

        public static GameObject Instantiate(this GameObject gameObject, Transform parent, bool worldPositionStays)
        {
            return Object.Instantiate(gameObject, parent, worldPositionStays);
        }

        public static GameObject Instantiate(this GameObject gameObject, Vector3 position, Quaternion rotation)
        {
            return Object.Instantiate(gameObject, position, rotation);
        }
        
        public static void Destroy(this GameObject gameObject)
        {
            Object.Destroy(gameObject);
        }
    }
}