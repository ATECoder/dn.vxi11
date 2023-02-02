// See https://aka.ms/new-console-template for more information

using cc.isr.VXI11.Logging;
using cc.isr.VXI11.LXI.Discover;

Console.WriteLine( $"VXI-11 {nameof( LxiDiscover )} " );
CommandLineParser.I.ParseArgs( args, CommandLineParser.DefaultArgs );

if ( CommandLineParser.I.Contains( CommandLineParser.HelpKey ) )
    Console.WriteLine( LxiDiscover.HelpDescription + "\n" );
else
{
    try
    {
        LxiDiscover.Discover( CommandLineParser.I.GetString( CommandLineParser.IPKey ), ( int ) CommandLineParser.I.GetLong( CommandLineParser.TimeoutKey ) );
    }
    catch ( Exception ex )
    {
        Logger.Writer.LogError( "There has been an exception, find the details below.." , ex );
    }
}

Console.Write( "\n Press any key to exit: " );
Console.ReadKey();




