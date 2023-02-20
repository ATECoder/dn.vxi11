using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using cc.isr.VXI11.Logging;

namespace cc.isr.VXI11.MSTest;

[TestClass]
public class AsyncEventHandlersTests
{

    #region " Fixture construction and cleanup "

    /// <summary>   Initializes the fixture. </summary>
    /// <param name="context">  The context. </param>
    [ClassInitialize]
    public static void InitializeFixture( TestContext context )
    {
        try
        {
            _classTestContext = context;
            Logger.Writer.LogInformation( $"{_classTestContext.FullyQualifiedTestClassName}.{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}" );
        }
        catch ( Exception ex )
        {
            Logger.Writer.LogMemberError( $"Failed initializing fixture:", ex );
            CleanupFixture();
        }
    }

    public TestContext? TestContext { get; set; }

    private static TestContext? _classTestContext;

    /// <summary>   Cleanup fixture. </summary>
    [ClassCleanup]
    public static void CleanupFixture()
    {
        _classTestContext = null;
    }

    #endregion

    /// <summary>   Event queue for all listeners interested in Event events. </summary>
    public event EventHandler<EventArgs>? Event;

    /// <summary>   (Unit Test Method) asynchronous event handler exception should be handled. </summary>
    /// <remarks>
    /// 2023-02-16.
    /// <code>
    /// Standard Output: 
    ///    2023-02-16 17:46:45.003,cc.isr.VXI11.MSTest.AsyncEventHandlersTests.AsyncEventHandlersTests
    ///    Entered the async callback method.
    ///    Started the Async task, which throws an exception on the next statement.
    ///    Entered the error handling method.
    ///    Success! Our exception handler caught: exception handled
    ///    Stack Trace:
    ///       at cc.isr.VXI11.MSTest.AsyncEventHandlersTests.&lt;&gt;c__DisplayClass10_0.&lt;AsyncEventHandlerExceptionShouldBeHandled&gt;b__2() in C:\my\lib\vs\iot\vxi\src\vxi\vxi.mstest\AsyncEventHandlersTests.cs:line 67
    ///       at System.Threading.ExecutionContext.RunFromThreadPoolDispatchLoop(Thread threadPoolThread, ExecutionContext executionContext, ContextCallback callback, Object state)
    ///    --- End of stack trace from previous location ---
    ///       at System.Threading.ExecutionContext.RunFromThreadPoolDispatchLoop(Thread threadPoolThread, ExecutionContext executionContext, ContextCallback callback, Object state)
    ///       at System.Threading.Tasks.Task.ExecuteWithThreadLocal(Task&amp; currentTaskSlot, Thread threadPoolThread )
    ///    --- End of stack trace from previous location ---
    ///       at cc.isr.VXI11.MSTest.AsyncEventHandlersTests.&lt;&gt;c__DisplayClass10_0.&lt;&lt;AsyncEventHandlerExceptionShouldBeHandled&gt;b__0&gt;d.MoveNext() in C:\my\lib\vs\iot\vxi\src\vxi\vxi.mstest\AsyncEventHandlersTests.cs:line 65
    ///    --- End of stack trace from previous location ---
    ///       at cc.isr.VXI11.AsyncEventHandlers.&lt;&gt;c__DisplayClass1_0`1.&lt;&lt;TryAsync&gt;b__0&gt;d.MoveNext() in C:\my\lib\vs\iot\vxi\src\vxi\vxi\VXI11\AsyncEventHandlers.cs:line 50
    ///    Exiting the error handling method.
    /// </code>
    /// </remarks>
    /// <exception cref="an">                           Thrown when an error condition occurs. </exception>
    /// <exception cref="InvalidOperationException">    Thrown when the requested operation is
    ///                                                 invalid. </exception>
    [TestMethod]
    public void AsyncEventHandlerExceptionShouldBeHandled()
    {
        Exception? handledException = null;
        string actulaExceptionMessage = string.Empty;
        string expectedExceptionMessage = "exception handled";
        EventHandler<EventArgs>? handler = AsyncEventHandlers.TryAsync<EventArgs>(
                async ( sender, e ) => {
                    Console.WriteLine( $"Entered the async callback method." );
                    await Task.Factory.StartNew( () => {
                        Console.WriteLine( $"Started the Async task, which throws an exception on the next statement." );
                        throw new InvalidOperationException( expectedExceptionMessage );
                    } );
                    Console.WriteLine( $"Exiting the async callback method." );
                },
                ( ex ) => {
                    Console.WriteLine( $"Entered the error handling method." );
                    handledException = ex;
                    actulaExceptionMessage = ex.Message;
                    Console.WriteLine( $"Success! Our exception handler caught: {ex.Message}\nStack Trace: \n{ex.StackTrace}" );
                    Console.WriteLine( $"Exiting the error handling method." );
                } );
        this.Event += handler;
        Event?.Invoke( this, EventArgs.Empty );

        TimeSpan timeout = TimeSpan.FromMilliseconds( 1000 );
        DateTime endTime = DateTime.Now.AddMilliseconds( 1000 );
        // wait for the event to get handled.
        while ( endTime > DateTime.Now && handledException is null )
        {
            Task.Delay( 10 ).Wait();
        }

        Assert.IsNotNull( handledException, "exception should be handled." );
        Assert.AreEqual( expectedExceptionMessage, actulaExceptionMessage, "Exception message should be set when the exception is caught" );
    }

}
