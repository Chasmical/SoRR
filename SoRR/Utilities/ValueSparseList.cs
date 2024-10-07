using System;

namespace SoRR
{
    public struct ValueSparseList<T> where T : class
    {
        private T?[] _items;
        private int _size;
        private int _nullCount;

        public ValueSparseList() => _items = [];

        public ReadOnlySpan<T?> GetItems() => _items[.._size];

        public void Add(T item)
        {
            T?[] items = _items;
            int size = _size;
            if (size < items.Length)
            {
                _size = size + 1;
                items[size] = item;
                return;
            }
            AddWithResize(item);
        }
        private void AddWithResize(T item)
        {
            int size = _size;
            Array.Resize(ref _items, size == 0 ? 4 : size * 2);
            _size = size + 1;
            _items[size] = item;
        }

        public void Remove(T item)
        {
            T?[] items = _items;
            int size = _size;

            for (int i = 0; i < size; i++)
                if (items[i] == item)
                {
                    items[i] = null;
                    if (size > 4 && ++_nullCount >= size / 2) TrimNulls();
                }
        }
        private void TrimNulls()
        {
            T?[] items = _items;
            int size = _size;

            int pos = 0;
            while (pos < size && items[pos] is not null) pos++;

            if (pos >= size) return;

            int i = pos + 1;
            while (i < size)
            {
                while (i < size && items[i] is null) i++;
                if (i < size) items[pos++] = items[i++];
            }

            _nullCount = 0;
            _size = pos;
        }

    }
}
