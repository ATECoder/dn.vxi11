using System.Collections;

namespace cc.isr.VXI11.MSTest;

/// <summary>   (Unit Test Class) a circular list tests. </summary>
/// <remarks>   David, 2020-09-10. </remarks>
[TestClass]
public class CircularListTests
{

    #region " construction and cleanup "

    /// <summary> Initializes the test class before running the first test. </summary>
    /// <param name="testContext"> Gets or sets the test context which provides information about
    /// and functionality for the current test run. </param>
    /// <remarks>Use ClassInitialize to run code before running the first test in the class</remarks>
    [ClassInitialize()]
    public static void InitializeTestClass( TestContext testContext )
    {
        try
        {
            string methodFullName = $"{testContext.FullyQualifiedTestClassName}.{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}";
            if ( Logger is null )
                Console.WriteLine( methodFullName );
            else
                Logger?.LogMemberInfo( methodFullName );
        }
        catch ( Exception ex )
        {
            if ( Logger is null )
                Console.WriteLine( $"Failed initializing the test class: {ex}" );
            else
                Logger.LogMemberError( "Failed initializing the test class:", ex );

            // cleanup to meet strong guarantees

            try
            {
                CleanupTestClass();
            }
            finally
            {
            }
        }
    }

    /// <summary> Cleans up the test class after all tests in the class have run. </summary>
    /// <remarks> Use <see cref="CleanupTestClass"/> to run code after all tests in the class have run. </remarks>
    [ClassCleanup()]
    public static void CleanupTestClass()
    { }

    private IDisposable? _loggerScope;

    private LoggerTraceListener<CircularListTests>? _traceListener;

    /// <summary> Initializes the test class instance before each test runs. </summary>
    [TestInitialize()]
    public void InitializeBeforeEachTest()
    {
        if ( Logger is not null )
        {
            this._loggerScope = Logger.BeginScope( this.TestContext?.TestName ?? string.Empty );
            this._traceListener = new LoggerTraceListener<CircularListTests>( Logger );
            _ = Trace.Listeners.Add( this._traceListener );
        }
    }

    /// <summary> Cleans up the test class instance after each test has run. </summary>
    [TestCleanup()]
    public void CleanupAfterEachTest()
    {
        Assert.IsFalse( this._traceListener?.Any( TraceEventType.Error ),
            $"{nameof( this._traceListener )} should have no {TraceEventType.Error} messages" );
        this._loggerScope?.Dispose();
        this._traceListener?.Dispose();
        Trace.Listeners.Clear();
    }

    /// <summary>
    /// Gets or sets the test context which provides information about and functionality for the
    /// current test run.
    /// </summary>
    /// <value> The test context. </value>
    public TestContext? TestContext { get; set; }

    /// <summary>   Gets a logger instance for this category. </summary>
    /// <value> The logger. </value>
    public static ILogger<CircularListTests>? Logger { get; } = LoggerProvider.InitLogger<CircularListTests>();

    #endregion

    #region " initialization tests "

    /// <summary>   (Unit Test Method) 00 logger should be enabled. </summary>
    /// <remarks>   2023-05-31. </remarks>
    [TestMethod]
    public void A00LoggerShouldBeEnabled()
    {
        Assert.IsNotNull( Logger, $"{nameof( Logger )} should initialize" );
        Assert.IsTrue( Logger.IsEnabled( LogLevel.Information ),
            $"{nameof( Logger )} should be enabled for the {LogLevel.Information} {nameof( LogLevel )}" );
    }

    /// <summary>   (Unit Test Method) 01 logger trace listener should have messages. </summary>
    /// <remarks>   2023-06-01. </remarks>
    [TestMethod]
    public void A01LoggerTraceListenerShouldHaveMessages()
    {
        Assert.IsNotNull( this._traceListener, $"{nameof( this._traceListener )} should initialize" );
        Assert.IsTrue( Trace.Listeners.Count > 0, $"{nameof( Trace )} should have non-zero {nameof( Trace.Listeners )}" );
        Trace.TraceInformation( "Testing tracing an info message" ); Trace.Flush();
        Assert.IsTrue( this._traceListener?.Any( TraceEventType.Information ), $"{nameof( this._traceListener )} should have {TraceEventType.Error} messages" );

        // no need to report errors for this test.

        this._traceListener?.Clear();
    }

    #endregion


    #region " random numbers generator "

    private static readonly Random? _rnd = new();

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

    #region " test methods "

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
