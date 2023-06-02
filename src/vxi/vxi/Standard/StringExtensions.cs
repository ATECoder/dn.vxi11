#pragma warning disable IDE0130 // Namespace does not match folder structure

namespace System;

/// <summary>   A string extensions. </summary>
/// <remarks>   2023-04-01. </remarks>
public static class StringExtensions
{
    /// <summary>
    /// A String extension method that query if this String contains the given str.
    /// </summary>
    /// <remarks>   2023-04-01. </remarks>
    /// <exception cref="ArgumentNullException">    Thrown when one or more required arguments are
    ///                                             null. </exception>
    /// <exception cref="ArgumentException">        Thrown when one or more arguments have
    ///                                             unsupported or illegal values. </exception>
    /// <param name="str">          The str to act on. </param>
    /// <param name="substring">    The substring. </param>
    /// <param name="comp">         The component. </param>
    /// <returns>   True if the substring is contained in the String, false if not. </returns>
    public static bool Contains( this String str, String substring, StringComparison comp )
    {
        if ( substring is null )
            throw new ArgumentNullException( nameof( substring ), $"{nameof( substring )} cannot be null." );
        else if ( !Enum.IsDefined( typeof( StringComparison ), comp ) )
            throw new ArgumentException( $"{nameof( comp )} is not a member of {nameof( StringComparison )}", nameof( comp ) );
        return str.IndexOf( substring, comp ) >= 0;
    }
}
