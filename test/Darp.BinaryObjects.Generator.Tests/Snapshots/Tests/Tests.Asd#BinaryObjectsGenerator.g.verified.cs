﻿//HintName: BinaryObjectsGenerator.g.cs
// <auto-generated/>
#nullable enable

/// <remarks> <list type="table">
/// <item> <term><b>Field</b></term> <description><b>Byte Length</b></description> </item>
/// <item> <term><see cref="Value"/></term> <description>1</description> </item>
/// <item> <term> --- </term> <description>1</description> </item>
/// </list> </remarks>
[global::Darp.BinaryObjects.BinaryConstant(1)]
public sealed partial record OneBool1 : global::Darp.BinaryObjects.IBinaryObject<OneBool1>
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
    public static bool TryReadLittleEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out OneBool1? value) => TryReadLittleEndian(source, out value, out _);
    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadLittleEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out OneBool1? value, out int bytesRead)
    {
        bytesRead = 0;
        value = default;

        if (source.Length < 1)
            return false;
        var ___readValue = global::Darp.BinaryObjects.Generated.Utilities.ReadBool(source[0..1]);
        bytesRead += 1;

        value = new OneBool1(___readValue);
        return true;
    }
    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadBigEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out OneBool1? value) => TryReadBigEndian(source, out value, out _);
    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadBigEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out OneBool1? value, out int bytesRead)
    {
        bytesRead = 0;
        value = default;

        if (source.Length < 1)
            return false;
        var ___readValue = global::Darp.BinaryObjects.Generated.Utilities.ReadBool(source[0..1]);
        bytesRead += 1;

        value = new OneBool1(___readValue);
        return true;
    }
}

/// <remarks> <list type="table">
/// <item> <term><b>Field</b></term> <description><b>Byte Length</b></description> </item>
/// <item> <term><see cref="Value"/></term> <description>1</description> </item>
/// <item> <term><see cref="Value2"/></term> <description>1</description> </item>
/// <item> <term> --- </term> <description>2</description> </item>
/// </list> </remarks>
[global::Darp.BinaryObjects.BinaryConstant(2)]
public sealed partial record OneBool2 : global::Darp.BinaryObjects.IBinaryObject<OneBool2>
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
        global::Darp.BinaryObjects.Generated.Utilities.WriteBool(destination[0..1], this.Value);
        global::Darp.BinaryObjects.Generated.Utilities.WriteBool(destination[1..2], this.Value2);
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
        global::Darp.BinaryObjects.Generated.Utilities.WriteBool(destination[0..1], this.Value);
        global::Darp.BinaryObjects.Generated.Utilities.WriteBool(destination[1..2], this.Value2);
        bytesWritten += 2;

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
        value = default;

        if (source.Length < 2)
            return false;
        var ___readValue = global::Darp.BinaryObjects.Generated.Utilities.ReadBool(source[0..1]);
        var ___readValue2 = global::Darp.BinaryObjects.Generated.Utilities.ReadBool(source[1..2]);
        bytesRead += 2;

        value = new OneBool2()
        {
            Value = ___readValue,
            Value2 = ___readValue2,
        };
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
        value = default;

        if (source.Length < 2)
            return false;
        var ___readValue = global::Darp.BinaryObjects.Generated.Utilities.ReadBool(source[0..1]);
        var ___readValue2 = global::Darp.BinaryObjects.Generated.Utilities.ReadBool(source[1..2]);
        bytesRead += 2;

        value = new OneBool2()
        {
            Value = ___readValue,
            Value2 = ___readValue2,
        };
        return true;
    }
}

