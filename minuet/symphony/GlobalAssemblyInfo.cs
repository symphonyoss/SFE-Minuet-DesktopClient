﻿using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyCompany("Goldman Sachs & Co.")]
[assembly: AssemblyProduct("Paragon")]
[assembly: AssemblyCopyright("Copyright © Goldman Sachs & Co. 2015")]
[assembly: ComVisible(false)]

#if DEBUG
[assembly: AssemblyConfiguration("DEBUG")]
#else
[assembly: AssemblyConfiguration("RELEASE")]
#endif