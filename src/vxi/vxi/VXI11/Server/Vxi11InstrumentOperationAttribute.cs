using System.ComponentModel;

namespace cc.isr.VXI11.Server;

/// <summary>   Values that represent VXI-11 instrument operation types. </summary>
public enum Vxi11InstrumentOperationType
{
    [Description( "Not specified" )] None = 0,
    [Description( "Send message tot he device." )] Write,
    [Description( "Read reply from the device." )] Read
}

/// <summary>
/// VXI-11 instrument operation tag attributes.
/// </summary>
[AttributeUsage( AttributeTargets.Method )]
public partial class Vxi11InstrumentOperationAttribute : Attribute
{

    /// <summary>
    /// VXI-11 Device command content can be marked with full name
    /// </summary>
    public string Content { get; private set; }

    /// <summary>
    /// VXI-11 Device Operation type
    /// </summary>
    public Vxi11InstrumentOperationType OperationType { get; private set; } = Vxi11InstrumentOperationType.None;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="content">Instruction content</param>
    /// <param name="operationType">I/O operation type</param>
    public Vxi11InstrumentOperationAttribute( string content, Vxi11InstrumentOperationType operationType )
    {
        this.Content = content;
        this.OperationType = operationType;
    }
}
