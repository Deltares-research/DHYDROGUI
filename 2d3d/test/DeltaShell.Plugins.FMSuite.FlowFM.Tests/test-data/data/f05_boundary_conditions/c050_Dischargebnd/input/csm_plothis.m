function csm_plothis(statname)

%% LDB plot
%global ldbx ldby;
%if isempty(ldbx) || isempty(ldby)
%    [ldbx, ldby] = readLdb('Awvs_coastline2.ldb');
%end
%global ldbPlot;
%if isempty(ldbPlot) || ldbPlot
%    figure(1); plotLdb(ldbx, ldby);
%    ldbPlot = false;
%end


%% Edwins files
tdpfile = ['a39_wl_obs_',statname,'.mat'];
haveTdp = exist(tdpfile)~=0;
if (haveTdp)
    load(tdpfile);
    valtdp=val;
end

waqfile = ['a39_wl_prd_',statname,'.mat'];
haveWaq = exist(waqfile);
if haveWaq
load(waqfile);
valwaq=val;
end 

obsfile = ['v72c_wl_obs_',statname,'.mat'];
haveObs = exist(obsfile)~=0;
if (haveObs)
    load(obsfile);
    valobs=val;
else
    valobs=zeros(2,0); % no data.
end

%% unstruc hisfile
%hisdata=unstruc.readHis('world_his.nc',statname);
%hisdataref=unstruc.readHis('csmgr3_03_his.nc',statname);
hisdata   =unstruc.readHis('csmgr3_04_his.nc',statname);
%hisdataref=unstruc.readHis('csmFINEa82dxunif_his.nc',statname);
hisdataref=unstruc.readHis('csmA82Q_his.nc',statname);

%% Start plotting
figure(1);
%ah=subplot('Position',[0.1300    0.1100    0.7750    0.3812]);
ah=subplot('Position',[0.05    0.05    0.90    0.43]);

cla;
nchecktimes = 1044; % 7*24'50 (+1) in stappen van 10 minuten
nchecktimes = min(nchecktimes, hisdata.time(end)/600); % if unstruc times are short

if (haveTdp)
    nchecktimes = min(nchecktimes, size(valtdp,2));
    if (haveWaq)
        nchecktimes = min(nchecktimes, size(valwaq,2));
    end

    % (waq and tpd are in minutes)
    data_tdpint = interp1(hisdata.time, hisdata.waterlevel,60*valtdp(1,1:nchecktimes)); % voor residuplot
    refdata_tdpint = interp1(hisdataref.time, hisdataref.waterlevel,60*valtdp(1,1:nchecktimes)); % voor residuplot

    % Bepaal middenstanden over nchecktimes-aangegeven periodes.
    mid_tdp    = sum(valtdp(2, 1:nchecktimes))/nchecktimes; % tidal pred (corrected obs)
    mid_his    = sum(data_tdpint(1:nchecktimes))/nchecktimes; %Unstruc
    mid_hisref = sum(refdata_tdpint(1:nchecktimes))/nchecktimes; %Unstruc ref
    fprintf('Middenstanden: TDP: %f\tUnstruc ref: %f\tUnstruc: %f', mid_tdp, mid_hisref, mid_his);
