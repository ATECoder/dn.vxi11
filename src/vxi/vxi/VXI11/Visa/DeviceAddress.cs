namespace cc.isr.VXI11.Visa;

/// <summary>   A VISA INSTR interface device address manager. </summary>
public struct DeviceAddress
{

    /// <summary>   Constructor. </summary>
    /// <param name="interfaceDeviceAddress">  The interface devices string, e.g., <c>gpib0,12,8</c>. </param>
    public DeviceAddress( string interfaceDeviceAddress )
    {
        this.InterfaceDeviceAddress = interfaceDeviceAddress;
        this.InterfaceFamily = string.Empty;
        this.InterfaceName = string.Empty;
        this.ManufacturerId = null;
        this.SerialNumber = null;
        this.ModelCode = null;
        this.UsbTmcInterfaceNumber = null;
        this._interfaceFamilies = new string[] { GenericInterfaceFamily, GpibInterfaceFamily };
        _ = interfaceDeviceAddress.StartsWith( UsbInterfaceFamily, StringComparison.OrdinalIgnoreCase )
            ? this.ParseUsbDeviceAddress( interfaceDeviceAddress )
            : this.ParseDeviceAddress( interfaceDeviceAddress );
    }

    /// <summary>   (Immutable) the generic interface family. </summary>
    public const string GenericInterfaceFamily = "inst";

    /// <summary>   (Immutable) the gpib interface family. </summary>
    public const string GpibInterfaceFamily = "gpib";

    /// <summary>   (Immutable) the USB interface family. </summary>
    public const string UsbInterfaceFamily = "usb";

    public const int MinimumInterfaceNumber = 0;

    /// <summary>   (Immutable) the minimum gpib address. </summary>
    public const int MinimumGpibAddress = 1;

    /// <summary>   (Immutable) the maximum gpib address. </summary>
    public const int MaximumGpibAddress = 31;

    private readonly string[] _interfaceFamilies;

    /// <summary>   Interface families. </summary>
    /// <returns>   A string[]. </returns>
    public string[] InterfaceFamilies()
    {
        return this._interfaceFamilies;
    }

    /// <summary>   Gets or sets the interface devices address, e.g., <c>gpib0,12,13</c>, where <c>gpib</c> is the <see cref="InterfaceFamily"/>
    /// and <c>0</c> is the <see cref="InterfaceNumber"/> and 12 and 13 are the primary and secondary addresses. </summary>
    /// <value> Information describing the device. </value>
    public string InterfaceDeviceAddress { get; set; }

    /// <summary>   Gets or sets the interface family , e.g., <c>gpib</c> or <c>inst</c> </summary>
    /// <value> The interface family. </value>
    public string InterfaceFamily { get; set; }

    /// <summary>
    /// Gets or sets the name of the interface, e.g., <c>gpib0</c>, where <c>gpib</c> is the <see cref="InterfaceFamily"/>
    /// and <c>0</c> is the <see cref="InterfaceNumber"/>.
    /// </summary>
    /// <value> The name of the interface. </value>
    public string InterfaceName { get; set; }

    /// <summary>   Gets or sets the interface number as parsed from the <see cref="InterfaceName"/>. </summary>
    /// <value> The interface number. </value>
    public int? InterfaceNumber { get; set; }

    /// <summary>   Gets or sets the primary address of the <see cref="GpibInterfaceFamily"/> gpib interface. </summary>
    /// <value> The primary address. </value>
    public int? PrimaryAddress { get; set; }

    /// <summary>   Gets or sets the secondary address of the <see cref="GpibInterfaceFamily"/> gpib interface. </summary>
    /// <value> The secondary address. </value>
    public int? SecondaryAddress { get; set; }

    /// <summary>   Gets or sets the identifier of the manufacturer. </summary>
    /// <value> The identifier of the manufacturer. </value>
    public int? ManufacturerId { get; set; }

    /// <summary>   Gets or sets the model code. </summary>
    /// <value> The model code. </value>
    public int? ModelCode { get; set; }

    /// <summary>   Gets or sets the serial number. </summary>
    /// <value> The serial number. </value>
    public string? SerialNumber { get; set; }

    /// <summary>   Gets or sets the USB TMC interface number. </summary>
    /// <value> The USB TMC interface number. </value>
    public int? UsbTmcInterfaceNumber { get; set; }

