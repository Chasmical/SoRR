using System;

namespace SoRR
{
    /// <summary>
    ///   <para>A simple structure managing a sparse collection of reference type values.</para>
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    public struct ValueSparseCollection<T> where T : class
    {
        private T?[] _items;
        private int _size;
        private int _nullCount;

        /// <summary>
        ///   <para>Initializes a new instance of the <see cref="ValueSparseCollection{T}"/> structure.</para>
        /// </summary>
        public ValueSparseCollection() => _items = [];

        /// <summary>
        ///   <para>Returns a read-only sparse view of the collection's items.</para>
        /// </summary>
        /// <returns>A read-only sparse view of the collection's items.</returns>
        public readonly ReadOnlySpan<T?> GetItems() => _items[.._size];

        /// <summary>
        ///   <para>Adds the specified <paramref name="item"/> to the sparse collection.</para>
        /// </summary>
        /// <param name="item">The item to add to the sparse collection.</param>
        public void Add(T item)
        {
            if (item is null) throw new ArgumentNullException(nameof(item));

            T?[] items = _items;
            int size = _size;

            if (size < items.Length)
            {
                _size = size + 1;
                items[size] = item;
                return;
            }
            if (_nullCount > 0)
                AddWithReplace(item);
            else
                AddWithResize(item);
        }
        private void AddWithReplace(T item)
        {
            T?[] items = _items;
            int size = _size;

            for (int i = 0; i < size; i++)
                if (items[i] is null)
                {
                    items[i] = item;
                    _nullCount--;
                    break;
                }
        }
        private void AddWithResize(T item)
        {
            int size = _size;

            T?[] oldArr = _items;
            T?[] newArr = new T[size == 0 ? 4 : size * 2];
            oldArr.CopyTo(newArr, 0);
            _items = newArr;

            _size = size + 1;
            newArr[size] = item;
        }

        /// <summary>
        ///   <para>Removes the specified <paramref name="item"/> from the sparse collection.</para>
        /// </summary>
        /// <param name="item">The item to remove from the sparse collection.</param>
        /// <returns><see langword="true"/>, if the item was successfully removed; otherwise, <see langword="false"/>.</returns>
        public bool Remove(T item)
        {
            if (item is null) throw new ArgumentNullException(nameof(item));

            T?[] items = _items;
            int size = _size;

            for (int i = 0; i < size; i++)
                if (items[i] == item)
                {
                    items[i] = null;
                    if (size > 4 && ++_nullCount >= size / 2) TrimNulls();
                    return true;
                }
            return false;
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
