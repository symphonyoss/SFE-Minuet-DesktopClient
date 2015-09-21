using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyCompany("Symphony OSF")]
[assembly: AssemblyProduct("Paragon")]
[assembly: AssemblyCopyright("Symphony OSF")]
[assembly: ComVisible(false)]

#if DEBUG
[assembly: AssemblyConfiguration("DEBUG")]
#else
[assembly: AssemblyConfiguration("RELEASE")]
#endif