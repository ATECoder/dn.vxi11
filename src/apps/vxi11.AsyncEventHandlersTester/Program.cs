using System.Runtime.CompilerServices;

Console.WriteLine("Starting example...");

ErrorHandlerExample currentExample = ErrorHandlerExample.NotSpecified;
string exampleDescription = string.Empty;
string notes = string.Empty;

System.AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

// added per @DevLeader 20230214
TaskScheduler.UnobservedTaskException += OnTaskSchedulerUnobsserverException;


var raisingObject = new RaisingObject();

foreach ( ErrorHandlerExample example in Enum.GetValues(typeof(ErrorHandlerExample)))
{
    Exception? ex = null;
    currentExample = example;
    EventHandler<EventArgs>? handler = null;
    switch ( currentExample )
    {
        case ErrorHandlerExample.NoErrorHandler:
            exampleDescription = "Base case: No Error Handler";
            notes = "this elicits an unhandled exception which crashes the application";

#if false
            handler = async ( s, e) =>
            {
                Console.WriteLine( $"[{solutionDescription}] Starting the event handler..." );
                await TaskThatThrowsAsync();
                Console.WriteLine( $"[{solutionDescription}] Event handler completed; exception not caught." );
            };
#else
            Console.WriteLine( $"[{exampleDescription}] not implemented because the unhandled exception crashed the program..." );
#endif
            break;

        case ErrorHandlerExample.InsideEventErrorHandler:
            exampleDescription = "Solution1: Inside Event Error Handler";
            notes = "";
            handler = async (sender, e) =>
            {
                Console.WriteLine( $"[{exampleDescription}] Starting the event handler..." );
                Exception? ex = null;
                try
                {
                    await TaskThatThrowsAsync();
                }
                catch ( Exception excep )
                {
                    ex = excep;
                }
                finally
                {
                    if ( ex is null )
                        Console.WriteLine( $"[{exampleDescription}] Event handler completed; exception not caught." );
                    else
                        Console.WriteLine( $"[{exampleDescription}] Our exception handler caught: {ex.Message}\nStack Trace: \n{ex.StackTrace}" );
                }
            };
            break;

        case ErrorHandlerExample.TryAsyncErrorCallback:
            exampleDescription = "Solution 2: TryAsync Error Callback";
            notes = "";
            handler = EventHandlers.TryAsync<EventArgs>(
                async ( sender, e ) => {
                    Console.WriteLine( $"[{exampleDescription}] Starting the event handler..." );
                    await TaskThatThrowsAsync();
                    Console.WriteLine( $"[{exampleDescription}] Event handler completed." );
                },
                ex => Console.WriteLine( $"[{exampleDescription}] Our exception handler caught: {ex.Message}\nStack Trace: \n{ex.StackTrace}" ) );
            break;
        case ErrorHandlerExample.ContinueWith:
            exampleDescription = "Solution 3: Continue With Error Handler";
            notes = "the IDE halts displaying a dialog indicating that an exception was thrown " +
                      "'but was not handled in user code'. Per @DevLeader, 'that's probably because there is " +
                      "legitimately no try/catch in the vicinity of where the exception is thrown. " +
                      "It's likely crossing some async boundary before it's caught.' Still, the exception " +
                      "is handled by the continuation, as, indeed, neither the unhandled exception hander nor the " +
                      "unobserved exception handler report the exception";
            Console.WriteLine( $"[{exampleDescription}] Starting the event handler..." );
            ex = null;
            try
            {
                await AsyncTaskThatThrowsAsync( (ex ) =>
                {
                    Console.WriteLine( $"[{exampleDescription}] Our exception handler caught aggregate exception: {ex.Message}\n" );
                    foreach ( Exception exep in ex.InnerExceptions)
                        Console.WriteLine( $"[{exampleDescription}] inner exception: {exep.Message}\nStack Trace: \n{exep.StackTrace}" );
                } );
            }
            catch ( Exception excep )
            {
                ex = excep;
            }
            finally
            {
                if ( ex is null )
                    Console.WriteLine( $"[{exampleDescription}] Event handler completed; exception not caught." );
                else
                    Console.WriteLine( $"[{exampleDescription}] Our exception handler caught: {ex.Message}\nStack Trace: \n{ex.StackTrace}" );
            }
            Console.WriteLine( $"[{exampleDescription}] completed.\n" );
            break;

        case ErrorHandlerExample.ContinueWithAndThrowInsideTryCatch:
            exampleDescription = "Solution 4: Continue With Error Handler and throw inside Try Catch";
            notes = "..similar to solution 3 with an exception thrown within the error handler, which " +
                "is now reported as caught by the try catch block of the 'exception hander'.";

            Console.WriteLine( $"[{exampleDescription}] Starting the event handler..." );
            ex = null;
            try
            {
                await AsyncTaskThatThrowsAsync( ( ex ) => {
                    Console.WriteLine( $"[{exampleDescription}] Our exception handler caught aggregate exception: {ex.Message}\n" );
                    foreach ( Exception exep in ex.InnerExceptions )
                        Console.WriteLine( $"[{exampleDescription}] inner exception: {exep.Message}\nStack Trace: \n{exep.StackTrace}" );

                    // @DevLeader I added this line!!
                    throw new InvalidOperationException( "Re-thrown from error handler!!", ex );
                } );
            }
            catch ( Exception excep )
            {
                // @DevLeader: 'ONLY with my new line will this now catch the exception.
                // The original exception is truly handled by the continuation already!'
                ex = excep;
            }
            finally
            {
                if ( ex is null )
                    Console.WriteLine( $"[{exampleDescription}] Event handler completed; exception not caught." );
                else
                    Console.WriteLine( $"[{exampleDescription}] Our exception handler caught: {ex.Message}\nStack Trace: \n{ex.StackTrace}" );
            }
            Console.WriteLine( $"[{exampleDescription}] completed.\n" );
            break;

        case ErrorHandlerExample.ContinueWithEventHandler:
            exampleDescription = "Solution 5: Continue With inside of Event Handler";
            notes = "@DevLeader: 'works to protect the event handler. If your continuation doesn't blow up, " +
                    "it successfully keeps execution working properly.'";
            handler += async ( sender, e ) =>
            {
                await AsyncTaskThatThrowsAsync( ( ex ) =>
                {
                    Console.WriteLine( $"[{exampleDescription}] Our exception handler caught aggregate exception: {ex.Message}\n" );
                    foreach ( Exception exep in ex.InnerExceptions )
                        Console.WriteLine( $"[{exampleDescription}] inner exception: {exep.Message}\nStack Trace: \n{exep.StackTrace}" );

                } );
            };
            break;
        case ErrorHandlerExample.ContinueWithThrowsEventHandler:
            exampleDescription = "Solution 6: Continue With inside of Event Handler";
            notes = notes = "@DevLeader: 'is almost the same, except that I throw inside the continuation. And as I expected, " +
                            "because this exception now needs to bubble up and cross that async void boundary, I do truly get " +
                            "an unhandled exception printed out and that extra console writeline inside scenario 6 is never written.";
            handler += async ( sender, e ) =>
            {
                await AsyncTaskThatThrowsAsync( ( ex ) =>
                {
                    Console.WriteLine( $"[{exampleDescription}] Our exception handler caught aggregate exception: {ex.Message}\n" );
                    foreach ( Exception exep in ex.InnerExceptions )
                        Console.WriteLine( $"[{exampleDescription}] inner exception: {exep.Message}\nStack Trace: \n{exep.StackTrace}" );

                    throw new InvalidOperationException( "Re-thrown from error handler!!", ex );
                } );

                Console.WriteLine( $"[{exampleDescription}] Does anything still run in the event handler after an awaited task blows up?" );
            };
            break;

            default:
            break;
    }

    if ( handler is not null )
    {

        raisingObject.Event += handler;

        Exception? caughtException  = null;
        try
        {
            Console.WriteLine( $"\n\n{currentExample}: {exampleDescription}:" );
            Console.WriteLine( "Raising our event..." );
            raisingObject.Raise( EventArgs.Empty );
        }
        catch ( Exception ex1 )
        {
            caughtException = ex1;
        }

        Console.WriteLine();
        if ( caughtException is null )
            Console.WriteLine( $"[{exampleDescription}] The Try...Catch block caught no exception\n" );
        else
            Console.WriteLine( $"[{exampleDescription}] The Try...Catch block caught: {caughtException.Message}\nStack Trace: \n{caughtException.StackTrace}\n" );

        if ( handler is not null )
            raisingObject.Event -= handler;

        Console.WriteLine( $"[{exampleDescription}] completed.\n" );

    }

    // allow time for handling unhandled exceptions.
    await Task.Delay( 100 );
}

