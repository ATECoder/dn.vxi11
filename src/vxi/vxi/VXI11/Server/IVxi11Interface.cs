using System.ComponentModel;

using cc.isr.VXI11;
using cc.isr.VXI11.Client;
using cc.isr.VXI11.Codecs;

namespace cc.isr.VXI11.Server;

/// <summary>
/// Interface for a base LXI interface, which implements standard IEEE 488.1 interface
/// commands.
/// </summary>
/// <remarks>
/// This interface defines the implementation for a 'physical' interface that is the end point
/// for the <see cref="Client.Vxi11InterfaceClient"/> Instrument. The remote procedure
/// call initiated at the <see cref="Client.Vxi11Client"/> side, passes to the instrument through
/// a <see cref="Vxi11Device"/>, which links the <see cref="Vxi11Server"/> and the 'physical'
/// <see cref="Vxi11Interface"/>.
/// 
/// Implementations of VXI-11 servers should inherit from the <see cref="Vxi11Interface"/> and,
/// perhaps also, from the <see cref="Vxi11Device"/>.
/// 
/// Instrument classes inheriting from the <see cref="Vxi11Interface"/> might override a few
/// methods as necessary for implementing the designed behavior.
/// 
/// The <see cref="Vxi11Server"/> and <see cref="Vxi11Device"/> classes implement the device_xxx
/// remote procedure calls as specified in the
/// <see href="https://vxibus.org/specifications.html">VXI-11 TCP/IP Instrument Protocol
/// Specification</see> VXI-11 Version 1.0 document.
/// 
/// The VXI-11 device procedures are from the host perspective, i.e., a device write writes to
/// the 'physical' instrument (also called 'Network Instrument') and device read reads from the
/// instrument.
/// </remarks>
public interface IVxi11Interface : INotifyPropertyChanged
{

    #region " Interface methods "

    /// <summary>   Sends a command; <see cref="InterfaceCommand.SendCommand"/> </summary>
    /// <param name="data"> The data. </param>
    /// <returns>   A byte[]. </returns>
    byte[] SendCommand( byte[] data );

    /// <summary>   Reads bus status; <see cref="InterfaceCommand.BusStatus"/>. </summary>
    /// <param name="interfaceCommand"> The interface command option <see cref="InterfaceCommandOption"/>. </param>
    /// <returns>   An int. </returns>
    int ReadBusStatus( InterfaceCommandOption interfaceCommand );

    /// <summary>   Read REN line; <see cref="InterfaceCommandOption.RemoteStatus"/>. </summary>
    /// <returns>   1 if the REN message is true, 0 otherwise. </returns>
    int ReadRenLine();

    /// <summary>   Reads service request (SRQ) line; <see cref="InterfaceCommandOption.ServiceRequestStatus"/>. </summary>
    /// <remarks>   2023-01-24. </remarks>
    /// <returns>   1 if the SRQ message is true, 0 otherwise. </returns>
    int ReadServiceRequest();

    /// <summary>   Reads <see cref="InterfaceCommandOption.NotDataAcceptedLineStatus"/> NDAC line. </summary>
    /// <remarks>   2023-01-24. </remarks>
    /// <returns>   1 if the NDAC message is true, 0 otherwise. </returns>
    int ReadNdacLine();

    /// <summary>   Check if interface device is a system controller; <see cref="InterfaceCommandOption.SystemControllerStatus"/>. </summary>
    /// <remarks>   2023-01-24. </remarks>
    /// <returns>   <see langword="true"/> if the TCP/IP-IEEE 488.1 Interface Device is in the
    /// system control active state, <see langword="false"/> otherwise. </returns>
    bool IsSystemController();

    /// <summary>   Check if interface device is the controller-in-charge; <see cref="InterfaceCommandOption.ControllerInChargeStatus"/>. </summary>
    /// <remarks>   2023-01-24. </remarks>
    /// <returns>
    /// <see langword="true"/> if the TCP/IP-IEEE 488.1 Interface Device is not in the controller
    /// idle state, <see langword="false"/> otherwise.
    /// </returns>
    bool IsControllerInCharge();

    /// <summary>   Check if interface device is addressed as a talker; <see cref="InterfaceCommandOption.TalkerStatus"/>. </summary>
    /// <remarks>   2023-01-24. </remarks>
    /// <returns>
    /// <see langword="true"/> if the TCP/IP-IEEE 488.1 Interface Device is addressed to talk, <see langword="false"/>
    /// otherwise.
    /// </returns>
    bool IsTalker();

    /// <summary>   Check if interface device is addressed as a listener; <see cref="InterfaceCommandOption.ListenerStatus"/>. </summary>
    /// <remarks>   2023-01-24. </remarks>
    /// <returns>
    /// <see langword="true"/> if the TCP/IP-IEEE 488.1 Interface Device is addressed to listen, <see langword="false"/>
    /// </returns>
    bool IsListener();

    /// <summary>   Get interface device bus address; <see cref="InterfaceCommandOption.BusAddressStatus"/> </summary>
    /// <remarks>   2023-01-25. </remarks>
    /// <returns>   The TCP/IP-IEEE 488.1 Interface Device's address (0-30). </returns>
    int GetBusAddress();

    #endregion

    #region " instrument state "

    /// <summary>   Gets or sets a value indicating whether lock is requested on the device. </summary>
    /// <value> True if lock enabled, false if not. </value>
    bool LockEnabled { get; set; }

    /// <summary>   Gets or sets a value indicating whether the remote is enabled. </summary>
    /// <value> True if remote enabled, false if not. </value>
    bool RemoteEnabled { get; set; }

    #endregion

    #region " RPC operation members "

    /// <summary>   The device executes a command. </summary>
    /// <remarks>   2023-01-26. </remarks>
    /// <param name="request">  The request of type of type <see cref="DeviceDoCmdParms"/> to
    ///                         use with the remote procedure call. </param>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="DeviceDoCmdResp"/>.
    /// </returns>
    DeviceDoCmdResp DeviceDoCmd( DeviceDoCmdParms request );

    /// <summary>   Gets or sets the identifier of the active client. </summary>
    /// <value> The identifier of the active client. </value>
    int ActiveClientId { get; set; }

    /// <summary>   Gets or sets the last device error. </summary>
    /// <value> The las <see cref="DeviceErrorCode"/> . </value>
    DeviceErrorCode LastDeviceError { get; set; }

    /// <summary>   Gets a <see cref="CircularList{T}"/> of (<see cref="DateTime"/> Timestamp, <see cref="String"/> Value)
    /// of the last messages that were sent to and received from the instrument. </summary>
    /// <value> The list of message tuples consisting of the client id, IO (R for read and W for write), 
    /// a timestamp and a value that were sent to or received from the instrument. </value>
    List<(int ClientId, char IO, DateTimeOffset Timestamp, String Value)> MessageLog { get; }

    /// <summary>   Gets or sets the number of I/O messages. </summary>
    /// <value> The number of I/O messages, which, in fact, flags the property change flag that can be used to 
    /// indicate the availability of new messages. </value>
    int MessageLogCount { get; set; }

    #endregion
}
