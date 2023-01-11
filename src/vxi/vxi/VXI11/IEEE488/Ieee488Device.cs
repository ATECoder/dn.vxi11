namespace cc.isr.VXI11.IEEE488;



/// <summary>   An IEEE 488 device. </summary>
public partial class Ieee488Device : IIeee488Device
{

    #region " construction and cleanup "

    /// <summary>   Constructor. </summary>
    /// <param name="identity"> (Optional) Device identification string. </param>
    public Ieee488Device( string identity = "PhaseLock Technology,EXXX0A" )
    {
        this.Identity = identity;
    }

    #endregion

    #region " Ieee488Device Interface implementation "

    /// <summary>
    /// Device identification string
    /// </summary>
    public string Identity { get; set; }

    /// <summary>   Clears status: *CLS. </summary>
    /// <remarks>
    /// Clear Status Command. Clears the event registers in all register groups. Also clears the
    /// error queue.
    /// </remarks>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    [Ieee488( "*CLS", Ieee488OperationType.Write )]
    public bool CLS()
    {
        return true;
    }

    /// <summary>   Enables Standard Event Status: *ESE. </summary>
    /// <remarks>
    /// Enables bits in the enable register for the Standard Event Register group. The selected bits
    /// are then reported to bit 5 of the Status Byte Register. Accepts the decimal sum of the bits
    /// in the register; default 0. For example, to enable bit 2 (value 4), bit 3 (value 8), and bit
    /// 7 (value 128), the decimal sum would be 140 (4 + 8 + 128). For example, *ESE 48 enables bit 4
    /// (value 16) and bit 5 (value 32) in the enable register.
    /// </remarks>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    [Ieee488( "*ESE", Ieee488OperationType.Write )]
    public bool ESE()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Reads Standard Event Status: *ESE?
    /// </summary>
    [Ieee488( "*ESE?", Ieee488OperationType.Read )]
    public string ESERead()
    {
        throw new NotImplementedException();
    }

    /// <summary>   Standard Event Status Register Query: *ESR?. </summary>
    /// <remarks>
    /// Queries the event register for the Standard Event Register group. Register is read-only; bits
    /// not cleared when read. Any or all conditions can be reported to the Standard Event summary
    /// bit through the enable register.To set the enable register mask, write a decimal value to the
    /// register using * ESE. Once a bit is set, it remains set until cleared by this query or *CLS.
    /// </remarks>
    /// <returns>   A string. </returns>
    [Ieee488( "*ESR?", Ieee488OperationType.Read )]
    public string ESRRead()
    {
        throw new NotImplementedException();
    }

    /// <summary>   Reads the device identity string: *IDN? </summary>
    /// <returns>   A string. </returns>
    [Ieee488( "*IDN?", Ieee488OperationType.Read )]
    public string IDNRead()
    {
        return this.Identity;
    }

    /// <summary>   Operation completion instruction: *OPC. </summary>
    /// <remarks>
    /// Sets "Operation Complete" (bit 0) in the Standard Event register at the completion of the
    /// current operation. The purpose of this command is to synchronize your application with the
    /// instrument. Used in triggered sweep, triggered burst, list, or arbitrary waveform sequence
    /// modes to provide a way to poll or interrupt the computer when the *TRG or
    /// INITiate[:IMMediate] is complete. Other commands may be executed before Operation Complete
    /// bit is set. The difference between *OPC and *OPC? is that *OPC? returns "1" to the output
    /// buffer when the current operation completes. This means that no further commands can be sent
    /// after an *OPC? until it has responded. In this way an explicit polling loop can be avoided.
    /// </remarks>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    [Ieee488( "*OPC", Ieee488OperationType.Write )]
    public bool OPC()
    {
        throw new NotImplementedException();
    }

