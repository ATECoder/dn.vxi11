using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace cc.isr.Logger
{
    /// <summary>   A writer. </summary>
    /// <remarks>   2022-12-16. </remarks>
    internal static class Writer
    {
        /// <summary>   Trace message. </summary>
        /// <remarks>   2022-12-16. </remarks>
        /// <param name="message">          The message. </param>
        public static void TraceMessage( string message )
        {
            Trace.WriteLine( $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff},{message}" );
        }

        public static void ConsoleWriteMessage( string message, ConsoleColor color = ConsoleColor.White )
        {
            Console.ForegroundColor = color;
            Console.WriteLine( $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff},{message}" );
            Console.ResetColor();
        }

        /// <summary>   Builds exception message. </summary>
        /// <remarks>   2022-12-16. </remarks>
        /// <param name="message">          The message. </param>
        /// <param name="ex">               The exception. </param>
        /// <param name="memberName">       Name of the member. </param>
        /// <param name="sourceFilePath">   Full pathname of the source file. </param>
        /// <param name="sourceLineNumber"> Source line number. </param>
        /// <returns>   A string. </returns>
        public static string BuildExceptionMessage( string message, Exception ex, string memberName, string sourceFilePath, int sourceLineNumber )
        {
            int indent = 4;
            StringBuilder builder = new();
            _ = builder.Append( $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff},{memberName}({sourceLineNumber})" );
            if ( !string.IsNullOrEmpty( message ) ) _ = builder.AppendLine( $"{new string( ' ', indent )}{message}" );
            _ = builder.AppendLine( $"{new string( ' ', indent )}{ex.Message}" );
            if ( !string.IsNullOrEmpty( ex.StackTrace ) ) _ = builder.AppendLine( $"{new string( ' ', indent )}{ex.StackTrace}" );
            if ( !string.IsNullOrEmpty( sourceFilePath ) ) _ = builder.AppendLine( $"{new string( ' ', indent )}{sourceFilePath}" );
            return builder.ToString();
        }

        /// <summary>   Trace exception. </summary>
        /// <remarks>   2022-12-16. </remarks>
        /// <param name="message">          The message. </param>
        /// <param name="ex">               The exception. </param>
        /// <param name="memberName">       (Optional) Name of the member. </param>
        /// <param name="sourceFilePath">   (Optional) Full pathname of the source file. </param>
        /// <param name="sourceLineNumber"> (Optional) Source line number. </param>
        public static void TraceException( string message, Exception ex,
                [CallerMemberName] string memberName = "",
                [CallerFilePath] string sourceFilePath = "",
                [CallerLineNumber] int sourceLineNumber = 0 )
        {
            Trace.WriteLine( BuildExceptionMessage( message, ex, memberName, sourceFilePath, sourceLineNumber ) );
        }

        /// <summary>   Console write exception. </summary>
        /// <remarks>   2022-12-16. </remarks>
        /// <param name="message">          The message. </param>
        /// <param name="ex">               The exception. </param>
        /// <param name="memberName">       (Optional) Name of the member. </param>
        /// <param name="sourceFilePath">   (Optional) Full pathname of the source file. </param>
        /// <param name="sourceLineNumber"> (Optional) Source line number. </param>
        public static void ConsoleWriteException( string message, Exception ex,
                [CallerMemberName] string memberName = "",
                [CallerFilePath] string sourceFilePath = "",
                [CallerLineNumber] int sourceLineNumber = 0 )
        {
            Console.ForegroundColor = System.ConsoleColor.Red;
            Console.WriteLine( BuildExceptionMessage( message, ex, memberName, sourceFilePath, sourceLineNumber ) );
            Console.ResetColor();
        }
    }
}
