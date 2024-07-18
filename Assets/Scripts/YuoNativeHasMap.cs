using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Jobs;

namespace Manager.Tool
{
    public struct YuoNativeHasMap<TKey, TValue> : INativeDisposable, IEnumerable<KVPair<TKey, TValue>>
        where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
    {
        private int _capacity;
        private NativeHashMap<TKey, TValue> _map;
        public int Capacity => _map.Capacity;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public YuoNativeHasMap(int capacity)
        {
            _capacity = capacity;
            _map = new NativeHashMap<TKey, TValue>(capacity, Allocator.Persistent);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(TKey key, out TValue value) => _map.TryGetValue(key, out value);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(TKey key) => _map.ContainsKey(key);
        
        /// <summary>
        /// NEW 了新兑现，需要注意释放 
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeList<TValue> Values(out NativeList<TValue> values)
        {
            values = new NativeList<TValue>(Allocator.Persistent); // 创建NativeArray用于存储值  
            // 使用foreach遍历所有的KeyValuePair，然后将值存入我们刚刚创建的NativeArray中
            foreach (var keyValuePair in _map)
            {
                values.Add(keyValuePair.Value); 
            }
            return values;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(TKey key)
        {
            if (_map.ContainsKey(key))
            {
                _map.Remove(key);
                _capacity--;
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetValue(TKey key, TValue value)
        {
            if (_map.ContainsKey(key))
            {
                _map[key] = value;
            }
            else
            {
                _capacity++;
                if (_capacity > _map.Capacity)
                {
                    Resize(_map.Capacity * 2);
                }

                _map.Add(key, value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Resize(int capacity)
        {
            if (_map.Capacity > capacity) throw new Exception("不允许缩小容量");
            var newMap = new NativeHashMap<TKey, TValue>(capacity, Allocator.Persistent);

            foreach (var item in _map)
            {
                newMap.Add(item.Key, item.Value);
            }

            _map.Dispose();
            _map = newMap;
        }

        public TValue this[TKey key]
        {
            get => _map[key];
            set => SetValue(key, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _map.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JobHandle Dispose(JobHandle inputDeps)
        {
            return _map.Dispose(inputDeps);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerator<KVPair<TKey, TValue>> GetEnumerator()
        {
            return _map.GetEnumerator();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}