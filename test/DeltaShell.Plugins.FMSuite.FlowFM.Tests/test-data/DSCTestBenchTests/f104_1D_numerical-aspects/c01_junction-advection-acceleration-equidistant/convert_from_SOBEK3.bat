@ echo off

    rem Usage:
    rem 1. Be sure that parameter "pythonscript" below points to the location of the Python conversion script on your PC
    rem 2. Be sure that parameter "inputfile"    below points to the correct SOBEK3 md1d file                 on your PC
    rem 3. Execute this batch file
    rem    result: directory "dflowfm" next to this batch file

set pythonscript=d:\Projects\RHU\Tests\scripts\Run.py
REM set inputfile=d:\Projects\RHU\Tests\cases\e02_dflowfm\f104_1D_numerical-aspects\c01_junction-advection-acceleration-equidistant\analysis\sobek\dflow1d_additionalsystems\Flow1D.md1d
set inputfile=d:\Projects\RHU\Tests\cases\e02_dflowfm\f104_1D_numerical-aspects\c01_junction-advection-acceleration-equidistant\analysis\sobek\dflow1d_additionalsystems\Flow1D.md1d

set ThisBatchFileLocation=%~dp0
set outputdir=%ThisBatchFileLocation%

    rem Execute the script
python.exe %pythonscript% -i %inputfile%  -o %outputdir%

    rem To avoid the command box to disappear immediately
pause
