function csm_batchrms()
% Bekijkt drie datasets: Unstruc uniform, Unstruc Courant, en Waqua.
% Alle drie worden vergeleken met de Tidal predicties.
% Voor elk station wordt voor de drie datasets de rms berekend en
% weggeschreven in een .xyz file.

%% unstruc hisfile
%hisdata=unstruc.readHis('world_his.nc',statname);
%hisdataref=unstruc.readHis('csmgr3_03_his.nc',statname);
hisdata   =unstruc.readHis('csmgr3_04_his.nc');
%hisdataref=unstruc.readHis('csmFINEa82dxunif_his.nc',statname);
hisdataref=unstruc.readHis('csmA82Q_his.nc');

nchecktimes = 1044; % 7*24'50 (+1) in stappen van 10 minuten
nchecktimes = min(nchecktimes, hisdata.time(end)/600); % if unstruc times are short

nstat = size(hisdata.station_x_coord, 1);
if (nstat > 0)
    fid_uns    = fopen('csm_batchrms_uns.xyz','w');
    fid_unsrel    = fopen('csm_batchrms_unsrel.xyz','w');
    fid_unsref = fopen('csm_batchrms_unsref.xyz','w');
    fid_unsrefrel = fopen('csm_batchrms_unsrefrel.xyz','w');
    fid_waq    = fopen('csm_batchrms_waq.xyz','w');
    fid_waqrel    = fopen('csm_batchrms_waqrel.xyz','w');
else
    fprintf('No stations in hisfile, exiting.');
    return;
end
for i=1:nstat
    statname = deblank(hisdata.station_name(:,i)');

    %% Edwins files
    tdpfile = ['a39_wl_obs_',statname,'.mat'];
    haveTdp = exist(tdpfile)~=0;
    if (haveTdp)
        load(tdpfile);
        valtdp=val;
    else
        fprintf('Skipping station ''%s'' (in Unstruc, but no tidal pred. available).\n', statname);
        continue
    end

    waqfile = ['a39_wl_prd_',statname,'.mat'];
    haveWaq = exist(waqfile);
    if (haveWaq)
        load(waqfile);
        valwaq=val;
    end
    
    nchecktimes = min(nchecktimes, size(valtdp,2));
    if (haveWaq)
        nchecktimes = min(nchecktimes, size(valwaq,2));
    end
    
    % (waq and tpd are in minutes)
    data_tdpint = interp1(hisdata.time, hisdata.waterlevel(i,:), 60*valtdp(1,1:nchecktimes)); % voor residuplot
    refdata_tdpint = interp1(hisdataref.time, hisdataref.waterlevel(i,:), 60*valtdp(1,1:nchecktimes)); % voor residuplot

    % Bepaal middenstanden over nchecktimes-aangegeven periodes.
    mid_tdp    = sum(valtdp(2, 1:nchecktimes))/nchecktimes; % tidal pred (corrected obs)
    mid_his    = sum(data_tdpint(1:nchecktimes))/nchecktimes; %Unstruc
    mid_hisref = sum(refdata_tdpint(1:nchecktimes))/nchecktimes; %Unstruc ref

    % Trek middenstanden direct van (geinterpoleerde) waterstanden af.
    valtdp(2,1:nchecktimes) = valtdp(2,1:nchecktimes) - mid_tdp;
    refdata_tdpint          = refdata_tdpint - mid_hisref;
    data_tdpint             = data_tdpint    - mid_his;

    rms_tdp  = norm(valtdp(2,1:nchecktimes))/sqrt(nchecktimes);
    rms_ref  = norm(refdata_tdpint(1:nchecktimes) - valtdp(2,1:nchecktimes))/sqrt(nchecktimes);
    rms_uns  = norm(data_tdpint(1:nchecktimes) - valtdp(2,1:nchecktimes))/sqrt(nchecktimes);
    
    % print line in xyz files
    fprintf(fid_uns,       '%f %f %f\n', hisdata.station_x_coord(i), hisdata.station_y_coord(i), rms_uns);
    fprintf(fid_unsrel,    '%f %f %f\n', hisdata.station_x_coord(i), hisdata.station_y_coord(i), rms_uns/rms_tdp);
    fprintf(fid_unsref,    '%f %f %f\n', hisdata.station_x_coord(i), hisdata.station_y_coord(i), rms_ref);
    fprintf(fid_unsrefrel, '%f %f %f\n', hisdata.station_x_coord(i), hisdata.station_y_coord(i), rms_ref/rms_tdp);
    if (haveWaq)
        mid_waq    = sum(valwaq(2, 1:nchecktimes))/nchecktimes; %WAQUA
        valwaq(2,1:nchecktimes) = valwaq(2,1:nchecktimes) - mid_waq;
        rms_waq  = norm(valwaq(2,1:nchecktimes)    - valtdp(2,1:nchecktimes))/sqrt(nchecktimes);
        fprintf(fid_waq,       '%f %f %f\n', hisdata.station_x_coord(i), hisdata.station_y_coord(i), rms_waq);
        fprintf(fid_waqrel,    '%f %f %f\n', hisdata.station_x_coord(i), hisdata.station_y_coord(i), rms_waq/rms_tdp);
        fprintf('%12s: Tidal pred. rms.: %7.5f    Unstruc uniform rms.: %7.5f    Unstruc Courant rms.: %7.5f    Waqua rms.: %7.5f\n',statname, rms_tdp, rms_ref, rms_uns, rms_waq);
    else
        fprintf('%12s: Tidal pred. rms.: %7.5f    Unstruc uniform rms.: %7.5f    Unstruc Courant rms.: %7.5f    NO WAQUA\n',statname, rms_tdp, rms_ref, rms_uns);
    end

end %stat loop
fclose(fid_uns);
fclose(fid_unsref);
fclose(fid_waq);
end %function

