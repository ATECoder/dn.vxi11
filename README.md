### VXI-11

A control and communication library for LXI-based instruments. 

- [Description](#Description)
- [Issues](#Issues)
- [Supported .Net Releases](#Supported-.Net-Releases)
* [Runtime Pre-Requisites](#Runtime-Pre-Requisites)
* [Known Issues](#Known-Issues)
* Project README files:
  * [NI.FindResources](/src/isr/ni/find.resources/readme.md) 
  * [NI.ServiceRequest](/src/isr/ni/service.request/readme.md) 
  * [NI.SimpleAsynchronousReadWrite](/src/isr/ni/simple.asynchronous.read.write/readme.md) 
  * [NI.SimpleReadWrite](/src/isr/ni/simple.read.write/readme.md) 
  * [cc.isr.Visa](/src/isr/visa/readme.md) 
  * [cc.isr.Visa.WinControls](/src/isr/visa.win.controls/readme.md) 
  * [Ivi.Visa](/src/ivi/ivi.visa/readme.md)
  * [Keysight.Visa](/src/ivi/keysight.visa/readme.md)
  * [NI.Visa](/src/ivi/ni.visa/readme.md)
* [Attributions](Attributions.md)
* [Change Log](./CHANGELOG.md)
* [Cloning](Cloning.md)
* [Code of Conduct](code_of_conduct.md)
* [Contributing](contributing.md)
* [Legal Notices](#legal-notices)
* [License](LICENSE)
* [Open Source](Open-Source.md)
* [Repository Owner](#Repository-Owner)
* [Security](security.md)


#### Description

[ISR's VXI-11] is a partial C# implementation of the [VXI Bus Specification].

#### History

[ISR's VXI-11] is a is a fork of [VXI11.CSharp].

[ISR's VXI-11] uses [ISR's ONC RPC], which is a C# implementation of the [Sun RPC] ported from the [Java ONC RPC] implementation termed Remote Tea.

[ISR's ONC RPC] is a fork of [GB1.RemoteTea.Net], which was forked from [Wes Day's RemoteTea.Net], which is a fork of [Jay Walter's SourceForge repository], which is a port of [Java ONC RPC] as documented in [org.acplt.oncrpc package].

[ISR's ONC RPC] uses [ISR's XDR], which is a C# implementation of the [XDR: External Data Representation Standard (May 2006)] as implemented in [Java ONC RPC] implementation called Remote Tea. [ISR's XDR] was split off from [GB1.RemoteTea.Net].


##### On the agenda
The following items has yet to be implemented:
* Device Clear;
* read Service Request register;
* Enable and register service call backs.

#### Issues

#### Supported .NET Releases


<a name="Repository-Owner"></a>
#### Repository Owner
[ATE Coder]

<a name="Authors"></a>
#### Authors
* [ATE Coder]  

<a name="legal-notices"></a>
#### Legal Notices

Integrated Scientific Resources, Inc., and any contributors grant you a license to the documentation and other content in this repository under the [Creative Commons Attribution 4.0 International Public License], see the [LICENSE](./LICENSE) file, and grant you a license to any code in the repository under the [MIT License], see the [LICENSE-CODE](./LICENSE-CODE) file.

Integrated Scientific Resources, Inc., and/or other Integrated Scientific Resources, Inc., products and services referenced in the documentation may be either trademarks or registered trademarks of Integrated Scientific Resources, Inc., in the United States and/or other countries. The licenses for this project do not grant you rights to use any Integrated Scientific Resources, Inc., names, logos, or trademarks.

Integrated Scientific Resources, Inc., and any contributors reserve all other rights, whether under their respective copyrights, patents, or trademarks, whether by implication, estoppel or otherwise.

[Creative Commons Attribution 4.0 International Public License]:(https://creativecommons.org/licenses/by/4.0/legalcode)
[MIT License]:(https://opensource.org/licenses/MIT)
 
[ATE Coder]: https://www.IntegratedScientificResources.com
[dn.core]: https://www.bitbucket.org/davidhary/dn.core

[IVI Foundation]: https://www.ivifoundation.org
[Microsoft .NET Framework]: https://dotnet.microsoft.com/download

[external repositories]: ExternalReposCommits.csv
[IDE Repository]: https://www.bitbucket.org/davidhary/vs.ide

[ISR's ONC RPC]: https://github.com/ATECoder/dn.onc.rpc
[ISR's XDR]: https://github.com/ATECoder/dn.xdr
[ISR's VXI-11]: https://github.com/ATECoder/dn.vxi11

[LXI]: https://www.lxistandard.org/About/AboutLXI.aspx
[Python VXI-11]: https://github.com/alexforencich/python-vxi11.git 

[VXI Bus Specification]: https://vxibus.org/specifications.html
[Sun RPC]: https://en.wikipedia.org/wiki/Sun_RPC
[XDR: External Data Representation Standard (May 2006)]: http://tools.ietf.org/html/rfc4506

[VXI11.CSharp]: https://github.com/Xanliang/VXI11.CSharp 
[Jay Walter's SourceForge repository]: https://sourceforge.net/p/remoteteanet
[Wes Day's RemoteTea.Net]: https://github.com/wespday/RemoteTea.Net
[GB1.RemoteTea.Net]: https://github.com/galenbancroft/RemoteTea.Net
[org.acplt.oncrpc package]: https://people.eecs.berkeley.edu/~jonah/javadoc/org/acplt/oncrpc/package-summary.html
[Java ONC RPC]: https://github.com/remotetea/remotetea/tree/master/src/tests/org/acplt/oncrpc

[Jay Walter's SourceForge repository]: https://sourceforge.net/p/remoteteanet
[Wes Day's RemoteTea.Net]: https://github.com/wespday/RemoteTea.Net
[GB1.RemoteTea.Net]: https://github.com/galenbancroft/RemoteTea.Net
[org.acplt.oncrpc package]: https://people.eecs.berkeley.edu/~jonah/javadoc/org/acplt/oncrpc/package-summary.html
[Java ONC RPC]: https://github.com/remotetea/remotetea/tree/master/src/tests/org/acplt/oncrpc
[VXI11.CSharp]: https://github.com/Xanliang/VXI11.CSharp 
[VXI Bus Specification]: https://vxibus.org/specifications.html