Console.WriteLine("Example complete.");

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
async Task TaskThatThrowsAsync()
{
    Console.WriteLine( $"[{exampleDescription}] Starting task that throws async..." );
    throw new InvalidOperationException( $"This is our [{exampleDescription}] exception" );
};
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

async Task AsyncTaskThatThrowsAsync( Action<AggregateException> errorHandler )
{
    await Task.Factory.StartNew( () => {
        Console.WriteLine( $"[{exampleDescription}] Starting Async task that throws async..." );
        throw new InvalidOperationException( $"This is our [{exampleDescription}] exception" );
    } )
        .ContinueWith( failedTask => errorHandler( failedTask.Exception! ), TaskContinuationOptions.OnlyOnFaulted );
}

#region " Unhandled exception handling "

void OnUnhandledException( object sender, UnhandledExceptionEventArgs e )
{
    Console.WriteLine( $"\n[{exampleDescription}] Unhandled exception occurred in {currentExample}: {e.ExceptionObject}\n");
}

void OnTaskSchedulerUnobsserverException( object? sender, UnobservedTaskExceptionEventArgs e )
{
    Console.WriteLine( $"\n[{exampleDescription}] {(e.Observed ? "" : "un")}observed exception occurred in {currentExample}: {e.Exception}\n" );
}

