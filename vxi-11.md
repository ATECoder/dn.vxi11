# TCP/IP Instrument Protocol (VXI-11)

A control and communication library for LXI-based instruments. 

- [TCP/IP Instrument Protocol (VXI-11)](#VXI11-Protocol)
- [RPC](#RPC)
- [References(#References)

<a name="VXI11-Protocol"></a>
## TCP/IP Instrument Protocol (VXI-11)

The VXI-11 protocol is an [RPC]-based communication protocol primarily designed for connecting instruments (such as oscilloscopes, multimeters, spectrum analyzers etc.) to controllers (e.g., PCs).

Instruments may support this protocol directly (e.g., as part of implementing the more recent [LXI] interface), or may be connected by way of an adapter (gateway) that attaches to a dedicated bus (such as GPIB).

The VXI-11 specification was published in 1995 by the VXIbus Consortium, Inc.

### Protocol dependencies

* ONC-RPC: VXI-11 consists of 3 separate RPC programs with the numeric identifiers 0x0607AF (DEVICE_CORE), 0x0607B0 (DEVICE_ASYNC) and 0x0607B1 (DEVICE_INTR).
* TCP: VXI-11 uses TCP as its transport protocol.

## RPC

RPC is a library of procedures. The procedures allow one process (the client process) to direct another process (the server process) to Runs procedure calls as if the client process had Runs the calls in its own address space. Because the client and the server are two separate processes, they need not exist on the same physical system (although they can).

Because the server and client processes can reside on two different physical systems which may have completely different architectures, RPC must address the possibility that the two systems might not represent data in the same way. For this reason, RPC uses data types defined by the [eXternal Data Representation (XDR)](#External-Data-Representation) protocol.

The first popular implementation of RPC on Unix was Sun's RPC (now called [ONC-RPC] Open Network Computing (ONC) Remote Procedure Call (RPC)), used as the basis for Network File System (NFS).

### External Data Representation 
External Data Representation (XDR) is a standard data serialization format, for uses such as computer network protocols. It allows data to be transferred between different kinds of computer systems. Converting from the local representation to XDR is called encoding. Converting from XDR to the local representation is called decoding. XDR is implemented as a software library of functions which is portable between different operating systems and is also independent of the transport layer.

XDR uses a base unit of 4 bytes, serialized in big-endian order; smaller data types still occupy four bytes each after encoding. Variable-length types such as string and opaque are padded to a total divisible by four bytes. Floating-point numbers are represented in IEEE 754 format.

### Message passing
RPC is a request–response protocol. An RPC is initiated by the client, which sends a request message to a known remote server to execute a specified procedure with supplied parameters. The remote server sends a response to the client, and the application continues its process. While the server is processing the call, the client is blocked (it waits until the server has finished processing before resuming execution), unless the client sends an asynchronous request to the server, such as an XMLHttpRequest. There are many variations and subtleties in various implementations, resulting in a variety of different (incompatible) RPC protocols.

An important difference between remote procedure calls and local calls is that remote calls can fail because of unpredictable network problems. Also, callers generally must deal with such failures without knowing whether the remote procedure was actually invoked. Idempotent procedures (those that have no additional effects if called more than once) are easily handled, but enough difficulties remain that code to call remote procedures is often confined to carefully written low-level subsystems.

#### Sequence of events
1. The client calls the client [stub](#stub). The call is a local procedure call, with parameters pushed on to the stack in the normal way.
2. The [client stub](#client-stub) packs the parameters into a message and makes a system call to send the message. Packing the parameters is called [marshaling](#marshaling).
3. The client's local operating system sends the message from the client machine to the server machine.
4. The local operating system on the server machine passes the incoming packets to the server stub.
5. The server stub unpacks the parameters from the message. Unpacking the parameters is called unmarshaling.
6. Finally, the server stub calls the server procedure. The reply traces the same steps in the reverse direction.

### RPCGEN Protocol Compiler
RPCGEN is an interface generator pre-compiler for Sun Microsystems ONC RPC. It uses an interface definition file to create client and server stubs in C.

#### RPC Language
RPCGEN creates stubs based on information contained within an IDL file. This file is written in a language called RPCL - remote procedure call language. This language closely mimics C in style, and is designed purely for defining specification to be used for ONC RPC.

## ONC-RPC
Open Network Computing (ONC) Remote Procedure Call (RPC) is the ONC variant of RPC. 

ONC-RPC clients will first use the [Portmap] service to map a well known program number (e.g. 100020 for KLM) into the current port address information at the server (e.g. servers KLM service is available at TCP port 1234) and then contact the actual required service at that port.

### Protocol dependencies
* UDP: ONC RPC can use UDP as its transport protocol; many RPC protocols are usually Runs on top of UDP.
* TCP: ONC RPC can use TCP as its transport protocol; some protocols, such as NFS, are, in recent times, more often being Runs over TCP.

Most ONC RPC services have no fixed port numbers assigned to them. The only exceptions are [Portmap] and NFS (Network File System).

## Definitions

### Stub
A stub is a piece of code that converts parameters passed between client and server during a remote procedure call (RPC).

### Client Stub
The client side object participating in distributed object communication is known as a stub or proxy, and is an example of a proxy object.

### Marshaling
In computer science, marshaling is the process of transforming the memory representation of an object into a data format suitable for storage or transmission. It is typically used when data must be moved between different parts of a computer program or from one program to another.

It simplifies complex communications, because it uses composite objects in order to communicate instead of primitive objects. The inverse process of marshaling is called _unmarshaling_, similar to deserialization. An unmarshaling interface takes a serialized object and transforms it into an internal data structure.

### Portmap
The [port mapper](https://en.wikipedia.org/wiki/Portmap) (rpc.portmap or just portmap, or rpcbind) is an Open Network Computing Remote Procedure Call (ONC RPC) service that runs on network nodes that provide other ONC RPC services.

Version 2 of the port mapper protocol maps ONC RPC program number/version number pairs to the network port number for that version of that program. When an ONC RPC server is started, it will tell the port mapper, for each particular program number/version number pair it implements for a particular transport protocol (TCP or UDP), what port number it is using for that particular program number/version number pair on that transport protocol. Clients wishing to make an ONC RPC call to a particular version of a particular ONC RPC service must first contact the port mapper on the server machine to determine the actual TCP or UDP port to use.

Versions 3 and 4 of the protocol, called the rpcbind protocol, map a program number/version number pair, and an indicator that specifies a transport protocol, to a transport-layer endpoint address for that program number/version number pair on that transport protocol.

The port mapper service always uses TCP or UDP port 111; a fixed port is required for it, as a client would not be able to get the port number for the port mapper service from the port mapper itself.

The port mapper must be started before any other RPC servers are started.

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

