### About

[ISR's VXI-11 Server] includes a set of low level classes for implementing instruments and interfaces using the [ISR's VXI-11] implementation of [Sun RPC]. 

[ISR's VXI-11] is a control and communication library for LXI-based instruments. 

[ISR's VXI-11] is a partial C### implementation of the [VXI Bus Specification].

#### History

[ISR's VXI-11] is a is a fork of [VXI11.CSharp].

[ISR's VXI-11] uses [ISR's ONC RPC], which is a C### implementation of the [Sun RPC] ported from the [Java ONC RPC] implementation termed Remote Tea.

[ISR's ONC RPC] is a fork of [GB1.RemoteTea.Net], which was forked from [Wes Day's RemoteTea.Net], which is a fork of [Jay Walter's SourceForge repository], which is a port of [Java ONC RPC] as documented in [org.acplt.oncrpc package].

[ISR's ONC RPC] uses [ISR's XDR], which is a C### implementation of the [XDR: External Data Representation Standard (May 2006)] as implemented in [Java ONC RPC] implementation called Remote Tea. [ISR's XDR] was split off from [GB1.RemoteTea.Net].

#### Standards

* [XDR: External Data Representation Standard (May 2006)]
* Open Network Computing Remote Procedure Call (ONC RPC)
	* [RPC: Remote Procedure Call Protocol Specification Version 2 (May 2009)]
	* [Binding Protocols for ONC RPC Version 2 (August 1995)]
* [VXI Bus Specification]

#### How to Use

For information on using [ISR's VXI-11] see the repository [ISR's VXI-11 Apps] projects.

The [ISR's VXI-11] VXI11 package consists of a set of fully functional low level classes implementing VXI-11 clients, servers, devices and instruments. 

Typically, the VXI-11 Instrument Client and Interface Client classed would be inherited for creating classes such as the Visa message-based session. 

On the server side the VXI-11 Instrument and, possibly, the VXI-11 Device classes would be inherited for creating the specific behavior desired from an instrument.

#### Feedback

[ISR's VXI-11] is released as open source under the MIT license.
Bug reports and contributions are welcome at the [ISR's VXI-11] repository.

[ISR's VXI-11]: https://github.com/ATECoder/dn.vxi11
[ISR's VXI-11 Client]: https://github.com/ATECoder/dn.vxi11/src/vxi/vxi/client
[ISR's VXI-11 Apps]: https://github.com/ATECoder/dn.vxi11/src/vxi/apps
[ISR's VXI-11 Server]: https://github.com/ATECoder/dn.vxi11/src/vxi/vxi/server
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