#endregion


internal class RaisingObject
{
    // you could in theory have your own event arguments here
    public event EventHandler<EventArgs>? Event;

    public void Raise(EventArgs e)
    {
        Event?.Invoke(this, e);
    }
}

#region " Wrapper Solution "

/// <summary>   An event handlers. </summary>
/// <remarks>   2023-02-15. <para>
/// 
/// References:
/// <see href="https://www.codeproject.com/Articles/5354588/Async-EventHandlers-A-Simple-Safety-Net-to-the-Res">Code project articles</see>,
/// <see href="https://github.com/ncosentino/DevLeader/blob/master/AsyncEvents/AsyncEvents.Wrapper/Program.cs">Repository</see>,
/// <see href="https://www.codeproject.com/script/Membership/View.aspx?mid=8784357">About Dev Leader</see>.
/// <see href="https://codeblog.jonskeet.uk/category/eduasync/">John Skeet on Async</see>
///</para>
/// </remarks>
internal static class EventHandlers
{
    public static EventHandler<TEventArgs> TryAsync<TEventArgs>(
        Func<object, TEventArgs, Task> callback,
        Action<Exception> errorHandler)
        where TEventArgs : EventArgs
            => TryAsync<TEventArgs>(
            callback,
            ex =>
            {
                errorHandler.Invoke(ex);
                return Task.CompletedTask;
            });

    public static EventHandler<TEventArgs> TryAsync<TEventArgs>(
        Func<object, TEventArgs, Task> callback,
        Func<Exception, Task> errorHandler)
        where TEventArgs : EventArgs
    {
        return new EventHandler<TEventArgs>(async (object? sender, TEventArgs e) =>
        {
            try
            {
                if ( sender is not null )
                    await callback.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                await errorHandler.Invoke(ex);
            }
        });
    }

}

#endregion

internal enum ErrorHandlerExample
{
    NotSpecified,
    NoErrorHandler,
    InsideEventErrorHandler,
    TryAsyncErrorCallback,
    ContinueWith,
    ContinueWithAndThrowInsideTryCatch,
    ContinueWithEventHandler,
    ContinueWithThrowsEventHandler,
}
