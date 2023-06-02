using System.Net;
using System.Net.Sockets;

using cc.isr.VXI11.Codecs;
using cc.isr.VXI11.EnumExtensions;

namespace cc.isr.VXI11.MSTest;

/// <summary>   (Unit Test Class) a support tests. </summary>
[TestClass]
public class Vxi11SupportTests
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

    private LoggerTraceListener<Vxi11SupportTests>? _traceListener;

    /// <summary> Initializes the test class instance before each test runs. </summary>
    [TestInitialize()]
    public void InitializeBeforeEachTest()
    {
        if ( Logger is not null )
        {
            this._loggerScope = Logger.BeginScope( this.TestContext?.TestName ?? string.Empty );
            this._traceListener = new LoggerTraceListener<Vxi11SupportTests>( Logger );
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
    public static ILogger<Vxi11SupportTests>? Logger { get; } = LoggerProvider.InitLogger<Vxi11SupportTests>();

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

    #region " unique client id "

    /// <summary>   Assert unique client identifier should be generated. </summary>
    /// <returns>   An int. </returns>
    private static int AssertUniqueClientIdShouldBeGenerated()
    {
        int clientId = Vxi11Support.GenerateUniqueRandomClientIdentifier();
        Assert.IsTrue( clientId >= 0 );
        Assert.IsTrue( clientId < Vxi11Support.ClientIdentifierUpperBound );
        return clientId;
    }

    /// <summary>   (Unit Test Method) unique client identifier should be generated. </summary>
    [TestMethod]
    public void UniqueClientIdShouldBeGenerated()
    {
        int clientId = AssertUniqueClientIdShouldBeGenerated();
        int nextClientId = AssertUniqueClientIdShouldBeGenerated();
        Assert.AreNotEqual( clientId, nextClientId, "the client id should be unique" );
        clientId = int.MaxValue - 2;
        clientId = ++clientId == int.MaxValue ? 0 : clientId;
        Assert.AreEqual( int.MaxValue - 1, clientId );
        clientId = ++clientId == int.MaxValue ? 0 : clientId;
        Assert.AreEqual( 0, clientId );
    }

    #endregion

    #region " ip address "
    private static void AssertIPAddressShouldRestoreFromUnsignedIntegerValue( IPAddress address )
    {
        uint unsignedIntAddress = address.ToUInt();
        IPAddress restoredAddress = unsignedIntAddress.ToIPAddress();
        Assert.AreEqual( address.ToString(), restoredAddress.ToString(), $"Address should restored from unsigned integer address {unsignedIntAddress}" );
    }

    [TestMethod]
    public void IPAddressShouldRestoreFromUnsignedIntegerValue()
    {
        Vxi11SupportTests.AssertIPAddressShouldRestoreFromUnsignedIntegerValue( IPAddress.Parse( "127.0.0.1" ) );
        Vxi11SupportTests.AssertIPAddressShouldRestoreFromUnsignedIntegerValue( IPAddress.Parse( "192.168.0.1" ) );
        Vxi11SupportTests.AssertIPAddressShouldRestoreFromUnsignedIntegerValue( IPAddress.Parse( "255.255.255.255" ) );
        Vxi11SupportTests.AssertIPAddressShouldRestoreFromUnsignedIntegerValue( IPAddress.Parse( "255.255.255.1" ) );
        Vxi11SupportTests.AssertIPAddressShouldRestoreFromUnsignedIntegerValue( IPAddress.Parse( "1.1.1.1" ) );
        Vxi11SupportTests.AssertIPAddressShouldRestoreFromUnsignedIntegerValue( IPAddress.Parse( "1.1.1.0" ) );
    }

    /// <summary>   Gets local inter network (IPv4) addresses. </summary>
    /// <returns>   An array of IPv4 addresses. </returns>
    public static IPAddress[] GetLocalInterNetworkAddresses()
    {
        IPAddress[] localIPs = Dns.GetHostAddresses( Dns.GetHostName() );
        return localIPs.Where( ip => ip.AddressFamily == AddressFamily.InterNetwork ).ToArray();
    }

    /// <summary>   Gets local broadcast addresses. </summary>
    /// <returns>   An array of IPv4 broadcast addresses. </returns>
    public static IPAddress[] GetLocalBroadcastAddresses()
    {
        List<IPAddress> ipv4s = new();
        foreach ( IPAddress ip in GetLocalInterNetworkAddresses() )
        {
            if ( ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork )
            {
                byte[] bytes = ip.GetAddressBytes();
                bytes[3] = 255;
                ipv4s.Add( new IPAddress( bytes ) );
            }
        }
        return ipv4s.ToArray();
    }

    /// <summary>   (Unit Test Method) IP address should get local broadcast addresses. </summary>
    /// <remarks> 
    /// <code>
    /// Standard Output: 
    ///    2023-02-02 17:38:03.191,cc.isr.VXI11.MSTest.Vxi11SupportTests.Vxi11SupportTests
    ///    192.168.4.255
    ///    192.168.0.255
    /// </code>
    /// </remarks>
    [TestMethod]
    public void IPAddressShouldGetLocalBroadcastAddresses()
    {
        IPAddress[] localIPs = GetLocalBroadcastAddresses();
        var s = string.Join( Environment.NewLine, localIPs.Select( x => $"{x}" ).ToArray() );
        Console.WriteLine( s );
    }

    /// <summary>   (Unit Test Method) IP address should get local IPv4 addresses. </summary>
    /// <remarks>   
    /// <code>
    /// Standard Output: 
    ///    2023-02-02 17:40:42.287,cc.isr.VXI11.MSTest.Vxi11SupportTests.Vxi11SupportTests
    ///    192.168.4.28
    ///    192.168.0.40
    /// </code>
    /// </remarks>
    [TestMethod]
    public void IPAddressShouldGetLocalIPv4Addresses()
    {
        IPAddress[] localIPs = GetLocalInterNetworkAddresses();
        var s = string.Join( Environment.NewLine, localIPs.Select( x => $"{x}" ).ToArray() );
        Console.WriteLine( s );
    }

    /// <summary>   (Unit Test Method) IP address should get local addresses. </summary>
    /// <remarks>   
    /// <code>
    /// Standard Output: 
    ///    2023-02-02 17:41:13.178,cc.isr.VXI11.MSTest.Vxi11SupportTests.Vxi11SupportTests
    ///    fe80::a91e:10bb:6315:822c%7
    ///    fe80::fcf9:d92f:1f6c:7cf%8
    ///    192.168.4.28
    ///    192.168.0.40
    ///    fdf9:18a3:7260:1:441d:91e7:3481:2638
    ///    fdf9:18a3:7260:1:1206:b71a:bb4f:334
    /// </code>
    /// </remarks>
    [TestMethod]
    public void IPAddressShouldGetLocalAddresses()
    {
        IPAddress[] localIPs = Dns.GetHostAddresses( Dns.GetHostName() );
        var s = string.Join( Environment.NewLine, localIPs.Select( x => $"{x}" ).ToArray() );
        Console.WriteLine( s );
    }

    #endregion

    #region " enum extensions "

    /// <summary>   Assert should get description. </summary>
    /// <param name="value">                The value. </param>
    /// <param name="expectedDescription">  Information describing the expected. </param>
    private static void AssertShouldGetDescription( DeviceErrorCode value, string expectedDescription )
    {
        string actual = value.GetDescription();
        Assert.AreEqual( expectedDescription, actual );
    }

    /// <summary>   (Unit Test Method) message type should get description. </summary>
    [TestMethod]
    public void DeviceErrorCodeValueShouldGetDescription()
    {
        AssertShouldGetDescription( DeviceErrorCode.NoError, "No error." );
    }

    /// <summary>   Assert <see cref="int"/> should cast to <see cref="DeviceErrorCode"/>. </summary>
    /// <param name="expected"> The expected value. </param>
    private static void AssertIntShouldCastToDeviceErrorCodeValue( int expected )
    {
        DeviceErrorCode actual = expected.ToDeviceErrorCode();
        Assert.AreEqual( expected, ( int ) actual );
    }

    /// <summary>   (Unit Test Method) <see cref="int"/> should cast to <see cref="DeviceErrorCode"/>. </summary>
    [TestMethod]
    public void IntShouldCastToDeviceErrorCodeValue()
    {
        int value = 0;
        int maxValue = 0;
        foreach ( var enumValue in Enum.GetValues( typeof( DeviceErrorCode ) ) )
        {
            value = ( int ) enumValue;
            maxValue = value > maxValue ? value : maxValue;
            AssertIntShouldCastToDeviceErrorCodeValue( value );
        }
        _ = Assert.ThrowsException<ArgumentException>( () => { AssertIntShouldCastToDeviceErrorCodeValue( maxValue + 1 ); } );
    }

    /// <summary>   Assert <see cref="int"/> should cast to <see cref="TransportProtocol"/>. </summary>
    /// <param name="expected"> The expected value. </param>
    private static void AssertIntShouldCastToDeviceAddressFamily( int expected )
    {
        TransportProtocol actual = expected.ToTransportProtocol();
        Assert.AreEqual( expected, ( int ) actual );
    }

    /// <summary>   (Unit Test Method) <see cref="int"/> should cast to <see cref="TransportProtocol"/>. </summary>
    [TestMethod]
    public void IntShouldCastToDeviceAddressFamily()
    {
        int value = 0;
        int maxValue = 0;
        foreach ( var enumValue in Enum.GetValues( typeof( TransportProtocol ) ) )
        {
            value = ( int ) enumValue;
            maxValue = value > maxValue ? value : maxValue;
            AssertIntShouldCastToDeviceAddressFamily( value );
        }
        _ = Assert.ThrowsException<ArgumentException>( () => { AssertIntShouldCastToDeviceAddressFamily( maxValue + 1 ); } );
    }

    /// <summary>   Assert <see cref="int"/> should cast to <see cref="DeviceOperationFlags"/>. </summary>
    /// <param name="expected"> The expected value. </param>
    private static void AssertIntShouldCastToDeviceOperationFlags( int expected )
    {
        DeviceOperationFlags actual = expected.ToDeviceOperationFlags();
        Assert.AreEqual( expected, ( int ) actual );
    }

    /// <summary>   (Unit Test Method) <see cref="int"/> should cast to <see cref="DeviceOperationFlags"/>. </summary>
    [TestMethod]
    public void IntShouldCastToDeviceOperationFlags()
    {
        int value = 0;
        int orValue = 0;
        foreach ( var enumValue in Enum.GetValues( typeof( DeviceOperationFlags ) ) )
        {
            value = ( int ) enumValue;
            AssertIntShouldCastToDeviceOperationFlags( value );
            orValue |= value;
            AssertIntShouldCastToDeviceOperationFlags( orValue );
        }
        _ = Assert.ThrowsException<ArgumentException>( () => { AssertIntShouldCastToDeviceOperationFlags( orValue + 1 ); } );
    }

    /// <summary>   Assert <see cref="int"/> should cast to <see cref="DeviceReadReasons"/>. </summary>
    /// <param name="expected"> The expected value. </param>
    private static void AssertIntShouldCastToDeviceReadReasons( int expected )
    {
        DeviceReadReasons actual = expected.ToDeviceReadReasons();
        Assert.AreEqual( expected, ( int ) actual );
    }

    /// <summary>   (Unit Test Method) <see cref="int"/> should cast to <see cref="DeviceReadReasons"/>. </summary>
    [TestMethod]
    public void IntShouldCastToDeviceReadReasons()
    {
        int value = 0;
        int orValue = 0;
        foreach ( var enumValue in Enum.GetValues( typeof( DeviceReadReasons ) ) )
        {
            value = ( int ) enumValue;
            AssertIntShouldCastToDeviceReadReasons( value );
            orValue |= value;
            AssertIntShouldCastToDeviceReadReasons( orValue );
        }
        _ = Assert.ThrowsException<ArgumentException>( () => { AssertIntShouldCastToDeviceReadReasons( orValue + 1 ); } );
    }

    #endregion

}
