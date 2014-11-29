Imazen.NativeDependencyManager
==============================

Work in progress, move on. See [one of these similar projects](https://github.com/imazen/paragon/blob/master/resources.md#native-interoploading-projects-to-investigate) for now.

## open questions

* Should we rename "Imazen.NativeDependencyManager" to "Udeps"? Is the brevity worth the clarity?
* 

## Use cases

### General use cases
* Use native dependencies without fragile build/deploy scripts and hand-maintained conditional references.
* Mix Any CPU and arch-specific binaries. No need to vary all projects just for one. 
* Evaluate dependency options based on runtime compatibility, not just existence in the search path.
* Verify assembly version/integrity with SHA-1.

### Development use cases
* Auto-load the correct archticture and the newest build of your dependencies (both managed and native) with recursive search. Eliminate handmade conditional references and BadImageFormatExceptions.
* Don't waste time debugging the wrong version of the dll. You can enable fail-at-boot when there aren't any versions of a dependency with valid build timestamps.)

### Production use cases
* Support multiple architectures in a single deployment. Arch/plat-specific bin subfolders would be a typical.
* Customize the assembly search path (usually to be subfolder aware), cross platform. This means replacing the entire assembly loading process. 

### Deployment use cases

* Given an arch + plat target, fetch external, then collapse and de-duplicate all assemblies to a single folder.
* Given a set of arch+target targets, create a lockfile that can be used to restore external dependencies at boot time.
* Boot or deploy-time external dependency fetching and verification based on lockfile.



# Dependency constraint resolution

## The flat feed schema

Each element in the feed represents a unique file. 

* module name
* semver="" or latest="true"
* architectures supported by file (comma delimited)
* platforms supported by file (comma delimited)
* sha1 hashes for valid files (optional)
* download locations (https, local files, or resource)

Dependencies can add to the flat feed, but the initial caller's feed items will take precedence

## Expressing dependencies.

* module name -> 
	* module name, version constraint

If there is any sharing between modules, the dependencies must be expressed at the top-level.

Assembly attributes to specify feed? Embedded resource names?

### Caveats
* C++/CLI apps should use delay-loading of their dependencies if they want to internally specify the feed or dependencies.


# Implementation

Runtime compatibility checks

Determine if a (win32, clr, mac, or linux) dynamic library or executable will work in the current (or an arbitrary) runtime.

* [x] BinaryParser - Read type, platform, architecture(s) of any file.
* [ ] EnvironmentProfile - Represents an exeuction environment
* [ ] CompatChecker - Compare IBinaryInfo to an EnvironmentProfile to see if it is compatible.

xplat dylib loading

* [ ] (NativeLoader) - Load (if not loaded) native DLLs from specific locations with dlopen/LoadLibraryEx, dlclose/UnloadLibraryEx.  P/Invoke otherwise uses a limited search path.
* [ ] (ManagedLoader) - Load managed assemblies

Search paths

* [ ] SearchPath - Pair directories with optional arch/plat expectations. Perhaps add exclusions?
* [ ] ConventionalSearch - Conventional search for a `/bin /bin/x86 /bin/Win32 /bin/x64` layout.
* [ ] ConventionalMultiPlatSearch - Conventional search for multi-plat layout - /bin /bin/linux /bin/win /bin/mac /bin/x86 /bin/x86/linux /bin/x86/win /bin/x86/mac /bin/Win32 /bin/x64 ...
* [ ] DevSearch - look recursively throughout entire project folder for candidates.

File-based constraints

* [ ] BuildTimeConstraint - requires that the dependency has been built within a given TimeSpan
* [ ] RelativeBuildTimeConstraint - requires the dependency has been built within a TimeSpan of a reference binary's build time.
* [ ] CompatibleConstraint - wrapper for CompatChecker + BinaryParsers

Direct locating (no versioning, recursion, or indirect loading)

* [ ] DirectLocator - Takes a search path and a set of constraints that operate on file names


Indirect/transitive dependency management




* [ ] (ManagedLoader) - Load managed assemblies, and run the dependency segment in the combined context
* [ ] AssemblyMetadataCache  - fast cache for (pairing file paths, creation/modified dates, sizes) with (sha1 values and assembly formats)
* [ ] HashVerifier - Takes sha1 of assembly
* [ ] InfoCache - cache binary info, sha-1, modified dates, etc. 
* [ ] Cleaner - Enforce expectations on the filesytem. 
* [ ] Downloader

# Unit tests

* BinaryParser Coverage is good on windows and .net binaries
* BinaryParser - No coverage on nix/mac parsing. Need headers from a variety of files (first 128 bytes).




### Ideas for integration tests to create

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
* Given an ASP.NET web app, and a caller within the /bin folder
* Compile managed caller to one location, then copy it to another, then shadow-copy it? 
* In an 


## Resources 

* [DLL search order](http://msdn.microsoft.com/en-us/library/windows/desktop/ms682586(v=vs.85).aspx)
* [Parsing .net headers (wish I'd found this before subsetting Cecil)](http://wyday.com/blog/2010/how-to-detect-net-assemblies-x86-x64-anycpu/)