using System;

namespace SoRR
{
    public static class ArrayEx
    {
        public static void RemoveRange<T>(ref T[] array, int index, int length)
        {
            T[] newArray = new T[array.Length - length];
            Array.Copy(array, newArray, index);
            Array.Copy(array, index + length, newArray, index, array.Length - index - length);
            array = newArray;
        }

        public static void AddItem<T>(ref T[] array, T item)
        {
            T[] newArray = new T[array.Length + 1];
            array.CopyTo(newArray, 0);
            newArray[array.Length] = item;
            array = newArray;
        }

    }
}
