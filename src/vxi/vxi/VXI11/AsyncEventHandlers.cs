namespace cc.isr.VXI11;

/// <summary>   An asynchronous event handlers. </summary>
/// <remarks>   2023-02-15. <para>
/// 
/// (c) 2023 <see href="https://www.codeproject.com/script/Membership/View.aspx?mid=8784357">Dev Leader</see>. </para><para>
/// 
/// References:
/// <see href="https://www.codeproject.com/Articles/5354588/Async-EventHandlers-A-Simple-Safety-Net-to-the-Res">Code project articles</see>,
/// <see href="https://github.com/ncosentino/DevLeader/blob/master/AsyncEvents/AsyncEvents.Wrapper/Program.cs">Repository</see>,
/// <see href="https://codeblog.jonskeet.uk/category/eduasync/">John Skeet on Async</see>
///</para>
/// </remarks>
public class AsyncEventHandlers
{
    /// <summary>   Asynchronous handler of events. </summary>
    /// <remarks>   2023-02-16. </remarks>
    /// <typeparam name="TEventArgs">   Type of the event arguments. </typeparam>
    /// <param name="callback">     An action to handle the event, which might throw and exception to
    ///                             be handled by the <paramref name="errorHandler"/> action. </param>
    /// <param name="errorHandler"> An error handler action. </param>
    /// <returns>   An EventHandler&lt;TEventArgs&gt; </returns>
    public static EventHandler<TEventArgs> TryAsync<TEventArgs>(
        Func<object, TEventArgs, Task> callback,
        Action<Exception> errorHandler )
        where TEventArgs : EventArgs
            => TryAsync<TEventArgs>(
            callback,
            ex => {
                errorHandler.Invoke( ex );
                return Task.CompletedTask;
            } );

    /// <summary>   Asynchronous handler of events. </summary>
    /// <remarks>   2023-02-16. </remarks>
    /// <typeparam name="TEventArgs">   Type of the event arguments. </typeparam>
    /// <param name="callback">     A function to handle the event, which might throw and exception to
    ///                             be handled by the <paramref name="errorHandler"/> action. </param>
    /// <param name="errorHandler"> An error handler function. </param>
    /// <returns>   An EventHandler&lt;TEventArgs&gt; </returns>
    public static EventHandler<TEventArgs> TryAsync<TEventArgs>(
        Func<object, TEventArgs, Task> callback,
        Func<Exception, Task> errorHandler )
        where TEventArgs : EventArgs
    {
        return new EventHandler<TEventArgs>( async ( object? sender, TEventArgs e ) => {
            try
            {
                if ( sender is not null )
                    await callback.Invoke( sender, e );
            }
            catch ( Exception ex )
            {
                await errorHandler.Invoke( ex );
            }
        } );
    }

}
