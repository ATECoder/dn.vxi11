using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace cc.isr.VXI11.IEEE488.MSTest;

/// <summary>   (Unit Test Class) an ieee 488 client tests. </summary>
/// <remarks>   2023-01-17. </remarks>
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
            System.Diagnostics.Debug.WriteLine( $"{context.FullyQualifiedTestClassName}.{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}" );
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

    #region " client identities "

    /// <summary>   Assert unique client identifier should be generated. </summary>
    /// <remarks>   2023-01-17. </remarks>
    /// <returns>   An int. </returns>
    private static int AssertUniqueClientIdShouldBeGenerated()
    {
        int clientId = IEEE488.Ieee488Client.GetNextClientId();
        Assert.IsTrue( clientId >= 0 );
        return clientId;
    }


    /// <summary>   (Unit Test Method) unique client identifier should be generated. </summary>
    /// <remarks>   2023-01-17. </remarks>
    [TestMethod]
    public void UniqueClientIdShouldBeGenerated()
    {
        int clientId = AssertUniqueClientIdShouldBeGenerated();
        int nextClientId = AssertUniqueClientIdShouldBeGenerated();
        Assert.AreNotEqual( nextClientId, clientId, $"The next client id {nextClientId} should not be the same as the previous id {clientId}" );
    }

    #endregion

}