    /// <summary>   Reads the operation completion status: *OPC? </summary>
    /// <remarks>
    /// Returns 1 to the output buffer after all pending commands complete. : The purpose of this
    /// command is to synchronize your application with the instrument. Other commands cannot be
    /// executed until this command completes. The difference between *OPC and
    /// *OPC? is that *OPC? returns "1" to the output buffer when the current operation
    /// completes. This means that no further commands can be sent after an *OPC? until it has
    /// responded. In this way an explicit polling loop can be avoided.That is, the IO driver will
    /// wait for the response.
    /// </remarks>
    /// <returns>   Returns 1 when all previous commands complete. </returns>
    [Ieee488( "*OPC?", Ieee488OperationType.Read )]
    public string OPCRead()
    {
        return "1";
    }

    /// <summary>   Resets the device: *RST. </summary>
    /// <remarks>
    /// Resets instrument to factory default state, independent of MEMory:STATe:RECall:AUTO setting.
    /// Does not affect stored instrument states, stored arbitrary waveforms, or I/O settings; these
    /// are stored in non-volatile memory. Aborts a sweep or burst in progress.
    /// </remarks>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    [Ieee488( "*RST", Ieee488OperationType.Write )]
    public bool RST()
    {
        throw new NotImplementedException();
    }

    /// <summary>   Enables the service request events: *SER. </summary>
    /// <remarks>
    /// This command enables bits in the enable register for the Status Byte Register group.
    /// Parameters consists of the decimal sum of the bits in the register; default 0. For example,
    /// to enable bit 2 (value 4), bit 3 (value 8), and bit 7 (value 128), the decimal sum would be
    /// 140 (4 + 8 + 128). for example, *SRE 24 enables bits 3 and 4 in the enable register. To
    /// enable specific bits, specify the decimal value corresponding to the binary-weighted sum of
    /// the bits in the register.The selected bits are summarized in the "Master Summary" bit (bit 6)
    /// of the Status Byte Register.If any of the selected bits change from 0 to 1, the instrument
    /// generates a Service Request signal.
    /// *CLS clears the event register, but not the enable register.
    /// *PSC (power-on status clear) determines whether Status Byte enable register is cleared at
    /// power on.For example, *PSC 0 preserves the contents of the enable register through power
    /// cycles. Status Byte enable register is not cleared by *RST.
    /// </remarks>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    [Ieee488( "*SRE", Ieee488OperationType.Write )]
    public bool SRE()
    {
        throw new NotImplementedException();
    }

    /// <summary>   Reads the service request enabled status: *SER? </summary>
    /// <returns>   A string. </returns>
    [Ieee488( "*SRE?", Ieee488OperationType.Read )]
    public string SRERead()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Read the status byte: *STB?
    /// </summary>
    [Ieee488( "*STB?", Ieee488OperationType.Read )]
    public string STBRead()
    {
        throw new NotImplementedException();
    }

    /// <summary>   Trigger command: *TRG. </summary>
    /// <remarks>
    /// Triggers a sweep, burst, arbitrary waveform advance, or LIST advance from the remote
    /// interface if the bus (software) trigger source is currently selected (TRIGger[1|2]:SOURce
    /// BUS).
    /// </remarks>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    [Ieee488( "*TRG", Ieee488OperationType.Write )]
    public bool TRG()
    {
        throw new NotImplementedException();
    }

    /// <summary>   Runs a self test and reads its status: *TST?. </summary>
    /// <remarks>
    /// Self-Test Query. Performs a complete instrument self-test. If test fails, one or more error
    /// messages will provide additional information. Use SYSTem:ERRor? to read error queue.
    /// </remarks>
    /// <returns>   Returns +0 (pass) or +1 (one or more tests failed). </returns>
    [Ieee488( "*TST?", Ieee488OperationType.Read )]
    public string TSTRead()
    {
        throw new NotImplementedException();
    }

    /// <summary>   Wait until all pending operations complete. *WAI. </summary>
    /// <remarks>
    /// Configures the instrument to wait for all pending operations to complete before executing any
    /// additional commands over the interface. For example, you can use this with the *TRG command
    /// to ensure that the instrument is ready for a trigger:
    /// *TRG;*WAI;*TRG.
    /// </remarks>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    [Ieee488( "*WAI", Ieee488OperationType.Write )]
    public bool WAI()
    {
        throw new NotImplementedException();
    }

    #endregion

}
