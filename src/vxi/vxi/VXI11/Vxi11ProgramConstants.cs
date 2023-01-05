namespace cc.isr.VXI11;


/// <summary>   A collection of VXI-11 program and program version constants. </summary>
internal static class Vxi11ProgramConstants
{
    /// <summary>  VXI-11 RPC Device Core channel program number. <para>
    /// Renamed from <c>DEVICE_CORE = 0x0607AF</c>. 395183 </para> </summary>
    public const int DeviceCoreProgram = 0x0607AF;

    /// <summary>  VXI-11 RPC Device Core channel program version. <para>
    /// Renamed from <c>DEVICE_CORE_VERSION = 1</c>. </para> </summary>
    public const int DeviceCoreVersion = 1;

    /// <summary>  VXI-11 RPC Device Interrupt channel program number. <para>
    /// Renamed from <c>DEVICE_INTR = 0x0607B1</c>. 395185. </para> </summary>
    public const int DeviceInterruptProgram = 0x0607B1;

    /// <summary>  VXI-11 RPC Device Interrupt channel program version number. <para>
    /// Renamed from <c>DEVICE_INTR_VERSION = 1</c>. </para> </summary>
    public const int DeviceInterruptVersion = 1;

    /// <summary>  VXI-11 RPC Device Async (Abort) channel program number. <para>
    /// Renamed from <c>DEVICE_ASYNC = 0x0607B0</c> 395184. </para> </summary>
    public const int DeviceAsyncProgram = 0x0607B0;

    /// <summary>  VXI-11 RPC Device Async (Abort) channel program version number. <para>
    /// Renamed from <c>DEVICE_ASYNC_VERSION = 1</c>. </para> </summary>
    public const int DeviceAsyncVersion = 1;

}
