# TCP/IP Instrument Protocol (VXI-11)

A control and communication library for LXI-based instruments. 

- [TCP/IP Instrument Protocol (VXI-11)](#VXI11-Protocol)
- [Network Instrument Protocol](#Network-Instrument-Protocol)
- [References](#References)

<a name="VXI11-Protocol"></a>
## TCP/IP Instrument Protocol (VXI-11)

The VXI-11 protocol is an [RPC]-based communication protocol primarily designed for connecting instruments (such as oscilloscopes, multimeters, spectrum analyzers etc.) to controllers (e.g., PCs).

Instruments may support this protocol directly (e.g., as part of implementing the more recent [LXI] interface), or may be connected by way of an adapter (gateway) that attaches to a dedicated bus (such as GPIB).

The VXI-11 specification was published in 1995 by the VXIbus Consortium, Inc.

### Protocol dependencies

* ONC-RPC: VXI-11 consists of 3 separate RPC programs with the numeric identifiers 0x0607AF (DEVICE_CORE), 0x0607B0 (DEVICE_ASYNC) and 0x0607B1 (DEVICE_INTR).
* TCP: VXI-11 uses TCP as its transport protocol.


## CONNECTION MODEL
This section defines the connection model of the network instrument protocol, as well as the relationship between controllers, devices, network instrument clients, and network instrument servers. The term controller, as used in this specification, typically refers to the RPC client, while the term device typically refers to the RPC server. The only exception is when the roles are reversed for interrupts, which will be described further later in the specification.
The network instrument protocol uses up to three channels between the controller and the device for passing network instrument messages. A network instrument connection is this set of channels:
* __Core Channel:__ Used to transfer all requests except the device_abort RPC and the device_intr_srq
RPC.
* __Abort Channel:__ Used to transfer the device_abort RPC (optional for client).
* __Interrupt Channel:__ Used to transfer the device_intr_srq RPC from the device to the controller
(optional for client).

These three channels correspond to three RPC clients/servers.



<a name="Network-Instrument-Protocol"></a>
## Network Instrument Protocol
The network instrument protocol uses the ONC remote procedure call (RPC) model. Conceptually, this model allows one application (typically called the client) to call procedures in a remote application (typically called the server) as if the remote procedures were local. This specification uses ONC/RPC for defining the network instrument messages which are passed over the network, but does not require that these RPCs be provided as the application interface. The ONC/RPC interface may, however, be used by a device designer as a matter of convenience.

The client identifies the remote procedure, or message, by a unique number. This number is then encoded into a message along with the procedure's argument types and values. The message is sent to the server machine where it is decoded by the server. The server uses the unique identifier to dispatch the request.
When the request is completed, the return values are encoded into a message which is sent back to the
client machine.
The interface definition ( see Appendix I, "Network instrument RPCL") gives the function prototypes as
well as the unique identifiers for the procedures. For ONC RPC, the unique identifier is a combination of
a program number (also known as an interface id), a procedure number, and a version number.
Table B.1 outlines the 17 messages that define the network instrument protocol. These messages are
expected to be supported by all devices that claim to be network instrument compliant. Most of these
messages will be familiar to those who have worked with IEEE 488 devices.

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

The messages are sent over three different channels: a core synchronous command channel, a secondary abort channel (for aborting core channel operations), and an interrupt channel.

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

