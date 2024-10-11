using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace SoRR
{
    public static class ObfuscationHelper
    {
        public static unsafe T XorReduce<T, TContainer>(ref TContainer container)
            where T : unmanaged where TContainer : unmanaged
        {
            Span<T> span = new Span<T>(Unsafe.AsPointer(ref container), sizeof(TContainer) / sizeof(T));

            if (sizeof(TContainer) / sizeof(T) == 2)
                return Xor(span[0], span[1]);

            if (sizeof(TContainer) / sizeof(T) == 4)
                return Xor(span[0], span[1], span[2], span[3]);

            if (sizeof(TContainer) / sizeof(T) == 8)
                return Xor(Xor(span[0], span[1], span[2], span[3]), Xor(span[4], span[5], span[6], span[7]));

            throw new NotSupportedException();
        }

        public static unsafe void XorRefresh<T, TContainer>(ref TContainer container, T value)
            where T : unmanaged where TContainer : unmanaged
        {
            Span<byte> bytes = new Span<byte>(Unsafe.AsPointer(ref container), sizeof(TContainer));
            RandomNumberGenerator.Fill(bytes);

            Span<T> span = MemoryMarshal.Cast<byte, T>(bytes);
            int offset = RandomNumberGenerator.GetInt32(sizeof(TContainer) / sizeof(T));

            span[offset] = value;
            span[offset] = XorReduce<T, TContainer>(ref container);
        }

        private static T Xor<T>(T a, T b, T c, T d) where T : unmanaged
            => Xor(Xor(Xor(a, b), c), d);

        private static unsafe T Xor<T>(T a, T b) where T : unmanaged
        {
            if (sizeof(T) == 1)
            {
                byte value = (byte)(Unsafe.As<T, byte>(ref a) ^ Unsafe.As<T, byte>(ref b));
                return Unsafe.As<byte, T>(ref value);
            }
            if (sizeof(T) == 2)
            {
                ushort value = (ushort)(Unsafe.As<T, ushort>(ref a) ^ Unsafe.As<T, ushort>(ref b));
                return Unsafe.As<ushort, T>(ref value);
            }
            if (sizeof(T) == 4)
            {
                uint value = Unsafe.As<T, uint>(ref a) ^ Unsafe.As<T, uint>(ref b);
                return Unsafe.As<uint, T>(ref value);
            }
            if (sizeof(T) == 8)
            {
                ulong value = Unsafe.As<T, ulong>(ref a) ^ Unsafe.As<T, ulong>(ref b);
                return Unsafe.As<ulong, T>(ref value);
            }
            throw new NotSupportedException();
        }

    }
    [StructLayout(LayoutKind.Sequential)]
    public struct ObfuscatedXor2<T> : IEquatable<ObfuscatedXor2<T>> where T : unmanaged
    {
        private T part1, part2;

        public T Value
        {
            get => ObfuscationHelper.XorReduce<T, ObfuscatedXor2<T>>(ref this);
            set => ObfuscationHelper.XorRefresh(ref this, value);
        }

        public ObfuscatedXor2() => Value = default;
        public ObfuscatedXor2(T value) => Value = value;
        public void Refresh() => Value = Value;

        public bool Equals(ObfuscatedXor2<T> other) => Value.Equals(other.Value);
        public override bool Equals(object? obj) => obj is ObfuscatedXor2<T> other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();

        public static implicit operator ObfuscatedXor2<T>(T value) => new(value);
        public static implicit operator T(ObfuscatedXor2<T> obfuscated) => obfuscated.Value;

    }
    [StructLayout(LayoutKind.Sequential)]
    public struct ObfuscatedXor4<T> : IEquatable<ObfuscatedXor4<T>> where T : unmanaged
    {
        private T part1, part2, part3, part4;

        public T Value
        {
            get => ObfuscationHelper.XorReduce<T, ObfuscatedXor4<T>>(ref this);
            set => ObfuscationHelper.XorRefresh(ref this, value);
        }

        public ObfuscatedXor4() => Value = default;
        public ObfuscatedXor4(T value) => Value = value;
        public void Refresh() => Value = Value;

        public bool Equals(ObfuscatedXor4<T> other) => Value.Equals(other.Value);
        public override bool Equals(object? obj) => obj is ObfuscatedXor4<T> other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();

        public static implicit operator ObfuscatedXor4<T>(T value) => new(value);
        public static implicit operator T(ObfuscatedXor4<T> obfuscated) => obfuscated.Value;

    }
    [StructLayout(LayoutKind.Sequential)]
    public struct ObfuscatedXor8<T> : IEquatable<ObfuscatedXor8<T>> where T : unmanaged
    {
        private T part1, part2, part3, part4, part5, part6, part7, part8;

        public T Value
        {
            get => ObfuscationHelper.XorReduce<T, ObfuscatedXor8<T>>(ref this);
            set => ObfuscationHelper.XorRefresh(ref this, value);
        }

        public ObfuscatedXor8() => Value = default;
        public ObfuscatedXor8(T value) => Value = value;
        public void Refresh() => Value = Value;

        public bool Equals(ObfuscatedXor8<T> other) => Value.Equals(other.Value);
        public override bool Equals(object? obj) => obj is ObfuscatedXor8<T> other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();

        public static implicit operator ObfuscatedXor8<T>(T value) => new(value);
        public static implicit operator T(ObfuscatedXor8<T> obfuscated) => obfuscated.Value;

    }
}
