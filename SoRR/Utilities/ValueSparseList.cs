using System;

namespace SoRR
{
    /// <summary>
    ///   <para>A simple structure managing a sparse collection of reference type values.</para>
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    public struct ValueSparseList<T> where T : class
    {
        private T?[] _items;
        private int _size;
        private int _nullCount;

        /// <summary>
        ///   <para>Initializes a new instance of the <see cref="ValueSparseList{T}"/> structure.</para>
        /// </summary>
        public ValueSparseList() => _items = [];

        /// <summary>
        ///   <para>Returns a read-only sparse view of the collection's items.</para>
        /// </summary>
        /// <returns>A read-only sparse view of the collection's items.</returns>
        public readonly ReadOnlySpan<T?> GetItems() => _items[.._size];

        /// <summary>
        ///   <para>Adds the specified <paramref name="item"/> to the sparse list.</para>
        /// </summary>
        /// <param name="item">The item to add to the sparse list.</param>
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

        /// <summary>
        ///   <para>Removes the specified <paramref name="item"/> from the sparse list.</para>
        /// </summary>
        /// <param name="item">The item to remove from the sparse list.</param>
        public void Remove(T item)
        {
            T?[] items = _items;
            int size = _size;

            for (int i = 0; i < size; i++)
                if (items[i] == item)
                {
                    items[i] = null;
                    if (size > 4 && ++_nullCount >= size / 2) TrimNulls();
                    break;
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
            Array.Clear(items, pos, size - pos);

            _nullCount = 0;
            _size = pos;
        }

    }
}
