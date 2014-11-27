Imazen.NativeDependencyManager
==============================

Work in progress, move on. See [one of these similar projects](https://github.com/imazen/paragon/blob/master/resources.md#native-interoploading-projects-to-investigate) for now.

### Use cases (both development and production)

* Loading native DLLs from arbitrary locations, xplat. P/Invoke otherwise uses a limited search path.
* Detecting the architecture compatibility of a dll, xplat.
* Scanning and removing incorrectly formatted dlls from a folder (+ optional caching).
* Scanning and removing dlls with mismatched hashes from a folder (+ optional caching).
* Scanning and ensuring dlls timestamp is within a certain time frame.
* Loading the correct version of a DLL, whether native or managed. Otherwise, all projects and all environments must pick a bitness and stick with it. This forces consuming apps to not use AnyCPU.

### Transitive dependencies

* Within a plugin
       * Specify 1-to-many sets of dependencies (assembly name and ver. constraints). 
       * Include a flat list of dependency versions, arch, corresponding (optional) hashes, and download locations (http or local files)
* The caller can
       * Include a flat list of dependency versions, arch, corresponding (optional) hashes, and download locations (http or local files) These take precendence.


### Classes

* NativeAssemblyLoader - lightweight platform abstraction around LoadLibraryEx/dlopen and UnloadLibrary/dlcose
* AssemblyMetadataCache  - fast cache for (pairing file paths, creation/modified dates, sizes) with (sha1 values and assembly formats)
* HashVerifier - Takes sha1 of assembly
* AssemblyFormatReader - For development, when we don't have sha1 hashes. Detects type and architecture of assembly.
* 






### Possible features

* AssemblyResolve for non-Any CPU managed dependencies. 

* LoadLibrary/mono for native dependencies, based on architecture variant (and possibly other pivots?)

* Runtime dependency fetching from nuget or https? Hash verification? Signed caching for fast boot?

* Fast parser to detect assembly format? We can't recover from BadImageFormatException
* 





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
* Compile managed caller to one location, then copy it to another, then shadow-copy it? 
* In an 
