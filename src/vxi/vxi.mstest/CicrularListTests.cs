using System.Collections;

using cc.isr.VXI11.Logging;

namespace cc.isr.VXI11.MSTest;

/// <summary>   (Unit Test Class) a circular list tests. </summary>
/// <remarks>   David, 2020-09-10. </remarks>
[TestClass]
public class CircularListTests
{

    #region " Fixture construction and cleanup "

    /// <summary>   Initializes the fixture. </summary>
    /// <param name="context">  The context. </param>
    [ClassInitialize]
    public static void InitializeFixture( TestContext context )
    {
        try
        {
            _classTestContext = context;
            Logger.Writer.LogInformation( $"{_classTestContext.FullyQualifiedTestClassName}.{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}" );
        }
        catch ( Exception ex )
        {
            Logger.Writer.LogMemberError( $"Failed initializing fixture:", ex );
            CleanupFixture();
        }
    }

    public TestContext? TestContext { get; set; }

    private static TestContext? _classTestContext;

    /// <summary>   Cleanup fixture. </summary>
    [ClassCleanup]
    public static void CleanupFixture()
    {
        _classTestContext = null;
    }

    #endregion

    #region " random numbers generator "

    private static readonly Random? _rnd = new ();

    /// <summary>   Generates a random byte array. </summary>
    /// <remarks>   David, 2020-09-09. </remarks>
    /// <param name="length">   The length. </param>
    /// <returns>   An array of byte. </returns>
    protected static byte[] GenerateRandomBytes( int length )
    {
        var bytes = new byte[length];
        _rnd?.NextBytes( bytes );
        return bytes;
    }

    #endregion

    #region " TEST METHODS "

    /// <summary>   (Unit Test Method) circular list should fill. </summary>
    /// <remarks>   David, 2020-09-10. </remarks>
    [TestMethod()]
    public void CircularListShouldFill()
    {
        var data = GenerateRandomBytes( 100 );
        var list = new CircularList<byte>( data.Length ) {
            data
        };
        CollectionAssert.AreEqual( data, list.ToArray() );
    }

    /// <summary>   (Unit Test Method) circular list should overflow. </summary>
    /// <remarks>   David, 2020-09-10. </remarks>
    [TestMethod()]
    public void CircularListShouldOverflow()
    {
        var data = GenerateRandomBytes( 100 );
        var list = new CircularList<byte>( data.Length ) {
            data
        };
        data = GenerateRandomBytes( 100 );
        list.Add( data );
        CollectionAssert.AreEqual( data, list.ToArray() );
    }

    /// <summary>   Circular list should enumerate as <see cref="ICollection"/>. </summary>
    /// <remarks>   David, 2020-09-10. </remarks>
    /// <param name="list">     The list. </param>
    /// <param name="bytes">    The bytes. </param>
    private static void AssertCircularListShouldEnumerateAsCollection( ICollection list, byte[] bytes )
    {
        int i = 0;
        foreach ( byte item in list )
        {
            Assert.AreEqual( item, bytes[i++] );
        }
    }

    /// <summary>   (Unit Test Method) circular list should enumerate as collection. </summary>
    /// <remarks>   2023-02-16. </remarks>
    [TestMethod()]
    public void CircularListShouldEnumerateAsCollection()
    {
        var data = GenerateRandomBytes( 100 );
        var list = new CircularList<byte>( data.Length ) {
            data
        };
        CircularListTests.AssertCircularListShouldEnumerateAsCollection( ( ICollection ) list, data );
    }

    /// <summary>
    /// Assert circular list should enumerate as <see cref="IReadOnlyCollection&lt;byte&gt;"/>.
    /// </summary>
    /// <remarks>   David, 2020-09-10. </remarks>
    /// <param name="list">     The list. </param>
    /// <param name="bytes">    The bytes. </param>
    private static void AssertCircularListShouldEnumerateAsReadOnlyCollection( IReadOnlyCollection<byte> list, byte[] bytes )
    {
        int i = 0;
        foreach ( byte item in list )
        {
            Assert.AreEqual( item, bytes[i++] );
        }
    }

    /// <summary>
    /// (Unit Test Method) circular list should enumerate as read only collection.
    /// </summary>
    /// <remarks>   2023-02-16. </remarks>
    [TestMethod()]
    public void CircularListShouldEnumerateAsReadOnlyCollection()
    {
        var data = GenerateRandomBytes( 100 );
        var list = new CircularList<byte>( data.Length ) {
            data
        };
        CircularListTests.AssertCircularListShouldEnumerateAsReadOnlyCollection( ( IReadOnlyCollection<byte> ) list, data );
    }


    /// <summary>   Assert circular list should enumerate as <see cref="IEnumerable"/>. </summary>
    /// <remarks>   David, 2020-09-10. </remarks>
    /// <param name="bytes">    The bytes. </param>
    /// <param name="list">     The list. </param>
    private static void AssertCircularListShouldEnumerateAsIEnumerable( byte[] bytes, IEnumerable list )
    {
        int i = 0;
        foreach ( byte item in list )
        {
            Assert.AreEqual( item, bytes[i++] );
        }
    }

    /// <summary>   (Unit Test Method) circular list should enumerate as i enumerable. </summary>
    /// <remarks>   2023-02-16. </remarks>
    [TestMethod()]
    public void CircularListShouldEnumerateAsIEnumerable()
    {
        var data = GenerateRandomBytes( 100 );
        var list = new CircularList<byte>( data.Length ) {
            data
        };
        CircularListTests.AssertCircularListShouldEnumerateAsIEnumerable( data, ( IEnumerable ) list );
    }

    /// <summary>
    /// Assert circular list should enumerate as <see cref="IEnumerable&lt;byte&gt;"/>.
    /// </summary>
    /// <remarks>   David, 2020-09-10. </remarks>
    /// <param name="bytes">    The bytes. </param>
    /// <param name="list">     The list. </param>
    private static void AssertCircularListShouldEnumerate( byte[] bytes, IEnumerable<byte> list )
    {
        int i = 0;
        foreach ( byte item in list )
        {
            Assert.AreEqual( item, bytes[i++] );
        }
    }

    /// <summary>
    /// (Unit Test Method) circular list should enumerate as i enumerable of byte.
    /// </summary>
    /// <remarks>   2023-02-16. </remarks>
    [TestMethod()]
    public void CircularListShouldEnumerateAsIEnumerableOfByte()
    {
        var data = GenerateRandomBytes( 100 );
        var list = new CircularList<byte>( data.Length ) {
            data
        };
        CircularListTests.AssertCircularListShouldEnumerate( data, ( IEnumerable<byte> ) list );
    }


    /// <summary>
    /// Assert circular list should enumerate as <see cref="IEnumerable&lt;byte&gt;"/>.
    /// </summary>
    /// <remarks>   2023-02-16. </remarks>
    /// <param name="bytes">    The bytes. </param>
    /// <param name="list">     The list. </param>
    private static void AssertCircularListShouldEnumerate( byte[] bytes, CircularList<byte> list )
    {
        int i = 0;
        foreach ( byte item in list )
        {
            Assert.AreEqual( item, bytes[i++] );
        }
    }

    /// <summary>   (Unit Test Method) circular list should enumerate. </summary>
    /// <remarks>   David, 2020-09-10. </remarks>
    [TestMethod()]
    public void CircularListShouldEnumerate()
    {
        var data = GenerateRandomBytes( 100 );
        var list = new CircularList<byte>( data.Length ) {
            data
        };
        CircularListTests.AssertCircularListShouldEnumerate( data, ( CircularList<byte> ) list );
    }

    #endregion
}
