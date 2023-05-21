using cc.isr.VXI11.Logging;

namespace cc.isr.VXI11.MSTest;

/// <summary>   (Unit Test Class) an VXI-11 client tests. </summary>
[TestClass]
public class Vxi11ClientTests
{

    #region " Fixture Construction and Cleanup "

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
    {
        _classTestContext = null;
    }

    #endregion

    #region " client identities "

    /// <summary>   Assert unique client identifier should be generated. </summary>
    /// <returns>   An int. </returns>
    private static int AssertUniqueClientIdShouldBeGenerated()
    {
        int clientId = cc.isr.VXI11.Client.Vxi11Client.GetNextClientId();
        Assert.IsTrue( clientId >= 0 );
        return clientId;
    }


    /// <summary>   (Unit Test Method) unique client identifier should be generated. </summary>
    [TestMethod]
    public void UniqueClientIdShouldBeGenerated()
    {
        int clientId = AssertUniqueClientIdShouldBeGenerated();
        int nextClientId = AssertUniqueClientIdShouldBeGenerated();
        Assert.AreNotEqual( nextClientId, clientId, $"The next client id {nextClientId} should not be the same as the previous id {clientId}" );
    }

    #endregion

}
