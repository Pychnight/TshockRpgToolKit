using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// This file is shared by all plugins and dependencies to globally set the version, and other metadata relating to copyright and ownership.
// each individual project also has its own private AssemblyInfo.cs in the Properties folder, to set the Title and Description( Or Configuration ).

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("Part of the RPG Toolkit")]
[assembly: AssemblyCopyright("Copyright © 2017-2018")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//

//version for the entire product as a whole, NOT used in any meaningful way by the CLR. 
[assembly: AssemblyInformationalVersion("1.0.1729.0")]

//version for individual assemblies; in our case, we version all assemblies together. Not used by the CLR. 
[assembly: AssemblyFileVersion("1.0.1729.0")]

//version actually used by the CLR, this should only be changed with breaking changes( not bug fixes, not backwards compatible changes! )
[assembly: AssemblyVersion("1.0.0.0")]
