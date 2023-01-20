using cc.isr.VXI11.Codecs;
using cc.isr.VXI11.EnumExtensions;
using cc.isr.XDR;

namespace cc.isr.VXI11.MSTest;

/// <summary>   (Unit Test Class) a support tests. </summary>
/// <remarks>   2023-01-17. </remarks>
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
            Console.WriteLine( $"{context.FullyQualifiedTestClassName}.{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}" );
            _classTestContext = context;
            Console.WriteLine( @$"{DateTime.Now:yyyy:MM:dd:hh:mm:ss.fff} starting {_classTestContext.FullyQualifiedTestClassName}.{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}" );
        }
        catch ( Exception ex )
        {
            Console.WriteLine( $"Failed initializing fixture: \n{ex} " );
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

    #region " support "

    /// <summary>   Assert unique client identifier should be generated. </summary>
    /// <remarks>   2023-01-17. </remarks>
    /// <returns>   An int. </returns>
    private static int AssertUniqueClientIdShouldBeGenerated()
    {
        int clientId = Support.GenerateUniqueRandomClientIdentifier();
        Assert.IsTrue( clientId >= 0 );
        Assert.IsTrue( clientId < Support.ClientIdentifierUpperBound );
        return clientId;
    }

    /// <summary>   (Unit Test Method) unique client identifier should be generated. </summary>
    /// <remarks>   2023-01-17. </remarks>
    [TestMethod]
    public void UniqueClientIdShouldBeGenerated()
    {
        int clientId = AssertUniqueClientIdShouldBeGenerated();
        int nextClientId = AssertUniqueClientIdShouldBeGenerated();
        Assert.AreNotEqual( clientId , nextClientId , "the client id should be unique" );
        clientId = int.MaxValue - 2;
        clientId = ++clientId == int.MaxValue ? 0 : clientId;
        Assert.AreEqual( int.MaxValue - 1, clientId );
        clientId = ++clientId == int.MaxValue ? 0 : clientId;
        Assert.AreEqual( 0, clientId );
    }

    #endregion

    #region " enum extensions "

    /// <summary>   Assert should get description. </summary>
    /// <param name="value">                The value. </param>
    /// <param name="expectedDescription">  Information describing the expected. </param>
    private static void AssertShouldGetDescription( DeviceErrorCodeValue value, string expectedDescription )
    {
        string actual = value.GetDescription();
        Assert.AreEqual( expectedDescription, actual );
    }

    /// <summary>   (Unit Test Method) message type should get description. </summary>
    [TestMethod]
    public void DeviceErrorCodeValueShouldGetDescription()
    {
        AssertShouldGetDescription( DeviceErrorCodeValue.NoError, "No error." );
    }

    /// <summary>   Assert <see langword="int"/> should cast to <see cref="DeviceErrorCodeValue"/>. </summary>
    /// <param name="expected"> The expected value. </param>
    private static void AssertIntShouldCastToDeviceErrorCodeValue( int expected )
    {
        DeviceErrorCodeValue actual = expected.ToDeviceErrorCodeValue();
        Assert.AreEqual( expected, ( int ) actual );
    }

    /// <summary>   (Unit Test Method) <see langword="int"/> should cast to <see cref="DeviceErrorCodeValue"/>. </summary>
    [TestMethod]
    public void IntShouldCastToDeviceErrorCodeValue()
    {
        int value = 0;
        int maxValue = 0;
        foreach ( var enumValue in Enum.GetValues( typeof( DeviceErrorCodeValue ) ) )
        {
            value = ( int ) enumValue;
            maxValue = value > maxValue ? value : maxValue;
            AssertIntShouldCastToDeviceErrorCodeValue( value );
        }
        _ = Assert.ThrowsException<ArgumentException>( () => { AssertIntShouldCastToDeviceErrorCodeValue( maxValue + 1 ); } );
    }

    /// <summary>   Assert <see langword="int"/> should cast to <see cref="DeviceAddrFamily"/>. </summary>
    /// <param name="expected"> The expected value. </param>
    private static void AssertIntShouldCastToDeviceAddrFamily( int expected )
    {
        DeviceAddrFamily actual = expected.ToDeviceAddrFamily();
        Assert.AreEqual( expected, ( int ) actual );
    }

    /// <summary>   (Unit Test Method) <see langword="int"/> should cast to <see cref="DeviceAddrFamily"/>. </summary>
    [TestMethod]
    public void IntShouldCastToDeviceAddrFamily()
    {
        int value = 0;
        int maxValue = 0;
        foreach ( var enumValue in Enum.GetValues( typeof( DeviceAddrFamily ) ) )
        {
            value = ( int ) enumValue;
            maxValue = value > maxValue ? value : maxValue;
            AssertIntShouldCastToDeviceAddrFamily( value );
        }
        _ = Assert.ThrowsException<ArgumentException>( () => { AssertIntShouldCastToDeviceAddrFamily( maxValue + 1 ); } );
    }

    /// <summary>   Assert <see langword="int"/> should cast to <see cref="DeviceOperationFlags"/>. </summary>
    /// <param name="expected"> The expected value. </param>
    private static void AssertIntShouldCastToDeviceOperationFlags( int expected )
    {
        DeviceOperationFlags actual = expected.ToDeviceOperationFlags();
        Assert.AreEqual( expected, ( int ) actual );
    }

    /// <summary>   (Unit Test Method) <see langword="int"/> should cast to <see cref="DeviceOperationFlags"/>. </summary>
    [TestMethod]
    public void IntShouldCastToDeviceOperationFlags()
    {
        int value = 0;
        int maxValue = 0;
        foreach ( var enumValue in Enum.GetValues( typeof( DeviceOperationFlags ) ) )
        {
            value = ( int ) enumValue;
            maxValue = value > maxValue ? value : maxValue;
            AssertIntShouldCastToDeviceOperationFlags( value );
        }
        _ = Assert.ThrowsException<ArgumentException>( () => { AssertIntShouldCastToDeviceOperationFlags( maxValue + 1 ); } );
    }

    /// <summary>   Assert <see langword="int"/> should cast to <see cref="DeviceReadReasons"/>. </summary>
    /// <param name="expected"> The expected value. </param>
    private static void AssertIntShouldCastToDeviceReadReasons( int expected )
    {
        DeviceReadReasons actual = expected.ToDeviceReadReasons();
        Assert.AreEqual( expected, ( int ) actual );
    }

    /// <summary>   (Unit Test Method) <see langword="int"/> should cast to <see cref="DeviceReadReasons"/>. </summary>
    [TestMethod]
    public void IntShouldCastToDeviceReadReasons()
    {
        int value = 0;
        int maxValue = 0;
        foreach ( var enumValue in Enum.GetValues( typeof( DeviceReadReasons ) ) )
        {
            value = ( int ) enumValue;
            maxValue = value > maxValue ? value : maxValue;
            AssertIntShouldCastToDeviceReadReasons( value );
        }
        _ = Assert.ThrowsException<ArgumentException>( () => { AssertIntShouldCastToDeviceReadReasons( maxValue + 1 ); } );
    }

    #endregion

}
