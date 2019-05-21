@ echo off
if "%1" == "" goto error

cd %1\

FOR /D %%G IN (Test_*) DO (
 pushd %%G
 echo Deleting old model_export data from %%G
 rmdir /S /Q model_export
 popd
)

cd ..
goto end

:error
echo missing argument!
echo example usage remove_model_export_dirs testcases
:end
