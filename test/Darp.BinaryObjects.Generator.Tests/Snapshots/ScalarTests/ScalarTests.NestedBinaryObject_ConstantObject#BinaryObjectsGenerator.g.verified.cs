﻿//HintName: BinaryObjectsGenerator.g.cs
// <auto-generated/>
#nullable enable

/// <remarks> <list type="table">
/// <item> <term><b>Field</b></term> <description><b>Byte Length</b></description> </item>
/// <item> <term><see cref="Value"/></term> <description>1</description> </item>
/// <item> <term> --- </term> <description>1</description> </item>
/// </list> </remarks>
[global::Darp.BinaryObjects.BinaryConstant(1)]
public sealed partial record TestObjectNested : global::Darp.BinaryObjects.IBinaryObject<TestObjectNested>
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
        global::Darp.BinaryObjects.Generated.Utilities.WriteBool(destination[0..1], this.Value);
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
        global::Darp.BinaryObjects.Generated.Utilities.WriteBool(destination[0..1], this.Value);
        bytesWritten += 1;

        return true;
    }

    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadLittleEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out TestObjectNested? value) => TryReadLittleEndian(source, out value, out _);
    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadLittleEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out TestObjectNested? value, out int bytesRead)
    {
        bytesRead = 0;
        value = default;

        if (source.Length < 1)
            return false;
        var ___readValue = global::Darp.BinaryObjects.Generated.Utilities.ReadBool(source[0..1]);
        bytesRead += 1;

        value = new TestObjectNested(___readValue);
        return true;
    }
    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadBigEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out TestObjectNested? value) => TryReadBigEndian(source, out value, out _);
    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadBigEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out TestObjectNested? value, out int bytesRead)
    {
        bytesRead = 0;
        value = default;

        if (source.Length < 1)
            return false;
        var ___readValue = global::Darp.BinaryObjects.Generated.Utilities.ReadBool(source[0..1]);
        bytesRead += 1;

        value = new TestObjectNested(___readValue);
        return true;
    }
}

/// <remarks> <list type="table">
/// <item> <term><b>Field</b></term> <description><b>Byte Length</b></description> </item>
/// <item> <term><see cref="Value"/></term> <description>1</description> </item>
/// <item> <term> --- </term> <description>1</description> </item>
/// </list> </remarks>
[global::Darp.BinaryObjects.BinaryConstant(1)]
public sealed partial record TestObject : global::Darp.BinaryObjects.IBinaryObject<TestObject>
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
        global::Darp.BinaryObjects.Generated.Utilities.WriteBinaryObjectLittleEndian(destination[0..1], this.Value);
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
        global::Darp.BinaryObjects.Generated.Utilities.WriteBinaryObjectBigEndian(destination[0..1], this.Value);
        bytesWritten += 1;

        return true;
    }

    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadLittleEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out TestObject? value) => TryReadLittleEndian(source, out value, out _);
    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadLittleEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out TestObject? value, out int bytesRead)
    {
        bytesRead = 0;
        value = default;

        if (source.Length < 1)
            return false;
        var ___readValue = global::Darp.BinaryObjects.Generated.Utilities.ReadBinaryObjectLittleEndian<TestObjectNested>(source[0..1]);
        bytesRead += 1;

        value = new TestObject(___readValue);
        return true;
    }
    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadBigEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out TestObject? value) => TryReadBigEndian(source, out value, out _);
    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadBigEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out TestObject? value, out int bytesRead)
    {
        bytesRead = 0;
        value = default;

        if (source.Length < 1)
            return false;
        var ___readValue = global::Darp.BinaryObjects.Generated.Utilities.ReadBinaryObjectBigEndian<TestObjectNested>(source[0..1]);
        bytesRead += 1;

        value = new TestObject(___readValue);
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
        /// <summary> Writes a <c>T</c> to the destination </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBinaryObjectLittleEndian<T>(Span<byte> destination, T value)
            where T : IBinaryWritable
        {
            if (!value.TryWriteLittleEndian(destination))
                throw new ArgumentOutOfRangeException(nameof(value));
        }
        /// <summary> Writes a <c>T</c> to the destination </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBinaryObjectBigEndian<T>(Span<byte> destination, T value)
            where T : IBinaryWritable
        {
            if (!value.TryWriteBigEndian(destination))
                throw new ArgumentOutOfRangeException(nameof(value));
        }
        /// <summary> Reads a <c>T</c> from the given source, as LittleEndian </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ReadBinaryObjectLittleEndian<T>(ReadOnlySpan<byte> source)
            where T : IBinaryReadable<T>
        {
            if (!T.TryReadLittleEndian(source, out var value))
                throw new ArgumentOutOfRangeException(nameof(source));
            return value;
        }
        /// <summary> Reads a <c>T</c> from the given source, as BigEndian </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ReadBinaryObjectBigEndian<T>(ReadOnlySpan<byte> source)
            where T : IBinaryReadable<T>
        {
            if (!T.TryReadBigEndian(source, out var value))
                throw new ArgumentOutOfRangeException(nameof(source));
            return value;
        }
    }
}