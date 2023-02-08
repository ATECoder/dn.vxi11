namespace cc.isr.VXI11.Client;

/// <summary>   Values that represent interface command options. </summary>
public enum InterfaceCommandOption
{
    /// <summary>   An enum constant representing the remote status option. <para>
    ///
    /// CMD_BUS_STATUS_REMOTE
    /// </para></summary>
    RemoteStatus = 1,

    /// <summary>   An enum constant representing the service request status option. <para>
    ///
    /// 
    /// CMD_BUS_STATUS_SRQ 
    /// </para></summary>
    ServiceRequestStatus = 2,

    /// <summary>   An enum constant representing the not data accepted line status option. <para>
    ///
    /// CMD_BUS_STATUS_NDAC
    /// </para></summary>
    NotDataAcceptedLineStatus = 3,

    /// <summary>   An enum constant representing the system controller status option. <para>
    ///
    /// CMD_BUS_STATUS_SYSTEM_CONTROLLER
    /// </para></summary>
    SystemControllerStatus = 4,

    /// <summary>   An enum constant representing the controller in charge status option. <para>
    ///
    /// CMD_BUS_STATUS_CONTROLLER_IN_CHARGE
    /// </para></summary>
    ControllerInChargeStatus = 5,

    /// <summary>   An enum constant representing the talker status option. <para>
    ///
    /// CMD_BUS_STATUS_TALKER
    /// </para></summary>
    TalkerStatus = 6,

    /// <summary>   An enum constant representing the listener status option. <para>
    ///
    /// 
    /// CMD_BUS_STATUS_LISTENER
    /// </para></summary>
    ListenerStatus = 7,

    /// <summary>   An enum constant representing the bus address status option. <para>
    ///
    /// CMD_BUS_STATUS_BUS_ADDRESS
    /// </para></summary>
    BusAddressStatus = 8,

}