%    mid_tdp = 0; mid_his=0; mid_waq=0;

    % Trek middenstanden direct van (geinterpoleerde) waterstanden af.
    valtdp(2,1:nchecktimes) = valtdp(2,1:nchecktimes) - mid_tdp;
    refdata_tdpint          = refdata_tdpint - mid_hisref;
    data_tdpint             = data_tdpint    - mid_his;

    rms_tdp  = norm(valtdp(2,1:nchecktimes))/sqrt(nchecktimes);
    rms_ref  = norm(refdata_tdpint(1:nchecktimes) - valtdp(2,1:nchecktimes))/sqrt(nchecktimes);
    rms_uns  = norm(data_tdpint(1:nchecktimes) - valtdp(2,1:nchecktimes))/sqrt(nchecktimes);

    if (haveWaq)
        % Bepaal middenstanden over nchecktimes-aangegeven periodes.
        mid_waq    = sum(valwaq(2, 1:nchecktimes))/nchecktimes; %WAQUA
        % Trek middenstanden direct van (geinterpoleerde) waterstanden af.
        valwaq(2,1:nchecktimes) = valwaq(2,1:nchecktimes) - mid_waq;
        rms_waq  = norm(valwaq(2,1:nchecktimes)    - valtdp(2,1:nchecktimes))/sqrt(nchecktimes);
        fprintf('\tWaqua: %f\n', mid_waq);
        rms_txt = sprintf('Tidal pred. rms.: %7.5f    Unstruc uniform rms.: %7.5f    Unstruc Courant rms.: %7.5f    Waqua rms.: %7.5f',rms_tdp, rms_ref, rms_uns, rms_waq);
    else
        fprintf('\n');
        rms_txt = sprintf('Tidal pred. rms.: %7.5f    Unstruc uniform rms.: %7.5f    Unstruc Courant rms.: %7.5f',rms_tdp, rms_ref, rms_uns);
    end
    
    if (haveWaq)
        lhs = plot(60*valtdp(1,1:nchecktimes), valtdp(2,1:nchecktimes), 'k-.', ...
                   60*valtdp(1,1:nchecktimes), refdata_tdpint, 'b-', ...
                   60*valtdp(1,1:nchecktimes), refdata_tdpint - valtdp(2,1:nchecktimes), 'b--',...
                   60*valtdp(1,1:nchecktimes), data_tdpint, 'm-', ...
                   60*valtdp(1,1:nchecktimes), data_tdpint - valtdp(2,1:nchecktimes), 'm--',...
                   60*valwaq(1,1:nchecktimes),valwaq(2,1:nchecktimes),'g-',...
                   60*valwaq(1,1:nchecktimes),valwaq(2,1:nchecktimes)-valtdp(2,1:nchecktimes),'g--', ...
                   [0 1000000],[0 0]);
        set(lhs(6), 'Color',[0 .5 0]); %kleur waqua
        set(lhs(7), 'Color',[0 .5 0]); %kleur residu
        legend({'Tidal prediction','Uns. uniform','','Uns. Courant', '','Waqua',''});
    else
        lhs = plot(60*valtdp(1,1:nchecktimes), valtdp(2,1:nchecktimes), 'k-.', ...
                   60*valtdp(1,1:nchecktimes), refdata_tdpint, 'b-', ...
                   60*valtdp(1,1:nchecktimes), refdata_tdpint - valtdp(2,1:nchecktimes), 'b--',...
                   60*valtdp(1,1:nchecktimes), data_tdpint, 'm-', ...
                   60*valtdp(1,1:nchecktimes), data_tdpint - valtdp(2,1:nchecktimes), 'm--',...
                   [0 1000000],[0 0]);
        legend({'Tidal prediction','Uns. uniform','','Uns. Courant', ''});
    end
    %               60*valobs(1,:),valobs(2,:), ...
    set(lhs(1), 'LineWidth',2);
    %     set(lhs(7), 'Color',[.7 .4 0]); % kleur obs
    set(lhs(end), 'Color',[.7 .7 .7]);
    %    plot(hisdata.time, hisdata.waterlevel, hisdatafijn.time, hisdatafijn.waterlevel, 60*valtdp(1,1:1000), valtdp(2,1:1000),60*valwaq(1,1:1000),valwaq(2,1:1000));%,60*valobs(1,1:1000),valobs(2,1:1000))


