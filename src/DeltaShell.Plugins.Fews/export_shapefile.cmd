@echo off

set RUN_INFO_PATH="%1"
set OUTPUT_FILE = "%2"

DeltaShell.Console.exe --run-command="from DeltaShell.Plugins.Fews import *;FewsPlugin.ExportShapeFile('%RUN_INFO_PATH%', '%OUTPUT_FILE%')"

