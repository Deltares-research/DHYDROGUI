function compareRestart;

close all;
fclose all;
clc;

% Read the way the restart information is specified in the mdu-file
mdu       = fopen('simplebox.mdu','r');
times     = [0 0 0];
fidrs     = fopen('doc/restartinfo.tex','wt');
while ~feof(mdu)
    tline = fgetl(mdu);
    if length(tline)>8;
        if strcmp(tline(1:9),'[restart]');
            tline     = strrep(tline,'\','/');
            tline     = strrep(tline,'_','\_');
            fprintf(fidrs,'%s\n',['\texttt{',tline,'}  \\']);
            tline     = fgetl(mdu);
            tline     = strrep(tline,'\','/');
            tline     = strrep(tline,'_','\_');
            fprintf(fidrs,'%s\n',['\texttt{',tline,'}  \\']);
            tline     = fgetl(mdu);
            tline     = strrep(tline,'\','/');
            tline     = strrep(tline,'_','\_');
            fprintf(fidrs,'%s'  ,['\texttt{',tline,'}']);
            continue
        end
    end
end

% Read restart-file: final timestep
hrd      = nc_varget('original/simplebox_map.nc','s1');
urd      = nc_varget('original/simplebox_map.nc','unorm');
srd      = nc_varget('original/simplebox_map.nc','sa1');

% Read newly generated output file
hnd      = nc_varget('dflowfmoutput/simplebox_map.nc','s1');
und      = nc_varget('dflowfmoutput/simplebox_map.nc','unorm');
snd      = nc_varget('dflowfmoutput/simplebox_map.nc','sa1');

% Take final timestep
hr       = hrd(end,:);
ur       = urd(end,:);
sr       = srd(end,:);
hn       = hnd(end,:);
un       = und(end,:);
sn       = snd(end,:);

% Determine RMS difference
dh       = sqrt(mean((hr-hn).^2))
du       = sqrt(mean((ur-un).^2))
ds       = sqrt(mean((sr-sn).^2))

% Write to tex-file
fiddh    = fopen('doc/dh.tex','wt');
fiddu    = fopen('doc/du.tex','wt');
fidds    = fopen('doc/ds.tex','wt');
fprintf(fiddh,'%s ',[num2str(dh,'%4.7f')]);
fprintf(fiddu,'%s ',[num2str(du,'%4.7f')]);
fprintf(fidds,'%s ',[num2str(ds,'%4.7f')]);
fclose all;