rem voeg aan pad toe d:\unstruc; rem  set PATH=d:\unstruc;%PATH%
if not exist unstruc.ini copy d:\unstruc\unstruc.ini .
if not exist isocolour.hls copy d:\unstruc\isocolour.hls .
if not exist interact.ini  copy d:\unstruc\interact.ini .
"d:\unstruc_svn\unstruc\Release\dflowfm.exe" %1
