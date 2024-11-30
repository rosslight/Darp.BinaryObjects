﻿//HintName: BinaryObjectsGenerator.g.cs
// <auto-generated/>
#nullable enable

namespace Test
{
    /// <remarks> <list type="table">
    /// <item> <term><b>Field</b></term> <description><b>Byte Length</b></description> </item>
    /// <item> <term><see cref="Value"/></term> <description>1</description> </item>
    /// <item> <term> --- </term> <description>1</description> </item>
    /// </list> </remarks>
    public sealed partial record OneBool1 : global::Darp.BinaryObjects.IWritable, global::Darp.BinaryObjects.ISpanReadable<OneBool1>
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
        public static bool TryReadLittleEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out OneBool1? value) => TryReadLittleEndian(source, out value, out _);
        /// <inheritdoc />
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
        public static bool TryReadLittleEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out OneBool1? value, out int bytesRead)
        {
            bytesRead = 0;
            value = default;

            if (source.Length < 1)
                return false;
            var ___readValue = global::Darp.BinaryObjects.Generated.Utilities.ReadBool(source[0..]);
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
            var ___readValue = global::Darp.BinaryObjects.Generated.Utilities.ReadBool(source[0..]);
            bytesRead += 1;

            value = new OneBool1(___readValue);
            return true;
        }
    }
}

namespace Test
{
    /// <remarks> <list type="table">
    /// <item> <term><b>Field</b></term> <description><b>Byte Length</b></description> </item>
    /// <item> <term><see cref="Value"/></term> <description>1</description> </item>
    /// <item> <term> --- </term> <description>1</description> </item>
    /// </list> </remarks>
    public sealed partial record OneBool2 : global::Darp.BinaryObjects.IWritable, global::Darp.BinaryObjects.ISpanReadable<OneBool2>
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
        public static bool TryReadLittleEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out OneBool2? value) => TryReadLittleEndian(source, out value, out _);
        /// <inheritdoc />
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.BinaryObjects.Generator", "GeneratorVersion")]
        public static bool TryReadLittleEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out OneBool2? value, out int bytesRead)
        {
            bytesRead = 0;
            value = default;

            if (source.Length < 1)
                return false;
            var ___readValue = global::Darp.BinaryObjects.Generated.Utilities.ReadBool(source[0..]);
            bytesRead += 1;

            value = new OneBool2(___readValue);
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

            if (source.Length < 1)
                return false;
            var ___readValue = global::Darp.BinaryObjects.Generated.Utilities.ReadBool(source[0..]);
            bytesRead += 1;

            value = new OneBool2(___readValue);
            return true;
        }
    }
}

namespace Darp.BinaryObjects.Generated
{
    using System;
    using System.Buffers.Binary;
    using System.CodeDom.Compiler;
    using System.Runtime.CompilerServices;

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