    /// <summary>   Parses the device address into its components. </summary>
    /// <param name="interfaceDeviceAddress">    Specifies the device address. </param>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    public bool ParseDeviceAddress( string interfaceDeviceAddress )
    {
        if ( string.IsNullOrEmpty( interfaceDeviceAddress ) ) return false;
        this.InterfaceNumber = 0;
        this.InterfaceDeviceAddress = interfaceDeviceAddress;
        string[] info = interfaceDeviceAddress.Split( ',' );
        this.InterfaceName = info[0];
        if ( string.IsNullOrEmpty( this.InterfaceName ) ) return false;
        foreach ( string interfaceFamily in this._interfaceFamilies )
            if ( this.InterfaceName.StartsWith( interfaceFamily, StringComparison.OrdinalIgnoreCase ) )
            {
                this.InterfaceFamily = interfaceFamily;
                if ( this.InterfaceName.Length > this.InterfaceFamily.Length )
                {
                    if ( int.TryParse( this.InterfaceName[(this.InterfaceFamily.Length)..], out int interfaceNumber ) )
                        this.InterfaceNumber = interfaceNumber;
                }
            }
        this.PrimaryAddress = info.Length > 1 ? Convert.ToInt16( info[1] ) : new int?();
        this.SecondaryAddress = info.Length > 2 ? Convert.ToInt16( info[2] ) : new int?();
        return this.IsValid();
    }

    /// <summary>   Parse the USB device address. </summary>
    /// <remarks>   For example:
    /// <list type="bullet">USB::0x5678::0x33::SN999::1<item>
    /// manufacturer ID 0x5678 </item><item>
    /// serial number SN999 </item><item>
    /// serial number SN999 </item><item>
    /// interface number 1 </item> </list> </remarks>
    /// <param name="interfaceDeviceAddress">   The USB interface device address. </param>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    public bool ParseUsbDeviceAddress( string interfaceDeviceAddress )
    {
        this.ManufacturerId = null;
        this.SerialNumber = null;
        this.ModelCode = null;
        this.UsbTmcInterfaceNumber = null;
        this.InterfaceNumber = 0;
        string[] info = interfaceDeviceAddress.TrimEnd( ']' ).Split( '[' );
        this.InterfaceName = info[0];
        this.InterfaceFamily = UsbInterfaceFamily;
        if ( this.InterfaceName.Length > this.InterfaceFamily.Length )
            if ( int.TryParse( this.InterfaceName[(this.InterfaceFamily.Length)..], out int interfaceNumber ) )
                this.InterfaceNumber = interfaceNumber;
        if ( info.Length < 2 ) return true; // address is like 'usb0'
        info = info[1].Split( ':' );
        this.ManufacturerId = Support.ConvertToInt32( info[0] );
        this.ModelCode = Support.ConvertToInt32( info[2] );
        this.SerialNumber = info[4];
        this.UsbTmcInterfaceNumber = info.Length > 6 ? Support.ConvertToInt32( info[6] ) : 0;

        return true;
    }

    /// <summary>   Query if this interface device is a generic instrument device. </summary>
    /// <returns>   True if a generic instrument device, false if not. </returns>
    public bool IsGenericInstrumentDevice()
    {
        return this.InterfaceFamily.Equals( GenericInterfaceFamily, StringComparison.OrdinalIgnoreCase );
    }

    /// <summary>   Query if this interface device is a GPIB instrument device. </summary>
    /// <returns>   True if a GPIB instrument device, false if not. </returns>
    public bool IsGpibInstrumentDevice()
    {
        return this.InterfaceFamily.Equals( GpibInterfaceFamily, StringComparison.OrdinalIgnoreCase );
    }

    public bool IsUsbInstrumentDevice()
    {
        return this.InterfaceFamily.Equals( UsbInterfaceFamily, StringComparison.OrdinalIgnoreCase );
    }

    /// <summary>   Query if interface device is valid. </summary>
    /// <returns>   True if valid, false if not. </returns>
    public bool IsValid()
    {
        return !string.IsNullOrEmpty( this.InterfaceDeviceAddress )
            && !string.IsNullOrEmpty( this.InterfaceName )
            && this.InterfaceNumber.HasValue && MinimumInterfaceNumber <= this.InterfaceNumber.Value
            && (this.IsGenericInstrumentDevice()
                 || this.IsUsbInstrumentDevice()
                 || this.IsGpibInstrumentDevice()
                      && MinimumGpibAddress <= this.PrimaryAddress.GetValueOrDefault( 1 )
                      && MaximumGpibAddress >= this.PrimaryAddress.GetValueOrDefault( 1 )
                      && MinimumGpibAddress <= this.SecondaryAddress.GetValueOrDefault( 1 )
                      && MaximumGpibAddress >= this.SecondaryAddress.GetValueOrDefault( 1 )

              );
    }
}
