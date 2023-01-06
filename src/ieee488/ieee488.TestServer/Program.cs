// See https://aka.ms/new-console-template for more information

Console.WriteLine( "Starting VXI-11 Demo Server..." );
try
{
    cc.isr.VXI11.IEEE488.Ieee488Server server = new ();
    // TestServer.Ieee488Server server = new();
    // Vxi11Server server = new Vxi11Server();
    server.Run();
}
catch ( System.Exception e )
{
    Console.WriteLine( "demoServer oops:" );
    Console.WriteLine( e.Message );
    Console.WriteLine( e.StackTrace );
}
//server.stopRpcProcessing();// stop the service
Console.WriteLine( "demoServer stopped." );

