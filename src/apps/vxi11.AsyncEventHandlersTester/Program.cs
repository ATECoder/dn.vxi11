using System.Runtime.CompilerServices;

Console.WriteLine("Starting example...");

ErrorHandlerExample currentExample = ErrorHandlerExample.NotSpecified;
string exampleDescription = string.Empty;

System.AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

var raisingObject = new RaisingObject();

foreach ( ErrorHandlerExample example in Enum.GetValues(typeof(ErrorHandlerExample)))
{
    currentExample = example;
    EventHandler<EventArgs>? handler = null;
    switch ( currentExample )
    {
        case ErrorHandlerExample.NoErrorHandler:
            exampleDescription = "Base case: No Error Handler";
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
            Console.WriteLine( $"[{exampleDescription}] Starting the event handler..." );
            Exception? ex = null;
            try
            {
                await AsyncTaskThatThrowsAsync( (ex ) =>
                {
                    Console.WriteLine( $"[{exampleDescription}] Our exception handler caught aggregate exception: {ex.Message}\n" );
                    foreach ( Exception exep in ex.InnerExceptions)
                        Console.WriteLine( $"[{exampleDescription}] inner exception: {exep.Message}\nStack Trace: \n{exep.StackTrace}" );
                });
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
        catch ( Exception ex )
        {
            caughtException = ex;
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
    Task.Delay( 100 ).Wait();
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
}
