namespace cc.isr.VXI11;

/// <summary>   An VXI-11 device name parser. </summary>
public class DeviceNameParser
{

    /// <summary>   Default constructor. </summary>
    public DeviceNameParser() : this ( string.Empty ) 
    {
    }

    /// <summary>   Constructor. </summary>
    /// <param name="deviceName">  The device name, e.g., <c>gpib0,12,8</c>. </param>
    public DeviceNameParser( string deviceName )
    {
        this.DeviceName = deviceName;
        this.InterfaceFamily = string.Empty;
        this.InterfaceName = string.Empty;
        this.ManufacturerId = null;
        this.SerialNumber = null;
        this.ModelCode = null;
        this.UsbTmcInterfaceNumber = null;
        this._interfaceFamilies = new string[] { GenericInterfaceFamily, GpibInterfaceFamily };
        _ = this.Parse( deviceName );
    }

    /// <summary>   (Immutable) the generic interface family. </summary>
    public const string GenericInterfaceFamily = "inst";

    /// <summary>   (Immutable) the gpib interface family. </summary>
    public const string GpibInterfaceFamily = "gpib";

    /// <summary>   (Immutable) the USB interface family. </summary>
    public const string UsbInterfaceFamily = "usb";

    /// <summary>   (Immutable) the minimum interface number. </summary>
    public const int MinimumInterfaceNumber = 0;

    /// <summary>   (Immutable) the maximum interface number. </summary>
    public const int MaximumInterfaceNumber = 127;

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

    /// <summary>
    /// Gets or sets the device name also called device name, e.g., <c>gpib0,12,13</c>,
    /// where <c>gpib</c> is the <see cref="InterfaceFamily"/>
    /// and <c>0</c> is the <see cref="InterfaceNumber"/> and 12 and 13 are the primary and secondary
    /// addresses.
    /// </summary>
    /// <value> The device name. </value>
    public string DeviceName { get; set; }

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

    /// <summary>   Builds a device name. </summary>
    /// <remarks>   2023-02-11. </remarks>
    /// <returns>   A string. </returns>
    public string BuildDeviceName()
    {
        return this.InterfaceFamily.Equals( GenericInterfaceFamily )
            ? this.InterfaceNumber is null ? this.InterfaceFamily : BuildGenericDeviceName( this.InterfaceNumber ?? 0 )
            : this.InterfaceFamily.Equals( GpibInterfaceFamily )
                ? this.InterfaceNumber is null
                    ? this.InterfaceFamily
                    : this.PrimaryAddress is null
                        ? BuildGpibDeviceName( this.InterfaceNumber ?? 0 )
                        : this.SecondaryAddress is null
                            ? BuildGpibDeviceName( this.InterfaceNumber ?? 0, this.PrimaryAddress ?? 0 )
                            : BuildGpibDeviceName( this.InterfaceNumber ?? 0, this.PrimaryAddress ?? 0, this.SecondaryAddress ?? 0 )
                : this.InterfaceFamily.Equals( UsbInterfaceFamily )
                    ? this.InterfaceNumber is null
                        ? this.InterfaceFamily
                        : this.ManufacturerId is null
                            ? BuildDeviceName( this.InterfaceFamily, this.InterfaceNumber ?? 0 )
                            : BuildUsbDeviceName( this.InterfaceNumber ?? 0, this.ManufacturerId ?? 0, this.ModelCode ?? 0, this.SerialNumber ?? string.Empty )
                    : string.Empty;

    }

    /// <summary>   Builds a device name. </summary>
    /// <exception cref="ArgumentNullException">        Thrown when one or more required arguments
    ///                                                 are null. </exception>
    /// <exception cref="ArgumentOutOfRangeException">  Thrown when one or more arguments are outside
    ///                                                 the required range. </exception>
    /// <param name="interfaceFamily">  The interface family. </param>
    /// <param name="interfaceNumber">  The interface number. </param>
    /// <returns>   A string. </returns>
    public static string BuildDeviceName( string interfaceFamily, int interfaceNumber )
    {
        return string.IsNullOrEmpty( interfaceFamily )
            ? throw new ArgumentNullException( nameof( interfaceFamily ) )
            : MinimumInterfaceNumber > interfaceNumber || MaximumInterfaceNumber < interfaceNumber
                ? throw new ArgumentOutOfRangeException(nameof( interfaceNumber ),
                    $"{interfaceNumber} must be within [{MinimumInterfaceNumber},{MaximumInterfaceNumber}]." )
                : string.Equals( interfaceFamily, GenericInterfaceFamily, StringComparison.OrdinalIgnoreCase )
                  || string.Equals( interfaceFamily, GpibInterfaceFamily, StringComparison.OrdinalIgnoreCase )
                    ? $"{interfaceFamily}{interfaceNumber}"
                    : throw new ArgumentOutOfRangeException( nameof( interfaceFamily ),
                        $"{interfaceFamily} must be either {GenericInterfaceFamily} or {GpibInterfaceFamily}");
    }

