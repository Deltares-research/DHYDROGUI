@echo off
set mdwave=%1

set waveexedir=%D3D_HOME%\%ARCH%\wave\bin
set swanexedir=%D3D_HOME%\%ARCH%\swan\bin
set swanbatdir=%D3D_HOME%\%ARCH%\swan\scripts

    rem Set some (environment) parameters
set PATH=%swanbatdir%;%swanexedir%;%waveexedir%;%PATH%

    rem Run
call "%waveexedir%\wave.exe" %mdwave% 0
 
:end
