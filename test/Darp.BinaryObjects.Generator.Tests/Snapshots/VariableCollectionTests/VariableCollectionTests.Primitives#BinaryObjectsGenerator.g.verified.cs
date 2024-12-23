﻿//HintName: BinaryObjectsGenerator.g.cs
// <auto-generated/>
#nullable enable

/// <remarks> <list type="table">
/// <item> <term><b>Field</b></term> <description><b>Byte Length</b></description> </item>
/// <item> <term><see cref="Length"/></term> <description>1</description> </item>
/// <item> <term><see cref="Value"/></term> <description>1 * <see cref="Length"/></description> </item>
/// <item> <term> --- </term> <description>1 + 1 * <see cref="Length"/></description> </item>
/// </list> </remarks>
public sealed partial record TestObject : global::Darp.BinaryObjects.IBinaryObject<TestObject>
{
    /// <inheritdoc />
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public int GetByteCount() => 1 + 1 * this.Length;

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
        global::Darp.BinaryObjects.Generated.Utilities.WriteUInt8(destination[0..1], this.Length);
        destination = destination[1..];
        bytesWritten += 1;

        if (destination.Length < this.Length)
            return false;
        global::Darp.BinaryObjects.Generated.Utilities.WriteUInt8Span(destination[0..this.Length], this.Value);
        bytesWritten += this.Length;

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
        global::Darp.BinaryObjects.Generated.Utilities.WriteUInt8(destination[0..1], this.Length);
        destination = destination[1..];
        bytesWritten += 1;

        if (destination.Length < this.Length)
            return false;
        global::Darp.BinaryObjects.Generated.Utilities.WriteUInt8Span(destination[0..this.Length], this.Value);
        bytesWritten += this.Length;

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
        var ___readLength = global::Darp.BinaryObjects.Generated.Utilities.ReadUInt8(source[0..1]);
        source = source[1..];
        bytesRead += 1;

        if (source.Length < ___readLength)
            return false;
        var ___readValue = global::Darp.BinaryObjects.Generated.Utilities.ReadUInt8Array(source[0..___readLength], out _);
        bytesRead += ___readLength;

        value = new TestObject(___readLength, ___readValue);
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
        var ___readLength = global::Darp.BinaryObjects.Generated.Utilities.ReadUInt8(source[0..1]);
        source = source[1..];
        bytesRead += 1;

        if (source.Length < ___readLength)
            return false;
        var ___readValue = global::Darp.BinaryObjects.Generated.Utilities.ReadUInt8Array(source[0..___readLength], out _);
        bytesRead += ___readLength;

        value = new TestObject(___readLength, ___readValue);
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
        /// <summary> Writes a <c>byte</c> to the destination </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt8(Span<byte> destination, byte value)
        {
            destination[0] = value;
        }
        /// <summary> Reads a <c>byte</c> from the given source </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte ReadUInt8(ReadOnlySpan<byte> source)
        {
            return source[0];
        }
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
    }
}