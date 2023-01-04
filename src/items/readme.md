# About

[ISR's VXI11] is a control and communication library for LXI-based instruments. 

## History

[ISR's VXI11] is a is a fork of [VXI11.CSharp], which uses [GB1.RemoteTea.Net], which is a pure C# implementation of ONC/RPC. 
[RemoteTea.Net] is a port of the pure Java RemoteTea package. 

## Standards

* [XDR: External Data Representation Standard (May 2006)]
* Open Network Computing Remote Procedure Call (ONC RPC)
	* [RPC: Remote Procedure Call Protocol Specification Version 2 (May 2009)]
	* [Binding Protocols for ONC RPC Version 2 (August 1995)]
* [VXI Bus Specification]

## How to Use

For information on using [ISR's VXI11] see the repository [ISR's VXI11 IEEE488] projects.

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

## Feedback

[ISR's VXI11] is released as open source under the MIT license.
Bug reports and contributions are welcome at the [ISR's VXI11] repository.

[ISR's VXI11]: https://github.com/ATECoder/dn.vxi11
[ISR's VXI11 IEEE488]: https://github.com/ATECoder/dn.vxi11/src/vxi/ieee488
[ISR's ONC RPC]: https://github.com/ATECoder/dn.onc.rpc
[ISR's XDR]: https://github.com/ATECoder/dn.xdr


[XDR: External Data Representation Standard (May 2006)]: http://tools.ietf.org/html/rfc4506
[RPC: Remote Procedure Call Protocol Specification Version 2 (May 2009)]: http://tools.ietf.org/html/rfc5531
[Binding Protocols for ONC RPC Version 2 (August 1995)]: http://tools.ietf.org/html/rfc1833

[Jay Walter's SourceForge repository]: https://sourceforge.net/p/remoteteanet
[Wes Day's GitHub repository]: https://github.com/wespday/RemoteTea.Net
[GB1.RemoteTea.Net]: https://github.com/galenbancroft/RemoteTea.Net
[org.acplt.oncrpc package]: https://people.eecs.berkeley.edu/~jonah/javadoc/org/acplt/oncrpc/package-summary.html
[Java ONC RPC]: https://github.com/remotetea/remotetea/tree/master/src/tests/org/acplt/oncrpc
[VXI11.CSharp]: https://github.com/Xanliang/VXI11.CSharp 
[VXI Bus Specification]: https://vxibus.org/specifications.html


