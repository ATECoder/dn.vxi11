# VXI-11

A control and communication library for LXI-based instruments. 

- [Description](#Description)
- [Issues](#Issues)
- [Supported .Net Releases](#Supported-.Net-Releases)
- [Source Code](#Source-Code)
  - [Repositories](#Repositories)
  - [Global Configuration Files](#Global-Configuration-Files)
  - [Packages](#Packages)
- [Facilitated By](#FacilitatedBy)
- [Repository Owner](#Repository-Owner)
- [Authors](#Authors)
- [Acknowledgments](#Acknowledgments)
- [Open Source](#Open-Source)
- [Closed Software](#Closed-software)
- [Legal Notices](#Legal-Notices)

## Description

[ISR's VXI11] is a fork of [VXI11.CSharp], which uses [GB1.RemoteTea.Net], which is a pure C# implementation of ONC/RPC. 
[RemoteTea.Net] is a port of the pure Java RemoteTea package. 

### On the agenda
The following items has yet to be implemented:
* Device Clear;
* read Service Request register;
* Enable and register service call backs.

## Issues

## Supported .NET Releases

## Source Code
Clone the repository along with its requisite repositories to their respective relative path.

### Repositories
The repositories listed in [external repositories] are required:
* [IDE Repository] - IDE support files.

```
git clone git@bitbucket.org:davidhary/vs.ide.git
git clone https://github.com/ATECoder/dn.vxi11.git
```

Clone the repositories into the following folders (parents of the .git folder):
```
%vslib%\core\ide
%dnlib%\iot\vxi
```
where %dnlib% and %vslib% are  the root folders of the .NET libraries, e.g., %my%\lib\vs 
and %my%\libraries\vs, respectively, and %my% is the root folder of the .NET solutions

### Global Configuration Files
ISR libraries use a global editor configuration file and a global test Runs settings file. 
These files can be found in the [IDE Repository].

Restoring Editor Configuration:
```
xcopy /Y %my%\.editorconfig %my%\.editorconfig.bak
xcopy /Y %vslib%\core\ide\code\.editorconfig %my%\.editorconfig
```

Restoring Runs Settings:
```
xcopy /Y %userprofile%\.runsettings %userprofile%\.runsettings.bak
xcopy /Y %vslib%\core\ide\code\.runsettings %userprofile%\.runsettings
```
where %userprofile% is the root user folder.

### Packages
TBA

<a name="FacilitatedBy"></a>
## Facilitated By
* [Visual Studio]
* [Atomineer Code Documentation]
* [EW Software Spell Checker]
* [Funduc Search and Replace]
* [IVI Foundation] - IVI Foundation VISA

## Repository Owner
* [ATE Coder]

<a name="Authors"></a>
## Authors
* [ATE Coder]  

## Acknowledgments
* [Its all a remix] -- we are but a spec on the shoulders of giants  
* [John Simmons] - outlaw programmer  
* [Stack overflow] - Joel Spolsky  
* [.Net Foundation] - The .NET Foundation

<a name="Open-Source"></a>
## Open source
Open source used by this software is described and licensed at the following sites:  

<a name="Closed-software"></a>
## Closed software
Closed software used by this software are described and licensed on the following sites:  

<a name="Resources"></a>
## Resources 

<a name="Legal-Notices"></a>
## Legal Notices

Integrated Scientific Resources, Inc., and any contributors grant you a license to the documentation and other content
in this repository under the [Creative Commons Attribution 4.0 International Public License](https://creativecommons.org/licenses/by/4.0/legalcode), see the [LICENSE](LICENSE) file, and grant you a license to any code in the repository under the [MIT License](https://opensource.org/licenses/MIT), see the [LICENSE-CODE](LICENSE-CODE) file.

Integrated Scientific Resources, Inc., and/or other Integrated Scientific Resources, Inc., products and services referenced in the documentation may be either trademarks or registered trademarks of Integrated Scientific Resources, Inc., in the United States and/or other countries. The licenses for this project do not grant you rights to use any Integrated Scientific Resources, Inc., names, logos, or trademarks.

Integrated Scientific Resources, Inc., and any contributors reserve all other rights, whether under their respective copyrights, patents, or trademarks, whether by implication, estoppel or otherwise.

[IVI Foundation]: https://www.ivifoundation.org

[Microsoft .NET Framework]: https://dotnet.microsoft.com/download

[external repositories]: ExternalReposCommits.csv

[IDE Repository]: https://www.bitbucket.org/davidhary/vs.ide

[ATE Coder]: https://www.IntegratedScientificResources.com

[Its all a remix]: https://www.everythingisaremix.info

[John Simmons]: https://www.codeproject.com/script/Membership/View.aspx?mid=7741

[Stack overflow]: https://www.stackoveflow.com

[Visual Studio]: https://www.visualstudio.com/

[Atomineer Code Documentation]: https://www.atomineerutils.com/

[EW Software Spell Checker]: https://github.com/EWSoftware/VSSpellChecker/wiki/

[Funduc Search and Replace]: http://www.funduc.com/search_replace.htm

[.Net Foundation]: https://source.dot.net

[LXI]: https://www.lxistandard.org/About/AboutLXI.aspx

[VXI11.CSharp]: https://github.com/Xanliang/VXI11.CSharp 

[RemoteTea.Net]: https://github.com/wespday/RemoteTea.Net  

[Python VXI-11]: https://github.com/alexforencich/python-vxi11.git 

[GB1.RemoteTea.Net]: https://github.com/galenbancroft/RemoteTea.Net

[ISR's VXI11]: https://github.com/ATECoder/dn.vxi11

[VXI Bus Specification]: https://vxibus.org/specifications.html
