set svnDir=%1
set msBuildDir=%2

"%svnDir%\build\SonarQube\SonarScanner.MSBuild.exe" begin /k:"DELFT3D Flexible Mesh B&O" /d:sonar.host.url="https://sonarqube.directory.intra" /d:sonar.login="9a303fd6746e3a8f3038db00fd90a028fe6d89b8"

%msBuildDir%\MsBuild.exe NGHS.sln /t:Rebuild /t:clean

"%svnDir%\build\SonarQube\SonarScanner.MSBuild.exe" end /d:sonar.login="9a303fd6746e3a8f3038db00fd90a028fe6d89b8"

