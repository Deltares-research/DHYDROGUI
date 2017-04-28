@ echo off
if "%1" == "" goto error

cd %1\

FOR /D %%G IN (Test_*) DO (
 pushd %%G

 cd analysis 

 echo Deleting his files for non-failed models from %%G

 IF NOT EXIST *.png rmdir /S /Q work

 cd ..

 popd
)

cd ..
goto end

:error
echo missing argument!
echo example usage remove_analysis_dirs testcases_waq
:end