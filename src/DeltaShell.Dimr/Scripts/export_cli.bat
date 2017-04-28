@ echo off

rem This script is only intended as an example for producing the CLI version

set root=%~dp0\..\..

  rem
  rem Choose a destination directory
set dest=d:\temp\cli
rmdir /s/q %dest%


  rem
  rem 64bit

set arch=win64


  rem
  rem create directory structure
mkdir %dest%\%arch%\dflowfm\bin
  rem mkdir %dest%\%arch%\dflowfm\scripts
mkdir %dest%\%arch%\dimr\bin
mkdir %dest%\%arch%\dimr\scripts
mkdir %dest%\%arch%\esmf\bin
mkdir %dest%\%arch%\esmf\scripts
mkdir %dest%\%arch%\flow1d2d\bin
  rem mkdir %dest%\%arch%\flow1d2d\scripts
mkdir %dest%\%arch%\mpiexec\bin
  rem mkdir %dest%\%arch%\mpiexec\scripts
mkdir %dest%\%arch%\rr\bin
  rem mkdir %dest%\%arch%\rr\scripts
mkdir %dest%\%arch%\RTCTools\bin
  rem mkdir %dest%\%arch%\RTCTools\scripts
mkdir %dest%\%arch%\swan\bin
mkdir %dest%\%arch%\swan\scripts
mkdir %dest%\%arch%\wave\bin
  rem mkdir %dest%\%arch%\wave\scripts

  rem
  rem collect/copy binaries and script
copy %root%\DeltaShell.Plugins.FMSuite.FlowFM\dflowfm_kernel\x64\*                  %dest%\%arch%\dflowfm\bin
copy %root%\DeltaShell.Dimr\kernels\x64\*                                           %dest%\%arch%\dimr\bin
copy %root%\DeltaShell.Dimr\Scripts\run_dimr.bat                                    %dest%\%arch%\dimr\scripts
copy %root%\DeltaShell.Dimr\Scripts\run_dimr_parallel.bat                           %dest%\%arch%\dimr\scripts
copy %root%\DeltaShell.Plugins.FMSuite.Wave\Delft3D\win64\esmf\bin\*                %dest%\%arch%\esmf\bin
copy %root%\DeltaShell.Plugins.FMSuite.Wave\Delft3D\win64\esmf\scripts\*            %dest%\%arch%\esmf\scripts
copy %root%\DeltaShell.Plugins.DelftModels.WaterFlowModel\flow1d_kernel\x64\*       %dest%\%arch%\flow1d2d\bin
copy %root%\DeltaShell.Plugins.DelftModels.HydroModel\kernels\x64\*                 %dest%\%arch%\flow1d2d\bin
copy %root%\DeltaShell.Dimr\kernels\x64\mpi*                                        %dest%\%arch%\mpiexec\bin
copy %root%\DeltaShell.Dimr\kernels\x64\smpd.exe                                    %dest%\%arch%\mpiexec\bin
copy %root%\DeltaShell.Plugins.DelftModels.RainfallRunoff\rr_kernel\x64\*           %dest%\%arch%\rr\bin
copy %root%\DeltaShell.Plugins.DelftModels.RealTimeControl\rtc_kernel\x64\*         %dest%\%arch%\RTCTools\bin
copy %root%\DeltaShell.Plugins.DelftModels.RealTimeControl\xsd\*                    %dest%\%arch%\RTCTools\bin
copy %root%\DeltaShell.Plugins.FMSuite.Wave\Delft3D\win64\swan\bin\*                %dest%\%arch%\swan\bin
copy %root%\DeltaShell.Plugins.FMSuite.Wave\Delft3D\win64\swan\scripts\*            %dest%\%arch%\swan\scripts
copy %root%\DeltaShell.Plugins.FMSuite.Wave\Delft3D\win64\wave\bin\*                %dest%\%arch%\wave\bin




  rem
  rem 32bit

set arch=win32


  rem
  rem create directory structure
mkdir %dest%\%arch%\dflowfm\bin
  rem mkdir %dest%\%arch%\dflowfm\scripts
mkdir %dest%\%arch%\dimr\bin
mkdir %dest%\%arch%\dimr\scripts
mkdir %dest%\%arch%\flow1d2d\bin
  rem mkdir %dest%\%arch%\flow1d2d\scripts
mkdir %dest%\%arch%\mpiexec\bin
  rem mkdir %dest%\%arch%\mpiexec\scripts
mkdir %dest%\%arch%\rr\bin
  rem mkdir %dest%\%arch%\rr\scripts
mkdir %dest%\%arch%\RTCTools\bin
  rem mkdir %dest%\%arch%\RTCTools\scripts

  rem
  rem collect/copy binaries and script
copy %root%\DeltaShell.Plugins.FMSuite.FlowFM\dflowfm_kernel\x86\*                  %dest%\%arch%\dflowfm\bin
copy %root%\DeltaShell.Dimr\kernels\x86\*                                           %dest%\%arch%\dimr\bin
copy %root%\DeltaShell.Dimr\Scripts\run_dimr_win32.bat                              %dest%\%arch%\dimr\scripts
copy %root%\DeltaShell.Plugins.DelftModels.WaterFlowModel\flow1d_kernel\x86\*       %dest%\%arch%\flow1d2d\bin
copy %root%\DeltaShell.Plugins.DelftModels.HydroModel\kernels\x86\*                 %dest%\%arch%\flow1d2d\bin
copy %root%\DeltaShell.Dimr\kernels\x86\mpi*                                        %dest%\%arch%\mpiexec\bin
copy %root%\DeltaShell.Dimr\kernels\x86\smpd.exe                                    %dest%\%arch%\mpiexec\bin
copy %root%\DeltaShell.Plugins.DelftModels.RainfallRunoff\rr_kernel\x86\*           %dest%\%arch%\rr\bin
copy %root%\DeltaShell.Plugins.DelftModels.RealTimeControl\rtc_kernel\x86\*         %dest%\%arch%\RTCTools\bin
copy %root%\DeltaShell.Plugins.DelftModels.RealTimeControl\xsd\*                    %dest%\%arch%\RTCTools\bin




pause
