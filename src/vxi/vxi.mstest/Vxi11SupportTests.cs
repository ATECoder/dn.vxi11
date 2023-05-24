using System.Net;
using System.Net.Sockets;

using cc.isr.VXI11.Codecs;
using cc.isr.VXI11.EnumExtensions;
using cc.isr.VXI11.Logging;

namespace cc.isr.VXI11.MSTest;

/// <summary>   (Unit Test Class) a support tests. </summary>
[TestClass]
public class Vxi11SupportTests
{

    #region " fixture construction and cleanup "

    /// <summary>   Initializes the fixture. </summary>
    /// <param name="testContext"> Gets or sets the test context which provides information about
    /// and functionality for the current test run. </param>
    [ClassInitialize]
    public static void InitializeFixture( TestContext testContext )
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

    /// <summary>
    /// Gets or sets the test context which provides information about and functionality for the
    /// current test run.
    /// </summary>
    /// <value> The test context. </value>
    public TestContext? TestContext { get; set; }

    private static TestContext? _classTestContext;

    /// <summary>   Cleanup fixture. </summary>
    [ClassCleanup]
    public static void CleanupFixture()
    { }
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
    ///    2023-02-02 17:38:03.191,cc.isr.VXI11.MSTest.SupportTests.SupportTests
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
    ///    2023-02-02 17:40:42.287,cc.isr.VXI11.MSTest.SupportTests.SupportTests
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
    ///    2023-02-02 17:41:13.178,cc.isr.VXI11.MSTest.SupportTests.SupportTests
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
    private static void AssertIntShouldCastToDeviceAddrFamily( int expected )
    {
        TransportProtocol actual = expected.ToTransportProtocol();
        Assert.AreEqual( expected, ( int ) actual );
    }

    /// <summary>   (Unit Test Method) <see cref="int"/> should cast to <see cref="TransportProtocol"/>. </summary>
    [TestMethod]
    public void IntShouldCastToDeviceAddrFamily()
    {
        int value = 0;
        int maxValue = 0;
        foreach ( var enumValue in Enum.GetValues( typeof( TransportProtocol ) ) )
        {
            value = ( int ) enumValue;
            maxValue = value > maxValue ? value : maxValue;
            AssertIntShouldCastToDeviceAddrFamily( value );
        }
        _ = Assert.ThrowsException<ArgumentException>( () => { AssertIntShouldCastToDeviceAddrFamily( maxValue + 1 ); } );
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
        int oredValue = 0;
        foreach ( var enumValue in Enum.GetValues( typeof( DeviceOperationFlags ) ) )
        {
            value = ( int ) enumValue;
            AssertIntShouldCastToDeviceOperationFlags( value );
            oredValue |= value;
            AssertIntShouldCastToDeviceOperationFlags( oredValue );
        }
        _ = Assert.ThrowsException<ArgumentException>( () => { AssertIntShouldCastToDeviceOperationFlags( oredValue + 1 ); } );
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
        int oredValue = 0;
        foreach ( var enumValue in Enum.GetValues( typeof( DeviceReadReasons ) ) )
        {
            value = ( int ) enumValue;
            AssertIntShouldCastToDeviceReadReasons( value );
            oredValue |= value;
            AssertIntShouldCastToDeviceReadReasons( oredValue );
        }
        _ = Assert.ThrowsException<ArgumentException>( () => { AssertIntShouldCastToDeviceReadReasons( oredValue + 1 ); } );
    }

    #endregion

}
