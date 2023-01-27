using System.ComponentModel;

namespace cc.isr.VXI11;

/// <summary>   The underlying transfer protocol as defined in <see cref="CoreChannelClient.CreateIntrChan(cc.isr.VXI11.Codecs.DeviceRemoteFunc)"/>
/// for <see cref="Vxi11Message.CreateInterruptChannelProcedure"/>RPC. </summary>
/// <remarks> <para> 
/// 
/// Renamed from <c>Device_AddrFamily</c>. </para><para>
/// 
/// LXI-11 Specification: </para>
/// <code>
/// enum Device_AddrFamily { /* used by interrupts */
/// DEVICE_TCP,
/// DEVICE_UDP
/// };
/// </code> </remarks>
public enum TransportProtocol
{
    /// <summary>   The device TCP transport protocol. <para>
    /// 
    /// Renamed from <c>DEVICE_TCP = 0</c>. </para></summary>
    [Description( "The device TCP transport protocol." )] Tcp = 0,

    /// <summary>   The device UDP transport protocol. <para>
    /// 
    /// Renamed from <c>DEVICE_UDP = 1</c>. </para>. </summary>
    [Description( "The device UDP transport protocol." )] Udp = 1,
}
