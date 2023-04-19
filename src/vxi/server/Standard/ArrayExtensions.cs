namespace System.Runtime1.CompilerServices;

/// <summary>   Array extensions. </summary>
/// <remarks>   David, 2021-04-27. </remarks>
internal static class ArrayExtensions
{
    /// <summary>   Slices the specified array using the specified range. </summary>
    /// <remarks>   David, 2021-04-27. </remarks>
    /// <exception cref="ArgumentNullException">    Thrown when one or more required arguments are
    ///                                             null. </exception>
    /// <typeparam name="T">    Generic type parameter. </typeparam>
    /// <param name="array">    The array. </param>
    /// <param name="range">    The range. </param>
    /// <returns>   An array of t. </returns>
    public static T[] GetSubArray<T>( this T[] array, Range range )
    {
        if ( array == null )
        {
            throw new ArgumentNullException( nameof( array ) );
        }

        (int offset, int length) = range.GetOffsetAndLength( array.Length );

        if ( default( T ) != null || typeof( T[] ) == array.GetType() )
        {
            // We know the type of the array to be exactly T[].

            if ( length == 0 )
            {
                return Array.Empty<T>();
            }

            var destination = new T[length];
            Array.Copy( array, offset, destination, 0, length );
            return destination;
        }
        else
        {
            // The array is actually a U[] where U:T.
            var destination = ( T[] ) Array.CreateInstance( array.GetType().GetElementType(), length );
            Array.Copy( array, offset, destination, 0, length );
            return destination;
        }
    }
}
