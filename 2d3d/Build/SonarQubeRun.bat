set svnDir=%1
set msBuildDir=%2

"%svnDir%\build\SonarQube\SonarScanner.MSBuild.exe" begin /k:"DELFT3D_Flexible_Mesh_BenO" /n:"Delft3D Flexible Mesh B&O" /d:sonar.scm.provider=svn /d:sonar.host.url="https://sonarqube.deltares.nl" /d:sonar.login="cdef2c31f166b3f27938997b0fc04d637c080600"

%msBuildDir%\MsBuild.exe NGHS.sln /t:Rebuild /t:clean

"%svnDir%\build\SonarQube\SonarScanner.MSBuild.exe" end /d:sonar.login="cdef2c31f166b3f27938997b0fc04d637c080600"

