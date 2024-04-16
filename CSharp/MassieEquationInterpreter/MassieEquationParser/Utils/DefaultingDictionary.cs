using System;
using System.Collections;
using System.Collections.Generic;

namespace Scot.Massie.EquationParser.Utils
{
    public class DefaultingDictionary<TKey, TValue> : IDictionary<TKey, TValue>
        where TKey : notnull
    {
        private readonly IDictionary<TKey, TValue> _inner;

        private readonly Func<TKey, TValue> _defaultSupplier;

        public int Count => _inner.Count;

        public bool IsReadOnly => _inner.IsReadOnly;

        public ICollection<TKey> Keys => _inner.Keys;

        public ICollection<TValue> Values => _inner.Values;

        public TValue this[TKey key]
        {
            get
            {
                if(!_inner.TryGetValue(key, out var value))
                    _inner[key] = value = _defaultSupplier(key);

                return value;
            }

            set => _inner[key] = value;
        }

        public DefaultingDictionary(IDictionary<TKey, TValue> inner, Func<TKey, TValue> defaultSupplier)
        {
            _inner           = inner;
            _defaultSupplier = defaultSupplier;
        }

        public DefaultingDictionary(IDictionary<TKey, TValue> inner, TValue defaultValue)
            : this(inner, _ => defaultValue)
        { }

        public DefaultingDictionary(Func<TKey, TValue> defaultSupplier)
            : this(new Dictionary<TKey, TValue>(), defaultSupplier)
        { }

        public DefaultingDictionary(TValue defaultValue)
            : this(new Dictionary<TKey, TValue>(), _ => defaultValue)
        { }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            _inner.Add(item);
        }

        public void Add(TKey key, TValue value)
        {
            _inner.Add(key, value);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return _inner.Remove(item);
        }

        public bool Remove(TKey key)
        {
            return _inner.Remove(key);
        }

        public void Clear()
        {
            _inner.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _inner.Contains(item);
        }

        public bool ContainsKey(TKey key)
        {
            return _inner.ContainsKey(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if(!_inner.TryGetValue(key, out value!))
                _inner[key] = value = _defaultSupplier(key);

            return true;
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            _inner.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _inner.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_inner).GetEnumerator();
        }
    }
}
