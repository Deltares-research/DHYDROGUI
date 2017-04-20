@ echo off
if "%1" == "" goto error

cd %1\

FOR /D %%G IN (Test_*) DO (
 pushd %%G
 echo Deleting old analysis data from %%G
 rmdir /S /Q analysis
 popd
)

cd ..
goto end

:error
echo missing argument!
echo example usage remove_analysis_dirs testcases_waq
:end