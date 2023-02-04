using System;
using System.Collections.Generic;
using System.Text;

namespace cc.isr.VXI11.IEEE488
{
    public class Ieee488Commands
    {

        /// <summary>   Clears status: *CLS. </summary>
        public const string CLS = "*CLS";

        /// <summary>   Enables Standard Event Status: *ESE. </summary>
        public const string ESE = "*ESE";

        /// <summary> Reads Standard Event Status: *ESE? </summary>
        public const string ESERead = "*ESE?";

        /// <summary>   Standard Event Status Register Query: *ESR?. </summary>
        public const string ESRRead = "*ESR?";

        /// <summary>   Reads the device identity string: *IDN? </summary>
        public const string IDNRead = "*IDN?";

        /// <summary>   Operation completion instruction: *OPC. </summary>
        public const string OPC = "*OPC";

        /// <summary>   Reads the operation completion status: *OPC? </summary>
        public const string OPCRead = "*OPC?";

        /// <summary>   Resets the device: *RST. </summary>
        public const string RST = "*RST";

        /// <summary>   Enables the service request events: *SRE. </summary>
        public const string SRE = "*SRE";

        /// <summary>   Reads the service request enabled status: *SER? </summary>
        public const string SRERead = "*SRE?";

        /// <summary> Read the status byte: *STB? </summary>
        public const string STBRead = "*STB?";

        /// <summary>   Trigger command: *TRG. </summary>
        public const string TRG = "*TRG";

        /// <summary>   Runs a self test and reads its status: *TST?. </summary>
        public const string TSTRead = "*TST?";

        /// <summary>   Wait until all pending operations complete. *WAI. </summary>
        public const string WAI = "*WAI";

    }
}
