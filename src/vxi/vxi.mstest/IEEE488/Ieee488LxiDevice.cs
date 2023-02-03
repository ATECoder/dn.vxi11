using cc.isr.VXI11.Logging;
using cc.isr.VXI11.IEEE488;
using cc.isr.VXI11.IEEE488.EnumExtensions;
using cc.isr.VXI11.IEEE488.Mock;
using cc.isr.VXI11.LXI;
using cc.isr.VXI11.LXI.Mock;
using cc.isr.VXI11.Codecs;
using System.Text;

namespace cc.isr.VXI11.MSTest.IEEE488;

/// <summary>   (Unit Test Class) a support tests. </summary>
[TestClass]
public class Ieee488LxiDevice
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

            _mockDevice = new Ieee488Device();
            _lxiDevice = new LxiDevice( _mockDevice );

            int clientId = 1;
            _lxiDevice.ClientId = clientId;




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

    private static IIeee488Device? _mockDevice;

    private static ILxiDevice? _lxiDevice;

    #endregion

    #region " client emulations "
    private static CreateLinkResp CreateLink( ILxiDevice? lxiDevice, string interfaceDeviceString, bool lockEnabled = false, int lockTimeout = 1000 )
    {
        if ( lxiDevice is null )
            return new CreateLinkResp() { ErrorCode = DeviceErrorCode.ChannelNotEstablished };

        CreateLinkParms createLinkParam = new() {
            Device = interfaceDeviceString,
            LockDevice = lockEnabled,
            LockTimeout = lockTimeout,
        };
        CreateLinkResp linkResp = lxiDevice.CreateLink( createLinkParam );
        if ( linkResp.ErrorCode == DeviceErrorCode.NoError )
        {
            lxiDevice.DeviceLink = linkResp.DeviceLink;
            lxiDevice.MaxReceiveLength = linkResp.MaxReceiveSize;
            lxiDevice.LastDeviceError = linkResp.ErrorCode;
            lxiDevice.AbortPortNumber = linkResp.AbortPort;

            lxiDevice.InterfaceDeviceString = interfaceDeviceString;
        }
        return linkResp;
    }

    public static DeviceWriteResp Send( ILxiDevice? lxiDevice, string message )
    {
        if ( lxiDevice is null )
            return new DeviceWriteResp() { ErrorCode = DeviceErrorCode.ChannelNotEstablished };
        return Send( lxiDevice, lxiDevice.CharacterEncoding.GetBytes( message ) );
    }

    public static DeviceWriteResp Send( ILxiDevice? lxiDevice, byte[] data )
    {
        if ( lxiDevice is null )
            return new DeviceWriteResp() { ErrorCode = DeviceErrorCode.ChannelNotEstablished };

        if ( lxiDevice.DeviceLink is null )
            return new DeviceWriteResp() { ErrorCode = DeviceErrorCode.ChannelNotEstablished };

        if ( data is null || data.Length == 0 ) return new DeviceWriteResp();

        if ( data.Length > lxiDevice.MaxReceiveLength )
            throw new DeviceException( $"Data size {data.Length} exceed {nameof( LxiDevice.MaxReceiveLength )}({lxiDevice.MaxReceiveLength})", DeviceErrorCode.IOError );

        DeviceWriteParms writeParam = new() {
            Link = lxiDevice.DeviceLink,
            IOTimeout = lxiDevice.IOTimeout, // in ms
            LockTimeout = lxiDevice.LockTimeout, // in ms
            Flags = lxiDevice.Eoi ? DeviceOperationFlags.EndIndicator : DeviceOperationFlags.None,
        };
        writeParam.SetData( data );
        return lxiDevice.DeviceWrite( writeParam );
    }

    public static DeviceReadResp Receive( ILxiDevice? lxiDevice, int byteCount )
    {
        if ( lxiDevice is null )
            return new DeviceReadResp() { ErrorCode = DeviceErrorCode.ChannelNotEstablished };

        if ( lxiDevice.DeviceLink is null )
            return new DeviceReadResp() { ErrorCode = DeviceErrorCode.ChannelNotEstablished };

        DeviceReadParms readParam = new() {
            Link =  lxiDevice.DeviceLink,
            RequestSize = byteCount,
            IOTimeout = lxiDevice.IOTimeout,
            LockTimeout = lxiDevice.LockTimeout,
            Flags = lxiDevice.ReadTermination > 0 ? DeviceOperationFlags.TerminationCharacterSet : DeviceOperationFlags.None,
            TermChar = lxiDevice.ReadTermination
        };
        return lxiDevice.DeviceRead( readParam );
    }

    #endregion

    /// <summary>   Assert should create link. </summary>
    /// <remarks>   2023-02-03. </remarks>
    private static void AssertShouldCreateLink()
    {
        if ( _lxiDevice is not null && _lxiDevice.CanCreateNewDeviceLink() )
        {
            CreateLinkResp linkResp = CreateLink( _lxiDevice, "inst0" );
            Assert.AreEqual( DeviceErrorCode.NoError, linkResp.ErrorCode );
        }

    }

    [TestMethod]
    public void ShouldReadIdentity()
    {
        if ( _mockDevice is null) { return ; }
        AssertShouldCreateLink();

        string command = "*IDN?\n";

        DeviceWriteResp writeResp = Send( _lxiDevice, command );
        Assert.AreEqual( DeviceErrorCode.NoError, writeResp.ErrorCode );
        Assert.AreEqual( command.Length, writeResp.Size );

        string expectedValue = _mockDevice.Identity;
        DeviceReadResp readResp = Receive( _lxiDevice, _lxiDevice!.MaxReceiveLength );
        Assert.AreEqual( DeviceErrorCode.NoError, readResp.ErrorCode );
        Assert.AreEqual( expectedValue, _lxiDevice!.CharacterEncoding.GetString(readResp.GetData() ) );
    }
}
