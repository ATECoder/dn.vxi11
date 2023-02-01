using System.Net;

using cc.isr.VXI11.Codecs;
using cc.isr.VXI11.EnumExtensions;
using cc.isr.VXI11.Logging;

namespace cc.isr.VXI11.MSTest;

/// <summary>   (Unit Test Class) a support tests. </summary>
[TestClass]
public class SupportTests
{

    #region " fixture construction and cleanup "

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
    }

    #endregion

    #region " unique client id "

    /// <summary>   Assert unique client identifier should be generated. </summary>
    /// <returns>   An int. </returns>
    private static int AssertUniqueClientIdShouldBeGenerated()
    {
        int clientId = Support.GenerateUniqueRandomClientIdentifier();
        Assert.IsTrue( clientId >= 0 );
        Assert.IsTrue( clientId < Support.ClientIdentifierUpperBound );
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

    #region " IP Address "
    private static void AssertIPAddressShouldRestoreFromUnsignedIntegerValue( IPAddress address )
    {
        uint unsignedIntAddress = address.ToUInt();
        IPAddress restoredAddress = unsignedIntAddress.ToIPAddress();
        Assert.AreEqual( address.ToString(), restoredAddress.ToString(), $"Address should restored from unsigned integer address {unsignedIntAddress}" );
    }

    [TestMethod]
    public void IPAddressShouldRestoreFromUnsignedIntegerValue()
    {
        SupportTests.AssertIPAddressShouldRestoreFromUnsignedIntegerValue( IPAddress.Parse( "127.0.0.1" ) );
        SupportTests.AssertIPAddressShouldRestoreFromUnsignedIntegerValue( IPAddress.Parse( "192.168.0.1" ) );
        SupportTests.AssertIPAddressShouldRestoreFromUnsignedIntegerValue( IPAddress.Parse( "255.255.255.255" ) );
        SupportTests.AssertIPAddressShouldRestoreFromUnsignedIntegerValue( IPAddress.Parse( "255.255.255.1" ) );
        SupportTests.AssertIPAddressShouldRestoreFromUnsignedIntegerValue( IPAddress.Parse( "1.1.1.1" ) );
        SupportTests.AssertIPAddressShouldRestoreFromUnsignedIntegerValue( IPAddress.Parse( "1.1.1.0" ) );
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
