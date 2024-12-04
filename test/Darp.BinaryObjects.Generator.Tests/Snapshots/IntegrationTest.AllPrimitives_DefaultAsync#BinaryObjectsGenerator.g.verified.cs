﻿//HintName: BinaryObjectsGenerator.g.cs
// <auto-generated/>
#nullable enable

/// <remarks> <list type="table">
/// <item> <term><b>Field</b></term> <description><b>Byte Length</b></description> </item>
/// <item> <term><see cref="ValueBool"/></term> <description>1</description> </item>
/// <item> <term><see cref="ValueSByte"/></term> <description>1</description> </item>
/// <item> <term><see cref="ValueShort"/></term> <description>2</description> </item>
/// <item> <term><see cref="ValueHalf"/></term> <description>2</description> </item>
/// <item> <term><see cref="ValueInt"/></term> <description>4</description> </item>
/// <item> <term><see cref="ValueFloat"/></term> <description>4</description> </item>
/// <item> <term><see cref="ValueLong"/></term> <description>8</description> </item>
/// <item> <term><see cref="ValueInt128"/></term> <description>16</description> </item>
/// <item> <term><see cref="ValueUInt128"/></term> <description>16</description> </item>
/// <item> <term><see cref="ValueULong"/></term> <description>8</description> </item>
/// <item> <term><see cref="ValueDouble"/></term> <description>8</description> </item>
/// <item> <term><see cref="ValueUInt"/></term> <description>4</description> </item>
/// <item> <term><see cref="ValueUShort"/></term> <description>2</description> </item>
/// <item> <term><see cref="ValueByte"/></term> <description>1</description> </item>
/// <item> <term> --- </term> <description>77</description> </item>
/// </list> </remarks>
public sealed partial record AllPrimitives : global::Darp.BinaryObjects.IBinaryConstantObject<AllPrimitives>
{
    /// <inheritdoc />
    [global::System.Diagnostics.Contracts.Pure]
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public int GetByteCount() => 77;
    
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    static int global::Darp.BinaryObjects.IBinaryConstantReadable<AllPrimitives>.ByteCount => 77;

    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public bool TryWriteLittleEndian(global::System.Span<byte> destination) => TryWriteLittleEndian(destination, out _);
    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public bool TryWriteLittleEndian(global::System.Span<byte> destination, out int bytesWritten)
    {
        bytesWritten = 0;

        if (destination.Length < 77)
            return false;
        global::Darp.BinaryObjects.Generated.Utilities.WriteBool(destination[0..], this.ValueBool);
        global::Darp.BinaryObjects.Generated.Utilities.WriteInt8(destination[1..], this.ValueSByte);
        global::Darp.BinaryObjects.Generated.Utilities.WriteInt16LittleEndian(destination[2..], this.ValueShort);
        global::Darp.BinaryObjects.Generated.Utilities.WriteHalfLittleEndian(destination[4..], this.ValueHalf);
        global::Darp.BinaryObjects.Generated.Utilities.WriteInt32LittleEndian(destination[6..], this.ValueInt);
        global::Darp.BinaryObjects.Generated.Utilities.WriteSingleLittleEndian(destination[10..], this.ValueFloat);
        global::Darp.BinaryObjects.Generated.Utilities.WriteInt64LittleEndian(destination[14..], this.ValueLong);
        global::Darp.BinaryObjects.Generated.Utilities.WriteInt128LittleEndian(destination[22..], this.ValueInt128);
        global::Darp.BinaryObjects.Generated.Utilities.WriteUInt128LittleEndian(destination[38..], this.ValueUInt128);
        global::Darp.BinaryObjects.Generated.Utilities.WriteUInt64LittleEndian(destination[54..], this.ValueULong);
        global::Darp.BinaryObjects.Generated.Utilities.WriteDoubleLittleEndian(destination[62..], this.ValueDouble);
        global::Darp.BinaryObjects.Generated.Utilities.WriteUInt32LittleEndian(destination[70..], this.ValueUInt);
        global::Darp.BinaryObjects.Generated.Utilities.WriteUInt16LittleEndian(destination[74..], this.ValueUShort);
        global::Darp.BinaryObjects.Generated.Utilities.WriteUInt8(destination[76..], this.ValueByte);
        bytesWritten += 77;

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

        if (destination.Length < 77)
            return false;
        global::Darp.BinaryObjects.Generated.Utilities.WriteBool(destination[0..], this.ValueBool);
        global::Darp.BinaryObjects.Generated.Utilities.WriteInt8(destination[1..], this.ValueSByte);
        global::Darp.BinaryObjects.Generated.Utilities.WriteInt16BigEndian(destination[2..], this.ValueShort);
        global::Darp.BinaryObjects.Generated.Utilities.WriteHalfBigEndian(destination[4..], this.ValueHalf);
        global::Darp.BinaryObjects.Generated.Utilities.WriteInt32BigEndian(destination[6..], this.ValueInt);
        global::Darp.BinaryObjects.Generated.Utilities.WriteSingleBigEndian(destination[10..], this.ValueFloat);
        global::Darp.BinaryObjects.Generated.Utilities.WriteInt64BigEndian(destination[14..], this.ValueLong);
        global::Darp.BinaryObjects.Generated.Utilities.WriteInt128BigEndian(destination[22..], this.ValueInt128);
        global::Darp.BinaryObjects.Generated.Utilities.WriteUInt128BigEndian(destination[38..], this.ValueUInt128);
        global::Darp.BinaryObjects.Generated.Utilities.WriteUInt64BigEndian(destination[54..], this.ValueULong);
        global::Darp.BinaryObjects.Generated.Utilities.WriteDoubleBigEndian(destination[62..], this.ValueDouble);
        global::Darp.BinaryObjects.Generated.Utilities.WriteUInt32BigEndian(destination[70..], this.ValueUInt);
        global::Darp.BinaryObjects.Generated.Utilities.WriteUInt16BigEndian(destination[74..], this.ValueUShort);
        global::Darp.BinaryObjects.Generated.Utilities.WriteUInt8(destination[76..], this.ValueByte);
        bytesWritten += 77;

        return true;
    }

    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadLittleEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out AllPrimitives? value) => TryReadLittleEndian(source, out value, out _);
    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadLittleEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out AllPrimitives? value, out int bytesRead)
    {
        bytesRead = 0;
        value = default;

        if (source.Length < 77)
            return false;
        var ___readValueBool = global::Darp.BinaryObjects.Generated.Utilities.ReadBool(source[0..1]);
        var ___readValueSByte = global::Darp.BinaryObjects.Generated.Utilities.ReadInt8(source[1..2]);
        var ___readValueShort = global::Darp.BinaryObjects.Generated.Utilities.ReadInt16LittleEndian(source[2..4]);
        var ___readValueHalf = global::Darp.BinaryObjects.Generated.Utilities.ReadHalfLittleEndian(source[4..6]);
        var ___readValueInt = global::Darp.BinaryObjects.Generated.Utilities.ReadInt32LittleEndian(source[6..10]);
        var ___readValueFloat = global::Darp.BinaryObjects.Generated.Utilities.ReadSingleLittleEndian(source[10..14]);
        var ___readValueLong = global::Darp.BinaryObjects.Generated.Utilities.ReadInt64LittleEndian(source[14..22]);
        var ___readValueInt128 = global::Darp.BinaryObjects.Generated.Utilities.ReadInt128LittleEndian(source[22..38]);
        var ___readValueUInt128 = global::Darp.BinaryObjects.Generated.Utilities.ReadUInt128LittleEndian(source[38..54]);
        var ___readValueULong = global::Darp.BinaryObjects.Generated.Utilities.ReadUInt64LittleEndian(source[54..62]);
        var ___readValueDouble = global::Darp.BinaryObjects.Generated.Utilities.ReadDoubleLittleEndian(source[62..70]);
        var ___readValueUInt = global::Darp.BinaryObjects.Generated.Utilities.ReadUInt32LittleEndian(source[70..74]);
        var ___readValueUShort = global::Darp.BinaryObjects.Generated.Utilities.ReadUInt16LittleEndian(source[74..76]);
        var ___readValueByte = global::Darp.BinaryObjects.Generated.Utilities.ReadUInt8(source[76..77]);
        bytesRead += 77;

        value = new AllPrimitives(___readValueBool, ___readValueSByte, ___readValueShort, ___readValueHalf, ___readValueInt, ___readValueFloat, ___readValueLong, ___readValueInt128, ___readValueUInt128, ___readValueULong, ___readValueDouble, ___readValueUInt, ___readValueUShort, ___readValueByte);
        return true;
    }
    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadBigEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out AllPrimitives? value) => TryReadBigEndian(source, out value, out _);
    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
    public static bool TryReadBigEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out AllPrimitives? value, out int bytesRead)
    {
        bytesRead = 0;
        value = default;

        if (source.Length < 77)
            return false;
        var ___readValueBool = global::Darp.BinaryObjects.Generated.Utilities.ReadBool(source[0..1]);
        var ___readValueSByte = global::Darp.BinaryObjects.Generated.Utilities.ReadInt8(source[1..2]);
        var ___readValueShort = global::Darp.BinaryObjects.Generated.Utilities.ReadInt16BigEndian(source[2..4]);
        var ___readValueHalf = global::Darp.BinaryObjects.Generated.Utilities.ReadHalfBigEndian(source[4..6]);
        var ___readValueInt = global::Darp.BinaryObjects.Generated.Utilities.ReadInt32BigEndian(source[6..10]);
        var ___readValueFloat = global::Darp.BinaryObjects.Generated.Utilities.ReadSingleBigEndian(source[10..14]);
        var ___readValueLong = global::Darp.BinaryObjects.Generated.Utilities.ReadInt64BigEndian(source[14..22]);
        var ___readValueInt128 = global::Darp.BinaryObjects.Generated.Utilities.ReadInt128BigEndian(source[22..38]);
        var ___readValueUInt128 = global::Darp.BinaryObjects.Generated.Utilities.ReadUInt128BigEndian(source[38..54]);
        var ___readValueULong = global::Darp.BinaryObjects.Generated.Utilities.ReadUInt64BigEndian(source[54..62]);
        var ___readValueDouble = global::Darp.BinaryObjects.Generated.Utilities.ReadDoubleBigEndian(source[62..70]);
        var ___readValueUInt = global::Darp.BinaryObjects.Generated.Utilities.ReadUInt32BigEndian(source[70..74]);
        var ___readValueUShort = global::Darp.BinaryObjects.Generated.Utilities.ReadUInt16BigEndian(source[74..76]);
        var ___readValueByte = global::Darp.BinaryObjects.Generated.Utilities.ReadUInt8(source[76..77]);
        bytesRead += 77;

        value = new AllPrimitives(___readValueBool, ___readValueSByte, ___readValueShort, ___readValueHalf, ___readValueInt, ___readValueFloat, ___readValueLong, ___readValueInt128, ___readValueUInt128, ___readValueULong, ___readValueDouble, ___readValueUInt, ___readValueUShort, ___readValueByte);
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
        /// <summary> Writes a <c>sbyte</c> to the destination </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt8(Span<byte> destination, sbyte value)
        {
            destination[0] = (byte)value;
        }
        /// <summary> Reads a <c>sbyte</c> from the given source </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte ReadInt8(ReadOnlySpan<byte> source)
        {
            return (sbyte)source[0];
        }
        /// <summary> Writes a <c>ushort</c> to the destination </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt16LittleEndian(Span<byte> destination, ushort value)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(destination, value);
        }
        /// <summary> Writes a <c>ushort</c> to the destination </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt16BigEndian(Span<byte> destination, ushort value)
        {
            BinaryPrimitives.WriteUInt16BigEndian(destination, value);
        }
        /// <summary> Reads a <c>ushort</c> from the given source, as LittleEndian </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ReadUInt16LittleEndian(ReadOnlySpan<byte> source)
        {
            return BinaryPrimitives.ReadUInt16LittleEndian(source);
        }
        /// <summary> Reads a <c>ushort</c> from the given source, as BigEndian </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ReadUInt16BigEndian(ReadOnlySpan<byte> source)
        {
            return BinaryPrimitives.ReadUInt16BigEndian(source);
        }
        /// <summary> Writes a <c>short</c> to the destination </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt16LittleEndian(Span<byte> destination, short value)
        {
            BinaryPrimitives.WriteInt16LittleEndian(destination, value);
        }
        /// <summary> Writes a <c>short</c> to the destination </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt16BigEndian(Span<byte> destination, short value)
        {
            BinaryPrimitives.WriteInt16BigEndian(destination, value);
        }
        /// <summary> Reads a <c>short</c> from the given source, as LittleEndian </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short ReadInt16LittleEndian(ReadOnlySpan<byte> source)
        {
            return BinaryPrimitives.ReadInt16LittleEndian(source);
        }
        /// <summary> Reads a <c>short</c> from the given source, as BigEndian </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short ReadInt16BigEndian(ReadOnlySpan<byte> source)
        {
            return BinaryPrimitives.ReadInt16BigEndian(source);
        }
        /// <summary> Writes a <c>System.Half</c> to the destination </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteHalfLittleEndian(Span<byte> destination, System.Half value)
        {
            BinaryPrimitives.WriteHalfLittleEndian(destination, value);
        }
        /// <summary> Writes a <c>System.Half</c> to the destination </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteHalfBigEndian(Span<byte> destination, System.Half value)
        {
            BinaryPrimitives.WriteHalfBigEndian(destination, value);
        }
        /// <summary> Reads a <c>System.Half</c> from the given source, as LittleEndian </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static System.Half ReadHalfLittleEndian(ReadOnlySpan<byte> source)
        {
            return BinaryPrimitives.ReadHalfLittleEndian(source);
        }
        /// <summary> Reads a <c>System.Half</c> from the given source, as BigEndian </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static System.Half ReadHalfBigEndian(ReadOnlySpan<byte> source)
        {
            return BinaryPrimitives.ReadHalfBigEndian(source);
        }
        /// <summary> Writes a <c>uint</c> to the destination </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt32LittleEndian(Span<byte> destination, uint value)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(destination, value);
        }
        /// <summary> Writes a <c>uint</c> to the destination </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt32BigEndian(Span<byte> destination, uint value)
        {
            BinaryPrimitives.WriteUInt32BigEndian(destination, value);
        }
        /// <summary> Reads a <c>uint</c> from the given source, as LittleEndian </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReadUInt32LittleEndian(ReadOnlySpan<byte> source)
        {
            return BinaryPrimitives.ReadUInt32LittleEndian(source);
        }
        /// <summary> Reads a <c>uint</c> from the given source, as BigEndian </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReadUInt32BigEndian(ReadOnlySpan<byte> source)
        {
            return BinaryPrimitives.ReadUInt32BigEndian(source);
        }
        /// <summary> Writes a <c>int</c> to the destination </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt32LittleEndian(Span<byte> destination, int value)
        {
            BinaryPrimitives.WriteInt32LittleEndian(destination, value);
        }
        /// <summary> Writes a <c>int</c> to the destination </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt32BigEndian(Span<byte> destination, int value)
        {
            BinaryPrimitives.WriteInt32BigEndian(destination, value);
        }
        /// <summary> Reads a <c>int</c> from the given source, as LittleEndian </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadInt32LittleEndian(ReadOnlySpan<byte> source)
        {
            return BinaryPrimitives.ReadInt32LittleEndian(source);
        }
        /// <summary> Reads a <c>int</c> from the given source, as BigEndian </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadInt32BigEndian(ReadOnlySpan<byte> source)
        {
            return BinaryPrimitives.ReadInt32BigEndian(source);
        }
        /// <summary> Writes a <c>float</c> to the destination </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteSingleLittleEndian(Span<byte> destination, float value)
        {
            BinaryPrimitives.WriteSingleLittleEndian(destination, value);
        }
        /// <summary> Writes a <c>float</c> to the destination </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteSingleBigEndian(Span<byte> destination, float value)
        {
            BinaryPrimitives.WriteSingleBigEndian(destination, value);
        }
        /// <summary> Reads a <c>float</c> from the given source, as LittleEndian </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ReadSingleLittleEndian(ReadOnlySpan<byte> source)
        {
            return BinaryPrimitives.ReadSingleLittleEndian(source);
        }
        /// <summary> Reads a <c>float</c> from the given source, as BigEndian </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ReadSingleBigEndian(ReadOnlySpan<byte> source)
        {
            return BinaryPrimitives.ReadSingleBigEndian(source);
        }
        /// <summary> Writes a <c>ulong</c> to the destination </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt64LittleEndian(Span<byte> destination, ulong value)
        {
            BinaryPrimitives.WriteUInt64LittleEndian(destination, value);
        }
        /// <summary> Writes a <c>ulong</c> to the destination </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt64BigEndian(Span<byte> destination, ulong value)
        {
            BinaryPrimitives.WriteUInt64BigEndian(destination, value);
        }
        /// <summary> Reads a <c>ulong</c> from the given source, as LittleEndian </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ReadUInt64LittleEndian(ReadOnlySpan<byte> source)
        {
            return BinaryPrimitives.ReadUInt64LittleEndian(source);
        }
        /// <summary> Reads a <c>ulong</c> from the given source, as BigEndian </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ReadUInt64BigEndian(ReadOnlySpan<byte> source)
        {
            return BinaryPrimitives.ReadUInt64BigEndian(source);
        }
        /// <summary> Writes a <c>long</c> to the destination </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt64LittleEndian(Span<byte> destination, long value)
        {
            BinaryPrimitives.WriteInt64LittleEndian(destination, value);
        }
        /// <summary> Writes a <c>long</c> to the destination </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt64BigEndian(Span<byte> destination, long value)
        {
            BinaryPrimitives.WriteInt64BigEndian(destination, value);
        }
        /// <summary> Reads a <c>long</c> from the given source, as LittleEndian </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ReadInt64LittleEndian(ReadOnlySpan<byte> source)
        {
            return BinaryPrimitives.ReadInt64LittleEndian(source);
        }
        /// <summary> Reads a <c>long</c> from the given source, as BigEndian </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ReadInt64BigEndian(ReadOnlySpan<byte> source)
        {
            return BinaryPrimitives.ReadInt64BigEndian(source);
        }
        /// <summary> Writes a <c>double</c> to the destination </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteDoubleLittleEndian(Span<byte> destination, double value)
        {
            BinaryPrimitives.WriteDoubleLittleEndian(destination, value);
        }
        /// <summary> Writes a <c>double</c> to the destination </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteDoubleBigEndian(Span<byte> destination, double value)
        {
            BinaryPrimitives.WriteDoubleBigEndian(destination, value);
        }
        /// <summary> Reads a <c>double</c> from the given source, as LittleEndian </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ReadDoubleLittleEndian(ReadOnlySpan<byte> source)
        {
            return BinaryPrimitives.ReadDoubleLittleEndian(source);
        }
        /// <summary> Reads a <c>double</c> from the given source, as BigEndian </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ReadDoubleBigEndian(ReadOnlySpan<byte> source)
        {
            return BinaryPrimitives.ReadDoubleBigEndian(source);
        }
        /// <summary> Writes a <c>System.UInt128</c> to the destination </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt128LittleEndian(Span<byte> destination, System.UInt128 value)
        {
            BinaryPrimitives.WriteUInt128LittleEndian(destination, value);
        }
        /// <summary> Writes a <c>System.UInt128</c> to the destination </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt128BigEndian(Span<byte> destination, System.UInt128 value)
        {
            BinaryPrimitives.WriteUInt128BigEndian(destination, value);
        }
        /// <summary> Reads a <c>System.UInt128</c> from the given source, as LittleEndian </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static System.UInt128 ReadUInt128LittleEndian(ReadOnlySpan<byte> source)
        {
            return BinaryPrimitives.ReadUInt128LittleEndian(source);
        }
        /// <summary> Reads a <c>System.UInt128</c> from the given source, as BigEndian </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static System.UInt128 ReadUInt128BigEndian(ReadOnlySpan<byte> source)
        {
            return BinaryPrimitives.ReadUInt128BigEndian(source);
        }
        /// <summary> Writes a <c>System.Int128</c> to the destination </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt128LittleEndian(Span<byte> destination, System.Int128 value)
        {
            BinaryPrimitives.WriteInt128LittleEndian(destination, value);
        }
        /// <summary> Writes a <c>System.Int128</c> to the destination </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt128BigEndian(Span<byte> destination, System.Int128 value)
        {
            BinaryPrimitives.WriteInt128BigEndian(destination, value);
        }
        /// <summary> Reads a <c>System.Int128</c> from the given source, as LittleEndian </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static System.Int128 ReadInt128LittleEndian(ReadOnlySpan<byte> source)
        {
            return BinaryPrimitives.ReadInt128LittleEndian(source);
        }
        /// <summary> Reads a <c>System.Int128</c> from the given source, as BigEndian </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static System.Int128 ReadInt128BigEndian(ReadOnlySpan<byte> source)
        {
            return BinaryPrimitives.ReadInt128BigEndian(source);
        }
    }
}
