using System.Text.RegularExpressions;

using cc.isr.VXI11.Logging;
namespace cc.isr.VXI11.Visa.MSTest
{
    /// <summary>   (Unit Test Class) a TCP/IP INSTR address (resource name) tests. </summary>
    [TestClass]
    public class TcpipInstrAddressTests
    {


        private readonly string[] _addresses = { "TCPIP::10.0.0.1::INSTR",
                                                 "TCPIP0::10.0.0.1::INSTR",
                                                 "TCPIP0::10.0.0.1::inst0::INSTR",
                                                 "TCPIP::10.0.0.1::gpib,5::INSTR",
                                                 "TCPIP0::10.0.0.1::gpib,5::INSTR",
                                                 "TCPIP0::10.0.0.1::gpib,5,10::INSTR",
                                                 "TCPIP0::10.0.0.1::usb0::INSTR",
                                                 "TCPIP0::10.0.0.1::usb0[0x5678::0x33::SN999::1]::INSTR",
                                                 "TCPIP0::10.0.0.1::usb0[1234::5678::MYSERIAL::0]::INSTR" };

        private static void AssertVisaAddressShouldParse( string pattern, string address )
        {
            var m = Regex.Match( address, pattern, RegexOptions.IgnoreCase );
            Assert.IsNotNull( m );
            Assert.IsTrue( m.Groups.Keys.Any() );
            Logger.Writer.LogInformation( $"\nParse of: {address}" );
            foreach ( var key in m.Groups.Keys ) { Logger.Writer.LogInformation( $"{key} {m.Groups[key]}" ); }
        }

        [TestMethod]
        public void TcpipVisaAddressShouldParse()
        {
            string pattern = @"^(?<board>(?<protocol>TCPIP)\d*)(::(?<host>[^\s:]+))(::(?<device>[^\s:]+(\[.+\])?))?(::(?<suffix>INSTR))$";
            foreach ( string address in this._addresses )
            {
                AssertVisaAddressShouldParse( pattern, address );
            }
        }

        private static void AssertTcpipInstrAddressShouldParse( string address )
        {
            cc.isr.VXI11.Visa.TcpipInstrAddress instrAddress = new( address );
            string actual = instrAddress.BuildAddress();
            cc.isr.VXI11.Visa.TcpipInstrAddress actualAddress = new( actual );
            Assert.IsTrue( actualAddress.Equals( instrAddress ), $"{address} not equals {actual}" );
            if ( !instrAddress.InterfaceDeviceAddress.IsValid() )
            {
                // instrAddress = new( address );
                _ = instrAddress.InterfaceDeviceAddress.IsValid();
            }
            Assert.IsTrue( instrAddress.InterfaceDeviceAddress.IsValid(), $"{instrAddress.Device} is invalid in {address}" );
            Logger.Writer.LogInformation( $"device is {(string.IsNullOrEmpty( instrAddress.Device ) ? "empty" : instrAddress.Device)} for {address} " );
        }

        [TestMethod]
        public void TcpipInstrAddressShouldParse()
        {
            foreach ( string address in this._addresses )
            {
                AssertTcpipInstrAddressShouldParse( address );
            }
        }




    }
}
