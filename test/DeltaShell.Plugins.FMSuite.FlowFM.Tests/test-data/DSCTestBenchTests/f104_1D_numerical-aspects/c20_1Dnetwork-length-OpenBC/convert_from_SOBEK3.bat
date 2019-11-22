@ echo off

    rem Usage:
    rem 1. Be sure that parameter "pythonscript" below points to the location of the Python conversion script on your PC
    rem 2. Be sure that parameter "inputfile"    below points to the correct SOBEK3 md1d file                 on your PC
    rem 3. Execute this batch file
    rem    result: directory "dflowfm" next to this batch file

set pythonscript=d:\checkouts\scripts\delft3dfm\convert_to_dflowfm\Run.py
set inputfile=d:\checkouts\DSCTestbench\cases\e02_dflowfm\f104_1D_numerical-aspects\c19_1Dnetwork-length-noOpenBC\analysis\Sobek_Models\with_open_boundaries\dflow1d.md1d

set ThisBatchFileLocation=%~dp0
set outputdir=%ThisBatchFileLocation%

call activate py36
    rem Execute the script
python.exe %pythonscript% -i %inputfile%  -o %outputdir%
call conda deactivate
    rem To avoid the command box to disappear immediately
pause
