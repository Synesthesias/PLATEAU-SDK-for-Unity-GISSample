
using System.Collections.Generic;

namespace GISSample.PlateauAttributeDisplay
{

    /// <summary>
    /// QueueやStackのように順番を管理しつつ、キーと値のペアでデータを保持するクラスです。
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class OrderedMap<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> dict = new();
        private readonly List<TKey> order = new();

        public void Add(TKey key, TValue value)
        {
            dict[key] = value;
            order.Add(key);
        }

        //順番更新
        public bool Upsert(TKey key, TValue value)
        {
            bool reordered = false;
            if (!dict.ContainsKey(key))
            {
                order.Add(key);
            }
            else
            {
                order.Remove(key);
                order.Add(key);
                reordered = true;
            }

            dict[key] = value;
            return reordered;
        }

        public void Remove(TKey key)
        {
            if (dict.ContainsKey(key))
                dict.Remove(key);
            if (order.Contains(key))
                order.Remove(key);
        }

        public void Clear()
        {
            dict.Clear();
            order.Clear();
        }

        // 最後の要素を返して削除
        public KeyValuePair<TKey, TValue> Pop()
        {
            int lastIndex = order.Count - 1;
            var key = order[lastIndex];
            var value = dict[key];

            order.RemoveAt(lastIndex);
            dict.Remove(key);

            return new KeyValuePair<TKey, TValue>(key, value);
        }

        // 最初の要素を返して削除
        public KeyValuePair<TKey, TValue> PopFirst()
        {
            int firstIndex = 0;
            var key = order[firstIndex];
            var value = dict[key];

            order.RemoveAt(firstIndex);
            dict.Remove(key);

            return new KeyValuePair<TKey, TValue>(key, value);
        }


        public TValue LastValue => dict[order[^1]];
        public TKey LastKey => order[^1];

        public TKey FirstKey => order[0];
        public TValue FirstValue => dict[order[0]];


        public int Count => order.Count;

        public IEnumerable<KeyValuePair<TKey, TValue>> Items
        {
            get
            {
                foreach (var key in order)
                    yield return new KeyValuePair<TKey, TValue>(key, dict[key]);
            }
        }
    }

}
