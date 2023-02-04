using cc.isr.VXI11.Logging;
using cc.isr.VXI11.IEEE488;
using cc.isr.VXI11.IEEE488.EnumExtensions;
using cc.isr.VXI11.IEEE488.Mock;
using cc.isr.VXI11.LXI;
using cc.isr.VXI11.LXI.Mock;
using cc.isr.VXI11.Codecs;
using System.Text;

namespace cc.isr.VXI11.IEEE488.MSTest;

/// <summary>   (Unit Test Class) a support tests. </summary>
[TestClass]
public class Ieee488LxiDeviceTests
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
        AssertShouldDestroyLink();
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
            InterfaceDeviceString = interfaceDeviceString,
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

        lxiDevice.RemoteEnabled = true;

        return linkResp;
    }

    /// <summary>
    /// Calls remote procedure <see cref="Vxi11Message.DestroyLinkProcedure"/>;
    /// Closes a link to a device.
    /// </summary>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="Codecs.DeviceError"/>.
    /// </returns>
    public static DeviceError DestroyLink( ILxiDevice? lxiDevice )
    {
        if ( lxiDevice is null )
            return new DeviceError() { ErrorCode = DeviceErrorCode.ChannelNotEstablished };
        DeviceLink? link = lxiDevice.DeviceLink;
        try
        {
            if ( link is not null )
            {
                return lxiDevice.DestroyLink( link );
            }
            else
            {
                return new DeviceError();
            }
        }
        catch ( Exception )
        {
            throw;
        }
        finally
        {
        }
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
            Link = lxiDevice.DeviceLink,
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

    /// <summary>   Assert should destroy link. </summary>
    private static void AssertShouldDestroyLink()
    {
        if ( _lxiDevice is not null && !_lxiDevice.CanCreateNewDeviceLink() )
        {
            DeviceError deviceError = DestroyLink( _lxiDevice );
            Assert.IsNotNull( deviceError );
            Assert.AreEqual( DeviceErrorCode.NoError, deviceError.ErrorCode );
        }
    }

    /// <summary>   (Unit Test Method) should read identity. </summary>
    /// <remarks>
    /// <code>
    /// Standard Output: 
    ///   2023-02-03 20:09:12.193,cc.isr.VXI11.IEEE488.MSTest.Ieee488LxiDevice.Ieee488LxiDevice
    ///   2023-02-03 20:09:12.275,creating link to inst0
    ///   2023-02-03 20:09:12.279, link ID: 1 -> Received：*IDN?
    ///   
    ///   2023-02-03 20:09:12.279,Process the instruction： *IDN?
    ///   2023-02-03 20:09:12.280,Query results： INTEGRATED SCIENTIFIC RESOURCES,MODEL IEEE488Mock,001,1.0.8434。
    /// </code>
    /// </remarks>
    [TestMethod]
    public void ShouldReadIdentity()
    {
        if ( _mockDevice is null ) return;         AssertShouldCreateLink();

        string command = $"{Ieee488Commands.IDNRead}\n";

        DeviceWriteResp writeResp = Send( _lxiDevice, command );
        Assert.AreEqual( DeviceErrorCode.NoError, writeResp.ErrorCode );
        Assert.AreEqual( command.Length, writeResp.Size );

        string expectedValue = _mockDevice.Identity;
        DeviceReadResp readResp = Receive( _lxiDevice, _lxiDevice!.MaxReceiveLength );
        Assert.AreEqual( DeviceErrorCode.NoError, readResp.ErrorCode );
        Assert.AreEqual( expectedValue, _lxiDevice!.CharacterEncoding.GetString( readResp.GetData() ) );
    }
}
