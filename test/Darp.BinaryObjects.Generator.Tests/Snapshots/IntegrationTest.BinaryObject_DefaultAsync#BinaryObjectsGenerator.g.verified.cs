﻿//HintName: BinaryObjectsGenerator.g.cs
// <auto-generated/>
#nullable enable

/// <remarks> <list type="table">
/// <item> <term><b>Field</b></term> <description><b>Byte Length</b></description> </item>
/// <item> <term><see cref="Value"/></term> <description>1</description> </item>
/// <item> <term> --- </term> <description>1</description> </item>
/// </list> </remarks>
public sealed partial record OneBool : global::Darp.BinaryObjects.IWritable, global::Darp.BinaryObjects.ISpanReadable<OneBool>
{
    /// <inheritdoc />
    [global::System.Diagnostics.Contracts.Pure]
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public int GetByteCount() => 1;

    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public bool TryWriteLittleEndian(global::System.Span<byte> destination) => TryWriteLittleEndian(destination, out _);
    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public bool TryWriteLittleEndian(global::System.Span<byte> destination, out int bytesWritten)
    {
        bytesWritten = 0;

        if (destination.Length < 1)
            return false;
        global::Darp.BinaryObjects.Generated.Utilities.WriteBool(destination[0..], this.Value);
        bytesWritten += 1;

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

        if (destination.Length < 1)
            return false;
        global::Darp.BinaryObjects.Generated.Utilities.WriteBool(destination[0..], this.Value);
        bytesWritten += 1;

        return true;
    }

    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadLittleEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out OneBool? value) => TryReadLittleEndian(source, out value, out _);
    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadLittleEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out OneBool? value, out int bytesRead)
    {
        bytesRead = 0;
        value = default;

        if (source.Length < 1)
            return false;
        var ___readValue = global::Darp.BinaryObjects.Generated.Utilities.ReadBool(source[0..1]);
        bytesRead += 1;

        value = new OneBool(___readValue);
        return true;
    }
    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadBigEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out OneBool? value) => TryReadBigEndian(source, out value, out _);
    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadBigEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out OneBool? value, out int bytesRead)
    {
        bytesRead = 0;
        value = default;

        if (source.Length < 1)
            return false;
        var ___readValue = global::Darp.BinaryObjects.Generated.Utilities.ReadBool(source[0..1]);
        bytesRead += 1;

        value = new OneBool(___readValue);
        return true;
    }
}

/// <remarks> <list type="table">
/// <item> <term><b>Field</b></term> <description><b>Byte Length</b></description> </item>
/// <item> <term><see cref="Value"/></term> <description>1 * 2</description> </item>
/// <item> <term> --- </term> <description>2</description> </item>
/// </list> </remarks>
public sealed partial record OneArray : global::Darp.BinaryObjects.IWritable, global::Darp.BinaryObjects.ISpanReadable<OneArray>
{
    /// <inheritdoc />
    [global::System.Diagnostics.Contracts.Pure]
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public int GetByteCount() => 2;

    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public bool TryWriteLittleEndian(global::System.Span<byte> destination) => TryWriteLittleEndian(destination, out _);
    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public bool TryWriteLittleEndian(global::System.Span<byte> destination, out int bytesWritten)
    {
        bytesWritten = 0;

        if (destination.Length < 2)
            return false;
        global::Darp.BinaryObjects.Generated.Utilities.WriteBoolSpan(destination[0..], this.Value, 2);
        bytesWritten += 2;

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

        if (destination.Length < 2)
            return false;
        global::Darp.BinaryObjects.Generated.Utilities.WriteBoolSpan(destination[0..], this.Value, 2);
        bytesWritten += 2;

        return true;
    }

    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadLittleEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out OneArray? value) => TryReadLittleEndian(source, out value, out _);
    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadLittleEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out OneArray? value, out int bytesRead)
    {
        bytesRead = 0;
        value = default;

        if (source.Length < 2)
            return false;
        var ___readValue = global::Darp.BinaryObjects.Generated.Utilities.ReadBoolArray(source[0..2]);
        bytesRead += 2;

        value = new OneArray(___readValue);
        return true;
    }
    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadBigEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out OneArray? value) => TryReadBigEndian(source, out value, out _);
    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadBigEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out OneArray? value, out int bytesRead)
    {
        bytesRead = 0;
        value = default;

        if (source.Length < 2)
            return false;
        var ___readValue = global::Darp.BinaryObjects.Generated.Utilities.ReadBoolArray(source[0..2]);
        bytesRead += 2;

        value = new OneArray(___readValue);
        return true;
    }
}

