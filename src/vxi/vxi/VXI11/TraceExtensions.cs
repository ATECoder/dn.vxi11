using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace cc.isr.VXI11;

/// <summary>   A trace extensions. </summary>
/// <remarks>   2023-06-02. </remarks>
public static class TraceExtensions
{

    /// <summary>   (Immutable) the multi line member message format. </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Naming", "CA1707:Identifiers should not contain underscores", Justification = "<Pending>" )]
    public const string MULTI_LINE_MEMBER_MESSAGE_FORMAT = "{message}\n  at '{sourceFilePath}' {memberName} line {sourceLineNumber})";

    /// <summary>   (Immutable) the single line member message format. </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Naming", "CA1707:Identifiers should not contain underscores", Justification = "<Pending>" )]
    public const string SINGLE_LINE_MEMBER_MESSAGE_FORMAT = "{message} at '{sourceFilePath}' {memberName} line {sourceLineNumber})";

    /// <summary>   Gets or sets the member message format. </summary>
    /// <value> The member message format. </value>
    public static string MemberMessageFormat { get; set; } = MULTI_LINE_MEMBER_MESSAGE_FORMAT;

    /// <summary>   (Immutable) the multi line member exception message format. </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Naming", "CA1707:Identifiers should not contain underscores", Justification = "<Pending>" )]
    public const string MULTI_LINE_MEMBER_EXCEPTION_MESSAGE_FORMAT = "{message}\n  {ex}\n  at '{sourceFilePath}' {memberName} line {sourceLineNumber})";

    /// <summary>   (Immutable) the single line member exception message format. </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Naming", "CA1707:Identifiers should not contain underscores", Justification = "<Pending>" )]
    public const string SINGLE_LINE_MEMBER_EXCEPTION_MESSAGE_FORMAT = "{message} {ex} at '{sourceFilePath}' {memberName} line {sourceLineNumber})";

    /// <summary>   Gets or sets the member exception message format. </summary>
    /// <value> The member exception message format. </value>
    public static string MemberExceptionMessageFormat { get; set; } = MULTI_LINE_MEMBER_EXCEPTION_MESSAGE_FORMAT;

    /// <summary>
    /// An <see cref="Diagnostics.Trace"/> extension method that traces member information.
    /// </summary>
    /// <remarks>   2023-03-23. </remarks>
    /// <param name="message">          The message. </param>
    /// <param name="memberName">       (Optional) Name of the member. </param>
    /// <param name="sourceFilePath">   (Optional) Full pathname of the source file. </param>
    /// <param name="sourceLineNumber"> (Optional) Source line number. </param>
    public static void TraceMemberInfo( string message,
                                        [CallerMemberName] string memberName = "",
                                        [CallerFilePath] string sourceFilePath = "",
                                        [CallerLineNumber] int sourceLineNumber = 0 )
    {
        Trace.TraceInformation( MemberMessageFormat, message, sourceFilePath, memberName, sourceLineNumber );
    }

    /// <summary>
    /// An <see cref="Diagnostics.Trace"/> extension method that traces member warning.
    /// </summary>
    /// <remarks>   2023-03-23. </remarks>
    /// <param name="message">          The message. </param>
    /// <param name="memberName">       (Optional) Name of the member. </param>
    /// <param name="sourceFilePath">   (Optional) Full pathname of the source file. </param>
    /// <param name="sourceLineNumber"> (Optional) Source line number. </param>
    public static void TraceMemberWarning( string message,
                                           [CallerMemberName] string memberName = "",
                                           [CallerFilePath] string sourceFilePath = "",
                                           [CallerLineNumber] int sourceLineNumber = 0 )
    {
        Trace.TraceWarning( MemberMessageFormat, message, sourceFilePath, memberName, sourceLineNumber );
    }

    /// <summary>   Console write exception. </summary>
    /// <remarks>   2023-03-23. </remarks>
    /// <param name="message">          The message. </param>
    /// <param name="ex">               The exception. </param>
    /// <param name="memberName">       (Optional) Name of the member. </param>
    /// <param name="sourceFilePath">   (Optional) Full pathname of the source file. </param>
    /// <param name="sourceLineNumber"> (Optional) Source line number. </param>
    public static void TraceMemberError( string message, System.Exception ex,
                                         [CallerMemberName] string memberName = "",
                                         [CallerFilePath] string sourceFilePath = "",
                                         [CallerLineNumber] int sourceLineNumber = 0 )
    {
            Trace.TraceError( MemberExceptionMessageFormat, message, ex, sourceFilePath, memberName, sourceLineNumber );
    }

}
