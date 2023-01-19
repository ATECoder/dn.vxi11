using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

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

}
