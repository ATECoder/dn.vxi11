using cc.isr.VXI11.Codecs;
using cc.isr.VXI11.Logging;
using cc.isr.VXI11.Server;

namespace cc.isr.VXI11.MSTest;

/// <summary>   (Unit Test Class) a VXI-11 device tests. </summary>
[TestClass]
public class Vxi11DeviceTests
{

    #region " Fixture Construction and Cleanup "

    /// <summary>   Initializes the fixture. </summary>
    /// <param name="context">  The context. </param>
    [ClassInitialize]
    public static void InitializeFixture( TestContext context )
    {
        try
        {
            _classTestContext = context;
            Logger.Writer.LogInformation( $"{_classTestContext.FullyQualifiedTestClassName}.{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}" );

            _vxi11Device = new Vxi11Device( new Vxi11InstrumentFactory(), new Vxi11InterfaceFactory() );

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
        AssertShouldDestroyLink();
    }

    private static IVxi11Device? _vxi11Device;

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
            CreateLinkResp linkResp = CreateLink( _vxi11Device, "inst0" );
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
    /// 2023-02-04 19:37:27.378,creating link to inst0
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
