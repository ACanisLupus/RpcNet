﻿namespace RpcNet.Internal
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Text;

    public static class Utilities
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ToInt32BigEndian(ReadOnlySpan<byte> value) => (value[0] << 24) | (value[1] << 16) | (value[2] << 8) | value[3];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBytesBigEndian(Span<byte> destination, int value)
        {
            destination[0] = (byte)(value >> 24);
            destination[1] = (byte)((value >> 16) & 0xff);
            destination[2] = (byte)((value >> 8) & 0xff);
            destination[3] = (byte)(value & 0xff);
        }

        // Not available in .NET Standard 2.0
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int SingleToInt32Bits(float value) => *(int*)&value;

        // Not available in .NET Standard 2.0
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe float Int32BitsToSingle(int value) => *(float*)&value;

        // Not available in .NET Standard 2.0
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int GetBytes(this Encoding encoding, string value, Span<byte> destination)
        {
            int length = value.Length;
            fixed (char* chars = &value.AsSpan().GetPinnableReference())
            {
                fixed (byte* bytes = &destination.GetPinnableReference())
                {
                    return encoding.GetBytes(chars, length, bytes, length);
                }
            }
        }

        // Not available in .NET Standard 2.0
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe string GetString(this Encoding encoding, ReadOnlySpan<byte> source)
        {
            int length = source.Length;
            fixed (byte* bytes = &source.GetPinnableReference())
            {
                return encoding.GetString(bytes, length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CalculateXdrPadding(int length) => (4 - (length & 3)) & 3;
    }
}
