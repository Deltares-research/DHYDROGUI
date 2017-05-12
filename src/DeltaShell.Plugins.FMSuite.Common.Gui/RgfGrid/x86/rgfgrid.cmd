@echo off
set WL_PLUGINS_HOME=%~dp0%
set EXECDIR=%~dp0%
set QT_PLUGIN_PATH="%EXECDIR%bin"
"%EXECDIR%bin\mfe_app.exe" rgfgrid.dll rgfgrid
