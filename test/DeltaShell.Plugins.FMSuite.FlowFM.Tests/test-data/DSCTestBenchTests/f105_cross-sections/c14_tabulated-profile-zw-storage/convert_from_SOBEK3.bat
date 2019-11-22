@ echo off

    rem Usage:
    rem 1. Be sure that parameter "pythonscript" below points to the location of the Python conversion script on your PC
    rem 2. Be sure that parameter "inputfile"    below points to the correct SOBEK3 md1d file                 on your PC
    rem 3. Execute this batch file
    rem    result: directory "dflowfm" next to this batch file

set pythonscript=d:\Projects\RHU\Tests\scripts\Run.py
set inputfile=d:\Projects\RHU\Tests\cases\e02_dflowfm\f105_cross-sections\c09_tabulated-profile-storage\analysis\SOBEK_Model\ZW_Storage.md1d
REM set inputfile=d:\checkouts\dsc_testbench\cases\e02_dflowfm\f115_1D_backwater-curves\c01_M1\analysis\e106_flow1D-f15_backwater-curves-c01_M1_iadvec1d_1-aangepastlinestring\dflow1d\Flow1d.md1d

set ThisBatchFileLocation=%~dp0
set outputdir=%ThisBatchFileLocation%

    rem Execute the script
python.exe %pythonscript% -i %inputfile%  -o %outputdir%

    rem To avoid the command box to disappear immediately
pause

