using System;
using System.Collections.Generic;
using System.Linq;

namespace YuoTools
{
    public static class YuoCollectionsExtension
    {
        public static void RemoveLinq<T>(this ICollection<T> collection, Func<T, bool> predicate)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
    
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            // 针对List<T>的优化
            if (collection is List<T> list)
            {
                list.RemoveAll(x => predicate(x));
                return;
            }

            // 针对HashSet<T>的优化
            if (collection is HashSet<T> hashSet)
            {
                hashSet.RemoveWhere(x => predicate(x));
                return;
            }

            // 其他类型的集合使用原始方法
            var itemsToRemove = collection.Where(predicate).ToList();
            foreach (var item in itemsToRemove)
            {
                collection.Remove(item);
            }
        }
    }
}