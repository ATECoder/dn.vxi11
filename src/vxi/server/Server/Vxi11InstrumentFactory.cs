using System.Collections.Concurrent;

namespace cc.isr.VXI11.Server
{
    /// <summary>   A VXI-11 instrument factory. </summary>
    /// <remarks>   2023-02-14. </remarks>
    public class Vxi11InstrumentFactory
    {

        /// <summary>   Default constructor. </summary>
        /// <remarks>   2023-02-14. </remarks>
        public Vxi11InstrumentFactory()
        {
            this._instruments = new ();
        }

        private readonly ConcurrentDictionary<string, IVxi11Instrument> _instruments;

        /// <summary>   Returns a new or an existing instrument. </summary>
        /// <remarks>   2023-02-14. </remarks>
        /// <param name="instrumentDeviceName"> Name of the instrument device. </param>
        /// <param name="identity">             The identity. </param>
        /// <returns>   An IVxi11Instrument. </returns>
        public virtual IVxi11Instrument Instrument( string instrumentDeviceName,
                                                    string identity = "INTEGRATED SCIENTIFIC RESOURCES,MODEL IEEE488Mock,001,1.0.8434" )
        {
            _ = this._instruments.TryAdd( instrumentDeviceName, new Vxi11Instrument( instrumentDeviceName, identity ) );
            return this._instruments[instrumentDeviceName];
        }

    }
}
