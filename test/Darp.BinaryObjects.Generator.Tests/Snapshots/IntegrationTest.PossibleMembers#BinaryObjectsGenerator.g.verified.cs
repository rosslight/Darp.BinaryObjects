﻿//HintName: BinaryObjectsGenerator.g.cs
// <auto-generated/>
#nullable enable

/// <remarks> <list type="table">
/// <item> <term><b>Field</b></term> <description><b>Byte Length</b></description> </item>
/// <item> <term><see cref="ValueOne"/></term> <description>1</description> </item>
/// <item> <term><see cref="valueEight"/></term> <description>1</description> </item>
/// <item> <term><see cref="ValueFour"/></term> <description>1</description> </item>
/// <item> <term><see cref="ValueFive"/></term> <description>1</description> </item>
/// <item> <term><see cref="ValueSix"/></term> <description>1</description> </item>
/// <item> <term> --- </term> <description>5</description> </item>
/// </list> </remarks>
[global::Darp.BinaryObjects.BinaryConstant(5)]
public partial record Members1 : global::Darp.BinaryObjects.IBinaryObject<Members1>
{
    /// <inheritdoc />
    [global::System.Diagnostics.Contracts.Pure]
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public int GetByteCount() => 5;

    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public bool TryWriteLittleEndian(global::System.Span<byte> destination) => TryWriteLittleEndian(destination, out _);
    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public bool TryWriteLittleEndian(global::System.Span<byte> destination, out int bytesWritten)
    {
        bytesWritten = 0;

        if (destination.Length < 5)
            return false;
        global::Darp.BinaryObjects.Generated.Utilities.WriteUInt8(destination[0..], this.ValueOne);
        global::Darp.BinaryObjects.Generated.Utilities.WriteUInt8(destination[1..], this.valueEight);
        global::Darp.BinaryObjects.Generated.Utilities.WriteUInt8(destination[2..], this.ValueFour);
        global::Darp.BinaryObjects.Generated.Utilities.WriteUInt8(destination[3..], this.ValueFive);
        global::Darp.BinaryObjects.Generated.Utilities.WriteUInt8(destination[4..], this.ValueSix);
        bytesWritten += 5;

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

        if (destination.Length < 5)
            return false;
        global::Darp.BinaryObjects.Generated.Utilities.WriteUInt8(destination[0..], this.ValueOne);
        global::Darp.BinaryObjects.Generated.Utilities.WriteUInt8(destination[1..], this.valueEight);
        global::Darp.BinaryObjects.Generated.Utilities.WriteUInt8(destination[2..], this.ValueFour);
        global::Darp.BinaryObjects.Generated.Utilities.WriteUInt8(destination[3..], this.ValueFive);
        global::Darp.BinaryObjects.Generated.Utilities.WriteUInt8(destination[4..], this.ValueSix);
        bytesWritten += 5;

        return true;
    }

    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadLittleEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out Members1? value) => TryReadLittleEndian(source, out value, out _);
    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadLittleEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out Members1? value, out int bytesRead)
    {
        bytesRead = 0;
        value = default;

        if (source.Length < 5)
            return false;
        var ___readValueOne = global::Darp.BinaryObjects.Generated.Utilities.ReadUInt8(source[0..1]);
        var ___readvalueEight = global::Darp.BinaryObjects.Generated.Utilities.ReadUInt8(source[1..2]);
        var ___readValueFour = global::Darp.BinaryObjects.Generated.Utilities.ReadUInt8(source[2..3]);
        var ___readValueFive = global::Darp.BinaryObjects.Generated.Utilities.ReadUInt8(source[3..4]);
        var ___readValueSix = global::Darp.BinaryObjects.Generated.Utilities.ReadUInt8(source[4..5]);
        bytesRead += 5;

        value = new Members1(___readValueOne)
        {
            valueEight = ___readvalueEight,
            ValueFour = ___readValueFour,
            ValueFive = ___readValueFive,
            ValueSix = ___readValueSix,
        };
        return true;
    }
    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadBigEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out Members1? value) => TryReadBigEndian(source, out value, out _);
    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadBigEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out Members1? value, out int bytesRead)
    {
        bytesRead = 0;
        value = default;

        if (source.Length < 5)
            return false;
        var ___readValueOne = global::Darp.BinaryObjects.Generated.Utilities.ReadUInt8(source[0..1]);
        var ___readvalueEight = global::Darp.BinaryObjects.Generated.Utilities.ReadUInt8(source[1..2]);
        var ___readValueFour = global::Darp.BinaryObjects.Generated.Utilities.ReadUInt8(source[2..3]);
        var ___readValueFive = global::Darp.BinaryObjects.Generated.Utilities.ReadUInt8(source[3..4]);
        var ___readValueSix = global::Darp.BinaryObjects.Generated.Utilities.ReadUInt8(source[4..5]);
        bytesRead += 5;

        value = new Members1(___readValueOne)
        {
            valueEight = ___readvalueEight,
            ValueFour = ___readValueFour,
            ValueFive = ___readValueFive,
            ValueSix = ___readValueSix,
        };
        return true;
    }
}

/// <remarks> <list type="table">
/// <item> <term><b>Field</b></term> <description><b>Byte Length</b></description> </item>
/// <item> <term> --- </term> <description>0</description> </item>
/// </list> </remarks>
[global::Darp.BinaryObjects.BinaryConstant(0)]
public partial record OneBool2 : global::Darp.BinaryObjects.IBinaryObject<OneBool2>
{
    /// <inheritdoc />
    [global::System.Diagnostics.Contracts.Pure]
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public int GetByteCount() => 0;

    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public bool TryWriteLittleEndian(global::System.Span<byte> destination) => TryWriteLittleEndian(destination, out _);
    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public bool TryWriteLittleEndian(global::System.Span<byte> destination, out int bytesWritten)
    {
        bytesWritten = 0;

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

        return true;
    }

    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadLittleEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out OneBool2? value) => TryReadLittleEndian(source, out value, out _);
    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadLittleEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out OneBool2? value, out int bytesRead)
    {
        bytesRead = 0;

        value = new OneBool2();
        return true;
    }
    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadBigEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out OneBool2? value) => TryReadBigEndian(source, out value, out _);
    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadBigEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out OneBool2? value, out int bytesRead)
    {
        bytesRead = 0;

        value = new OneBool2();
        return true;
    }
}

/// <remarks> <list type="table">
/// <item> <term><b>Field</b></term> <description><b>Byte Length</b></description> </item>
/// <item> <term><see cref="Value"/></term> <description>1</description> </item>
/// <item> <term> --- </term> <description>1</description> </item>
/// </list> </remarks>
[global::Darp.BinaryObjects.BinaryConstant(1)]
public partial record OneBool : global::Darp.BinaryObjects.IBinaryObject<OneBool>
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

        value = new OneBool()
        {
            Value = ___readValue,
        };
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

        value = new OneBool()
        {
            Value = ___readValue,
        };
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
    }
}
