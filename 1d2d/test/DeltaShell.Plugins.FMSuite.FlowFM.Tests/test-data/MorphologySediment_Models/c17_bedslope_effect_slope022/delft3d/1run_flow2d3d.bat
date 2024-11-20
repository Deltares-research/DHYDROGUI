@ echo off
    rem
    rem This script is an example for running Delft3D-FLOW 6.00 (Windows)
    rem Adapt and use it for your own purpose
    rem
    rem adri.mourits@deltares.nl
    rem 30 oct 2013
    rem 


    rem
    rem Set version numbers here
    rem example:
set flowversionnr=6.02.07.6118
    rem Please use the latest version available at "p:\1204550-danube\2nd_loop_model\7_Pilottest_D3D_morphology\SecondaryFlow_5thloop13112015\scripts" when possible
if ["%flowversionnr%"] EQU ["6.09.09.9999"] (
    echo "ERROR in runscript: version number not specified"
    echo "                    Please choose a version at d:\software\ds-trunk-45915_oss-trunk-6118,"
    echo "                    preferably the latest version"
    echo "      Open the runscript in a text editor and change the following line:"
    echo "      set flowversionnr=6.09.09.9999"
    goto end
)

    
    rem
    rem Set the config file here
    rem 
set argfile=config_d_hydro.xml





    rem
    rem Set the directory containing d_hydro.exe here
    rem
echo When error message "The system cannot find the path specified." appears below:
echo   Check "ARCH" in the run-script:
rem echo     Version 6.01.17.5275 and older: default ARCH=win32
echo     Version 6.02.07.6118 and newer: default ARCH=win64
set ARCH=win64
set D3D_HOME=d:\software\ds-trunk-45915_oss-trunk-6118> %flowversionnr%
set exedir=%D3D_HOME%\%ARCH%\flow2d3d\bin\

    rem
    rem No adaptions needed below
    rem

    rem Set some (environment) parameters
set PATH=%exedir%;%PATH%

    rem Run
%exedir%\d_hydro.exe %argfile%


:end
    rem To prevent the DOS box from disappearing immediately: remove the rem on the following line
rem pause
