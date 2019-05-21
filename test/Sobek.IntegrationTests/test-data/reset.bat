rmdir /s /q testOpenDa2.dsproj_data
mkdir testOpenDa2.dsproj_data
xcopy /s bck\testOpenDa2.dsproj_data testOpenDa2.dsproj_data

rem rmdir /s /q testOpenDa2-org.dsproj_data
rem mkdir testOpenDa2-org.dsproj_data
rem xcopy /s bck\testOpenDa2-org.dsproj_data testOpenDa2-org.dsproj_data

rem rmdir /s /q testOpenDa3.dsproj_data
rem mkdir testOpenDa3.dsproj_data
rem xcopy /s bck\testOpenDa3.dsproj_data testOpenDa3.dsproj_data

del *.dsproj
copy bck\*.dsproj

