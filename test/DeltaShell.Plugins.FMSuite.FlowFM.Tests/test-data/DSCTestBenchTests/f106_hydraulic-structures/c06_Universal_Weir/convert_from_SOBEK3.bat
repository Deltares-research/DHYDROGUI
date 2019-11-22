@ echo off

    rem Usage:
    rem 1. Be sure that parameter "pythonscript" below points to the location of the Python conversion script on your PC
    rem 2. Be sure that parameter "inputfile"    below points to the correct SOBEK3 md1d file                 on your PC
    rem 3. Execute this batch file
    rem    result: directory "dflowfm" next to this batch file

set pythonscript=d:\05_Projects\05_27_FM_testing\SOBEK_FM_Conversion_scripts\Run.py
set inputfile=d:\05_Projects\05_27_FM_testing\testbench\cases\e02_dflowfm\f106_hydraulic-structures\c06_Universal_Weir\Analysis\SOBEK_model\universal_weir.dsproj_data\Flow1D_output\dflow1d\Flow1D.md1d
set ThisBatchFileLocation=%~dp0
set outputdir=%ThisBatchFileLocation%

    rem Execute the script
python.exe %pythonscript% -i %inputfile%  -o %outputdir%

    rem To avoid the command box to disappear immediately
pause
