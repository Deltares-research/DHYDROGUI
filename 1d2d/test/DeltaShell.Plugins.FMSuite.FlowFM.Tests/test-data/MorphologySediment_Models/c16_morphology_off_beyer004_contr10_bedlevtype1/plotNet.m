function plotNet;

close all;

% Print?
printen     = 1;

% Read the grid
xn          = nc_varget('beyer_004_net.nc','NetNode_x');
yn          = nc_varget('beyer_004_net.nc','NetNode_y');
zn          = nc_varget('beyer_004_net.nc','NetNode_z');
netlinks    = nc_varget('beyer_004_net.nc','NetLink');
xnn         = xn(netlinks);
ynn         = yn(netlinks);

% Read the weir
readweir    = dflowfm_io_xydata('read','weir_tdk.pliz');
weir        = cell2mat(readweir.DATA);

% Read the observation file
readobs     = dflowfm_io_xydata('read','beyer_004_obs.xyn');
obs         = cell2mat(readobs.DATA(:,1:2));

% Figure settings
ms          = 4;
fs          = 12;
lw          = 1;
c1          = 0.3;
c2          = 0.8;
c3          = 0.1;

% Maak plotje van rooster
figure(1);
plot(xn  ,yn  ,'.','markersize',ms,'color','k'               ); hold on;
line(xnn',ynn',                    'color','k','linewidth',lw); 
plot(weir(:,1),weir(:,2),'color','r','linewidth',lw+2);
scatter(xn,yn,ms*12,zn,'filled'); 
plot(obs(:,1) ,obs (:,2),'.k','markersize',ms*6);
caxis([1 11]);
hcb = colorbar;
set(hcb,'YTick',[1 11])
hold off;
daspect([1 1 1]);
box off;
xlim([-10 250]);
set(gca,'xtick',[0:30:240]);
ylim([-10 40]);
set(gca,'ytick',[0 30]);

% Labels
xlabel(['distance [m]'],'fontsize',fs);
ylabel(['distance [m]'],'fontsize',fs);
set(gca,'fontsize',fs);

% Print?
if printen == 1;
   print(figure(1),'-dpng','-r300','doc/gridbeyer004.png');
   close all;
end