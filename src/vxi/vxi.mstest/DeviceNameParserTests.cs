using cc.isr.VXI11.Logging;
namespace cc.isr.VXI11.MSTest
{
    [TestClass]
    public class DeviceNameParserTests
    {


        private readonly string[] _deviceNames = { ":inst0",
                                                 "gpib,5",
                                                 ":gpib,5",
                                                 ":gpib,5,10",
                                                 ":usb0",
                                                 ":usb0[0x5678::0x33::SN999::1]",
                                                 ":usb0[1234::5678::MYSERIAL::0]" };

        /// <summary>   Assert device name parse. </summary>
        /// <remarks>   2023-02-11. </remarks>
        /// <param name="deviceName">   Name of the device. </param>
        private static void AssertDeviceNameParse( string deviceName )
        {
            cc.isr.VXI11.DeviceNameParser parser = new( deviceName );
            string builtDeviceName = parser.BuildDeviceName();
            Assert.AreEqual( deviceName, builtDeviceName, $"device name {builtDeviceName} built from parsed {deviceName} not matching" );
            Logger.Writer.LogInformation( $"device is {(string.IsNullOrEmpty( parser.DeviceName ) ? "empty" : parser.DeviceName)} for {deviceName} " );
        }

        /// <summary>   (Unit Test Method) device name parse. </summary>
        /// <remarks>   2023-02-11.
        /// <code>
        /// </code>
        /// </remarks>
        [TestMethod]
        public void DeviceNameParse()
        {
            foreach ( string deviceName in this._deviceNames )
            {
                AssertDeviceNameParse( deviceName );
            }
        }




    }
}
