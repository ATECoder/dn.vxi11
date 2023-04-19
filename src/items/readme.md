# About

[ISR's VXI-11] is a control and communication library for LXI-based instruments. 

[ISR's VXI-11] is a partial C# implementation of the [VXI Bus Specification].

## History

[ISR's VXI-11] is a is a fork of [VXI11.CSharp].

[ISR's VXI-11] uses [ISR's ONC RPC], which is a C# implementation of the [Sun RPC] ported from the [Java ONC RPC] implementation termed Remote Tea.

[ISR's ONC RPC] is a fork of [GB1.RemoteTea.Net], which was forked from [Wes Day's RemoteTea.Net], which is a fork of [Jay Walter's SourceForge repository], which is a port of [Java ONC RPC] as documented in [org.acplt.oncrpc package].

[ISR's ONC RPC] uses [ISR's XDR], which is a C# implementation of the [XDR: External Data Representation Standard (May 2006)] as implemented in [Java ONC RPC] implementation called Remote Tea. [ISR's XDR] was split off from [GB1.RemoteTea.Net].

## Standards

* [XDR: External Data Representation Standard (May 2006)]
* Open Network Computing Remote Procedure Call (ONC RPC)
	* [RPC: Remote Procedure Call Protocol Specification Version 2 (May 2009)]
	* [Binding Protocols for ONC RPC Version 2 (August 1995)]
* [VXI Bus Specification]

## Terminology

In implementing the [VXI Bus Specification] [ISR's VXI-11] adopted the following terms to describe the software elements that are often associated with hardware components:

### _VXI-11 Instrument Client_ or _VXI-Client_ or _Client_ or _Virtual Instrument_ or _VI_

The _VXI-11 Instrument Client_ or _Client_ or _VI_ is a purely software entity that is capable of controlling an LXI piece of hardware using, in this case, the VXI-11 protocols. 

A VXI-11 Client typically implements a VXI-11 Core Channel ONC/RPC client, an Abort Channel ONC/RPC client and an Interrupt Channel ONC/RPC server.

The [ISR's VXI-11 Client] project includes an implementation of a _VI_ that is used for unit and console application testing. 

### _VXI-11 Instrument_ or _Instrument_

The _VXI-11 Instrument_ or _Instrument_ is a physical piece of hardware that implements the VXI-11 Server protocol and, thus, can be controlled by a _VI_. 

A VXI-11 Client typically implements a VXI-11 Core Channel ONC/RPC server, an Abort Channel ONC/RPC server and an Interrupt Channel ONC/RPC client.

### _VXI-11 Interface_

The _VXI-11 Interface_ is a physical piece of hardware that implements a VXI-11 client protocol for interface commands such as those that control a GPIB interface. 

### _VXI-11 Device_

A _VXI-11 Device_ is a software implementation of a _VXI-11 Instrument_ and _VXI-11 Interface_.

The [ISR's VXI-11 Server] project includes an implementation of a _VXI-11 Device_ for testing purposes.

## How to Use

The [ISR's VXI-11] package implements the core [VXI Bus Specification].

The [ISR's VXI-11 Client] and [ISR's VXI-11 Server] VXI-11 packages include a set of fully functional low level classes that implement VXI-11 clients, servers, devices and instruments. These classes are then used by the [ISR's VXI-11 Apps] projects.

Typically, the VXI-11 Instrument Client and Interface Client classes would be inherited for creating classes such as the Visa message-based session. 

On the server side the VXI-11 Instrument and, possibly, the VXI-11 Device classes would be inherited for creating the specific behavior desired from an instrument.

## Testers

### [ISR's VXI-11 LXI Discover]

This application attempts to discover all the LXI devices on the local network.

### [ISR's VXI-11 Client Tester]

This application tests a most basic IEEE488.2 instrument.

### [ISR's VXI-11 Instrument Client Tester]

This application tests a physical IEEE488.2 instrument as implemented in the [ISR's VXI-11 Server].

### [ISR's VXI-11 Single Client Server Tester]

This application tests a mock IEEE488.2 instrument as implemented in the [ISR's VXI-11 Server].

## Unit Tests

### AsyncEventHandlersTests

Test exception handling in async methods. 

### CicrularListTests

Tests the circular list, which is used for keeping a log of instrument and interface commands. 

### DeviceNameParserTests

Tests parsing the standard VXI-11 device name string. 

### IdentityParserTests

Tests parsing the standard VXI-11 instrument identity string.

### Vxi11ClientTests

Tests the VXI-11 client for generating a unique client ID on new instances of the client.

### Vxi11DeviceTests

Tests the implementation a VXI-11 device using methods for emulating socket communication thus bypassing the VXI-11 client server communication.

### Vxi11DiscovererBoardcastTests

Tests discovery of registered VXI-11 servers on the local network using broadcasting.

### Vxi11DiscovererLoopbackTests

Tests discovery of VXI-11 servers on the local network.

### Vxi11DiscovererTests

Tests discovery of registered servers from a list of known instrument addresses.

### Vxi11DualClientServerTests

Tests querying a VXI-11 server from two clients.

### Vxi11SingleClientServerTests

Tests querying a VXI-11 server from a single client.

### Vxi11SupportTests

#### Unique Client ID

Tests generating a unique client identities by an instance of the VXI-11 server.

#### IP Address Conversion

Tests converting between representations of IPv4 IP addresses.

#### Enums

Tests parsing flag enumerations from whole numbers.

## Departures from [VXI11.CSharp]

* The XDR and ONC/RPC classes were removed in favor of using [ISR's XDR] and [ISR's ONC RPC] packages;
* The base namespace was changed to cc.isr;
* The VXI11 namespace was changed to cc.isr.VXI11;
* The casing of namespace suffices, such as server and web, were changed to Pascal.
* Pascal case naming convention is used for classes, methods and properties;
* Interface names are prefixed with 'I';
* Base class names are suffixes with Base;
* the xdrAble interface was renamed to IXdrCodec;
* The xdr prefixes were removed from the codec methods;
* Uppercase constant names were converted to Pascal casing while retaining the original constant names in the code documentation;
* The namespace of the ONC/REPC client classes was changed fro cc.isr.cc.ONC.RPC.Clients;
* Getters and setters, such as Get and Set Character Encoding, where changed to properties where possible.
* Static constant classes where converted to Enum constructs.
* Added connect timeout to the constructors of the VXI-11 clients. 
* `Device_Flags` and `Device_ErrorCode` were replaced by `int`.

## Feedback

[ISR's VXI-11] is released as open source under the MIT license.
Bug reports and contributions are welcome at the [ISR's VXI-11] repository.

[ISR's VXI-11]: https://github.com/ATECoder/dn.vxi11
[ISR's VXI-11 Client]: https://github.com/ATECoder/dn.vxi11/src/vxi/vxi/client
[ISR's VXI-11 Apps]: https://github.com/ATECoder/dn.vxi11/src/vxi/apps
[ISR's VXI-11 Server]: https://github.com/ATECoder/dn.vxi11/src/vxi/vxi/server
[ISR's ONC RPC]: https://github.com/ATECoder/dn.onc.rpc
[ISR's XDR]: https://github.com/ATECoder/dn.xdr
[ISR's VXI-11 LXI Discover]: https://github.com/ATECoder/dn.vxi11/src/vxi/apps/lxi.discover/
[ISR's VXI-11 Client Tester]: https://github.com/ATECoder/dn.vxi11/src/vxi/apps/vxi11.client.tester/
[ISR's VXI-11 Instrument Client Tester]: https://github.com/ATECoder/dn.vxi11/src/vxi/apps/vxi11.instrument.client.tester/
[ISR's VXI-11 Single Client Server Tester]: https://github.com/ATECoder/dn.vxi11/src/vxi/apps/vxi11.single.client.server.tester/

[XDR: External Data Representation Standard (May 2006)]: http://tools.ietf.org/html/rfc4506
[RPC: Remote Procedure Call Protocol Specification Version 2 (May 2009)]: http://tools.ietf.org/html/rfc5531
[Binding Protocols for ONC RPC Version 2 (August 1995)]: http://tools.ietf.org/html/rfc1833
[Sun RPC]: https://en.wikipedia.org/wiki/Sun_RPC

[Jay Walter's SourceForge repository]: https://sourceforge.net/p/remoteteanet
[Wes Day's RemoteTea.Net]: https://github.com/wespday/RemoteTea.Net
[GB1.RemoteTea.Net]: https://github.com/galenbancroft/RemoteTea.Net
[org.acplt.oncrpc package]: https://people.eecs.berkeley.edu/~jonah/javadoc/org/acplt/oncrpc/package-summary.html
[Java ONC RPC]: https://github.com/remotetea/remotetea/tree/master/src/tests/org/acplt/oncrpc
[VXI11.CSharp]: https://github.com/Xanliang/VXI11.CSharp 
[VXI Bus Specification]: https://vxibus.org/specifications.html
