@ECHO off

:: SUMMARY:
:: 		This script restores the NuGet packages and builds the D-HYDRO solution of this repository.

:: REQUIREMENTS:
:: 		- You need to have NuGet installed: https://www.nuget.org/downloads
:: 		- The nuget.exe location needs to be referenced in the PATH environment variable (e.g. D:\bin\nuget)
:: 		- You need to have MSBuild installed: https://visualstudio.microsoft.com/downloads/#build-tools-for-visual-studio-2019
:: 		- The MSBuild.exe location needs to be referenced in the PATH environment variable (e.g. C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin)


:: ========== RUN SCRIPT ==========
	SET solution=..\..\NGHS.sln
	
	CALL :RESTORE_NUGET
	CALL :BUILD_RELEASE
	
	pause
	EXIT /B


:: ========== FUNCTIONS ==========
	:RESTORE_NUGET
		:: Restores the NuGet packages.
		
		nuget restore %solution%
		EXIT /B
	
	:BUILD_RELEASE
		:: Build the solution in Release mode.
		
		msbuild %solution% /p:Configuration=Release
		EXIT /B