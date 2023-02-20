using System.Collections.Concurrent;

namespace cc.isr.VXI11.Server
{
    /// <summary>   A VXI-11 Interface factory. </summary>
    /// <remarks>   2023-02-14. </remarks>
    public class Vxi11InterfaceFactory
    {

        /// <summary>   Default constructor. </summary>
        /// <remarks>   2023-02-14. </remarks>
        public Vxi11InterfaceFactory()
        {
            this._interfaces = new();
        }

        private readonly ConcurrentDictionary<string, IVxi11Interface> _interfaces;

        /// <summary>   Returns a new or an existing Interface. </summary>
        /// <remarks>   2023-02-14. </remarks>
        /// <param name="interfaceDeviceName"> Name of the Interface device. </param>
        /// <returns>   An IVxi11Interface. </returns>
        public virtual IVxi11Interface Interface( string interfaceDeviceName )
        {
            _ = this._interfaces.TryAdd( interfaceDeviceName, new Vxi11Interface( interfaceDeviceName ) );
            return this._interfaces[interfaceDeviceName];
        }

    }
}