/// <remarks> <list type="table">
/// <item> <term><b>Field</b></term> <description><b>Byte Length</b></description> </item>
/// <item> <term><see cref="Value"/></term> <description>1</description> </item>
/// <item> <term><see cref="Array"/></term> <description><see cref="OneArray.GetByteCount()"/></description> </item>
/// <item> <term> --- </term> <description>1 + <see cref="OneArray.GetByteCount()"/></description> </item>
/// </list> </remarks>
public sealed partial record OneBinaryObject : global::Darp.BinaryObjects.IWritable, global::Darp.BinaryObjects.ISpanReadable<OneBinaryObject>
{
    /// <inheritdoc />
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public int GetByteCount() => 1 + this.Array.GetByteCount();

    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public bool TryWriteLittleEndian(global::System.Span<byte> destination) => TryWriteLittleEndian(destination, out _);
    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public bool TryWriteLittleEndian(global::System.Span<byte> destination, out int bytesWritten)
    {
        bytesWritten = 0;

        if (destination.Length < 1)
            return false;
        global::Darp.BinaryObjects.Generated.Utilities.WriteBinaryObjectLittleEndian(destination[0..], this.Value);
        bytesWritten += 1;

        if (!this.Array.TryWriteLittleEndian(destination[1..], out var ___bytesWrittenArray))
            return false;
        bytesWritten += ___bytesWrittenArray;

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

        if (destination.Length < 1)
            return false;
        global::Darp.BinaryObjects.Generated.Utilities.WriteBinaryObjectBigEndian(destination[0..], this.Value);
        bytesWritten += 1;

        if (!this.Array.TryWriteBigEndian(destination[1..], out var ___bytesWrittenArray))
            return false;
        bytesWritten += ___bytesWrittenArray;

        return true;
    }

    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadLittleEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out OneBinaryObject? value) => TryReadLittleEndian(source, out value, out _);
    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadLittleEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out OneBinaryObject? value, out int bytesRead)
    {
        bytesRead = 0;
        value = default;

        if (source.Length < 1)
            return false;
        var ___readValue = global::Darp.BinaryObjects.Generated.Utilities.ReadBinaryObjectLittleEndian<OneBool>(source[0..1]);
        bytesRead += 1;

        if (!OneArray.TryReadLittleEndian(source[1..], out var ___readArray, out var ___bytesReadArray))
            return false;
        bytesRead += ___bytesReadArray;

        value = new OneBinaryObject(___readValue, ___readArray);
        return true;
    }
    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadBigEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out OneBinaryObject? value) => TryReadBigEndian(source, out value, out _);
    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadBigEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out OneBinaryObject? value, out int bytesRead)
    {
        bytesRead = 0;
        value = default;

        if (source.Length < 1)
            return false;
        var ___readValue = global::Darp.BinaryObjects.Generated.Utilities.ReadBinaryObjectBigEndian<OneBool>(source[0..1]);
        bytesRead += 1;

        if (!OneArray.TryReadBigEndian(source[1..], out var ___readArray, out var ___bytesReadArray))
            return false;
        bytesRead += ___bytesReadArray;

        value = new OneBinaryObject(___readValue, ___readArray);
        return true;
    }
}

namespace Darp.BinaryObjects.Generated
{
    using Darp.BinaryObjects;
    using System;
    using System.Buffers.Binary;
    using System.CodeDom.Compiler;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    /// <summary>Helper methods used by generated BinaryObjects.</summary>
    [GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    file static class Utilities
    {
        /// <summary> Writes a <c>bool</c> to the destination </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBool(Span<byte> destination, bool value)
        {
            destination[0] = value ? (byte)0b1 : (byte)0b0;
        }
        /// <summary> Reads a <c>bool</c> from the given source </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ReadBool(ReadOnlySpan<byte> source)
        {
            return source[0] > 0;
        }
        /// <summary> Writes a <c>ReadOnlySpan&lt;bool&gt;</c> with a <c>maxElementLength</c> to the destination </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBoolSpan(Span<byte> destination, ReadOnlySpan<bool> value, int maxElementLength)
        {
            var length = Math.Min(value.Length, maxElementLength);
            MemoryMarshal.Cast<bool, byte>(value.Slice(0, length)).CopyTo(destination);
        }
        /// <summary> Reads a <c>bool[]</c> from the given source </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool[] ReadBoolArray(ReadOnlySpan<byte> source)
        {
            return MemoryMarshal.Cast<byte, bool>(source).ToArray();
        }
        /// <summary> Writes a <c>T</c> to the destination </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBinaryObjectLittleEndian<T>(Span<byte> destination, T value)
            where T : IWritable
        {
            if (!value.TryWriteLittleEndian(destination))
                throw new ArgumentOutOfRangeException(nameof(value));
        }
        /// <summary> Writes a <c>T</c> to the destination </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBinaryObjectBigEndian<T>(Span<byte> destination, T value)
            where T : IWritable
        {
            if (!value.TryWriteBigEndian(destination))
                throw new ArgumentOutOfRangeException(nameof(value));
        }
        /// <summary> Reads a <c>T</c> from the given source, as LittleEndian </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ReadBinaryObjectLittleEndian<T>(ReadOnlySpan<byte> source)
            where T : ISpanReadable<T>
        {
            if (!T.TryReadLittleEndian(source, out var value))
                throw new ArgumentOutOfRangeException(nameof(source));
            return value;
        }
        /// <summary> Reads a <c>T</c> from the given source, as BigEndian </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ReadBinaryObjectBigEndian<T>(ReadOnlySpan<byte> source)
            where T : ISpanReadable<T>
        {
            if (!T.TryReadBigEndian(source, out var value))
                throw new ArgumentOutOfRangeException(nameof(source));
            return value;
        }
    }
}