/// <remarks> <list type="table">
/// <item> <term><b>Field</b></term> <description><b>Byte Length</b></description> </item>
/// <item> <term><see cref="Value2"/></term> <description>1</description> </item>
/// <item> <term><see cref="Value3"/></term> <description>1</description> </item>
/// <item> <term><see cref="Value"/></term> <description>1</description> </item>
/// <item> <term> --- </term> <description>3</description> </item>
/// </list> </remarks>
[global::Darp.BinaryObjects.BinaryConstant(3)]
public sealed partial record OneBool3 : global::Darp.BinaryObjects.IBinaryObject<OneBool3>
{
    /// <inheritdoc />
    [global::System.Diagnostics.Contracts.Pure]
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public int GetByteCount() => 3;

    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public bool TryWriteLittleEndian(global::System.Span<byte> destination) => TryWriteLittleEndian(destination, out _);
    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public bool TryWriteLittleEndian(global::System.Span<byte> destination, out int bytesWritten)
    {
        bytesWritten = 0;

        if (destination.Length < 3)
            return false;
        global::Darp.BinaryObjects.Generated.Utilities.WriteBool(destination[0..1], this.Value2);
        global::Darp.BinaryObjects.Generated.Utilities.WriteBool(destination[1..2], this.Value3);
        global::Darp.BinaryObjects.Generated.Utilities.WriteBool(destination[2..3], this.Value);
        bytesWritten += 3;

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

        if (destination.Length < 3)
            return false;
        global::Darp.BinaryObjects.Generated.Utilities.WriteBool(destination[0..1], this.Value2);
        global::Darp.BinaryObjects.Generated.Utilities.WriteBool(destination[1..2], this.Value3);
        global::Darp.BinaryObjects.Generated.Utilities.WriteBool(destination[2..3], this.Value);
        bytesWritten += 3;

        return true;
    }

    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadLittleEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out OneBool3? value) => TryReadLittleEndian(source, out value, out _);
    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadLittleEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out OneBool3? value, out int bytesRead)
    {
        bytesRead = 0;
        value = default;

        if (source.Length < 3)
            return false;
        var ___readValue2 = global::Darp.BinaryObjects.Generated.Utilities.ReadBool(source[0..1]);
        var ___readValue3 = global::Darp.BinaryObjects.Generated.Utilities.ReadBool(source[1..2]);
        var ___readValue = global::Darp.BinaryObjects.Generated.Utilities.ReadBool(source[2..3]);
        bytesRead += 3;

        value = new OneBool3(___readValue3, ___readValue)
        {
            Value2 = ___readValue2,
        };
        return true;
    }
    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadBigEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out OneBool3? value) => TryReadBigEndian(source, out value, out _);
    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadBigEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out OneBool3? value, out int bytesRead)
    {
        bytesRead = 0;
        value = default;

        if (source.Length < 3)
            return false;
        var ___readValue2 = global::Darp.BinaryObjects.Generated.Utilities.ReadBool(source[0..1]);
        var ___readValue3 = global::Darp.BinaryObjects.Generated.Utilities.ReadBool(source[1..2]);
        var ___readValue = global::Darp.BinaryObjects.Generated.Utilities.ReadBool(source[2..3]);
        bytesRead += 3;

        value = new OneBool3(___readValue3, ___readValue)
        {
            Value2 = ___readValue2,
        };
        return true;
    }
}

/// <remarks> <list type="table">
/// <item> <term><b>Field</b></term> <description><b>Byte Length</b></description> </item>
/// <item> <term> --- </term> <description>0</description> </item>
/// </list> </remarks>
[global::Darp.BinaryObjects.BinaryConstant(0)]
public sealed partial record OneBool4 : global::Darp.BinaryObjects.IBinaryObject<OneBool4>
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
    public static bool TryReadLittleEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out OneBool4? value) => TryReadLittleEndian(source, out value, out _);
    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadLittleEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out OneBool4? value, out int bytesRead)
    {
        bytesRead = 0;

        value = new OneBool4();
        return true;
    }
    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadBigEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out OneBool4? value) => TryReadBigEndian(source, out value, out _);
    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadBigEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out OneBool4? value, out int bytesRead)
    {
        bytesRead = 0;

        value = new OneBool4();
        return true;
    }
}

/// <remarks> <list type="table">
/// <item> <term><b>Field</b></term> <description><b>Byte Length</b></description> </item>
/// <item> <term><see cref="_value1"/></term> <description>1</description> </item>
/// <item> <term><see cref="_value2"/></term> <description>1</description> </item>
/// <item> <term> --- </term> <description>2</description> </item>
/// </list> </remarks>
[global::Darp.BinaryObjects.BinaryConstant(2)]
public sealed partial class OneBool5 : global::Darp.BinaryObjects.IBinaryObject<OneBool5>
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
        global::Darp.BinaryObjects.Generated.Utilities.WriteBool(destination[0..1], this._value1);
        global::Darp.BinaryObjects.Generated.Utilities.WriteBool(destination[1..2], this._value2);
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
        global::Darp.BinaryObjects.Generated.Utilities.WriteBool(destination[0..1], this._value1);
        global::Darp.BinaryObjects.Generated.Utilities.WriteBool(destination[1..2], this._value2);
        bytesWritten += 2;

        return true;
    }

    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadLittleEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out OneBool5? value) => TryReadLittleEndian(source, out value, out _);
    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadLittleEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out OneBool5? value, out int bytesRead)
    {
        bytesRead = 0;
        value = default;

        if (source.Length < 2)
            return false;
        var ___read_value1 = global::Darp.BinaryObjects.Generated.Utilities.ReadBool(source[0..1]);
        var ___read_value2 = global::Darp.BinaryObjects.Generated.Utilities.ReadBool(source[1..2]);
        bytesRead += 2;

        value = new OneBool5(___read_value1, ___read_value2);
        return true;
    }
    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadBigEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out OneBool5? value) => TryReadBigEndian(source, out value, out _);
    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadBigEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out OneBool5? value, out int bytesRead)
    {
        bytesRead = 0;
        value = default;

        if (source.Length < 2)
            return false;
        var ___read_value1 = global::Darp.BinaryObjects.Generated.Utilities.ReadBool(source[0..1]);
        var ___read_value2 = global::Darp.BinaryObjects.Generated.Utilities.ReadBool(source[1..2]);
        bytesRead += 2;

        value = new OneBool5(___read_value1, ___read_value2);
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
    }
}
