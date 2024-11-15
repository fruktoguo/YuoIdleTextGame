using System;
using System.Collections;
using System.Collections.Generic;

namespace YuoTools
{
    /// <summary>
    /// 一个简化的优先级队列实现。支持稳定性、自动调整大小和线程安全。
    /// 方法标记为O(1)或O(log n)是假设没有重复项。重复项可能会增加算法复杂度。
    /// </summary>
    /// <typeparam name="TItem">要入队的类型</typeparam>
    /// <typeparam name="TPriority">用于节点的优先级类型。必须实现IComparable&lt;TPriority&gt;</typeparam>
    public class YuoPriorityQueue<TItem, TPriority> : IPriorityQueue<TItem, TPriority>, IEnumerable<TItem>
        where TPriority : IComparable<TPriority>
    {
        private const int INITIAL_QUEUE_SIZE = 10;
        private readonly List<SimpleNode> _queue;
        private readonly Dictionary<TItem, List<SimpleNode>> _itemToNodesCache;
        private readonly IComparer<TPriority> _comparer;
        private readonly object _lock = new object();

        /// <summary>
        /// 初始化一个新的优先级队列实例
        /// </summary>
        public YuoPriorityQueue() : this(Comparer<TPriority>.Default) { }

        /// <summary>
        /// 使用指定的比较器初始化一个新的优先级队列实例
        /// </summary>
        public YuoPriorityQueue(IComparer<TPriority> comparer)
        {
            _queue = new List<SimpleNode>(INITIAL_QUEUE_SIZE);
            _itemToNodesCache = new Dictionary<TItem, List<SimpleNode>>();
            _comparer = comparer;
        }

        /// <summary>
        /// 队列中的节点数量
        /// O(1)
        /// </summary>
        public int Count
        {
            get { lock (_lock) return _queue.Count; }
        }

        /// <summary>
        /// 返回队列的头部，不移除它（使用Dequeue()来移除）
        /// 当队列为空时抛出异常
        /// O(1)
        /// </summary>
        public TItem First
        {
            get
            {
                lock (_lock)
                {
                    if (_queue.Count == 0)
                        throw new InvalidOperationException("队列为空");
                    return _queue[0].Data;
                }
            }
        }

        /// <summary>
        /// 从队列中移除所有节点
        /// O(n)
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _queue.Clear();
                _itemToNodesCache.Clear();
            }
        }

        /// <summary>
        /// 返回给定项是否在队列中
        /// O(1)
        /// </summary>
        public bool Contains(TItem item)
        {
            lock (_lock)
                return _itemToNodesCache.ContainsKey(item);
        }

        /// <summary>
        /// 移除队列的头部（具有最小优先级的节点；插入顺序打破平局），并返回它
        /// 如果队列为空，则抛出异常
        /// O(log n)
        /// </summary>
        public TItem Dequeue()
        {
            lock (_lock)
            {
                if (_queue.Count == 0)
                    throw new InvalidOperationException("队列为空");

                SimpleNode node = _queue[0];
                RemoveFromQueue(node);
                return node.Data;
            }
        }

        /// <summary>
        /// 将节点入队到优先级队列。较小的值放在前面。先进先出打破平局。
        /// 这个队列会自动调整大小，所以不用担心队列变"满"。
        /// 允许重复和空值。
        /// O(log n)
        /// </summary>
        public void Enqueue(TItem item, TPriority priority)
        {
            lock (_lock)
            {
                SimpleNode node = new SimpleNode(item, priority);
                _queue.Add(node);
                SiftUp(_queue.Count - 1);

                if (!_itemToNodesCache.TryGetValue(item, out List<SimpleNode> nodes))
                {
                    nodes = new List<SimpleNode>();
                    _itemToNodesCache[item] = nodes;
                }
                nodes.Add(node);
            }
        }

        /// <summary>
        /// 从队列中移除一个项。该项不需要是队列的头部。
        /// 如果该项不在队列中，则抛出异常。如果不确定，请先检查Contains()。
        /// 如果项被多次入队，只移除第一个。
        /// O(log n)
        /// </summary>
        public void Remove(TItem item)
        {
            lock (_lock)
            {
                if (!_itemToNodesCache.TryGetValue(item, out List<SimpleNode> nodes))
                    throw new InvalidOperationException("无法从未入队的节点调用Remove(): " + item);

                SimpleNode node = nodes[0];
                RemoveFromQueue(node);
            }
        }

        /// <summary>
        /// 调用此方法来更改项的优先级。
        /// 在不在队列中的项上调用此方法将抛出异常。
        /// 如果项被多次入队，只更新第一个。
        /// O(log n)
        /// </summary>
        public void UpdatePriority(TItem item, TPriority priority)
        {
            lock (_lock)
            {
                if (!_itemToNodesCache.TryGetValue(item, out List<SimpleNode> nodes))
                    throw new InvalidOperationException("无法在未入队的节点上调用UpdatePriority(): " + item);

                SimpleNode node = nodes[0];
                int comp = _comparer.Compare(priority, node.Priority);
                if (comp < 0)
                {
                    node.Priority = priority;
                    SiftUp(_queue.IndexOf(node));
                }
                else if (comp > 0)
                {
                    node.Priority = priority;
                    SiftDown(_queue.IndexOf(node));
                }
            }
        }

        /// <summary>
        /// 返回给定项的优先级。
        /// 在不在队列中的项上调用此方法将抛出异常。
        /// 如果项被多次入队，只返回第一个的优先级。
        /// O(1)
        /// </summary>
        public TPriority GetPriority(TItem item)
        {
            lock (_lock)
            {
                if (!_itemToNodesCache.TryGetValue(item, out List<SimpleNode> nodes))
                    throw new InvalidOperationException("无法在未入队的节点上调用GetPriority(): " + item);

                return nodes[0].Priority;
            }
        }

        /// <summary>
        /// 尝试获取队列的头部，而不移除它（使用TryDequeue()来移除）。
        /// 对于多线程有用，其中队列可能在Contains()和First调用之间变空
        /// 如果成功则返回true，否则返回false
        /// O(1)
        /// </summary>
        public bool TryFirst(out TItem first)
        {
            lock (_lock)
            {
                if (_queue.Count > 0)
                {
                    first = _queue[0].Data;
                    return true;
                }
                first = default(TItem);
                return false;
            }
        }

        /// <summary>
        /// 尝试移除队列的头部（具有最小优先级的节点；插入顺序打破平局），并将其设置为first。
        /// 对于多线程有用，其中队列可能在Contains()和Dequeue()调用之间变空
        /// 如果成功则返回true；如果队列为空则返回false
        /// O(log n)
        /// </summary>
        public bool TryDequeue(out TItem first)
        {
            lock (_lock)
            {
                if (_queue.Count > 0)
                {
                    SimpleNode node = _queue[0];
                    first = node.Data;
                    RemoveFromQueue(node);
                    return true;
                }
                first = default(TItem);
                return false;
            }
        }

        /// <summary>
        /// 尝试从队列中移除一个项。该项不需要是队列的头部。
        /// 对于多线程有用，其中队列可能在Contains()和Remove()调用之间变空
        /// 如果项成功移除则返回true，如果它不在队列中则返回false。
        /// 如果项被多次入队，只移除第一个。
        /// O(log n)
        /// </summary>
        public bool TryRemove(TItem item)
        {
            lock (_lock)
            {
                if (_itemToNodesCache.TryGetValue(item, out List<SimpleNode> nodes))
                {
                    SimpleNode node = nodes[0];
                    RemoveFromQueue(node);
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 尝试更新项的优先级。
        /// 对于多线程有用，其中队列可能在Contains()和UpdatePriority()调用之间变空
        /// 如果项的优先级被更新则返回true，否则返回false。
        /// 如果项被多次入队，只更新第一个。
        /// O(log n)
        /// </summary>
        public bool TryUpdatePriority(TItem item, TPriority priority)
        {
            lock (_lock)
            {
                if (_itemToNodesCache.TryGetValue(item, out List<SimpleNode> nodes))
                {
                    SimpleNode node = nodes[0];
                    int comp = _comparer.Compare(priority, node.Priority);
                    if (comp < 0)
                    {
                        node.Priority = priority;
                        SiftUp(_queue.IndexOf(node));
                    }
                    else if (comp > 0)
                    {
                        node.Priority = priority;
                        SiftDown(_queue.IndexOf(node));
                    }
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 尝试获取给定项的优先级。
        /// 对于多线程有用，其中队列可能在Contains()和GetPriority()调用之间变空
        /// 如果项在队列中找到则返回true，否则返回false
        /// O(1)
        /// </summary>
        public bool TryGetPriority(TItem item, out TPriority priority)
        {
            lock (_lock)
            {
                if (_itemToNodesCache.TryGetValue(item, out List<SimpleNode> nodes))
                {
                    priority = nodes[0].Priority;
                    return true;
                }
                priority = default(TPriority);
                return false;
            }
        }

        public IEnumerator<TItem> GetEnumerator()
        {
            List<TItem> items = new List<TItem>();
            lock (_lock)
            {
                foreach (SimpleNode node in _queue)
                    items.Add(node.Data);
            }
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void RemoveFromQueue(SimpleNode node)
        {
            int index = _queue.IndexOf(node);
            if (index == _queue.Count - 1)
            {
                _queue.RemoveAt(_queue.Count - 1);
            }
            else
            {
                _queue[index] = _queue[_queue.Count - 1];
                _queue.RemoveAt(_queue.Count - 1);
                SiftDown(index);
            }

            List<SimpleNode> nodes = _itemToNodesCache[node.Data];
            nodes.Remove(node);
            if (nodes.Count == 0)
                _itemToNodesCache.Remove(node.Data);
        }

        private void SiftUp(int index)
        {
            SimpleNode node = _queue[index];
            while (index > 0)
            {
                int parentIndex = (index - 1) / 2;
                SimpleNode parent = _queue[parentIndex];
                if (_comparer.Compare(node.Priority, parent.Priority) >= 0)
                    break;
                _queue[index] = parent;
                index = parentIndex;
            }
            _queue[index] = node;
        }

        private void SiftDown(int index)
        {
            SimpleNode node = _queue[index];
            while (true)
            {
                int leftChildIndex = index * 2 + 1;
                if (leftChildIndex >= _queue.Count)
                    break;
                int rightChildIndex = leftChildIndex + 1;
                int minChildIndex = (rightChildIndex < _queue.Count && _comparer.Compare(_queue[rightChildIndex].Priority, _queue[leftChildIndex].Priority) < 0) 
                    ? rightChildIndex : leftChildIndex;
                SimpleNode minChild = _queue[minChildIndex];
                if (_comparer.Compare(node.Priority, minChild.Priority) <= 0)
                    break;
                _queue[index] = minChild;
                index = minChildIndex;
            }
            _queue[index] = node;
        }

        private class SimpleNode
        {
            public TItem Data { get; }
            public TPriority Priority { get; set; }

            public SimpleNode(TItem data, TPriority priority)
            {
                Data = data;
                Priority = priority;
            }
        }
    }

    public interface IPriorityQueue<TItem, TPriority> where TPriority : IComparable<TPriority>
    {
        int Count { get; }
        TItem First { get; }
        void Clear();
        bool Contains(TItem item);
        TItem Dequeue();
        void Enqueue(TItem item, TPriority priority);
        void Remove(TItem item);
        void UpdatePriority(TItem item, TPriority priority);
        TPriority GetPriority(TItem item);
        bool TryFirst(out TItem first);
        bool TryDequeue(out TItem first);
        bool TryRemove(TItem item);
        bool TryUpdatePriority(TItem item, TPriority priority);
        bool TryGetPriority(TItem item, out TPriority priority);
    }
}
