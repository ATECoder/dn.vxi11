// See https://aka.ms/new-console-template for more information

Console.WriteLine( "Starting VXI-11 IEEE 488 Server..." );
try
{
    cc.isr.VXI11.IEEE488.Ieee488Server server = new ();
    // TestServer.Ieee488Server server = new();
    // Vxi11Server server = new Vxi11Server();
    server.Run();
}
catch ( System.Exception e )
{
    Console.WriteLine( e.ToString() );
}
//server.stopRpcProcessing();// stop the service
Console.WriteLine( "VXI-11 IEEE 488 Server stopped." );

