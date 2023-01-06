# TCP/IP Instrument Protocol (VXI-11)

A control and communication library for LXI-based instruments. 

- [TCP/IP Instrument Protocol (VXI-11)](#VXI11-Protocol)
- [Network Instrument Protocol](#Network-Instrument-Protocol)
- [References](#References)

<a name="VXI11-Protocol"></a>
## TCP/IP Instrument Protocol (VXI-11)

The VXI-11 protocol is an [RPC]-based communication protocol primarily designed for connecting instruments, defined as devices, (such as oscilloscopes, multimeters, spectrum analyzers etc.) to controllers.

The term controller refers to the RPC client, while the term device typically refers to the RPC server. The only exception is when the roles are reversed for interrupts.

Instruments may support this protocol directly (e.g., as part of implementing the more recent [LXI] interface), or may be connected by way of an adapter (gateway) that attaches to a dedicated bus (such as GPIB).

VXI-11 uses TCP as its transport protocol.

The [VXI Bus Specification] was published in 1995 by the VXIbus Consortium, Inc.

<a name="Network-Instrument-Protocol"></a>
## Network Instrument Protocol
The network instrument protocol uses the ONC remote procedure call (RPC) model.

Conceptually, this model allows one application (typically called the client) to call procedures in a remote application (typically called the server) as if the remote procedures were local. This specification uses ONC/RPC for defining the network instrument messages which are passed over the network, but does not require that these RPCs be provided as the application interface. The ONC/RPC interface may, however, be used by a device designer as a matter of convenience.

The client identifies the remote procedure, or message, by a unique number. This number is then encoded into a message along with the procedure's argument types and values. The message is sent to the server machine where it is decoded by the server. The server uses the unique identifier to dispatch the request.

When the request is completed, the return values are encoded into a message which is sent back to the
client machine.

The interface definition ( see Appendix I, "Network instrument RPCL") gives the function prototypes as well as the unique identifiers for the procedures. For ONC RPC, the unique identifier is a combination of a program number (also known as an interface id), a procedure number, and a version number.

|VXI-11 Message    |C# Procedure    | Channel | Description  
|------------------|:--------------:|:-------:|:------------  
|create_link       |CreateLink      |core     |opens a link to a device  
|device_write      |DeviceWrite     |core     |device receives a message
|device_read       |DeviceRead      |core     |device returns a result
|device_readstb    |DeviceReadStb   |core     |device returns its status byte
|device_trigger    |DeviceTrigger   |core     |device executes a trigger
|device_clear      |DeviceClear     |core     |device clears itself
|device_remote     |DeviceRemote    |core     |device disables its front panel
|device_local      |DeviceLocal     |core     |device enables its front panel
|device_lock       |DeviceLock      |core     |device is locked
|device_unlock     |DeviceUnlock    |core     |device is unlocked
|create_intr_chan  |CreateIntrChan  |core     |device creates interrupt channel
|destroy_intr_chan |DestroyIntrChan |core     |device destroys interrupt channel
|device_enable_srq |DeviceEnableSrq |core     |device enables/disables sending of service requests
|device_docmd      |DeviceDoCmd     |core     |device   
|destroy_link      |DestroyLink     |core     |closes a link to a device
|device_abort      |DeviceAbort     |abort    |device aborts an in-progress call
|device_intr_srq   |DeviceIntrSrq   |interrupt|used by device to send a service request

#### Table 1 Network instrument Protocol
The 17 messages that define the network instrument protocol. These messages are expected to be supported by all devices that claim to be network instrument compliant. Most of these messages will be familiar to those who have worked with IEEE 488 devices.

## CONNECTION MODEL

The network instrument protocol uses up to three channels between the controller and the device for passing network instrument messages. A network instrument connection is this set of channels:
* __Core Channel:__ a core synchronous command channel for transfer of all requests except the `device_abort` RPC and the `device_intr_srq` remote procedure calls.
* __Abort Channel:__ a secondary, asynchronous abort channel for aborting core channel operations using the `device_abort` remote procedure call (optional for client). A network instrument server's abort channel is typically implemented as an interrupt or signal handler in a single threaded operating system, or as a higher priority thread in a multi-threaded operating system.
* __Interrupt Channel:__ a this, interrupt channel to transfer the `device_intr_srq` remote procedure call from the device to the controller (optional for client). The interrupt channel is used by the network instrument server to deliver service requests to the network instrument client. This effectively reverses the role of client and server. The network instrument server acts as an RPC client, making a remote procedure request of the network instrument client, acting as an RPC server.

These three channels correspond to three RPC clients/servers.

|VXI-11         | C# Program      | ID
|---------------|-----------------|-------
|DEVICE_CORE    |DeviceCore       |0x0607AF
|DEVICE_ASYNC   |DeviceAsync      |0x0607B0 
|DEVICE_INTR    |DeviceInterrupt  |0x0607B1

#### Table 2 VXI-11 RPC Programs


## Device Core Program implementation

|Response           | Message          |Parameters            | #
|-------------------|------------------|----------------------|---
|Create_LinkResp    |create_link       |Create_LinkParms      | 10
|Device_WriteResp   |device_write      |Device_WriteParms     | 11
|Device_ReadResp    |device_read       |Device_ReadParms      | 12
|Device_ReadStbResp |device_readstb    |Device_GenericParms   | 13
|Device_Error       |device_trigger    |Device_GenericParms   | 14
|Device_Error       |device_clear      |Device_GenericParms   | 15
|Device_Error       |device_remote     |Device_GenericParms   | 16
|Device_Error       |device_local      |Device_GenericParms   | 17
|Device_Error       |device_lock       |Device_LockParms      | 18
|Device_Error       |device_unlock     |Device_Link           | 19
|Device_Error       |device_enable_srq |Device_EnableSrqParms | 20
|Device_DocmdResp   |device_docmd      |Device_DocmdParms     | 22
|Device_Error       |destroy_link      |Device_Link           | 23
|Device_Error       |create_intr_chan  |Device_RemoteFunc     | 25
|Device_Error       |destroy_intr_chan |void                  | 26

#### Table 3 Device Core Program Implementation

## Device Async Program implementation

|Response           | Message          |Parameters       | #
|-------------------|------------------|-----------------|---
|Device_Error       |create_link       |Device_Link      | 1

#### Table 4 Device Core Program Implementation

## Device Interrupt Program implementation

|Response           | Message          |Parameters       | #
|-------------------|------------------|-----------------|---
|void               |device_intr_srq   |Device_SrqParms  | 30

#### Table 5 Device Interrupt Program Implementation

|VXI-11 Name           | C# Name 
|----------------------|--------------
|Create_LinkParms      | CreateLinkParms      
|Device_DocmdParms     | DeviceDoCmdParms     
|Device_DocmdResp      | DeviceDoCmdResp     
|Device_EnableSrqParms | DeviceEnableSrqParms
|Device_GenericParms   | DeviceGenericParms   
|Device_Link           | DeviceLink           
|Create_LinkResp       | CreateLinkResp      
|Device_LockParms      | DeviceLockParms     
|Device_Error          | DeviceError         
|Device_ReadParms      | DeviceReadParms      
|Device_ReadResp       | DeviceReadResp      
|Device_ReadStbResp    | DeviceReadStbResp   
|Device_RemoteFunc     | DeviceRemoteFunc     
|Device_WriteParms     | DeviceWriteParms     
|Device_WriteResp      | DeviceWriteResp     
|void                  |  

#### Table 4 VXI-11 and C# Nomenclature

## Core and Abort Channel Establishment Sequence
Implementation details may vary from one
operating system to another:

1. Create RPC server (abort/core)- create listen socket upon which connection requests will be accepted and set up any local data structures required to track the RPC server, typically done by a
svctcp_create. 
2. Register core channel with port mapper - register the program number and version number with the local port mapper, typically associated with the svc_register, which also sets up local data structures to dispatch requests.
3. create RPC client (core/abort)- temporarily connect to the port mapper on the server to find the port for the program number and version being used by the network instrument protocol. After determining the port number, create the core channel by connecting to that port. Set up any local data structures necessary to track the RPC client. This step is typically done by a clnttcp_create.
4. create_link requests and replies - These steps represent sending network instrument protocol create_link requests and replies. 

After the first `create_link`, the network instrument client may create an RPC client for the abort channel, but no additional client creations are necessary after subsequent create_links. These connections may be torn down by the network instrument client once all links have been closed with destroy_link. The whole sequence could then start over.

## Interrupt Channel Establishment Sequence
The interrupt is established as follows:

| Network instrument client (controller) | Network instrument server (device)
|----------------------------------------|:----------------------------------
| Create RPC server (interrupt channel)  |
| create_intr_chan                       |
|                                        | create RPC client (interrupt channel)
|                                        | reply to create_intr_chan

#### Table 5. Interrupt Channel Establishment Sequence

### Note
* A `create_intr_chan` request received when an interrupt channel already exists is ignored; the
existing interrupt channel is used such that there is only one interrupt channel used by all links on that network instrument connection.
* If the network instrument client issues `destroy_intr_chan`, then the network instrument server destroys the RPC client to tear down the interrupt channel.
* If the network instrument client never calls `destroy_intr_chan`, the interrupt channel is closed by the network instrument server when the core channel is closed by the network instrument client.

### INTERRUPT LOGIC
The interrupt mechanism allows the device to send a notification call to the controller (effectively switching the roles of RPC client and RPC server). One way a controller could implement the interrupt mechanism is to register a handler for the interrupt, inform the current device to enable the interrupt, and then service the interrupt when it occurs. 

| client               | Network instrument server (device)
|---------------------:|:----------------------------------
| create_intr_chan =>  |
|                      | <= acknowledge
| device_enable_srq => | 
|                      | <= acknowledge
| device_write =>      | <i>SRQ in the middle of another call</i>:
|                      | <= device_intr_srq
|                      | <= acknowledge to write
| device_write =>      | <i>SRQ after another call</i>:
|                      | <= acknowledge to write
|                      | <= device_intr_srq

#### Table 6 Possible sequences of interrupt channel creation and usage.

### Note
* Network instrument clients can implement interrupts by using either a separate interrupt process, threads, or by emulating threads using a signal handling routine that is invoked on incoming messages to the interrupt port.
* The network instrument server MAY issue interrupts in the middle of an active call. In general, this implementation gives more timely responses, and can be easier than delaying the interrupt until an in-progress action has finished.
* The device_intr_srq RPC is implemented as a one-way RPC. This means that the network instrument server does not expect a response from the network instrument client. This is necessary to avoid deadlock situations in a single-threaded environment where if a response were expected to an interrupt both the network instrument client and network instrument server could be waiting for a response from the other, with neither proceeding. 
* The `create_intr_chan` RPC is used to identify the host or port that can service the interrupt. The `device_enable_srq` RPC is used to enable or disable an interrupt. The `destroy_intr_chan` RPC is used to close the interrupt channel.
* The `device_enable_srq` RPC contains a handle parameter. The same data contained in handle is passed back in the handle parameter of the `device_intr_srq` RPC. Since the same data is passed back, the network instrument client can identify the link associated with the `device_intr_srq`.
* The network instrument protocol recognizes one type of interrupt, service request. Note that the return type to the interrupt RPC is void, denoting a one-way RPC.

## Locking
A single device may be accessed by multiple controllers over separate links. For these situations the network instrument server supports locking access to a link, which guarantees exclusive access to the device associated with that link to that link only. If a controller expects to have exclusive access to a device, it must have the lock. When no link has the lock, multiple controllers may be sending data and generally manipulating the state of the device. Under such circumstances, the behavior of the device is unpredictable.

The first call to `device_lock` for an unlocked device acquires the lock. Subsequent calls to `device_lock` for the same device return an error. `device_unlock` unlocks the device if this link has the lock, otherwise `device_unlock` returns an error.

### Time-outs
Many of the remote procedures are passed timeout values. Values may be specified for I/O operations and locks. 

The network instrument server allows at least `io_timeout` milliseconds for an I/O operation to
complete. The time it takes for the I/O operation to complete does not include any time spent waiting for the lock.

An `io_timeout` of zero may be interpreted by the network instrument server to mean that the associated I/O operation should not block.

If the device is locked by another link and the `lock_timeout` is non-zero, the network instrument server allows at least `lock_timeout` milliseconds for a lock to be released. If the device is locked by another link and the `lock_timeout` is equal to zero, the network instrument server does not wait for a lock to be released, but returns an error immediately.

A network instrument client shall provide a client side (local) timeout mechanism which is used in the event that the network instrument server does not respond in the specified amount of time. This timeout mechanism is typically provided by the RPC subsystem. How this timeout value is set and what values it may take depend on aspects of the RPC subsystem beyond the scope of this specification.

The RPC client side (local) timeout value should be set to a value greater than the sum of the `io_timeout` and `lock_timeout` values passed to the network instrument server. If the RPC client timeout is too short, the client may timeout and stop listening for a reply prior to the network instrument server successfully completing the requested operation and replying.

A reply sent by the network instrument server after the network instrument client is no longer listening for it should be discarded by the network instrument client when it next sends or receives a network instrument message (and this is typically handled by the RPC subsystem, if such a subsystem is used by a particular implementation).

## Dropped or Broken Connections
When the core channel is reset or closed (as defined by TCP), the network instrument server recognizes this condition and releases all resources associated with all links which were active on that network instrument connection (as if a `destroy_link` was executed for each open link on that connection). Resources to be released include locks, the abort channel, and the interrupt channel.

The network instrument server is also configured to use an implementation defined mechanism to discover if the network is down or if a network instrument client has crashed, and perform the same cleanup actions.

## Security Control
The RPC interface defined by this specification provides no services to authenticate a user for security. A controller must merely know a network instrument host's IP address to access all of its functions. Security control is beyond the scope of this specification, though a network instrument host may support security control methods.

## Concurrent Operations
The protocol defined by this specification does not preclude the network instrument server or network instrument client from attempting to perform operations concurrently. However, due to the nature of most commercially available RPC software packages which may be used to implement the protocol defined by this specification, it is expected that a typical network instrument host's implementation will serialize the RPCs.

If a network instrument client's implementation does allow multiple RPCs to be outstanding, then the network instrument server on the receiving end may have multiple RPC requests queued in its TCP input buffer. These RPCs are pending, but not in progress, and therefore are unaffected by `device_abort`.

## TCP/IP-IEEE 488.1 Device String Format
The routing of messages from the LAN to the appropriate IEEE 488.1 device takes place via the `create_link` RPC. This RPC is used to create a network instrument link. The created link is associated with a particular IEEE 488.1 device or interface via the device parameter. This parameter is a character string which is parsed by the TCP/IP-IEEE 488.1 Interface Device to determine the IEEE 488.1 device or interface associated with the link.

The TCP/IP-IEEE 488.1 Interface Device supports a device string of the following format:
`<intf_name>[,<primary_addr>[,<secondary_addr>]]`

where:
* `<intf_name>` A name corresponding to a single IEEE 488.1 interface. This name shall uniquely identify the interface on the TCP/IP-IEEE 488.1 Interface Device.
* `<primary_addr>` The primary address of a IEEE 488.1 device on the IEEE 488.1 interface (optional).
* `<secondary_addr>` The secondary address of a IEEE 488.1 device on the IEEE 488.1 interface (optional).

The TCP/IP-IEEE 488.1 Interface Device recognizes an `<intf_name>` of `gpib0` as the first or only IEEE 488.1 interface within the TCP/IP-IEEE 488.1 Interface Device. Additional interfaces are  identified by `gpibN` where N is a non-negative integer assigned sequentially starting at one.

A device string which contains just `<intf_name>` is interpreted by the TCP/IP-IEEE 488.1 Interface Device as a link to the interface.

A device string which contains `<intf_name>` and an address is interpreted by the TCP/IP-IEEE 488.1 Interface Device as a link to a device at that address on the interface.

Some examples of valid device strings are:
* `gpib0` link is associated with the interface;
* `gpib0,12` link is associated with a device at primary address 12;
* `gpib0,12,5` link is associated with a device at primary address 12 and secondary address 5

A network instrument server could potentially support links using protocols described in related documents. See A.6. It might be able to convert network instrument protocols to VXIbus operations as well as IEEE 488.1 operations. In such a case, the allowed contents of the device string depend on the total capability of the network instrument server.

A controller should use links to the interface with great care. They must be used with `device_docmd`. They should only be used with `device_write`, `device_read`, and `device_trigger`, when a device needs a special addressing sequence. Some older devices connected to an IEEE 488.1 bus do not properly implement the state machines for the `Talker` and `Listener` functions and require a specific order of `unlisten`, `my talk` or `listen` address, and controller's listen or talk address. Where this required order is different from the one specified in this standard, a `device_docmd` must be used to establish the addressing followed by an RPC that does no addressing. In other situations, links to the device should be used.

## TCP/IP-IEEE 488.2 Device String Format
The contents of the device string determines which Message Exchange Interface is associated with a link. This structure allows multiple instruments to use the same IP address.

A TCP/IP-IEEE 488.2 Instrument Interface supports a device string of the following format:
`<inst_name>` where:
* `<inst_name>` A name corresponding to a single instrument.

The TCP/IP-IEEE 488.2 Instrument Interface recognizes an `<inst_name>` of `"inst0"` as the first or only instrument within the TCP/IP-IEEE 488.2 Instrument Interface. Additional instruments are identified by `"instN"` where N is a non-negative integer assigned sequentially beginning at one.

One and only one Message Exchange Interface and Status Reporting exists for each `instN` supported in a TCP/IP IEEE 488.2 Instrument Interface even when connections to multiple controllers are active.

## Interface Communication 

The `device_docmd` RPC is a general purpose RPC which provides interface specific operations not
covered by the other defined RPCs. Table 7 describes the allowed commands.

| Name        | `cmd`    | len   | `datasize`
|-------------|----------|-------|-----------
|Send Command | 0x020000 | 0-128 | 1
|Bus Status   | 0x020001 | 2     | 2
|ATN Control  | 0x020002 | 2     | 2
|REN Control  | 0x020003 | 2     | 2
|Pass Control | 0x020004 | 4     | 4
|Bus Address  | 0x02000A | 4     | 4
|IFC Control  | 0x020010 | 0     | X

## References

* VMEbus Extensions for Instrumentation: TCP/IP Instrument Protocol Specification VXI-11, Revision 1.0, July 17, 1995, [VXI Bus Specification].
* Power Programming with RPC, John Bloomer, O’Reilly & Associates, Inc., 1999.
* VXI-11 RPC Programming Guide for the 8065: An Introduction to RPC Programming, Application Note AB80-3, [ICS Electronics].
* SRQ Handling With a VXI-11 Interrupt Channel On ICS’s Model 8065 Ethernet-To-GPIB Controller, Application Note AB80-4, [ICS Electronics].
* VXI-11 vs LXI, Application Note AB80-10, [ICS Electronics].
* VXI-11 tutorial and RPC Programming Guide, Application Note AB80-11, [ICS Electronics].
* Coding for Reverse Channel Service Requests from VXI-11 compatible instruments, Application Note AB80-13, [ICS Electronics].
* RPC: Remote Procedure Call Protocol Specification, Request for Comments 1057, Sun Microsystems, DDN Network Information Center, SRI International, June, 1988.

[ICS Electronics]: https://www.icselect.com/
[LXI]: https://www.lxistandard.org/About/AboutLXI.aspx
[VXI Consortium]: http://www.vxibus.org
[VXI Bus Specification]: https://vxibus.org/specifications.html
[RPC]: https://en.wikipedia.org/wiki/Sun_RPC
