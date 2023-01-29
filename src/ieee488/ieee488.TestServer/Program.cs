// See https://aka.ms/new-console-template for more information

using cc.isr.ONC.RPC.Logging;
using cc.isr.ONC.RPC.Portmap;

try
{
    cc.isr.VXI11.IEEE488.Mock.Ieee488SingleClientMockServer server = new();
    // TestServer.Ieee488Server server = new();
    // Vxi11Server server = new Vxi11Server();
    Logger.Writer.LogInformation( "Starting the embedded port map service" );
    using OncRpcEmbeddedPortmapServiceStub epm = OncRpcEmbeddedPortmapServiceStub.StartEmbeddedPortmapService();

    Logger.Writer.LogInformation( "Starting VXI-11 IEEE 488 Server" );
    server.Run();
}
catch ( System.Exception e )
{
    Console.WriteLine( e.ToString() );
}
//server.stopRpcProcessing();// stop the service
Console.WriteLine( "VXI-11 IEEE 488 Server stopped." );

