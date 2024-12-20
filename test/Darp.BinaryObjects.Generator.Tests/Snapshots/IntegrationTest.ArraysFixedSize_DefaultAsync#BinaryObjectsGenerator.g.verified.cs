﻿//HintName: BinaryObjectsGenerator.g.cs
// <auto-generated/>
#nullable enable

/// <remarks> <list type="table">
/// <item> <term><b>Field</b></term> <description><b>Byte Length</b></description> </item>
/// <item> <term><see cref="ValueByteMemory"/></term> <description>1 * 2</description> </item>
/// <item> <term><see cref="ValueByteArray"/></term> <description>1 * 2</description> </item>
/// <item> <term><see cref="ValueByteList"/></term> <description>1 * 2</description> </item>
/// <item> <term><see cref="ValueByteEnumerable"/></term> <description>1 * 2</description> </item>
/// <item> <term><see cref="ValueUShortMemory"/></term> <description>2 * 2</description> </item>
/// <item> <term><see cref="ValueUShortArray"/></term> <description>2 * 2</description> </item>
/// <item> <term><see cref="ValueUShortList"/></term> <description>2 * 2</description> </item>
/// <item> <term><see cref="ValueUShortEnumerable"/></term> <description>2 * 2</description> </item>
/// <item> <term> --- </term> <description>24</description> </item>
/// </list> </remarks>
[global::Darp.BinaryObjects.BinaryConstant(24)]
public partial record ArraysFixedSize : global::Darp.BinaryObjects.IBinaryObject<ArraysFixedSize>
{
    /// <inheritdoc />
    [global::System.Diagnostics.Contracts.Pure]
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public int GetByteCount() => 24;

    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public bool TryWriteLittleEndian(global::System.Span<byte> destination) => TryWriteLittleEndian(destination, out _);
    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public bool TryWriteLittleEndian(global::System.Span<byte> destination, out int bytesWritten)
    {
        bytesWritten = 0;

        if (destination.Length < 24)
            return false;
        global::Darp.BinaryObjects.Generated.Utilities.WriteUInt8Span(destination.Slice(0, 2), this.ValueByteMemory.Span);
        global::Darp.BinaryObjects.Generated.Utilities.WriteUInt8Span(destination.Slice(2, 2), this.ValueByteArray);
        global::Darp.BinaryObjects.Generated.Utilities.WriteUInt8List(destination.Slice(4, 2), this.ValueByteList);
        global::Darp.BinaryObjects.Generated.Utilities.WriteUInt8Enumerable(destination.Slice(6, 2), this.ValueByteEnumerable);
        global::Darp.BinaryObjects.Generated.Utilities.WriteUInt16SpanLittleEndian(destination.Slice(8, 4), this.ValueUShortMemory.Span);
        global::Darp.BinaryObjects.Generated.Utilities.WriteUInt16SpanLittleEndian(destination.Slice(12, 4), this.ValueUShortArray);
        global::Darp.BinaryObjects.Generated.Utilities.WriteUInt16ListLittleEndian(destination.Slice(16, 4), this.ValueUShortList);
        global::Darp.BinaryObjects.Generated.Utilities.WriteUInt16EnumerableLittleEndian(destination.Slice(20, 4), this.ValueUShortEnumerable);
        bytesWritten += 24;

        return true;
    }
    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public bool TryWriteBigEndian(global::System.Span<byte> destination) => TryWriteBigEndian(destination, out _);
    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public bool TryWriteBigEndian(global::System.Span<byte> destination, out int bytesWritten)
    {
        bytesWritten = 0;

        if (destination.Length < 24)
            return false;
        global::Darp.BinaryObjects.Generated.Utilities.WriteUInt8Span(destination.Slice(0, 2), this.ValueByteMemory.Span);
        global::Darp.BinaryObjects.Generated.Utilities.WriteUInt8Span(destination.Slice(2, 2), this.ValueByteArray);
        global::Darp.BinaryObjects.Generated.Utilities.WriteUInt8List(destination.Slice(4, 2), this.ValueByteList);
        global::Darp.BinaryObjects.Generated.Utilities.WriteUInt8Enumerable(destination.Slice(6, 2), this.ValueByteEnumerable);
        global::Darp.BinaryObjects.Generated.Utilities.WriteUInt16SpanBigEndian(destination.Slice(8, 4), this.ValueUShortMemory.Span);
        global::Darp.BinaryObjects.Generated.Utilities.WriteUInt16SpanBigEndian(destination.Slice(12, 4), this.ValueUShortArray);
        global::Darp.BinaryObjects.Generated.Utilities.WriteUInt16ListBigEndian(destination.Slice(16, 4), this.ValueUShortList);
        global::Darp.BinaryObjects.Generated.Utilities.WriteUInt16EnumerableBigEndian(destination.Slice(20, 4), this.ValueUShortEnumerable);
        bytesWritten += 24;

        return true;
    }

    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadLittleEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out ArraysFixedSize? value) => TryReadLittleEndian(source, out value, out _);
    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadLittleEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out ArraysFixedSize? value, out int bytesRead)
    {
        bytesRead = 0;
        value = default;

        if (source.Length < 24)
            return false;
        var ___readValueByteMemory = global::Darp.BinaryObjects.Generated.Utilities.ReadUInt8Array(source[0..2], out _);
        var ___readValueByteArray = global::Darp.BinaryObjects.Generated.Utilities.ReadUInt8Array(source[2..4], out _);
        var ___readValueByteList = global::Darp.BinaryObjects.Generated.Utilities.ReadUInt8List(source[4..6], out _);
        var ___readValueByteEnumerable = global::Darp.BinaryObjects.Generated.Utilities.ReadUInt8Array(source[6..8], out _);
        var ___readValueUShortMemory = global::Darp.BinaryObjects.Generated.Utilities.ReadUInt16ArrayLittleEndian(source[8..12], out _);
        var ___readValueUShortArray = global::Darp.BinaryObjects.Generated.Utilities.ReadUInt16ArrayLittleEndian(source[12..16], out _);
        var ___readValueUShortList = global::Darp.BinaryObjects.Generated.Utilities.ReadUInt16ListLittleEndian(source[16..20], out _);
        var ___readValueUShortEnumerable = global::Darp.BinaryObjects.Generated.Utilities.ReadUInt16ArrayLittleEndian(source[20..24], out _);
        bytesRead += 24;

        value = new ArraysFixedSize(___readValueByteMemory, ___readValueByteArray, ___readValueByteList, ___readValueByteEnumerable, ___readValueUShortMemory, ___readValueUShortArray, ___readValueUShortList, ___readValueUShortEnumerable);
        return true;
    }
    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadBigEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out ArraysFixedSize? value) => TryReadBigEndian(source, out value, out _);
    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadBigEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out ArraysFixedSize? value, out int bytesRead)
    {
        bytesRead = 0;
        value = default;

        if (source.Length < 24)
            return false;
        var ___readValueByteMemory = global::Darp.BinaryObjects.Generated.Utilities.ReadUInt8Array(source[0..2], out _);
        var ___readValueByteArray = global::Darp.BinaryObjects.Generated.Utilities.ReadUInt8Array(source[2..4], out _);
        var ___readValueByteList = global::Darp.BinaryObjects.Generated.Utilities.ReadUInt8List(source[4..6], out _);
        var ___readValueByteEnumerable = global::Darp.BinaryObjects.Generated.Utilities.ReadUInt8Array(source[6..8], out _);
        var ___readValueUShortMemory = global::Darp.BinaryObjects.Generated.Utilities.ReadUInt16ArrayBigEndian(source[8..12], out _);
        var ___readValueUShortArray = global::Darp.BinaryObjects.Generated.Utilities.ReadUInt16ArrayBigEndian(source[12..16], out _);
        var ___readValueUShortList = global::Darp.BinaryObjects.Generated.Utilities.ReadUInt16ListBigEndian(source[16..20], out _);
        var ___readValueUShortEnumerable = global::Darp.BinaryObjects.Generated.Utilities.ReadUInt16ArrayBigEndian(source[20..24], out _);
        bytesRead += 24;

        value = new ArraysFixedSize(___readValueByteMemory, ___readValueByteArray, ___readValueByteList, ___readValueByteEnumerable, ___readValueUShortMemory, ___readValueUShortArray, ___readValueUShortList, ___readValueUShortEnumerable);
        return true;
    }
}

