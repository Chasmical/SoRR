using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// This class is basically a copy of Dictionary<TKey, TValue>,
// with everything unnecessary removed, and with added methods
// taking read-only spans as parameters.

namespace SoRR
{
    /// <summary>
    ///   <para>Represents a collection of <see cref="string"/> keys and <typeparamref name="TValue"/> values.<br/>Provides methods that use read-only spans of characters, mimicking the functionality of alternate lookups in .NET 9.</para>
    /// </summary>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    [Serializable]
    public sealed class StringKeyedDictionary<TValue>
    {
        private int[]? _buckets;
        private Entry[]? _entries;
        private int _count;
        private int _freeList;
        private int _freeCount;
        private bool usesRandomizedHash;
        private const int StartOfFreeList = -3;

        /// <inheritdoc cref="Dictionary{TKey,TValue}()"/>
        public StringKeyedDictionary() : this(0) { }

        /// <inheritdoc cref="Dictionary{TKey,TValue}(int)"/>
        public StringKeyedDictionary(int capacity)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            if (capacity > 0) Initialize(capacity);
        }

        /// <inheritdoc cref="Dictionary{TKey,TValue}.Count"/>
        public int Count => _count - _freeCount;

        /// <inheritdoc cref="Dictionary{TKey,TValue}.this[TKey]"/>
        public TValue this[string key]
        {
            get
            {
                ref TValue value = ref FindValue(key);
                if (!Unsafe.IsNullRef(ref value)) return value;
                throw new KeyNotFoundException($"Could find key {key} in the dictionary.");
            }
            set => TryInsert(key, value, InsertionBehavior.OverwriteExisting);
        }

        /// <inheritdoc cref="Dictionary{TKey,TValue}.Add"/>
        public void Add(string key, TValue value)
            => TryInsert(key, value, InsertionBehavior.ThrowOnExisting);

        /// <inheritdoc cref="Dictionary{TKey,TValue}.Clear"/>
        public void Clear()
        {
            int count = _count;
            if (count > 0)
            {
                Array.Clear(_buckets!, 0, _buckets!.Length);
                _count = 0;
                _freeList = -1;
                _freeCount = 0;
                Array.Clear(_entries!, 0, count);
            }
        }

        /// <inheritdoc cref="Dictionary{TKey,TValue}.ContainsKey"/>
        public bool ContainsKey(string key) =>
            !Unsafe.IsNullRef(ref FindValue(key));
        /// <inheritdoc cref="Dictionary{TKey,TValue}.ContainsKey"/>
        public bool ContainsKey(ReadOnlySpan<char> key) =>
            !Unsafe.IsNullRef(ref FindValue(key));

