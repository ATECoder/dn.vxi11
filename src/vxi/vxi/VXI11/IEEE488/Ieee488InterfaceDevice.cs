
namespace cc.isr.VXI11.IEEE488;

public struct Ieee488InterfaceDevice
{

    /// <summary>   Constructor. </summary>
    /// <remarks>   2022-12-15. </remarks>
    /// <param name="interfaceDeviceString">  The interface devices string, e.g., <c>gpib0,12,8</c>. </param>
    public Ieee488InterfaceDevice( string interfaceDeviceString )
    {
        this.InterfaceDeviceString = interfaceDeviceString;
        this.InterfaceFamily = string.Empty;
        this.InterfaceName = string.Empty;
        this._interfaceFamilies = new string[] { Ieee488InterfaceDevice.GenericInstrumentFamily, Ieee488InterfaceDevice.GpibInstrumentFamily };
        _ = this.ParseDeviceInfo( interfaceDeviceString );
    }

    /// <summary>   (Immutable) the generic instrument family. </summary>
    public const string GenericInstrumentFamily = "inst";

    /// <summary>   (Immutable) the gpib instrument family. </summary>
    public const string GpibInstrumentFamily = "gpib";

    public const int MinimumInterfaceNumber = 1;

    /// <summary>   (Immutable) the minimum gpib address. </summary>
    public const int MinimumGpibAddress = 1;

    /// <summary>   (Immutable) the maximum gpib address. </summary>
    public const int MaximumGpibAddress = 31;

    private readonly string[] _interfaceFamilies;

    /// <summary>   Interface families. </summary>
    /// <remarks>   2022-12-15. </remarks>
    /// <returns>   A string[]. </returns>
    public string[] InterfaceFamilies()
    {
        return this._interfaceFamilies;
    }

    /// <summary>   Gets or sets information describing the device, e.g., <c>gpib0,12,13</c>, where <c>gpib</c> is the <see cref="InterfaceFamily"/>
    /// and <c>0</c> is the <see cref="InterfaceNumber"/>. </summary>
    /// <value> Information describing the device. </value>
    public string InterfaceDeviceString { get; set; }

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

    /// <summary>   Gets or sets the primary address of the <see cref="GpibInstrumentFamily"/> gpib interface. </summary>
    /// <value> The primary address. </value>
    public int? PrimaryAddress { get; set; }

    /// <summary>   Gets or sets the secondary address of the <see cref="GpibInstrumentFamily"/> gpib interface. </summary>
    /// <value> The secondary address. </value>
    public int? SecondaryAddress { get; set; }

    /// <summary>   Parse device information. </summary>
    /// <remarks>   2022-12-15. </remarks>
    /// <param name="interfaceDeviceString">    Information describing the device. </param>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    public bool ParseDeviceInfo( string interfaceDeviceString )
    {
        if ( string.IsNullOrEmpty( interfaceDeviceString ) ) return false;
        this.InterfaceDeviceString = interfaceDeviceString;
        string[] info = interfaceDeviceString.Split( ',' );
        this.InterfaceName = info[0];
        if ( string.IsNullOrEmpty( this.InterfaceName ) ) return false;
        foreach ( string interfaceFamily in this._interfaceFamilies )
        {
            if ( this.InterfaceName.StartsWith( interfaceFamily, StringComparison.OrdinalIgnoreCase ) )
            {
                this.InterfaceFamily = interfaceFamily;
                if ( this.InterfaceName.Length > this.InterfaceFamily.Length )
                {
                    if ( int.TryParse( this.InterfaceName.Substring( this.InterfaceFamily.Length - 1 ), out int interfaceNumber ) )
                        this.InterfaceNumber = interfaceNumber;
                }
            }
        }
        this.PrimaryAddress = info.Length > 1 ? Convert.ToInt16( info[1] ) : new int?();
        this.SecondaryAddress = info.Length > 2 ? Convert.ToInt16( info[2] ) : new int?();
        return this.IsValid();
    }

    /// <summary>   Query if this interface device is a generic instrument device. </summary>
    /// <remarks>   2022-12-15. </remarks>
    /// <returns>   True if a generic instrument device, false if not. </returns>
    public bool IsGenericInstrumentDevice()
    {
        return this.InterfaceFamily.Equals( Ieee488InterfaceDevice.GenericInstrumentFamily, StringComparison.OrdinalIgnoreCase );
    }

    /// <summary>   Query if this interface device is a GPIB instrument device. </summary>
    /// <remarks>   2022-12-15. </remarks>
    /// <returns>   True if a GPIB instrument device, false if not. </returns>
    public bool IsGpibInstrumentDevice()
    {
        return this.InterfaceFamily.Equals( Ieee488InterfaceDevice.GpibInstrumentFamily, StringComparison.OrdinalIgnoreCase );
    }

    /// <summary>   Query if interface device is valid. </summary>
    /// <remarks>   2022-12-15. </remarks>
    /// <returns>   True if valid, false if not. </returns>
    public bool IsValid()
    {
        return !string.IsNullOrEmpty( this.InterfaceDeviceString )
            && !string.IsNullOrEmpty( this.InterfaceName )
            && this.InterfaceNumber.HasValue && Ieee488InterfaceDevice.MinimumInterfaceNumber <= this.InterfaceNumber.Value
            && (this.IsGenericInstrumentDevice()
                 || (this.IsGpibInstrumentDevice()
                      && (Ieee488InterfaceDevice.MinimumGpibAddress <= this.PrimaryAddress.GetValueOrDefault( 1 ))
                      && (Ieee488InterfaceDevice.MaximumGpibAddress >= this.PrimaryAddress.GetValueOrDefault( 1 ))
                      && (Ieee488InterfaceDevice.MinimumGpibAddress <= this.SecondaryAddress.GetValueOrDefault( 1 ))
                      && (Ieee488InterfaceDevice.MaximumGpibAddress >= this.SecondaryAddress.GetValueOrDefault( 1 ))
                    )
              );
    }

}
