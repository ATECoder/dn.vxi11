using cc.isr.VXI11.Logging;
using cc.isr.VXI11.IEEE488;
using cc.isr.VXI11.IEEE488.EnumExtensions;
using cc.isr.VXI11.IEEE488.Mock;

namespace cc.isr.VXI11.IEEE488.MSTest;

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

    #region " enum extensions "

    /// <summary>   Assert should get description. </summary>
    /// <param name="value">                The value. </param>
    /// <param name="expectedDescription">  Information describing the expected. </param>
    private static void AssertShouldGetDescription( Ieee488OperationType value, string expectedDescription )
    {
        string actual = value.GetDescription();
        Assert.AreEqual( expectedDescription, actual );
    }

    /// <summary>   (Unit Test Method) message type should get description. </summary>
    [TestMethod]
    public void Ieee488OperationTypeShouldGetDescription()
    {
        AssertShouldGetDescription( Ieee488OperationType.Read, "Read reply from the device." );
    }

    /// <summary>   Assert <see cref="int"/> should cast to <see cref="Ieee488OperationType"/>. </summary>
    /// <param name="expected"> The expected value. </param>
    private static void AssertIntShouldCastToIeee488OperationType( int expected )
    {
        Ieee488OperationType actual = expected.ToIeee488OperationType();
        Assert.AreEqual( expected, ( int ) actual );
    }

    /// <summary>   (Unit Test Method) <see cref="int"/> should cast to <see cref="Ieee488OperationType"/>. </summary>
    [TestMethod]
    public void IntShouldCastToIeee488OperationType()
    {
        int value = 0;
        int maxValue = 0;
        foreach ( var enumValue in Enum.GetValues( typeof( Ieee488OperationType ) ) )
        {
            value = ( int ) enumValue;
            maxValue = value > maxValue ? value : maxValue;
            AssertIntShouldCastToIeee488OperationType( value );
        }
        _ = Assert.ThrowsException<ArgumentException>( () => { AssertIntShouldCastToIeee488OperationType( maxValue + 1 ); } );
    }

    /// <summary>   Assert <see cref="int"/> should cast to <see cref="Ieee488InterfaceCommand"/>. </summary>
    /// <param name="expected"> The expected value. </param>
    private static void AssertIntShouldCastToIeee488InterfaceCommand( int expected )
    {
        Ieee488InterfaceCommand actual = expected.ToIeee488InterfaceCommand();
        Assert.AreEqual( expected, ( int ) actual );
    }

    /// <summary>   (Unit Test Method) <see cref="int"/> should cast to <see cref="Ieee488InterfaceCommand"/>. </summary>
    [TestMethod]
    public void IntShouldCastToIeee488InterfaceCommand()
    {
        int value = 0;
        int maxValue = 0;
        foreach ( var enumValue in Enum.GetValues( typeof( Ieee488InterfaceCommand ) ) )
        {
            value = ( int ) enumValue;
            maxValue = value > maxValue ? value : maxValue;
            AssertIntShouldCastToIeee488InterfaceCommand( value );
        }
        _ = Assert.ThrowsException<ArgumentException>( () => { AssertIntShouldCastToIeee488InterfaceCommand( maxValue + 1 ); } );
    }

    /// <summary>   Assert <see cref="int"/> should cast to <see cref="Ieee488InterfaceCommandOption"/>. </summary>
    /// <param name="expected"> The expected value. </param>
    private static void AssertIntShouldCastToIeee488InterfaceCommandOption( int expected )
    {
        Ieee488InterfaceCommandOption actual = expected.ToIeee488InterfaceCommandOption();
        Assert.AreEqual( expected, ( int ) actual );
    }

    /// <summary>   (Unit Test Method) <see cref="int"/> should cast to <see cref="Ieee488InterfaceCommandOption"/>. </summary>
    [TestMethod]
    public void IntShouldCastToIeee488InterfaceCommandOption()
    {
        int value = 0;
        int maxValue = 0;
        foreach ( var enumValue in Enum.GetValues( typeof( Ieee488InterfaceCommandOption ) ) )
        {
            value = ( int ) enumValue;
            maxValue = value > maxValue ? value : maxValue;
            AssertIntShouldCastToIeee488InterfaceCommandOption( value );
        }
        _ = Assert.ThrowsException<ArgumentException>( () => { AssertIntShouldCastToIeee488InterfaceCommandOption( maxValue + 1 ); } );
    }

    /// <summary>   Assert <see cref="int"/> should cast to <see cref="GpibCommandArgument"/>. </summary>
    /// <param name="expected"> The expected value. </param>
    private static void AssertIntShouldCastToGpibCommandArgument( int expected )
    {
        GpibCommandArgument actual = expected.ToGpibCommandArgument();
        Assert.AreEqual( expected, ( int ) actual );
    }

    /// <summary>   (Unit Test Method) <see cref="int"/> should cast to <see cref="GpibCommandArgument"/>. </summary>
    [TestMethod]
    public void IntShouldCastToGpibCommandArgument()
    {
        int value = 0;
        int maxValue = 0;
        foreach ( var enumValue in Enum.GetValues( typeof( GpibCommandArgument ) ) )
        {
            value = ( int ) enumValue;
            maxValue = value > maxValue ? value : maxValue;
            AssertIntShouldCastToGpibCommandArgument( value );
        }
        _ = Assert.ThrowsException<ArgumentException>( () => { AssertIntShouldCastToGpibCommandArgument( maxValue + 1 ); } );
    }


    #endregion

}
