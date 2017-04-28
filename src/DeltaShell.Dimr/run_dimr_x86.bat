@echo off

set DIMR_DIR=%~dp0

set DIMR_BIN=%DIMR_DIR%kernels\x86
set FLOW1D_BIN=%DIMR_DIR%..\DeltaShell.Plugins.WaterFlowModel\flow1d_kernel\x86
set RTC_BIN=%DIMR_DIR%..\DeltaShell.Plugins.RealTimeControl\rtc_kernel\x86
set FM_BIN=%DIMR_DIR%..\DeltaShell.Plugins.FMSuite.FlowFM\dflowfm_kernel\x86
set RR_BIN=%DIMR_DIR%..\DeltaShell.Plugins.RainfallRunoffModel\rr_kernel\x86

set PATH=%DIMR_BIN%;%FLOW1D_BIN%;%RTC_BIN%;%FM_BIN%;%RR_BIN%;%PATH%

rem echo ====
rem echo PATH
rem echo %PATH%
rem echo ====

echo starting dimr.exe %1 %2 %3 %4

dimr.exe %1 %2 %3 %4
