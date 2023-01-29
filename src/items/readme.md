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

## How to Use

For information on using [ISR's VXI-11] see the repository [ISR's VXI-11 IEEE488] projects.

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
* The namespace of the ONC/REPC client classes was changed fro isr.cc.ONC.RPC.Clients;
* Getters and setters, such as Get and Set Character Encoding, where changed to properties where possible.
* Static constant classes where converted to Enum constructs.
* Added connect timeout to the constructors of the VXI-11 clients. 
* `Device_Flags` and `Device_ErrorCode` were replaced by `int`.

## Feedback

[ISR's VXI-11] is released as open source under the MIT license.
Bug reports and contributions are welcome at the [ISR's VXI-11] repository.

[ISR's VXI-11]: https://github.com/ATECoder/dn.vxi11
[ISR's VXI-11 IEEE488]: https://github.com/ATECoder/dn.vxi11/src/vxi/ieee488
[ISR's ONC RPC]: https://github.com/ATECoder/dn.onc.rpc
[ISR's XDR]: https://github.com/ATECoder/dn.xdr


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

