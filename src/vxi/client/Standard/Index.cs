// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;

#pragma warning disable IDE0130 // Namespace does not match folder structure

namespace System;

/// <summary>
/// Represent a type can be used to index a collection either from the start or the end.
/// </summary>
/// <remarks>
/// Index is used by the C# compiler to support the new index syntax
/// <code>
/// int[] someArray = new int[5] { 1, 2, 3, 4, 5 } ;
/// int lastElement = someArray[^1]; // lastElement = 5
/// </code>
/// <see href="https://GitHub.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Index.cs"/>
/// </remarks>
internal readonly struct Index : IEquatable<Index>
{
    /// <summary>   (Immutable) the value. </summary>
    private readonly int _value;

    /// <summary>
    /// Construct an Index using a value and indicating if the index is from the start or from the
    /// end.
    /// </summary>
    /// <remarks>
    /// If the Index constructed from the end, index value 1 means pointing at the last element and
    /// index value 0 means pointing at beyond last element.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">  Thrown when one or more arguments are outside
    ///                                                 the required range. </exception>
    /// <param name="value">    The index value. it has to be zero or positive number. </param>
    /// <param name="fromEnd">  (Optional) Indicating if the index is from the start or from the end. </param>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Index( int value, bool fromEnd = false )
    {
        if ( value < 0 )
        {
            throw new ArgumentOutOfRangeException( nameof( value ), "value must be non-negative" );
        }

        this._value = fromEnd ? ~value : value;
    }

    /// <summary>
    /// The following private constructors mainly created for perf reason to avoid the checks.
    /// </summary>
    /// <remarks>   David, 2021-04-27. </remarks>
    /// <param name="value">    The index value. it has to be zero or positive number. </param>
    private Index( int value )
    {
        this._value = value;
    }

    /// <summary>   Create an Index pointing at first element. </summary>
    /// <value> The start. </value>
    public static Index Start => new( 0 );

    /// <summary>   Create an Index pointing at beyond last element. </summary>
    /// <value> The end. </value>
    public static Index End => new( ~0 );

    /// <summary>   Create an Index from the start at the position indicated by the value. </summary>
    /// <remarks>   David, 2021-04-27. </remarks>
    /// <exception cref="ArgumentOutOfRangeException">  Thrown when one or more arguments are outside
    ///                                                 the required range. </exception>
    /// <param name="value">    The index value from the start. </param>
    /// <returns>   An Index. </returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Index FromStart( int value )
    {
        return value < 0 ? throw new ArgumentOutOfRangeException( nameof( value ), "value must be non-negative" ) : new Index( value );
    }

    /// <summary>   Create an Index from the end at the position indicated by the value. </summary>
    /// <remarks>   David, 2021-04-27. </remarks>
    /// <exception cref="ArgumentOutOfRangeException">  Thrown when one or more arguments are outside
    ///                                                 the required range. </exception>
    /// <param name="value">    The index value from the end. </param>
    /// <returns>   An Index. </returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Index FromEnd( int value )
    {
        return value < 0 ? throw new ArgumentOutOfRangeException( nameof( value ), "value must be non-negative" ) : new Index( ~value );
    }

    /// <summary>   Returns the index value. </summary>
    /// <value> The value. </value>
    public int Value => this._value < 0 ? ~this._value : this._value;

    /// <summary>   Indicates whether the index is from the start or the end. </summary>
    /// <value> True if this object is from end, false if not. </value>
    public bool IsFromEnd => this._value < 0;

    /// <summary>   Calculate the offset from the start using the giving collection length. </summary>
    /// <remarks>
    /// For performance reason, we don't validate the input length parameter and the returned offset
    /// value against negative values. we don't validate either the returned offset is greater than
    /// the input length. It is expected Index will be used with collections which always have non
    /// negative length/count. If the returned offset is negative and then used to index a collection
    /// will get out of range exception which will be same affect as the validation.
    /// </remarks>
    /// <param name="length">   The length of the collection that the Index will be used with. length
    ///                         has to be a positive value. </param>
    /// <returns>   The offset. </returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public int GetOffset( int length )
    {
        var offset = this._value;
        if ( this.IsFromEnd )
        {
            // offset = length - (~value)
            // offset = length + (~(~value) + 1)
            // offset = length + value + 1

            offset += length + 1;
        }
        return offset;
    }

    /// <summary>
    /// Indicates whether the current Index object is equal to another object of the same type.
    /// </summary>
    /// <remarks>   David, 2021-04-27. </remarks>
    /// <param name="value">    An object to compare with this object. </param>
    /// <returns>
    /// true if <paramref name="value">object?</paramref> and this instance are the same type and represent
    /// the same value; otherwise, false.
    /// </returns>
    [Diagnostics.CodeAnalysis.SuppressMessage( "Style", "IDE0038:Use pattern matching", Justification = "<Pending>" )]
    public override bool Equals( object? value ) => value is Index && this._value == (( Index ) value)._value;

    /// <summary>
    /// Indicates whether the current Index object is equal to another Index object.
    /// </summary>
    /// <remarks>   David, 2021-04-27. </remarks>
    /// <param name="other">    An object to compare with this object. </param>
    /// <returns>
    /// true if the current object is equal to the <paramref name="other">other</paramref> parameter;
    /// otherwise, false.
    /// </returns>
    public bool Equals( Index other ) => this._value == other._value;

    /// <summary>   Returns the hash code for this instance. </summary>
    /// <remarks>   David, 2021-04-27. </remarks>
    /// <returns>   A 32-bit signed integer that is the hash code for this instance. </returns>
    public override int GetHashCode() => this._value;

    /// <summary>   Converts integer number to an Index. </summary>
    /// <remarks>   David, 2021-04-27. </remarks>
    /// <param name="value">    The index value from the start. </param>
    /// <returns>   The result of the operation. </returns>
    public static implicit operator Index( int value ) => FromStart( value );

    /// <summary>
    /// Converts the value of the current Index object to its equivalent string representation.
    /// </summary>
    /// <remarks>   David, 2021-04-27. </remarks>
    /// <returns>   The fully qualified type name. </returns>
    public override string ToString()
    {
        return this.IsFromEnd ? "^" + (( uint ) this.Value).ToString() : (( uint ) this.Value).ToString();
    }
}
