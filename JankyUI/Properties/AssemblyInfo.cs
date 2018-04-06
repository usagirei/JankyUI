using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("JankyUI")]
[assembly: AssemblyDescription("Janky UI - Unity Immediate Mode GUI Wrapper")]
[assembly: AssemblyCompany("Usagirei")]
[assembly: AssemblyProduct("JankyUI")]
[assembly: AssemblyCopyright("Copyright © Usagirei 2018")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("a975fa23-bfc8-4a86-b1c6-6909d7511bf4")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]

[assembly: InternalsVisibleTo("Sandbox")]
[assembly: InternalsVisibleTo("JankyUI.Tests")]
[assembly: InternalsVisibleTo("JankyUI.Dynamic")]

#if !GIT 

[assembly: AssemblyConfiguration("DEBUG")]
[assembly: AssemblyVersion("0.0.0.0")]
[assembly: AssemblyFileVersion("0.0.0.0")]
[assembly: AssemblyInformationalVersion("development build - internal use only")]

#endif
