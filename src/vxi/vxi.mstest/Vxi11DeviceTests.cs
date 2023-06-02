using cc.isr.VXI11.Codecs;
using cc.isr.VXI11.Server;

namespace cc.isr.VXI11.MSTest;

/// <summary>   (Unit Test Class) a VXI-11 device tests. </summary>
[TestClass]
public class Vxi11DeviceTests
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

            _vxi11Device = new Vxi11Device( new Vxi11InstrumentFactory(), new Vxi11InterfaceFactory() );

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
    {
        AssertShouldDestroyLink();
    }

    private static IVxi11Device? _vxi11Device;

    private IDisposable? _loggerScope;

    private LoggerTraceListener<Vxi11DeviceTests>? _traceListener;

    /// <summary> Initializes the test class instance before each test runs. </summary>
    [TestInitialize()]
    public void InitializeBeforeEachTest()
    {
        if ( Logger is not null )
        {
            this._loggerScope = Logger.BeginScope( this.TestContext?.TestName ?? string.Empty );
            this._traceListener = new LoggerTraceListener<Vxi11DeviceTests>( Logger );
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
    public static ILogger<Vxi11DeviceTests>? Logger { get; } = LoggerProvider.InitLogger<Vxi11DeviceTests>();

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
        Trace.TraceError( "Testing tracing an error" ); Trace.Flush();
        Assert.IsTrue( this._traceListener?.Any( TraceEventType.Error ), $"{nameof( this._traceListener )} should have {TraceEventType.Error} messages" );

        // no need to report errors for this test.

        this._traceListener?.Clear();
    }

    #endregion

    #region " unique link "

    /// <summary>   Assert unique Link identifier should be generated. </summary>
    /// <returns>   An int. </returns>
    private static int AssertUniqueLinkIdShouldBeGenerated()
    {
        int LinkId = Vxi11Device.GetNextLinkId();
        Assert.IsTrue( LinkId >= 0 );
        return LinkId;
    }


    /// <summary>   (Unit Test Method) unique Link identifier should be generated. </summary>
    [TestMethod]
    public void UniqueLinkIdShouldBeGenerated()
    {
        int LinkId = AssertUniqueLinkIdShouldBeGenerated();
        int nextLinkId = AssertUniqueLinkIdShouldBeGenerated();
        Assert.AreNotEqual( nextLinkId, LinkId, $"The next Link id {nextLinkId} should not be the same as the previous id {LinkId}" );
    }


    #endregion

    #region " client emulations "

    private static CreateLinkResp CreateLink( IVxi11Device? vxi11Device, string deviceName, bool lockEnabled = false, int lockTimeout = 1000 )
    {
        if ( vxi11Device is null )
            return new CreateLinkResp() { ErrorCode = DeviceErrorCode.ChannelNotEstablished };

        CreateLinkParms createLinkParam = new() {
            DeviceName = deviceName,
            LockDevice = lockEnabled,
            LockTimeout = lockTimeout,
            ClientId = 1,
        };
        CreateLinkResp linkResp = vxi11Device.CreateLink( createLinkParam );

        return linkResp;
    }

    /// <summary>
    /// Calls remote procedure <see cref="Vxi11Message.DestroyLinkProcedure"/>;
    /// Closes a link to a device.
    /// </summary>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="Codecs.DeviceError"/>.
    /// </returns>
    public static DeviceError DestroyLink( IVxi11Device? vxi11Device )
    {
        if ( vxi11Device is null )
            return new DeviceError() { ErrorCode = DeviceErrorCode.DeviceNotAccessible };

        if ( vxi11Device.ActiveInstrument is null )
            return new DeviceError() { ErrorCode = DeviceErrorCode.DeviceNotAccessible };

        if ( vxi11Device.ActiveInstrument.ActiveServerClient is null )
            return new DeviceError() { ErrorCode = DeviceErrorCode.ChannelNotEstablished };

        DeviceLink? link = new( vxi11Device.ActiveInstrument.ActiveServerClient.LinkId );
        try
        {
            return link is not null ? vxi11Device.DestroyLink( link ) : new DeviceError();
        }
        catch ( Exception )
        {
            throw;
        }
        finally
        {
        }
    }


    public static DeviceWriteResp Send( IVxi11Device? vxi11Device, string message )
    {
        return vxi11Device is null || vxi11Device.ActiveInstrument is null
            ? new DeviceWriteResp() { ErrorCode = DeviceErrorCode.ChannelNotEstablished }
            : Send( vxi11Device, vxi11Device.ActiveInstrument.CharacterEncoding.GetBytes( message ) );
    }

    public static DeviceWriteResp Send( IVxi11Device? vxi11Device, byte[] data )
    {
        if ( vxi11Device is null )
            return new DeviceWriteResp() { ErrorCode = DeviceErrorCode.DeviceNotAccessible };

        if ( vxi11Device.ActiveInstrument is null )
            return new DeviceWriteResp() { ErrorCode = DeviceErrorCode.DeviceNotAccessible };

        if ( vxi11Device.ActiveInstrument.ActiveServerClient is null )
            return new DeviceWriteResp() { ErrorCode = DeviceErrorCode.ChannelNotEstablished };

        if ( data is null || data.Length == 0 ) return new DeviceWriteResp();

        if ( data.Length > vxi11Device.ActiveInstrument.MaxReceiveLength )
            throw new DeviceException( $"Data size {data.Length} exceed {nameof( Vxi11Device.ActiveInstrument.MaxReceiveLength )}({vxi11Device.ActiveInstrument.MaxReceiveLength})", DeviceErrorCode.IOError );

        bool eoi = data.Length < vxi11Device.ActiveInstrument.MaxReceiveLength;
        DeviceWriteParms writeParam = new() {
            Link = new DeviceLink( vxi11Device.ActiveInstrument.ActiveServerClient.LinkId ),
            IOTimeout = vxi11Device.ActiveInstrument.IOTimeout, // in ms
            LockTimeout = vxi11Device.ActiveInstrument.LockTimeout, // in ms
            Flags = eoi ? DeviceOperationFlags.EndIndicator : DeviceOperationFlags.None,
        };
        writeParam.SetData( data );
        return vxi11Device.DeviceWrite( writeParam );
    }

    public static DeviceReadResp Receive( IVxi11Device? vxi11Device, int byteCount )
    {
        if ( vxi11Device is null )
            return new DeviceReadResp() { ErrorCode = DeviceErrorCode.DeviceNotAccessible };

        if ( vxi11Device.ActiveInstrument is null )
            return new DeviceReadResp() { ErrorCode = DeviceErrorCode.DeviceNotAccessible };

        if ( vxi11Device.ActiveInstrument.ActiveServerClient is null )
            return new DeviceReadResp() { ErrorCode = DeviceErrorCode.ChannelNotEstablished };

        DeviceReadParms readParam = new() {
            Link = new DeviceLink( vxi11Device.ActiveInstrument.ActiveServerClient.LinkId ),
            RequestSize = byteCount,
            IOTimeout = vxi11Device.ActiveInstrument.IOTimeout,
            LockTimeout = vxi11Device.ActiveInstrument.LockTimeout,
            Flags = vxi11Device.ActiveInstrument.ReadTermination > 0 ? DeviceOperationFlags.TerminationCharacterSet : DeviceOperationFlags.None,
            TermChar = vxi11Device.ActiveInstrument.ReadTermination
        };
        return vxi11Device.DeviceRead( readParam );
    }

    #endregion

    /// <summary>   Assert should create link. </summary>
    /// <remarks>   2023-02-03. </remarks>
    private static void AssertShouldCreateLink()
    {
        Assert.IsNotNull( _vxi11Device, nameof( _vxi11Device ) );
        if ( _vxi11Device.ActiveInstrument?.ActiveServerClient is null )
        {
            CreateLinkResp linkResp = CreateLink( _vxi11Device, "INST0" );
            Assert.AreEqual( DeviceErrorCode.NoError, linkResp.ErrorCode );
        }
    }

    /// <summary>   Assert should destroy link. </summary>
    private static void AssertShouldDestroyLink()
    {
        Assert.IsNotNull( _vxi11Device, nameof( _vxi11Device ) );
        if ( _vxi11Device.ActiveInstrument?.ActiveServerClient is not null )
        {
            DeviceError deviceError = DestroyLink( _vxi11Device );
            Assert.IsNotNull( deviceError );
            Assert.AreEqual( DeviceErrorCode.NoError, deviceError.ErrorCode );
        }
    }

    /// <summary>   (Unit Test Method) should read identity. </summary>
    /// <remarks>
    /// <code>
    /// Standard Output: 
    /// 2023-02-04 19:37:27.368,cc.isr.VXI11.MSTest.Vxi11DeviceTests.Vxi11DeviceTests
    /// 2023-02-04 19:37:27.378,creating link to INST0
    /// 2023-02-04 19:37:27.382, link ID: 1 -> Received：*IDN?
    ///
    /// 2023-02-04 19:37:27.382,Process the instruction： *IDN?
    /// 2023-02-04 19:37:27.383,Query results： INTEGRATED SCIENTIFIC RESOURCES,MODEL IEEE488Mock,001,1.0.8434。
    /// </code>
    /// </remarks>
    [TestMethod]
    public void ShouldReadIdentity()
    {
        AssertShouldCreateLink();

        string command = $"{Vxi11InstrumentCommands.IDNRead}\n";

        DeviceWriteResp writeResp = Send( _vxi11Device, command );
        Assert.AreEqual( DeviceErrorCode.NoError, writeResp.ErrorCode );
        Assert.AreEqual( command.Length, writeResp.Size );

        string expectedValue = _vxi11Device?.ActiveInstrument?.Identity ?? string.Empty;
        DeviceReadResp readResp = Receive( _vxi11Device, _vxi11Device!.ActiveInstrument!.MaxReceiveLength );
        Assert.AreEqual( DeviceErrorCode.NoError, readResp.ErrorCode );
        Assert.AreEqual( expectedValue, _vxi11Device!.ActiveInstrument!.CharacterEncoding.GetString( readResp.GetData() ) );
    }
}