namespace Darp.BinaryObjects.Generated
{
    using Darp.BinaryObjects;
    using System;
    using System.Buffers.Binary;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    /// <summary>Helper methods used by generated BinaryObjects.</summary>
    [GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    file static class Utilities
    {
        /// <summary> Writes a <c>ReadOnlySpan&lt;byte&gt;</c> with a <c>maxElementLength</c> to the destination </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WriteUInt8Span(Span<byte> destination, ReadOnlySpan<byte> value)
        {
            var length = Math.Min(value.Length, destination.Length);
            value.Slice(0, length).CopyTo(destination);
            return length;
        }
        /// <summary> Reads a <c>byte[]</c> from the given source </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ReadUInt8Array(ReadOnlySpan<byte> source, out int bytesRead)
        {
            bytesRead = source.Length;
            return source.ToArray();
        }
        /// <summary> Writes a <c>List&lt;byte&gt;</c> with a <c>maxElementLength</c> to the destination </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WriteUInt8List(Span<byte> destination, List<byte> value)
        {
            return WriteUInt8Span(destination, CollectionsMarshal.AsSpan(value));
        }
        /// <summary> Reads a <c>List&lt;byte&gt;</c> from the given source </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<byte> ReadUInt8List(ReadOnlySpan<byte> source, out int bytesRead)
        {
            var list = new List<byte>(source.Length);
            list.AddRange(source);
            bytesRead = list.Count * 1;
            return list;
        }
        /// <summary> Writes a <c>IEnumerable&lt;byte&gt;</c> with a <c>maxElementLength</c> to the destination </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WriteUInt8Enumerable(Span<byte> destination, IEnumerable<byte> value)
        {
            switch (value)
            {
                case byte[] arrayValue:
                    return WriteUInt8Span(destination, arrayValue);
                case List<byte> listValue:
                    return WriteUInt8List(destination, listValue);
            }
            var maxElementLength = destination.Length;
            var index = 0;
            foreach (var val in value)
            {
                destination[index++] = val;
                if (index >= maxElementLength)
                    return index;
            }
            return index;
        }
        /// <summary> Writes a <c>ReadOnlySpan&lt;ushort&gt;</c> with a <c>maxElementLength</c> to the destination, as LittleEndian </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WriteUInt16SpanLittleEndian(Span<byte> destination, ReadOnlySpan<ushort> value)
        {
            var length = Math.Min(value.Length, destination.Length / 2);
            if (!BitConverter.IsLittleEndian)
            {
                Span<ushort> reinterpretedDestination = MemoryMarshal.Cast<byte, ushort>(destination);
                BinaryPrimitives.ReverseEndianness(value[..length], reinterpretedDestination);
                return length * 2;
            }
            MemoryMarshal.Cast<ushort, byte>(value[..length]).CopyTo(destination);
            return length * 2;
        }
        /// <summary> Writes a <c>ReadOnlySpan&lt;ushort&gt;</c> with a <c>maxElementLength</c> to the destination, as BigEndian </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WriteUInt16SpanBigEndian(Span<byte> destination, ReadOnlySpan<ushort> value)
        {
            var length = Math.Min(value.Length, destination.Length / 2);
            if (BitConverter.IsLittleEndian)
            {
                Span<ushort> reinterpretedDestination = MemoryMarshal.Cast<byte, ushort>(destination);
                BinaryPrimitives.ReverseEndianness(value[..length], reinterpretedDestination);
                return length * 2;
            }
            MemoryMarshal.Cast<ushort, byte>(value[..length]).CopyTo(destination);
            return length * 2;
        }
        /// <summary> Reads a <c>ushort[]</c> from the given source, as LittleEndian </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort[] ReadUInt16ArrayLittleEndian(ReadOnlySpan<byte> source, out int bytesRead)
        {
            var array = MemoryMarshal.Cast<byte, ushort>(source).ToArray();
            if (!BitConverter.IsLittleEndian)
                BinaryPrimitives.ReverseEndianness(array, array);
            bytesRead = array.Length * 2;
            return array;
        }
        /// <summary> Reads a <c>ushort[]</c> from the given source, as BigEndian </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort[] ReadUInt16ArrayBigEndian(ReadOnlySpan<byte> source, out int bytesRead)
        {
            var array = MemoryMarshal.Cast<byte, ushort>(source).ToArray();
            if (BitConverter.IsLittleEndian)
                BinaryPrimitives.ReverseEndianness(array, array);
            bytesRead = array.Length * 2;
            return array;
        }
        /// <summary> Writes a <c>List&lt;ushort&gt;</c> with a <c>maxElementLength</c> to the destination, as LittleEndian </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WriteUInt16ListLittleEndian(Span<byte> destination, List<ushort> value)
        {
            return WriteUInt16SpanLittleEndian(destination, CollectionsMarshal.AsSpan(value));
        }
        /// <summary> Writes a <c>List&lt;ushort&gt;</c> with a <c>maxElementLength</c> to the destination, as BigEndian </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WriteUInt16ListBigEndian(Span<byte> destination, List<ushort> value)
        {
            return WriteUInt16SpanBigEndian(destination, CollectionsMarshal.AsSpan(value));
        }
        /// <summary> Reads a <c>List&lt;ushort&gt;</c> from the given source, as LittleEndian </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<ushort> ReadUInt16ListLittleEndian(ReadOnlySpan<byte> source, out int bytesRead)
        {
            ReadOnlySpan<ushort> span = MemoryMarshal.Cast<byte, ushort>(source);
            var list = new List<ushort>(span.Length);
            list.AddRange(span);
            if (!BitConverter.IsLittleEndian)
            {
                Span<ushort> listSpan = CollectionsMarshal.AsSpan(list);
                BinaryPrimitives.ReverseEndianness(span, listSpan);
            }
            bytesRead = list.Count * 2;
            return list;
        }
        /// <summary> Reads a <c>List&lt;ushort&gt;</c> from the given source, as BigEndian </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<ushort> ReadUInt16ListBigEndian(ReadOnlySpan<byte> source, out int bytesRead)
        {
            ReadOnlySpan<ushort> span = MemoryMarshal.Cast<byte, ushort>(source);
            var list = new List<ushort>(span.Length);
            list.AddRange(span);
            if (BitConverter.IsLittleEndian)
            {
                Span<ushort> listSpan = CollectionsMarshal.AsSpan(list);
                BinaryPrimitives.ReverseEndianness(span, listSpan);
            }
            bytesRead = list.Count * 2;
            return list;
        }
        /// <summary> Writes a <c>IEnumerable&lt;ushort&gt;</c> with a <c>maxElementLength</c> to the destination, as LittleEndian </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WriteUInt16EnumerableLittleEndian(Span<byte> destination, IEnumerable<ushort> value)
        {
            switch (value)
            {
                case ushort[] arrayValue:
                    return WriteUInt16SpanLittleEndian(destination, arrayValue);
                case List<ushort> listValue:
                    return WriteUInt16ListLittleEndian(destination, listValue);
            }
            var maxElementLength = destination.Length / 2;
            var index = 0;
            foreach (var val in value)
            {
                BinaryPrimitives.WriteUInt16LittleEndian(destination[(2 * index++)..], val);
                if (index >= maxElementLength)
                    return index * 2;
            }
            return index * 2;
        }
        /// <summary> Writes a <c>IEnumerable&lt;ushort&gt;</c> with a <c>maxElementLength</c> to the destination, as BigEndian </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WriteUInt16EnumerableBigEndian(Span<byte> destination, IEnumerable<ushort> value)
        {
            switch (value)
            {
                case ushort[] arrayValue:
                    return WriteUInt16SpanBigEndian(destination, arrayValue);
                case List<ushort> listValue:
                    return WriteUInt16ListBigEndian(destination, listValue);
            }
            var maxElementLength = destination.Length / 2;
            var index = 0;
            foreach (var val in value)
            {
                BinaryPrimitives.WriteUInt16BigEndian(destination[(2 * index++)..], val);
                if (index >= maxElementLength)
                    return index * 2;
            }
            return index * 2;
        }
    }
}
