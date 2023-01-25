using System.ComponentModel;

namespace cc.isr.VXI11.IEEE488.Mock;

/// <summary>   Values that represent IEEE488 operation types. </summary>
public enum Ieee488OperationType
{
    [Description( "Not specified" )] None = 0,
    [Description( "Send message tot he device." )] Write,
    [Description( "Read reply from the device." )] Read
}

/// <summary>
/// IEEE488 Directive tag attributes.
/// </summary>
[AttributeUsage( AttributeTargets.Method )]
public partial class Ieee488Attribute : Attribute
{

    /// <summary>
    /// IEEE488 command content can be marked with full name
    /// </summary>
    public string Content { get; private set; }

    /// <summary>
    /// Operation type
    /// </summary>
    public Ieee488OperationType OperationType { get; private set; } = Ieee488OperationType.None;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="content">Instruction content</param>
    /// <param name="operationType">I/O operation type</param>
    public Ieee488Attribute( string content, Ieee488OperationType operationType )
    {
        this.Content = content;
        this.OperationType = operationType;
    }
}
