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
            System.Diagnostics.Debug.WriteLine( $"{context.FullyQualifiedTestClassName}.{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}" );
            _classTestContext = context;
            Console.WriteLine( @$"{DateTime.Now:yyyy:MM:dd:hh:mm:ss.fff} starting {_classTestContext.FullyQualifiedTestClassName}.{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}" );
            DeviceExplorerTests.EnumerateHosts();
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

    public static int GenerateClientIdentifiertest()
    {
        // Initialize the client identifier with some more-or-less random value.
        long seed = DateTime.Now.Ticks;
        int shift = 0x1f;
        long shifted = (seed >> shift);
        int try1 = ( int ) seed ^ ( int ) (seed >> shift);
        int try2 = ( int ) seed ^ ( int ) (seed >> (0x1f));
        int code = seed.GetHashCode();
        return ( int ) seed ^ ( int ) (seed >> (32 & 0x1f));
    }

    [TestMethod]
    public void ClientIdShoudlBeGenerated()
    {
        int clientId = GenerateClientIdentifiertest(); // Support.GenerateClientIdentifier();
        Assert.IsTrue( clientId >= 0 );
        Assert.IsTrue( clientId <= 0x1F );
    }

    #endregion

}
