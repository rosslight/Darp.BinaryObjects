﻿//HintName: BinaryObjectsGenerator.g.cs
// <auto-generated/>
#nullable enable

/// <remarks> <list type="table">
/// <item> <term><b>Field</b></term> <description><b>Byte Length</b></description> </item>
/// <item> <term><see cref="Value"/></term> <description>1 * 2</description> </item>
/// <item> <term> --- </term> <description>2</description> </item>
/// </list> </remarks>
public sealed partial record ArrayByteFixedSize : global::Darp.BinaryObjects.IWritable, global::Darp.BinaryObjects.ISpanReadable<ArrayByteFixedSize>
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
        global::Darp.BinaryObjects.Generated.Utilities.WriteUInt8Span(destination[0..], this.Value, 2);
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
        global::Darp.BinaryObjects.Generated.Utilities.WriteUInt8Span(destination[0..], this.Value, 2);
        bytesWritten += 2;

        return true;
    }

    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadLittleEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out ArrayByteFixedSize? value) => TryReadLittleEndian(source, out value, out _);
    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadLittleEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out ArrayByteFixedSize? value, out int bytesRead)
    {
        bytesRead = 0;
        value = default;

        if (source.Length < 2)
            return false;
        var ___readValue = global::Darp.BinaryObjects.Generated.Utilities.ReadUInt8Array(source[0..2]);
        bytesRead += 2;

        value = new ArrayByteFixedSize(___readValue);
        return true;
    }
    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadBigEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out ArrayByteFixedSize? value) => TryReadBigEndian(source, out value, out _);
    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadBigEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out ArrayByteFixedSize? value, out int bytesRead)
    {
        bytesRead = 0;
        value = default;

        if (source.Length < 2)
            return false;
        var ___readValue = global::Darp.BinaryObjects.Generated.Utilities.ReadUInt8Array(source[0..2]);
        bytesRead += 2;

        value = new ArrayByteFixedSize(___readValue);
        return true;
    }
}

namespace Darp.BinaryObjects.Generated
{
    using System;
    using System.Buffers.Binary;
    using System.CodeDom.Compiler;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    /// <summary>Helper methods used by generated BinaryObjects.</summary>
    [GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    file static class Utilities
    {
        /// <summary> Writes a <c>ReadOnlySpan&lt;byte&gt;</c> with a <c>maxElementLength</c> to the destination </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt8Span(Span<byte> destination, ReadOnlySpan<byte> value, int maxElementLength)
        {
            var length = Math.Min(value.Length, maxElementLength);
            value.Slice(0, length).CopyTo(destination);
        }
        /// <summary> Reads a <c>byte[]</c> from the given source </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ReadUInt8Array(ReadOnlySpan<byte> source)
        {
            return source.ToArray();
        }
    }
}