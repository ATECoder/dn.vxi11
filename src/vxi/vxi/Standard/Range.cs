// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;

#pragma warning disable IDE0130 // Namespace does not match folder structure

namespace System;

/// <summary>   Represent a range has start and end indexes. </summary>
/// <remarks>
/// Range is used by the C# compiler to support the range syntax.
/// <code>
/// int[] someArray = new int[5] { 1, 2, 3, 4, 5 };
/// int[] subArray1 = someArray[0..2]; // { 1, 2 }
/// int[] subArray2 = someArray[1..^0]; // { 2, 3, 4, 5 }
/// </code>
/// <see href="https://GitHub.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Range.cs"/>
/// </remarks>
internal readonly struct Range : IEquatable<Range>
{
    /// <summary>   Represent the inclusive start index of the Range. </summary>
    /// <value> The start. </value>
    public Index Start { get; }

    /// <summary>   Represent the exclusive end index of the Range. </summary>
    /// <value> The end. </value>
    public Index End { get; }

    /// <summary>   Construct a Range object using the start and end indexes. </summary>
    /// <remarks>   David, 2021-04-27. </remarks>
    /// <param name="start">    Represent the inclusive start index of the range. </param>
    /// <param name="end">      Represent the exclusive end index of the range. </param>
    public Range( Index start, Index end )
    {
        this.Start = start;
        this.End = end;
    }

    /// <summary>
    /// Indicates whether the current Range object is equal to another object of the same type.
    /// </summary>
    /// <remarks>   David, 2021-04-27. </remarks>
    /// <param name="value">    An object to compare with this object. </param>
    /// <returns>
    /// true if <paramref name="value">object?</paramref> and this instance are the same type and represent
    /// the same value; otherwise, false.
    /// </returns>
    public override bool Equals( object? value ) =>
        value is Range r &&
        r.Start.Equals( this.Start ) &&
        r.End.Equals( this.End );

    /// <summary>
    /// Indicates whether the current Range object is equal to another Range object.
    /// </summary>
    /// <remarks>   David, 2021-04-27. </remarks>
    /// <param name="other">    An object to compare with this object. </param>
    /// <returns>
    /// true if the current object is equal to the <paramref name="other">other</paramref> parameter;
    /// otherwise, false.
    /// </returns>
    public bool Equals( Range other ) => other.Start.Equals( this.Start ) && other.End.Equals( this.End );

    /// <summary>   Returns the hash code for this instance. </summary>
    /// <remarks>   David, 2021-04-27. </remarks>
    /// <returns>   A 32-bit signed integer that is the hash code for this instance. </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Style", "IDE0070:Use 'System.HashCode'", Justification = "<Pending>" )]
    public override int GetHashCode()
    {
        return (this.Start, this.End).GetHashCode();  // this.Start.GetHashCode() * 31 + this.End.GetHashCode();
    }

    /// <summary>
    /// Converts the value of the current Range object to its equivalent string representation.
    /// </summary>
    /// <remarks>   David, 2021-04-27. </remarks>
    /// <returns>   The fully qualified type name. </returns>
    public override string ToString()
    {
        return this.Start + ".." + this.End;
    }

    /// <summary>
    /// Create a Range object starting from start index to the end of the collection.
    /// </summary>
    /// <remarks>   David, 2021-04-27. </remarks>
    /// <param name="start">    Represent the inclusive start index of the range. </param>
    /// <returns>   A Range. </returns>
    public static Range StartAt( Index start ) => new( start, Index.End );

    /// <summary>
    /// Create a Range object starting from first element in the collection to the end Index.
    /// </summary>
    /// <remarks>   David, 2021-04-27. </remarks>
    /// <param name="end">  Represent the exclusive end index of the range. </param>
    /// <returns>   A Range. </returns>
    public static Range EndAt( Index end ) => new( Index.Start, end );

    /// <summary>   Create a Range object starting from first element to the end. </summary>
    /// <value> all. </value>
    public static Range All => new( Index.Start, Index.End );

    /// <summary>
    /// Calculate the start offset and length of range object using a collection length.
    /// </summary>
    /// <remarks>
    /// For performance reason, we don't validate the input length parameter against negative values.
    /// It is expected Range will be used with collections which always have non negative
    /// length/count. We validate the range is inside the length scope though.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">  Thrown when one or more arguments are outside
    ///                                                 the required range. </exception>
    /// <param name="length">   The length of the collection that the range will be used with. length
    ///                         has to be a positive value. </param>
    /// <returns>   The offset and length. </returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public (int Offset, int Length) GetOffsetAndLength( int length )
    {
        int start;
        var startIndex = this.Start;
        start = startIndex.IsFromEnd ? length - startIndex.Value : startIndex.Value;

        int end;
        var endIndex = this.End;
        end = endIndex.IsFromEnd ? length - endIndex.Value : endIndex.Value;

        return ( uint ) end > ( uint ) length || ( uint ) start > ( uint ) end
            ? throw new ArgumentOutOfRangeException( nameof( length ) )
            : (start, end - start);
    }
}