    /// <summary>   Builds generic device name. </summary>
    /// <exception cref="ArgumentOutOfRangeException">  Thrown when one or more arguments are outside
    ///                                                 the required range. </exception>
    /// <param name="interfaceNumber">  The interface number. </param>
    /// <returns>   A string. </returns>
    public static string BuildGenericDeviceName( int interfaceNumber )
    {
        return MinimumInterfaceNumber > interfaceNumber || MaximumInterfaceNumber < interfaceNumber
                ? throw new ArgumentOutOfRangeException( nameof( interfaceNumber ),
                        $"{interfaceNumber} must be within [{MinimumInterfaceNumber},{MaximumInterfaceNumber}]." )
                : $"{GenericInterfaceFamily}{interfaceNumber}";
    }

    /// <summary>   Builds GPIB device name. </summary>
    /// <exception cref="ArgumentOutOfRangeException">  Thrown when one or more arguments are outside
    ///                                                 the required range. </exception>
    /// <param name="interfaceNumber">  The interface number. </param>
    /// <returns>   A string. </returns>
    public static string BuildGpibDeviceName( int interfaceNumber )
    {
        return MinimumInterfaceNumber > interfaceNumber || MaximumInterfaceNumber < interfaceNumber
                ? throw new ArgumentOutOfRangeException( nameof( interfaceNumber ),
                        $"{interfaceNumber} must be within [{MinimumInterfaceNumber},{MaximumInterfaceNumber}]." )
                : $"{GpibInterfaceFamily}{interfaceNumber}";
    }

    /// <summary>   Builds a GPIB device name. </summary>
    /// <exception cref="ArgumentOutOfRangeException">  Thrown when one or more arguments are outside
    ///                                                 the required range. </exception>
    /// <param name="interfaceNumber">  The interface number. </param>
    /// <param name="primaryAddress">   The primary address. </param>
    /// <returns>   A string. </returns>
    public static string BuildGpibDeviceName( int interfaceNumber, int primaryAddress )
    {
        return MinimumInterfaceNumber > interfaceNumber || MaximumInterfaceNumber < interfaceNumber
                ? throw new ArgumentOutOfRangeException( nameof( interfaceNumber ),
                        $"{interfaceNumber} must be within [{MinimumInterfaceNumber},{MaximumInterfaceNumber}]." )
                : MinimumGpibAddress > primaryAddress || MaximumGpibAddress < primaryAddress
                    ? throw new ArgumentOutOfRangeException( nameof( primaryAddress ),
                        $"{primaryAddress} must be within [{MinimumGpibAddress},{MaximumGpibAddress}]." )
                    : $"{GpibInterfaceFamily}{interfaceNumber},{primaryAddress}";
    }

    /// <summary>   Builds a GPIB device name. </summary>
    /// <exception cref="ArgumentOutOfRangeException">  Thrown when one or more arguments are outside
    ///                                                 the required range. </exception>
    /// <param name="interfaceNumber">  The interface number. </param>
    /// <param name="primaryAddress">   The primary address. </param>
    /// <param name="secondaryAddress"> The secondary address. </param>
    /// <returns>   A string. </returns>
    public static string BuildGpibDeviceName( int interfaceNumber, int primaryAddress, int secondaryAddress )
    {
        return MinimumInterfaceNumber > interfaceNumber || MaximumInterfaceNumber < interfaceNumber
                ? throw new ArgumentOutOfRangeException( nameof( interfaceNumber ),
                        $"{interfaceNumber} must be within [{MinimumInterfaceNumber},{MaximumInterfaceNumber}]." )
                : MinimumGpibAddress > primaryAddress || MaximumGpibAddress < primaryAddress
                    ? throw new ArgumentOutOfRangeException( nameof( primaryAddress ),
                        $"{primaryAddress} must be within [{MinimumGpibAddress},{MaximumGpibAddress}]." )
                    : MinimumGpibAddress > secondaryAddress || MaximumGpibAddress < secondaryAddress
                        ? throw new ArgumentOutOfRangeException( nameof( secondaryAddress ),
                            $"{secondaryAddress} must be within [{MinimumGpibAddress},{MaximumGpibAddress}]." )
                        : $"{GpibInterfaceFamily}{interfaceNumber},{primaryAddress},{secondaryAddress}";
    }

