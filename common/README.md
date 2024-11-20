# DHYDRO.Common is a shared D-HYDRO library with full debug support

For issue [D3DFMIQ-2938](https://issuetracker.deltares.nl/browse/D3DFMIQ-2938), a common D-HYDRO library was requested to use within the D-HYDRO 1D2D and D-HYDRO 2D3D products to reduce code duplicity. 

DHYDRO.Common has no dependencies to other external libraries such as the DeltaShell Framework. Currently, the aim is to keep the depencies as low as possible.

## Debugging support

When using and developing internal packages it is important that debugging is easy and hassle-free.

To support a nice debugging experience when used in another project, we need two things:
- Symbol (PDB) files; they hold the information to link the debugger with the source files
- Source files

## Delivering the symbol files
There are various ways to deliver symbol files through or together with a NuGet package, for example by:
1. Setting the property `AllowedOutputExtensionsInPackageBuildOutputFolder` with `$(AllowedOutputExtensionsInPackageBuildOutputFolder);.pdb` in the project file. This will allow the corresponding symbol file to be included in the NuGet package.

2. Setting the property `DebugType` with `embedded` in the project file. This will embed the debug information into the DLL itself, so no symbol file will be generated.

3. Creating a symbol package (`*.snupkg` or `.symbols.nupkg`). A symbol package cannot be consumed like a regular NuGet package but should be published on a symbol server. This will allow the IDE to download the symbols from the specified symbol server while debugging.

4. Specifying in the Nuspec file that the symbol files will be included in the package. A Nuspec file contains package metadata and is used to create the NuGet package.

We have chosen option 4 (the Nuspec file) as this was the most consistent with the way DeltaShell creates its NuGet packages and we have the most control over which files will be included in the NuGet package.
However, we might want to consider embedding the debug information in the DLLs (option 3). This ensures that the debug information is always delivered without the need of including PDB files.

## Copying the symbol files to the correct location
Currently, there is a [bug](https://github.com/dotnet/sdk/issues/1458) in .NET Framework SDK style projects. The symbol files will not be copied to the correct output location of the consuming project. The symbol files need to be at the correct location, next to their DLLs, to be able to debug. 
There are various ways of achieving this, for example by:
1. Setting the property  `AllowedReferenceRelatedFileExtensions` with `.pdb` in the consuming project file.
2. Installing `SourceLink.Copy.PdbFiles` in the consuming project. This NuGet package was introduced specifically as a workaround for the bug.
3. Using a targets file together with the NuGet package. Target files are files that can be used to define the build process of a project. In the targets file we can specify which files need to be copied and where to.

We have choosen to use the targets file (option 3) and to include this file in the NuGet package. This file harvests the files delivered in the lib folder and copies these files to the `.\bin\<configuration>\DeltaShell` folder after the consuming project has been built. We choose this option so that the NuGet package works *out of the box* and the consuming end does not need to set additional properties or install additional packages (options 1 and 2).    

## Delivering the source files
There are also various ways to deliver the source files as well, but we have only focused on using Source Link for this. Source Link is a way to provide source control information. When debugging, this information is used to download the source files from the host, such as GitHub.
Source Link support for GitHub is added by installing the `Microsoft.SourceLink.GitHub` NuGet package in the project. Some additional properties are recommended to specify as well:

1. `PublishRepositoryUrl` > `true`: publishes the repository URL in the build NuGet package.
2. `EmbedUntrackedSources` > `true`: embeds the source files that are not tracked by source control in the symbol file.
3. `DebugType` > `embedded`: embeds the debugging information in the DLL. *Currently this option is overwritten*.


## Configuring the IDE
To make use of Source Link while debugging you have to make sure your IDE settings are correct:
- **For Visual Studio**:
	- Go to *Tools* > *Options* > *Debugging* > *General*
	- Disable *Enable Just My Code*
	- Enable *Source Link Support* 

- **For Rider**:
	- Go to *Settings* > *Tools* > *External Symbols*
	- Enable *Enable private Source Link support*