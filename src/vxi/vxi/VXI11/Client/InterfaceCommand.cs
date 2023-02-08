namespace cc.isr.VXI11.Client;

/// <summary>   Values that represent IEEE 488 interface commands. </summary>
public enum InterfaceCommand
{
    /// <summary>   An enum constant representing the send command option. <para>
    /// 
    /// CMD_SEND_COMMAND
    /// </para></summary>
    SendCommand = 0x020000,

    /// <summary>   An enum constant representing the bus status option. <para>
    /// 
    /// CMD_BUS_STATUS
    /// </para></summary>
    BusStatus = 0x020001,

    /// <summary>   An enum constant representing the attention control option. <para>
    ///
    /// CMD_ATN_CTRL
    /// </para></summary>
    AttentionControl = 0x020002,


    /// <summary>   An enum constant representing the remote enable control option. <para>
    ///
    /// CMD_REN_CTRL
    /// </para></summary>
    RemoteEnableControl = 0x020003,


    /// <summary>   An enum constant representing the pass control option. <para>
    ///
    /// CMD_PASS_CTRL
    /// </para></summary>
    PassControl = 0x020004,

    /// <summary>   An enum constant representing the bus address option. <para>
    ///
    /// CMD_BUS_ADDRESS
    /// </para></summary>
    BusAddress = 0x02000A,


    /// <summary>   An enum constant representing the interface clear control option. 
    ///
    /// CMD_IFC_CTRL
    /// </summary>
    InterfaceClearControl = 0x020010,
}

