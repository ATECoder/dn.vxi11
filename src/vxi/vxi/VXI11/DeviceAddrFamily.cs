namespace cc.isr.VXI11;

/// <summary>   A device address family; used by interrupts. </summary>
/// <remarks>   Renamed from <c>Device_AddrFamily</c>. <para>
/// LXI-11 Specification: </para>
/// <code>
/// enum Device_AddrFamily { /* used by interrupts */
/// DEVICE_TCP,
/// DEVICE_UDP
/// };
/// </code> </remarks>
public class DeviceAddrFamily
{
    /// <summary>   The device TCP Address family. <para>
    /// Renamed from <c>DEVICE_TCP = 0</c>. </para></summary>
    public static int DeviceTcpAddressFamily = 0;

    /// <summary>   The device UDP address family <para>
    /// Renamed from <c>DEVICE_UDP = 1</c>. </para>. </summary>
    public static int DeviceUdpAddressFamily = 1;
}
