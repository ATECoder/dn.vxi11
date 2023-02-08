using cc.isr.VXI11.Codecs;
using cc.isr.VXI11.Logging;
using cc.isr.VXI11.Server;

namespace cc.isr.VXI11.MSTest;

/// <summary>   (Unit Test Class) a VXI-11 device tests. </summary>
/// <remarks>   2023-02-06. </remarks>
[TestClass]
public class Vxi11DeviceTests
{

    #region " Fixture construction and cleanup "

    /// <summary>   Initializes the fixture. </summary>
    /// <param name="context">  The context. </param>
    [ClassInitialize]
    public static void InitializeFixture( TestContext context )
    {
        try
        {
            _classTestContext = context;
            Logger.Writer.LogInformation( $"{_classTestContext.FullyQualifiedTestClassName}.{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}" );

            _vxi11Instrument = new Vxi11Instrument();
            _vxi11Device = new Vxi11Device( _vxi11Instrument );

            int clientId = 1;
            _vxi11Device.ClientId = clientId;
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

    private static IVxi11Instrument? _vxi11Instrument;

    private static IVxi11Device? _vxi11Device;

    #endregion

    #region " client emulations "
    private static CreateLinkResp CreateLink( IVxi11Device? vxi11Device, string interfaceDeviceString, bool lockEnabled = false, int lockTimeout = 1000 )
    {
        if ( vxi11Device is null )
            return new CreateLinkResp() { ErrorCode = DeviceErrorCode.ChannelNotEstablished };

        CreateLinkParms createLinkParam = new() {
            DeviceName = interfaceDeviceString,
            LockDevice = lockEnabled,
            LockTimeout = lockTimeout,
        };
        CreateLinkResp linkResp = vxi11Device.CreateLink( createLinkParam );
        if ( linkResp.ErrorCode == DeviceErrorCode.NoError )
        {
            vxi11Device.DeviceLink = linkResp.DeviceLink;
            vxi11Device.MaxReceiveLength = linkResp.MaxReceiveSize;
            vxi11Device.LastDeviceError = linkResp.ErrorCode;
            vxi11Device.AbortPortNumber = linkResp.AbortPort;

            vxi11Device.DeviceName = interfaceDeviceString;
        }

        vxi11Device.RemoteEnabled = true;

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
            return new DeviceError() { ErrorCode = DeviceErrorCode.ChannelNotEstablished };
        DeviceLink? link = vxi11Device.DeviceLink;
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
        return vxi11Device is null
            ? new DeviceWriteResp() { ErrorCode = DeviceErrorCode.ChannelNotEstablished }
            : Send( vxi11Device, vxi11Device.CharacterEncoding.GetBytes( message ) );
    }

    public static DeviceWriteResp Send( IVxi11Device? vxi11Device, byte[] data )
    {
        if ( vxi11Device is null )
            return new DeviceWriteResp() { ErrorCode = DeviceErrorCode.ChannelNotEstablished };

        if ( vxi11Device.DeviceLink is null )
            return new DeviceWriteResp() { ErrorCode = DeviceErrorCode.ChannelNotEstablished };

        if ( data is null || data.Length == 0 ) return new DeviceWriteResp();

        if ( data.Length > vxi11Device.MaxReceiveLength )
            throw new DeviceException( $"Data size {data.Length} exceed {nameof( Vxi11Device.MaxReceiveLength )}({vxi11Device.MaxReceiveLength})", DeviceErrorCode.IOError );

        DeviceWriteParms writeParam = new() {
            Link = vxi11Device.DeviceLink,
            IOTimeout = vxi11Device.IOTimeout, // in ms
            LockTimeout = vxi11Device.LockTimeout, // in ms
            Flags = vxi11Device.Eoi ? DeviceOperationFlags.EndIndicator : DeviceOperationFlags.None,
        };
        writeParam.SetData( data );
        return vxi11Device.DeviceWrite( writeParam );
    }

    public static DeviceReadResp Receive( IVxi11Device? vxi11Device, int byteCount )
    {
        if ( vxi11Device is null )
            return new DeviceReadResp() { ErrorCode = DeviceErrorCode.ChannelNotEstablished };

        if ( vxi11Device.DeviceLink is null )
            return new DeviceReadResp() { ErrorCode = DeviceErrorCode.ChannelNotEstablished };

        DeviceReadParms readParam = new() {
            Link = vxi11Device.DeviceLink,
            RequestSize = byteCount,
            IOTimeout = vxi11Device.IOTimeout,
            LockTimeout = vxi11Device.LockTimeout,
            Flags = vxi11Device.ReadTermination > 0 ? DeviceOperationFlags.TerminationCharacterSet : DeviceOperationFlags.None,
            TermChar = vxi11Device.ReadTermination
        };
        return vxi11Device.DeviceRead( readParam );
    }

    #endregion

    /// <summary>   Assert should create link. </summary>
    /// <remarks>   2023-02-03. </remarks>
    private static void AssertShouldCreateLink()
    {
        if ( _vxi11Device is not null && _vxi11Device.CanCreateNewDeviceLink() )
        {
            CreateLinkResp linkResp = CreateLink( _vxi11Device, "inst0" );
            Assert.AreEqual( DeviceErrorCode.NoError, linkResp.ErrorCode );
        }
    }

    /// <summary>   Assert should destroy link. </summary>
    private static void AssertShouldDestroyLink()
    {
        if ( _vxi11Device is not null && !_vxi11Device.CanCreateNewDeviceLink() )
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
    /// 2023-02-04 19:37:27.368,cc.isr.LXI.IEEE488.MSTest.Ieee488LxiDeviceTests.Ieee488LxiDeviceTests
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
        if ( _vxi11Instrument is null ) return;         AssertShouldCreateLink();

        string command = $"{Vxi11InstrumentCommands.IDNRead}\n";

        DeviceWriteResp writeResp = Send( _vxi11Device, command );
        Assert.AreEqual( DeviceErrorCode.NoError, writeResp.ErrorCode );
        Assert.AreEqual( command.Length, writeResp.Size );

        string expectedValue = _vxi11Instrument.Identity;
        DeviceReadResp readResp = Receive( _vxi11Device, _vxi11Device!.MaxReceiveLength );
        Assert.AreEqual( DeviceErrorCode.NoError, readResp.ErrorCode );
        Assert.AreEqual( expectedValue, _vxi11Device!.CharacterEncoding.GetString( readResp.GetData() ) );
    }
}
