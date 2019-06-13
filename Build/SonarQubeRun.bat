set svnDir=%1
set msBuildDir=%2

"%svnDir%\build\SonarQube\SonarScanner.MSBuild.exe" begin /k:"DELFT3D_Flexible_Mesh_B&O" /d:sonar.host.url="https://sonarqube.directory.intra" /d:sonar.login="d2492e6373f2bec76f88b20051e86a01c9fbfd16"

%msBuildDir%\MsBuild.exe NGHS.sln /t:Rebuild /t:clean

"%svnDir%\build\SonarQube\SonarScanner.MSBuild.exe" end /d:sonar.login="d2492e6373f2bec76f88b20051e86a01c9fbfd16"

