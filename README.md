Imazen.NativeDependencyManager
==============================

Work in progress, move on. See [one of these similar projects](https://github.com/imazen/paragon/blob/master/resources.md#native-interoploading-projects-to-investigate) for now.

### Possible features

* AssemblyResolve for non-Any CPU managed dependencies. 

* LoadLibrary/mono for native dependencies, based on architecture variant (and possibly other pivots?)

* Runtime dependency fetching from nuget or https? Hash verification? Signed caching for fast boot?

* Fast parser to detect assembly format? We can't recover from BadImageFormatException

### Integration tests to create

#### Variants

* Caller assembly and compile-time location may both be different from where the native dependencies are present. For ASP.NET, we may be able to figure this out. For tests, we may have to set an env var.
* Environment variables
* Loader arguments
* Filesystem state (are there even x64, x32, and Win32 folders)?
* Is there an arch-specific dll arealdy in the current folder?
* Type of native dependencies (C++/CLI, fully native?)
* Transitive dependencies? How can we ensure the right copy is present and loaded? Should we require metadata for this? (probably)
* How many different dependency trees do we need to manage? What if they have conflicts.
* 

[DLL search order](http://msdn.microsoft.com/en-us/library/windows/desktop/ms682586(v=vs.85).aspx)



* Given an ASP.NET web app, and a caller within the /bin folder
        * D
* Compile managed caller to one location, then copy it to another, then shadow-copy it? 
* In an 
