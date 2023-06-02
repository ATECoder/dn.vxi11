namespace cc.isr.VXI11.MSTest;

/// <summary>   (Unit Test Class) a device name parser tests. </summary>
/// <remarks>   2023-06-01. </remarks>
[TestClass]
public class DeviceNameParserTests
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
    { }

    private IDisposable? _loggerScope;

    private LoggerTraceListener<DeviceNameParserTests>? _traceListener;

    /// <summary> Initializes the test class instance before each test runs. </summary>
    [TestInitialize()]
    public void InitializeBeforeEachTest()
    {
        if ( Logger is not null )
        {
            this._loggerScope = Logger.BeginScope( this.TestContext?.TestName ?? string.Empty );
            this._traceListener = new LoggerTraceListener<DeviceNameParserTests>( Logger );
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
    public static ILogger<DeviceNameParserTests>? Logger { get; } = LoggerProvider.InitLogger<DeviceNameParserTests>();

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
        Trace.TraceInformation( "Testing tracing an info message" ); Trace.Flush();
        Assert.IsTrue( this._traceListener?.Any( TraceEventType.Information ), $"{nameof( this._traceListener )} should have {TraceEventType.Error} messages" );

        // no need to report errors for this test.

        this._traceListener?.Clear();
    }

    #endregion

    #region " device name parser tests "

    private readonly string[] _deviceNames = { "INST0",
                                             "gpib,5",
                                             "gpib,5,10",
                                             "usb0",
                                             "usb0[0x5678::0x33::SN999::1]",
                                             "usb0[1234::5678::MYSERIAL::0]" };

    /// <summary>   Assert device name parse. </summary>
    /// <remarks>   2023-02-11. </remarks>
    /// <param name="deviceName">   Name of the device. </param>
    private static void AssertDeviceNameParse( string deviceName )
    {
        cc.isr.VXI11.DeviceNameParser expectedParser = new( deviceName );
        string builtDeviceName = expectedParser.BuildDeviceName();
        cc.isr.VXI11.DeviceNameParser actualParser = new( builtDeviceName );
        Assert.IsTrue( expectedParser.Equals( actualParser ), $"device name {builtDeviceName} built from parsed {deviceName} not matching" );
        Logger?.LogInformation( $"device is {(string.IsNullOrEmpty( expectedParser.DeviceName ) ? "empty" : expectedParser.DeviceName)} for {deviceName} " );
    }

    /// <summary>   (Unit Test Method) device name parse. </summary>
    /// <remarks>   2023-02-11.
    /// <code>
    /// </code>
    /// </remarks>
    [TestMethod]
    public void DeviceNameParse()
    {
        foreach ( string deviceName in this._deviceNames )
        {
            AssertDeviceNameParse( deviceName );
        }
    }

    #endregion

}
