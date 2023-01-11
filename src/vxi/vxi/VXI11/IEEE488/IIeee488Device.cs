
namespace cc.isr.VXI11.IEEE488;

/// <summary>   Interface for a base IEEE488 device. </summary>
public interface IIeee488Device
{

    /// <summary>   Gets or sets the identity. </summary>
    /// <value> The identity. </value>
    string Identity { get; set; }

    /// <summary>   Clears status: *CLS. </summary>
    /// <remarks>
    /// Clear Status Command. Clears the event registers in all register groups. Also clears the
    /// error queue.
    /// </remarks>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    bool CLS();

    /// <summary>   Enables Standard Event Status: *ESE. </summary>
    /// <remarks>
    /// Enables bits in the enable register for the Standard Event Register group. The selected bits
    /// are then reported to bit 5 of the Status Byte Register. Accepts the decimal sum of the bits
    /// in the register; default 0. For example, to enable bit 2 (value 4), bit 3 (value 8), and bit
    /// 7 (value 128), the decimal sum would be 140 (4 + 8 + 128). For example, *ESE 48 enables bit 4
    /// (value 16) and bit 5 (value 32) in the enable register.
    /// </remarks>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    bool ESE();

    /// <summary>   Reads Standard Event Status: *ESE? </summary>
    /// <returns>   A string. </returns>
    string ESERead();

    /// <summary>   Standard Event Status Register Query: *ESR. </summary>
    /// <remarks>
    /// Queries the event register for the Standard Event Register group. Register is read-only; bits
    /// not cleared when read. Any or all conditions can be reported to the Standard Event summary
    /// bit through the enable register.To set the enable register mask, write a decimal value to the
    /// register using *ESE. Once a bit is set, it remains set until cleared by this query or *CLS.
    /// </remarks>
    /// <returns>   A string. </returns>
    string ESRRead();

    /// <summary>   Reads the device identity string: *IDN? </summary>
    /// <returns>   A string. </returns>
    string IDNRead();

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
    bool OPC();

    /// <summary>   Reads the operation completion status: *OPC? </summary>
    /// <remarks>
    /// Returns 1 to the output buffer after all pending commands complete. :
    /// The purpose of this command is to synchronize your application with the instrument. Other
    /// commands cannot be executed until this command completes. The difference between *OPC and
    /// *OPC? is that *OPC? returns "1" to the output buffer when the current operation
    /// completes. This means that no further commands can be sent after an *OPC? until it has
    /// responded. In this way an explicit polling loop can be avoided.That is, the IO driver will
    /// wait for the response.
    /// </remarks>
    /// <returns>   Returns 1 when all previous commands complete. </returns>
    string OPCRead();

    /// <summary>   Resets the device: *RST. </summary>
    /// <remarks>
    /// Resets instrument to factory default state, independent of MEMory:STATe:RECall:AUTO setting.
    /// Does not affect stored instrument states, stored arbitrary waveforms, or I/O settings; these
    /// are stored in non-volatile memory. Aborts a sweep or burst in progress.
    /// </remarks>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    bool RST();

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
    /// cycles.
    /// Status Byte enable register is not cleared by *RST.
    /// </remarks>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    bool SRE();

    /// <summary>   Reads the service request enabled status: *SER? </summary>
    /// <returns>   A string. </returns>
    string SRERead();

    /// <summary>   Reads the status byte: *STB? </summary>
    /// <remarks>
    /// This command queries the condition register for the Status Byte Register group. For example
    /// *STB?: +40 indicates that condition register bits 3 and 5 are set. Similar to a Serial Poll,
    /// but processed like any other instrument command .Register is read-only; bits not cleared when
    /// read. Returns same result as a Serial Poll, but "Master Summary" bit (bit 6) is not cleared
    /// by *STB?. Power cycle or *RST clears all bits in condition register.
    /// </remarks>
    /// <returns>
    /// Returns a decimal value that corresponds to the binary-weighted sum of all bits set in the
    /// register. For example, with bit 3 (value 8) and bit 5 (value 32) set( and corresponding bits
    /// enabled ), the query returns +40.
    /// </returns>
    string STBRead();

    /// <summary>   Trigger command: *TRG. </summary>
    /// <remarks>
    /// Triggers a sweep, burst, arbitrary waveform advance, or LIST advance from
    /// the remote interface if the bus (software) trigger source is currently selected
    /// (TRIGger[1|2]:SOURce BUS).
    /// </remarks>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    bool TRG();

    /// <summary>   Runs a self test and reads its status: *TST?. </summary>
    /// <remarks>
    /// Self-Test Query. Performs a complete instrument self-test. If test fails, one or more error
    /// messages will provide additional information. Use SYSTem:ERRor? to read error queue.
    /// </remarks>
    /// <returns>   Returns +0 (pass) or +1 (one or more tests failed). </returns>
    string TSTRead();

    /// <summary>   Wait until all pending operations complete. *WAI. </summary>
    /// <remarks>
    /// Configures the instrument to wait for all pending operations to complete before executing any
    /// additional commands over the interface. For example, you can use this with the *TRG command
    /// to ensure that the instrument is ready for a trigger:
    /// *TRG;*WAI;*TRG.
    /// </remarks>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    bool WAI();

}
