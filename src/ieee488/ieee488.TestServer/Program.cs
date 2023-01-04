// See https://aka.ms/new-console-template for more information

Console.Out.WriteLine( "Starting VXI-11 Demo Server..." );
try
{
    cc.isr.VXI11.IEEE488.Ieee488Server server = new ();
    // TestServer.Ieee488Server server = new();
    // Vxi11Server server = new Vxi11Server();
    server.Run();
}
catch ( System.Exception e )
{
    Console.Out.WriteLine( "demoServer oops:" );
    Console.Out.WriteLine( e.Message );
    Console.Out.WriteLine( e.StackTrace );
}
//server.stopRpcProcessing();// stop the service
Console.Out.WriteLine( "demoServer stopped." );

