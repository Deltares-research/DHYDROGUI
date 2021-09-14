# Microsoft_VC142_CRT_x64.msm

`Microsoft_VC142_CRT_x64.msm` is the merge module which adds the Visual C++ runtime libraries as a feature to the DeltaShell-based installer. These runtime libraries are necessary for the license check software as well as C++ components like MeshKernel. For more information see ["Redistributing Visual C++ Files"](https://docs.microsoft.com/en-us/cpp/windows/redistributing-visual-cpp-files?view=msvc-160).

`Microsoft_VC142_CRT_x64.msm` is a copy from `C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\VC\Redist\MSVC\14.29.30133\MergeModules\Microsoft_VC142_CRT_x64.msm`, which is the latest version at the time of writing. Note that in order to have these merge modules located in this location, the "C++ 2019 Redistributable MSMs" component of VS2019 should be installed.
If the runtime version should be updated, then this file should be updated.
