cd Scanners
call ..\..\..\..\..\build\tools\gplex\gplex.exe /noEmbedBuffers /frame:..\..\..\..\..\build\tools\gplex\gplexx.frame /out:ProfileDatFileScanner.generated.cs  ProfileDatFileScanner.lex
call ..\..\..\..\..\build\tools\gplex\gplex.exe /noEmbedBuffers /frame:..\..\..\..\..\build\tools\gplex\gplexx.frame /out:CRNetworkFileScanner.generated.cs  CRNetworkFileScanner.lex
call ..\..\..\..\..\build\tools\gplex\gplex.exe /noEmbedBuffers /frame:..\..\..\..\..\build\tools\gplex\gplexx.frame /out:TPNetworkFileScanner.generated.cs  TPNetworkFileScanner.lex
pause