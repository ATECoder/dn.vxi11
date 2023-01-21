using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

using cc.isr.VXI11.Logging;
namespace cc.isr.VXI11.IEEE488.MSTest;

/// <summary>   (Unit Test Class) an ieee 488 client tests. </summary>
[TestClass]
public class Ieee488ClientTests
{

    #region " fixture construction and cleanup "

    /// <summary>   Initializes the fixture. </summary>
    /// <param name="context">  The context. </param>
    [ClassInitialize]
    public static void InitializeFixture( TestContext context )
    {
        try
        {
            Logger.Writer.LogInformation( $"{context.FullyQualifiedTestClassName}.{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}" );
            _classTestContext = context;
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

    #region " client identities "

    /// <summary>   Assert unique client identifier should be generated. </summary>
    /// <returns>   An int. </returns>
    private static int AssertUniqueClientIdShouldBeGenerated()
    {
        int clientId = IEEE488.Ieee488Client.GetNextClientId();
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