    /// <summary>   Clears this object to its blank/initial state. </summary>
    private void Clear()
    {
        this.DeviceName = string.Empty;
        this.InterfaceFamily = string.Empty;
        this.InterfaceName = string.Empty;
        this.ManufacturerId = null;
        this.SerialNumber = null;
        this.ModelCode = null;
        this.UsbTmcInterfaceNumber = null;
    }

    /// <summary>   Parses the device name. </summary>
    /// <param name="deviceName">   The device name, e.g., <c>gpib0,12,8</c>. </param>
    /// <returns>   <see langword="true"/> if it succeeds; otherwise, <see langword="false"/>. </returns>
    public bool Parse( string deviceName )
    {
        this.Clear();
        return string.IsNullOrWhiteSpace( deviceName )
            ? false
            : deviceName.StartsWith( UsbInterfaceFamily )
                ? this.ParseUsbDeviceName( deviceName )
                : this.ParseDeviceName( deviceName );
    }

    /// <summary>   Parses a genetic (inst) or GPIB device name into its components. </summary>
    /// <param name="deviceName">   The device name, e.g., <c>gpib0,12,8</c>. </param>
    /// <returns>   <see langword="true"/> if it succeeds; otherwise, <see langword="false"/>. </returns>
    private bool ParseDeviceName( string deviceName )
    {
        this.Clear();
        if ( string.IsNullOrEmpty( deviceName ) ) return false;
        this.InterfaceNumber = 0;
        this.DeviceName = deviceName;
        string[] info = deviceName.Split( ',' );
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

    /// <summary>   Builds USB device name. </summary>
    /// <exception cref="ArgumentOutOfRangeException">  Thrown when one or more arguments are outside
    ///                                                 the required range. </exception>
    /// <param name="interfaceNumber">  The interface number. </param>
    /// <param name="manufacturerId">   The identifier of the manufacturer. </param>
    /// <param name="modelCode">        The model code. </param>
    /// <param name="serialNumber">     The serial number. </param>
    /// <returns>   A string. </returns>
    public static string BuildUsbDeviceName( int interfaceNumber, int manufacturerId, int modelCode, string serialNumber )
    {
        return MinimumInterfaceNumber > interfaceNumber || MaximumInterfaceNumber < interfaceNumber
                ? throw new ArgumentOutOfRangeException( nameof( interfaceNumber ),
                        $"{interfaceNumber} must be within [{MinimumInterfaceNumber},{MaximumInterfaceNumber}]." )
                : $"{UsbInterfaceFamily}::0x{manufacturerId:X}::0x{modelCode}::{serialNumber}::{interfaceNumber}";
    }

    /// <summary>   Parse the USB device name. </summary>
    /// <remarks>
    /// For example:
    /// <list type="bullet">USB::0x5678::0x33::SN999::1<item>
    /// manufacturer ID 0x5678 </item><item>
    /// mode code 0x33 </item><item>
    /// serial number SN999 </item><item>
    /// interface number 1 </item> </list>
    /// </remarks>
    /// <param name="usbDeviceName">    The USB device name. </param>
    /// <returns>   <see langword="true"/> if it succeeds; otherwise, <see langword="false"/>. </returns>
    private bool ParseUsbDeviceName( string usbDeviceName )
    {
        this.Clear();
        this.DeviceName = usbDeviceName;
        this.ManufacturerId = null;
        this.SerialNumber = null;
        this.ModelCode = null;
        this.UsbTmcInterfaceNumber = null;
        this.InterfaceNumber = 0;
        string[] info = usbDeviceName.TrimEnd( ']' ).Split( '[' );
        this.InterfaceName = info[0];
        this.InterfaceFamily = UsbInterfaceFamily;
        if ( this.InterfaceName.Length > this.InterfaceFamily.Length )
            if ( int.TryParse( this.InterfaceName[(this.InterfaceFamily.Length)..], out int interfaceNumber ) )
                this.InterfaceNumber = interfaceNumber;
        if ( info.Length < 2 ) return true; // address is like 'usb0'
        info = info[1].Split( ':' );
        this.ManufacturerId = Vxi11Support.ToInt( info[0] );
        this.ModelCode = Vxi11Support.ToInt( info[2] );
        this.SerialNumber = info[4];
        this.UsbTmcInterfaceNumber = info.Length > 6 ? Vxi11Support.ToInt( info[6] ) : 0;

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
        return !string.IsNullOrEmpty( this.DeviceName )
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
