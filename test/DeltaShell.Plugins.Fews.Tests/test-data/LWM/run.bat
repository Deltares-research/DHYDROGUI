set TEST_DIR=%CD%
..\..\..\..\..\src\DeltaShell\DeltaShell.Loader\bin\Debug\DeltaShell.Console.exe --run-command="from DeltaShell.Plugins.Fews import *;FewsPlugin.RunAdapter(r'%CD%\Input\pi-run.xml', r'%CD%')"