        internal ref TValue FindValue(string key)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));

            ref Entry entry = ref Unsafe.NullRef<Entry>();
            if (_buckets != null)
            {
                uint hashCode = StringKeyedDictionaryHelper.ComputeHash(key, usesRandomizedHash);
                int i = GetBucket(hashCode);
                Entry[]? entries = _entries;
                uint collisionCount = 0;
                i--;
                do
                {
                    if ((uint)i >= (uint)entries!.Length)
                        goto ReturnNotFound;

                    entry = ref entries[i];
                    if (entry.hashCode == hashCode && string.Equals(entry.key, key))
                        goto ReturnFound;

                    i = entry.next;
                    collisionCount++;
                } while (collisionCount <= (uint)entries.Length);
                goto ConcurrentOperation;
            }
            goto ReturnNotFound;

        ConcurrentOperation:
            throw new InvalidOperationException("Concurrent operations are not supported.");
        ReturnFound:
            ref TValue value = ref entry.value;
        Return:
            return ref value;
        ReturnNotFound:
            value = ref Unsafe.NullRef<TValue>();
            goto Return;
        }
        internal ref TValue FindValue(ReadOnlySpan<char> key)
        {
            ref Entry entry = ref Unsafe.NullRef<Entry>();
            if (_buckets != null)
            {
                uint hashCode = StringKeyedDictionaryHelper.ComputeHash(key, usesRandomizedHash);
                int i = GetBucket(hashCode);
                Entry[]? entries = _entries;
                uint collisionCount = 0;
                i--;
                do
                {
                    if ((uint)i >= (uint)entries!.Length)
                        goto ReturnNotFound;

                    entry = ref entries[i];
                    if (entry.hashCode == hashCode && key.SequenceEqual(entry.key))
                        goto ReturnFound;

                    i = entry.next;
                    collisionCount++;
                } while (collisionCount <= (uint)entries.Length);
                goto ConcurrentOperation;
            }

            goto ReturnNotFound;

        ConcurrentOperation:
            throw new InvalidOperationException("Concurrent operations are not supported.");
        ReturnFound:
            ref TValue value = ref entry.value;
        Return:
            return ref value;
        ReturnNotFound:
            value = ref Unsafe.NullRef<TValue>();
            goto Return;
        }

        public string? FindKey(ReadOnlySpan<char> key)
        {
            string? res;
            if (_buckets != null)
            {
                uint hashCode = StringKeyedDictionaryHelper.ComputeHash(key, usesRandomizedHash);
                int i = GetBucket(hashCode);
                Entry[]? entries = _entries;
                uint collisionCount = 0;
                i--;
                do
                {
                    if ((uint)i >= (uint)entries!.Length)
                        goto ReturnNotFound;

                    ref Entry entry = ref entries[i];
                    if (entry.hashCode == hashCode && key.SequenceEqual(entry.key))
                    {
                        res = entry.key;
                        goto Return;
                    }

                    i = entry.next;
                    collisionCount++;
                } while (collisionCount <= (uint)entries.Length);
                goto ConcurrentOperation;
            }

            goto ReturnNotFound;

        ConcurrentOperation:
            throw new InvalidOperationException("Concurrent operations are not supported.");
        ReturnNotFound:
            res = null;
        Return:
            return res;
        }
        public bool TryGetKey(ReadOnlySpan<char> key, [NotNullWhen(true)] out string? keyString)
            => (keyString = FindKey(key)) is not null;

        private int Initialize(int capacity)
        {
            int size = StringKeyedDictionaryHelper.GetPrime(capacity);
            int[] buckets = new int[size];
            Entry[] entries = new Entry[size];
            _freeList = -1;
            _buckets = buckets;
            _entries = entries;
            return size;
        }

        private bool TryInsert(string key, TValue value, InsertionBehavior behavior)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));
            if (_buckets == null) Initialize(0);
            Entry[] entries = _entries!;

            uint hashCode = StringKeyedDictionaryHelper.ComputeHash(key, usesRandomizedHash);
            uint collisionCount = 0;
            ref int bucket = ref GetBucket(hashCode);
            int i = bucket - 1;

            while ((uint)i < (uint)entries.Length)
            {
                if (entries[i].hashCode == hashCode && string.Equals(entries[i].key, key))
                {
                    if (behavior == InsertionBehavior.OverwriteExisting)
                    {
                        entries[i].value = value;
                        return true;
                    }
                    if (behavior == InsertionBehavior.ThrowOnExisting)
                        throw new ArgumentException($"Entry with key {key} already exists in the dictionary.");
                    return false;
                }

                i = entries[i].next;

                collisionCount++;
                if (collisionCount > (uint)entries.Length)
                    throw new InvalidOperationException("Concurrent operations are not supported.");
            }

            int index;
            if (_freeCount > 0)
            {
                index = _freeList;
                _freeList = StartOfFreeList - entries[_freeList].next;
                _freeCount--;
            }
            else
            {
                int count = _count;
                if (count == entries.Length)
                {
                    Resize();
                    bucket = ref GetBucket(hashCode);
                }
                index = count;
                _count = count + 1;
                entries = _entries!;
            }

            ref Entry entry = ref entries[index];
            entry.hashCode = hashCode;
            entry.next = bucket - 1;
            entry.key = key;
            entry.value = value;
            bucket = index + 1;
            if (collisionCount > StringKeyedDictionaryHelper.HashCollisionThreshold && !usesRandomizedHash)
                Resize(entries.Length, true);
            return true;
        }

        private void Resize()
            => Resize(StringKeyedDictionaryHelper.ExpandPrime(_count), false);
        private void Resize(int newSize, bool forceNewHashCodes)
        {
            Entry[] entries = new Entry[newSize];
            int count = _count;
            Array.Copy(_entries!, entries, count);

            if (forceNewHashCodes)
            {
                usesRandomizedHash = true;
                for (int i = 0; i < count; i++)
                    if (entries[i].next >= -1)
                        entries[i].hashCode = (uint)entries[i].key.GetHashCode();
            }
            _buckets = new int[newSize];
            for (int i = 0; i < count; i++)
                if (entries[i].next >= -1)
                {
                    ref int bucket = ref GetBucket(entries[i].hashCode);
                    entries[i].next = bucket - 1;
                    bucket = i + 1;
                }
            _entries = entries;
        }

        /// <inheritdoc cref="Dictionary{TKey,TValue}.Remove(TKey)"/>
        public bool Remove(string key)
            => Remove(key, out _);
        /// <inheritdoc cref="Dictionary{TKey,TValue}.Remove(TKey)"/>
        public bool Remove(ReadOnlySpan<char> key)
            => Remove(key, out _);

        /// <inheritdoc cref="Dictionary{TKey,TValue}.Remove(TKey, out TValue)"/>
        public bool Remove(string key, [MaybeNullWhen(false)] out TValue value)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));

            if (_buckets != null)
            {
                uint collisionCount = 0;
                uint hashCode = StringKeyedDictionaryHelper.ComputeHash(key, usesRandomizedHash);
                ref int bucket = ref GetBucket(hashCode);
                Entry[]? entries = _entries;
                int last = -1;
                int i = bucket - 1;
                while (i >= 0)
                {
                    ref Entry entry = ref entries![i];

                    if (entry.hashCode == hashCode && string.Equals(entry.key, key))
                    {
                        if (last < 0) bucket = entry.next + 1;
                        else entries[last].next = entry.next;

                        value = entry.value;
                        entry.next = StartOfFreeList - _freeList;
                        entry.key = default!;
                        if (RuntimeHelpers.IsReferenceOrContainsReferences<TValue>())
                            entry.value = default!;
                        _freeList = i;
                        _freeCount++;
                        return true;
                    }

                    last = i;
                    i = entry.next;
                    collisionCount++;
                    if (collisionCount > (uint)entries.Length)
                        throw new InvalidOperationException("Concurrent operations are not supported.");
                }
            }
            value = default;
            return false;
        }
        /// <inheritdoc cref="Dictionary{TKey,TValue}.Remove(TKey, out TValue)"/>
        public bool Remove(ReadOnlySpan<char> key, [MaybeNullWhen(false)] out TValue value)
        {
            if (_buckets != null)
            {
                uint collisionCount = 0;
                uint hashCode = StringKeyedDictionaryHelper.ComputeHash(key, usesRandomizedHash);
                ref int bucket = ref GetBucket(hashCode);
                Entry[]? entries = _entries;
                int last = -1;
                int i = bucket - 1;

                while (i >= 0)
                {
                    ref Entry entry = ref entries![i];

                    if (entry.hashCode == hashCode && key.SequenceEqual(entry.key))
                    {
                        if (last < 0) bucket = entry.next + 1;
                        else entries[last].next = entry.next;

                        value = entry.value;
                        entry.next = StartOfFreeList - _freeList;
                        entry.key = default!;
                        if (RuntimeHelpers.IsReferenceOrContainsReferences<TValue>())
                            entry.value = default!;
                        _freeList = i;
                        _freeCount++;
                        return true;
                    }

                    last = i;
                    i = entry.next;
                    collisionCount++;
                    if (collisionCount > (uint)entries.Length)
                        throw new InvalidOperationException("Concurrent operations are not supported.");
                }
            }

            value = default;
            return false;
        }

        /// <inheritdoc cref="Dictionary{TKey,TValue}.TryGetValue(TKey, out TValue)"/>
        public bool TryGetValue(string key, [MaybeNullWhen(false)] out TValue value)
        {
            ref TValue valRef = ref FindValue(key);
            if (!Unsafe.IsNullRef(ref valRef))
            {
                value = valRef;
                return true;
            }
            value = default;
            return false;
        }
        /// <inheritdoc cref="Dictionary{TKey,TValue}.TryGetValue(TKey, out TValue)"/>
        public bool TryGetValue(ReadOnlySpan<char> key, [MaybeNullWhen(false)] out TValue value)
        {
            ref TValue valRef = ref FindValue(key);
            if (!Unsafe.IsNullRef(ref valRef))
            {
                value = valRef;
                return true;
            }
            value = default;
            return false;
        }

        /// <inheritdoc cref="Dictionary{TKey,TValue}.TryAdd(TKey, TValue)"/>
        public bool TryAdd(string key, TValue value)
            => TryInsert(key, value, InsertionBehavior.None);

        /// <inheritdoc cref="Dictionary{TKey,TValue}.EnsureCapacity(int)"/>
        public int EnsureCapacity(int capacity)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));

            int currentCapacity = _entries?.Length ?? 0;
            if (currentCapacity >= capacity) return currentCapacity;
            if (_buckets == null) return Initialize(capacity);

            int newSize = StringKeyedDictionaryHelper.GetPrime(capacity);
            Resize(newSize, false);
            return newSize;
        }
        /// <inheritdoc cref="Dictionary{TKey,TValue}.TrimExcess()"/>
        public void TrimExcess() => TrimExcess(Count);
        /// <inheritdoc cref="Dictionary{TKey,TValue}.TrimExcess(int)"/>
        public void TrimExcess(int capacity)
        {
            if (capacity < Count) throw new ArgumentOutOfRangeException(nameof(capacity));

            int newSize = StringKeyedDictionaryHelper.GetPrime(capacity);
            Entry[]? oldEntries = _entries;
            int currentCapacity = oldEntries?.Length ?? 0;
            if (newSize >= currentCapacity) return;

            int oldCount = _count;
            Initialize(newSize);
            CopyEntries(oldEntries!, oldCount);
        }

        private void CopyEntries(Entry[] entries, int count)
        {
            Entry[] newEntries = _entries!;
            int newCount = 0;
            for (int i = 0; i < count; i++)
            {
                uint hashCode = entries[i].hashCode;
                if (entries[i].next >= -1)
                {
                    ref Entry entry = ref newEntries[newCount];
                    entry = entries[i];
                    ref int bucket = ref GetBucket(hashCode);
                    entry.next = bucket - 1;
                    bucket = newCount + 1;
                    newCount++;
                }
            }
            _count = newCount;
            _freeCount = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref int GetBucket(uint hashCode)
        {
            int[] buckets = _buckets!;
            return ref buckets[hashCode % (uint)buckets.Length];
        }

        private struct Entry
        {
            public uint hashCode;
            public int next;
            public string key;
            public TValue value;
        }
        private enum InsertionBehavior : byte
        {
            None = 0,
            OverwriteExisting = 1,
            ThrowOnExisting = 2,
        }
    }
    internal static class StringKeyedDictionaryHelper
    {
        public const uint HashCollisionThreshold = 100;

        public const int MaxPrimeArrayLength = 0x7FFFFFC3;

        public const int HashPrime = 101;

        private static readonly int[] primesArray =
        [
            3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197, 239, 293, 353, 431, 521, 631, 761, 919,
            1103, 1327, 1597, 1931, 2333, 2801, 3371, 4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591,
            17519, 21023, 25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363, 156437,
            187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403, 968897, 1162687, 1395263,
            1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559, 5999471, 7199369,
        ];

        public static bool IsPrime(int candidate)
        {
            if ((candidate & 1) != 0)
            {
                int limit = (int)Math.Sqrt(candidate);
                for (int divisor = 3; divisor <= limit; divisor += 2)
                {
                    if (candidate % divisor == 0)
                        return false;
                }
                return true;
            }
            return candidate == 2;
        }

        public static int GetPrime(int min)
        {
            if (min < 0) throw new ArgumentException("", nameof(min));

            ReadOnlySpan<int> primes = primesArray;
            for (int i = 0; i < primes.Length; i++)
                if (primes[i] >= min)
                    return primes[i];
            for (int i = min | 1; i < int.MaxValue; i += 2)
                if (IsPrime(i) && (i - 1) % HashPrime != 0)
                    return i;
            return min;
        }
        public static int ExpandPrime(int oldSize)
        {
            int newSize = 2 * oldSize;
            if ((uint)newSize > MaxPrimeArrayLength && MaxPrimeArrayLength > oldSize)
                return MaxPrimeArrayLength;
            return GetPrime(newSize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ComputeHash(ReadOnlySpan<char> span, bool useRandomized)
            => useRandomized ? ComputeHash(span) : ComputeNonRandomizedHash(span);

        private static readonly ulong hashSeed = GenerateSeed();
        private static ulong GenerateSeed()
        {
            Span<byte> bytes = stackalloc byte[sizeof(ulong)];
            new Random().NextBytes(bytes);
            return BitConverter.ToUInt64(bytes);
        }

        private static uint ComputeHash(ReadOnlySpan<char> span)
        {
            ref byte ptr = ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(span));
            return ComputeHash32(ref ptr, (uint)span.Length * 2);
        }
        private static unsafe uint ComputeNonRandomizedHash(ReadOnlySpan<char> span)
        {
            uint hash1 = (5381 << 16) + 5381;
            uint hash2 = hash1;

            int length = span.Length;
            fixed (char* src = &MemoryMarshal.GetReference(span))
            {
                uint* ptr = (uint*)src;

            LengthSwitch:
                switch (length)
                {
                    default:
                        do
                        {
                            length -= 4;
                            hash1 = (RotateLeft(hash1, 5) + hash1) ^ Unsafe.ReadUnaligned<uint>(ptr);
                            hash2 = (RotateLeft(hash2, 5) + hash2) ^ Unsafe.ReadUnaligned<uint>(ptr + 1);
                            ptr += 2;
                        }
                        while (length >= 4);
                        goto LengthSwitch;

                    case 3:
                        hash1 = (RotateLeft(hash1, 5) + hash1) ^ Unsafe.ReadUnaligned<uint>(ptr);
                        uint p1 = *(char*)(ptr + 1);
                        if (!BitConverter.IsLittleEndian) p1 <<= 16;
                        hash2 = (RotateLeft(hash2, 5) + hash2) ^ p1;
                        break;

                    case 2:
                        hash2 = (RotateLeft(hash2, 5) + hash2) ^ Unsafe.ReadUnaligned<uint>(ptr);
                        break;

                    case 1:
                        uint p0 = *(char*)ptr;
                        if (!BitConverter.IsLittleEndian) p0 <<= 16;
                        hash2 = (RotateLeft(hash2, 5) + hash2) ^ p0;
                        break;

                    case 0:
                        break;
                }
            }

            return hash1 + hash2 * 1_566_083_941;
        }

        private static uint ComputeHash32(ref byte data, uint count)
        {
            ulong seed = hashSeed;
            return ComputeHash32(ref data, count, (uint)seed, (uint)(seed >> 32));
        }
        private static uint ComputeHash32(ref byte data, uint count, uint p0, uint p1)
        {
            if (count < 8)
            {
                if (count >= 4) goto Between4And7BytesRemain;
                goto InputTooSmallToEnterMainLoop;
            }

            uint loopCount = count / 8;

            do
            {
                p0 += Unsafe.ReadUnaligned<uint>(ref data);
                uint nextUInt32 = Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref data, 4));

                Block(ref p0, ref p1);
                p0 += nextUInt32;
                Block(ref p0, ref p1);

                data = ref Unsafe.AddByteOffset(ref data, 8);
            } while (--loopCount > 0);

            if ((count & 0b_0100) == 0) goto DoFinalPartialRead;

        Between4And7BytesRemain:

            p0 += Unsafe.ReadUnaligned<uint>(ref data);
            Block(ref p0, ref p1);

        DoFinalPartialRead:

            uint partialResult = Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref Unsafe.AddByteOffset(ref data, (nuint)count & 7), -4));

            count = ~count << 3;

            if (BitConverter.IsLittleEndian)
            {
                partialResult >>= 8;
                partialResult |= 0x8000_0000u;
                partialResult >>= (int)count & 0x1F;
            }
            else
            {
                partialResult <<= 8;
                partialResult |= 0x80u;
                partialResult <<= (int)count & 0x1F;
            }

        DoFinalRoundsAndReturn:

            p0 += partialResult;
            Block(ref p0, ref p1);
            Block(ref p0, ref p1);

            return p1 ^ p0;

        InputTooSmallToEnterMainLoop:

            partialResult = BitConverter.IsLittleEndian ? 0x80u : 0x80000000u;

            if ((count & 0b_0001) != 0)
            {
                partialResult = Unsafe.AddByteOffset(ref data, (nuint)count & 2);

                if (BitConverter.IsLittleEndian)
                {
                    partialResult |= 0x8000;
                }
                else
                {
                    partialResult <<= 24;
                    partialResult |= 0x800000u;
                }
            }

            if ((count & 0b_0010) != 0)
            {
                if (BitConverter.IsLittleEndian)
                {
                    partialResult <<= 16;
                    partialResult |= Unsafe.ReadUnaligned<ushort>(ref data);
                }
                else
                {
                    partialResult |= Unsafe.ReadUnaligned<ushort>(ref data);
                    partialResult = RotateLeft(partialResult, 16);
                }
            }

            goto DoFinalRoundsAndReturn;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint RotateLeft(uint value, int offset)
            => (value << offset) | (value >> (32 - offset));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Block(ref uint rp0, ref uint rp1)
        {
            uint p0 = rp0;
            uint p1 = rp1;

            p1 ^= p0;
            p0 = RotateLeft(p0, 20);
            p0 += p1;
            p1 = RotateLeft(p1, 9);
            p1 ^= p0;
            p0 = RotateLeft(p0, 27);
            p0 += p1;
            p1 = RotateLeft(p1, 19);

            rp0 = p0;
            rp1 = p1;
        }

    }
}