else
    if (haveWaq)
        nchecktimes = min(nchecktimes, size(valwaq,2));
        data_waqint = interp1(hisdata.time, hisdata.waterlevel,60*valwaq(1,1:nchecktimes)); % voor residuplot
        lhs = plot(hisdataref.time, hisdataref.waterlevel, 'b-', ...
                   hisdata.time, hisdata.waterlevel, 'm-', ...
                   60*valwaq(1,1:nchecktimes),valwaq(2,1:nchecktimes), 'g-',...
                   60*valwaq(1,1:nchecktimes),data_waqint - valwaq(2,1:nchecktimes),'g--', ...
                   [0 1000000],[0 0]);
%         60*valobs(1,:),valobs(2,:), ...
        set(lhs(3), 'Color',[0 .5 0]); %kleur waqua
        set(lhs(4), 'Color','m');%[0 .5 0]); %kleur residu
        legend({'Uns Ref','Unstruc','Waqua','Unstruc-Waqua'});
    else
        lhs = plot(hisdataref.time, hisdataref.waterlevel, 'b-', ...
                   hisdata.time, hisdata.waterlevel, 'm-', ...
             [0 1000000],[0 0]);
        legend({'Uns Ref','Unstruc'});
    end
%     set(lhs(5), 'Color',[.7 .4 0]); % kleur obs
     set(lhs(end), 'Color',[.7 .7 .7]);

%    plot(hisdata.time, hisdata.waterlevel, hisdatafijn.time, hisdatafijn.waterlevel, 60*valwaq(1,1:1000),valwaq(2,1:1000));%,60*valobs(1,1:1000),valobs(2,1:1000))
rms_txt = '';
end
 set(ah, 'XLim',[600000,772800]); % Na 1 week, toon ruim 1,5 dag.
%   set(ah, 'XLim',[-36000,150000]); % Na 1 week, toon ruim 1,5 dag.

title(statname,'Interpreter','none')
xticks = 7*24*3600+3600*[0 12 24 36 48];
set(ah, 'Xtick',xticks);
set(ah, 'XtickLabel',{datestr(xticks(1)/24/3600,'HH:MM'),...
                      datestr(xticks(2)/24/3600,'HH:MM'),...
                      datestr(xticks(3)/24/3600,'HH:MM'),...
                      datestr(xticks(4)/24/3600,'HH:MM')
                      });
set(ah,'Box','on');

al = axis(ah);
h = text(.5*(al(2)+al(1)), al(3)+.05*(al(4)-al(3)), rms_txt);
set(h, 'HorizontalAlignment','center');

%% Now highlight the selected station
figure(1);
global ldb_hdl;
axes(ldb_hdl);%subplot(2,1,1);
hold on;
global stat_hdl statname_hdl;
if ~isempty(stat_hdl)
    try
    delete(stat_hdl);
    delete(statname_hdl);
    stat_hdl=[];
    statname_hdl=[];
    catch e
    end
end
stat_hdl     = plot(hisdata.station_x_coord, hisdata.station_y_coord, 'o','MarkerFaceColor','r','MarkerEdgeColor','r','Clipping','on');
statname_hdl = text(hisdata.station_x_coord, hisdata.station_y_coord, ['  ', hisdata.station_name]);
set(statname_hdl,'Color','r','FontSize',7,'Clipping','on','Interpreter','none');
hold off;

print('-dbitmap', [statname, '_his.bmp'])
end
% 
% 
% load a39_wl_obs_eurpfm.mat
% valtdp=val;
% load a39_wl_prd_eurpfm.mat
% valwaq=val;
% load v72c_wl_obs_eurpfm.mat
% valobs=val;
% hisdata=unstruc.readHis('csmgrof_his.nc','eurpfm')
% figure(2); plot(hisdata.time, hisdata.waterlevel, 60*valtdp(1,1:1000), valtdp(2,1:1000),60*valwaq(1,1:1000),valwaq(2,1:1000),60*valobs(1,1:1000),valobs(2,1:1000))
% title('EURPFM')
% legend({'unstruc','tidal pred','waqua','obs'})
