rem this script generates the ikvm-converted readers/writers for the Fews-PI files

rem settings
rem ikvm-executable and directory where to put the resulting dll
rem settings

rem set IKVMC=..\..\..\..\build\tools\IKVM\bin\ikvmc.exe -debug
set IKVMC=..\..\..\..\build\tools\IKVM\bin\ikvmc.exe
set OUTDIR=-out:..\DeltaShell.Plugins.Fews
set OUTDLLNAME=FEWSPiForDotNet

echo IKVMC: %IKVMC%
echo OUTDIR: %OUTDIR%

rem action
rem perform compilation
rem action

%IKVMC% log4j-1.2.14.jar Delft_Util.jar xercesImpl.jar castor-0.9.5.jar Delft_PI_castor.jar Delft_PI.jar %OUTDIR%\%OUTDLLNAME%.dll 2>%OUTDLLNAME%-log.txt
