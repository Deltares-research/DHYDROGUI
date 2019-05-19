@echo oFF
if "%1" == "" goto error

cd %1\

FOR /D %%G IN (Test_*) DO (
 pushd %%G

 cd analysis 

 echo Deleting data for non-failed models from %%G

 IF NOT EXIST HasDiff.txt rmdir /S /Q work
 IF NOT EXIST HasDiff.txt rmdir /S /Q FileWr~1
 IF NOT EXIST HasDiff.txt del sobek.log
 
 cd ..

 popd
)

cd ..
goto end

:error
echo missing argument!
echo example usage remove_analysis_dirs testcases_waq
:end
