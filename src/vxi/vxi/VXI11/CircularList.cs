
namespace cc.isr.VXI11;

/// <summary>   A circular list of fixed size. </summary>
/// <remarks>   David, 2020-09-10. </remarks>
/// <license>
/// (c) 2020 Integrated Scientific Resources, Inc. All rights reserved.<para>
/// Licensed under The MIT License.</para><para>
/// THE SOFTWARE IS PROVIDED 'AS IS', WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
/// BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
/// NON-INFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
/// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
/// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.</para>
/// </license>
/// <typeparamref name="T"> Generic type parameter. </typeparamref>
public class CircularList<T> : List<T>
{

    /// <summary>
    /// Constructor that prevents a default instance of this class from being created.
    /// </summary>
    /// <remarks>   David, 2020-09-10. </remarks>
    private CircularList()
    { }
    /// <summary>
    /// Creates a circular list with room for capacity of items. Extra items are removed upon adding new items
    /// beyond the capacity.
    /// </summary>
    /// <remarks>   David, 2020-09-10. </remarks>
    /// <param name="capacity"> The capacity. </param>
    public CircularList( int capacity ) : base( capacity )
    {
        this.FixedCapacity = capacity;
    }

    /// <summary>   The synchronize root. </summary>
    [NonSerialized]
    private Object? _syncRoot;

    /// <summary>
    /// Gets an object that can be used to synchronize access to the
    /// <see cref="T:System.Collections.ICollection" />.
    /// </summary>
    /// <value>
    /// An object that can be used to synchronize access to the
    /// <see cref="T:System.Collections.ICollection" />.
    /// </value>
    public virtual Object SyncRoot
    {
        get {
            if ( this._syncRoot == null )
            {
                _ = System.Threading.Interlocked.CompareExchange( ref this._syncRoot, new Object(), null );
            }
            return this._syncRoot;
        }
    }

    /// <summary>   Gets or sets the fixed capacity. </summary>
    /// <value> The fixed capacity. </value>
    public int FixedCapacity { get; private set; }

    /// <summary>   Gets a value indicating whether this object is fixed size. </summary>
    /// <value> True if this object is fixed size, false if not. </value>
    public bool IsFixedSize => true;

    /// <summary>
    /// Adds an item to the end of the
    /// <see cref="T:System.Collections.Generic.List`1" />. Extra items are removed 
    /// upon adding items beyond the fixed <see cref="List{T}.Capacity"/>.
    /// </summary>
    /// <remarks>   David, 2020-09-10. </remarks>
    /// <param name="item">  The item to add to the end of the
    ///                     <see cref="T:System.Collections.Generic.List`1" />. The
    ///                     value can be a null reference (Nothing in Visual Basic) for reference
    ///                     types. </param>
    public new void Add( T item )
    {
        base.Insert( base.Count, item );
        lock ( this.SyncRoot )
        {
            while ( base.Count > this.FixedCapacity )
            { base.RemoveAt( 0 ); }
        }
    }

    /// <summary>
    /// Adds items to the end of the
    /// <see cref="T:System.Collections.Generic.List`1" />. Extra items are removed 
    /// upon adding items beyond the fixed <see cref="List{T}.Capacity"/>.
    /// </summary>
    /// <remarks>   David, 2020-09-10. </remarks>
    /// <param name="items">    The items. </param>
    public void Add( T[] items )
    {
        base.InsertRange( base.Count, items );
        lock ( this.SyncRoot )
        {
            while ( base.Count > this.FixedCapacity )
            { base.RemoveAt( 0 ); }
        }
    }

    /// <summary>
    /// Adds items to the end of the
    /// <see cref="T:System.Collections.Generic.List`1" />. Extra items are removed
    /// upon adding items beyond the fixed <see cref="List{T}.Capacity"/>.
    /// </summary>
    /// <remarks>   David, 2020-09-10. </remarks>
    /// <param name="items">    The items. </param>
    public void Add( IEnumerable<T> items )
    {
        base.InsertRange( base.Count, items );
        lock ( this.SyncRoot )
        {
            while ( base.Count > this.FixedCapacity )
            { base.RemoveAt( 0 ); }
        }
    }

    /// <summary>
    /// Adds the elements of the specified collection to the end of the
    /// <see cref="T:System.Collections.Generic.List`1" />.
    /// </summary>
    /// <remarks>   David, 2020-09-10. </remarks>
    /// <param name="collection">   The collection whose elements should be added to the end of the
    ///                             <see cref="T:System.Collections.Generic.List`1" />. The
    ///                             collection itself cannot be <see langword="null" />, but it can
    ///                             contain elements that are <see langword="null" />, if type
    ///                             <typeparamref name="T"/> is a reference type. </param>
    public new void AddRange( IEnumerable<T> collection )
    {
        this.Add( collection );
    }

    /// <summary>
    /// Not allowed
    /// </summary>
    /// <remarks>   David, 2020-09-10. </remarks>
    /// <exception cref="InvalidOperationException">    Thrown when the requested operation is
    ///                                                 invalid. </exception>
    /// <param name="index">    The zero-based index at which <paramref name="item" /> should be
    ///                         inserted. </param>
    /// <param name="item">     The object to insert into the
    ///                         <see cref="T:System.Collections.IList" />. </param>
    public void Insert( int index, object item )
    {
        throw new InvalidOperationException( $"Insert({index}, ({typeof( Object )}){nameof( item )}) into a {nameof( CircularList<T> )} is not permitted" );
    }

    /// <summary>
    /// Not allowed
    /// </summary>
    /// <remarks>   David, 2020-09-10. </remarks>
    /// <exception cref="InvalidOperationException">    Thrown when the requested operation is
    ///                                                 invalid. </exception>
    /// <param name="index">        The zero-based index at which the new elements should be
    ///                             inserted. </param>
    /// <param name="collection">   The collection whose elements should be inserted into the
    ///                             <see cref="T:System.Collections.Generic.List`1" />. The
    ///                             collection itself cannot be <see langword="null" />, but it can
    ///                             contain elements that are <see langword="null" />, if 
    ///                             <typeparamref name="T"/> is a reference type. </param>
    public new void InsertRange( int index, IEnumerable<T> collection )
    {
        throw new InvalidOperationException( $"InsertRange({index}, ({typeof( IEnumerable<T> )}){nameof( collection )}) into a {nameof( CircularList<T> )} is not permitted" );
    }
}
