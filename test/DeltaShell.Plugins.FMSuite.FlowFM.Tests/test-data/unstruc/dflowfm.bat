set install_dir=%1
set mdu_filepath=%2
set mdu_file=%3

set work_dir=%CD%
cd %mdu_filepath%

copy %install_dir%\unstruc.ini .
copy %install_dir%\interact.ini .

%install_dir%\dflowfm.exe --sander --nodisplay --autostartstop %mdu_file%
cd %work_dir%