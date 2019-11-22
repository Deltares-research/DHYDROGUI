clear all;  close all; clc;

%% Create sideview

x = [0 15000 25000];
y = [10.15 10.0 0];

fig = figure('Position', [0.5,6,1200, 500])
plot(x,y,'k')
hold on;
plot(x,y,'g.')
ylim([-0.2,10.5]);

xlabel('X [km]');
ylabel('Elevation [mAD]');
xticks([0:2500:25000]);
xticklabels({'0.0','2.5','5.0','7.5','10.0','12.5','15.0','17.5','20.0','22.5','25.0'})
yticks([0:0.5:10]);
% txt = texlabel(['(' num2str(x(1)) ',' num2str(y(1)) ')']);  
% text(x(1),y(1)-0.4,txt)
grid on;
saveas(fig,'d:\checkouts\dsc_testbench\cases\e02_dflowfm\f104_1D_numerical-aspects\c01_junction-advection-acceleration-equidistant\doc\figures\e02_f104_c01_SideView.jpg');
