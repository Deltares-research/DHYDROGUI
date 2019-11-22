clear all; close all; clc;
 
%% Analysis of testcase https://repos.deltares.nl/repos/DSCTestbench/trunk/cases/e02_dflowfm/f101_1D-boundaries/c01_steady-state-flow

%% Settings and parameters
format long

% To use the qp plot path. This should be adjusted to your path
run('d:\Software\OpenEarthTools\oetsettings.m')

% Select outputpath of model run
f_map = '..\..\DFM_OUTPUT_Boundary\Boundary_map.nc';
FileInfo = qpfopen(f_map);
[DataFields, Dims, Nval ] = qpread(FileInfo);

% Define variabele of interest
var1 = 'Water depth at pressure points';
var2 = 'Discharge through flow link at current time - mesh1d_nEdges: sum';

% Timestep of interest based on 
meta = qpread(FileInfo, var1 ,'size');

% Because we are only interested in last timestep 
t = meta(1);

% Get water depth data at last timestep
s1 = qpread(FileInfo, var1,'griddata', 0);

s1_T1 = s1.Val(73,1:41);
s1_T2 = s1.Val(73,42:82);
s1_T3 = s1.Val(73,83:123);
s1_T4 = s1.Val(73,124:164);

%get the maximum differences of all sub-model at a pressure point location
s1_diff = zeros(41,1);
for i = 1:41
    s1_diff(i) = max([s1_T1(i),s1_T2(i),s1_T3(i),s1_T4(i)]) - min([s1_T1(i),s1_T2(i),s1_T3(i),s1_T4(i)])
end

X = s1.X(1:41)/1000;
chainage = flip(X);

fig1 = figure('Position',[1,1,800,380]);
plot(chainage,s1_T1,'-g');
hold on;
plot(chainage,s1_T2,'-b');
hold on;
plot(chainage,s1_T3,'-m');
hold on;
plot(chainage,s1_T4,'-k');
hold on;
ylabel('Water Depth [m]');
ytickformat('%.2f');
set(gca, 'YColor', 'k');
% set(fig1, 'YColor', 'k');
xlabel('Chainage [km]');
set(gca, 'XDir','reverse');

yyaxis right;
plot(chainage,s1_diff,'--r');
hold on;
ylim([0 14*10^-15]);
ylabel('Maximum Difference [m]','Color','k');
set(gca, 'YColor', 'k');

lgd = legend({'T1','T2','T3','T4','Max Difference'}); 
lgd.Location = 'southeast';
grid on;

% saveas(fig1,['d:\Projects\RHU\Tests\cases\e02_dflowfm\f101_1D-boundaries\c01_steady-state-flow\doc\figures\e02_f101_c01_Water_Depth_EntireLength.jpg']);

% Get water depth data at all timesteps at chainage=10000

s1_T1 = s1.Val(:,21);
s1_T2 = s1.Val(:,62);
s1_T3 = s1.Val(:,103);
s1_T4 = s1.Val(:,144);

s1_all_diff = zeros(73,1);
for i = 1:73
    s1_all_diff(i) = max([s1_T1(i),s1_T2(i),s1_T3(i),s1_T4(i)]) - min([s1_T1(i),s1_T2(i),s1_T3(i),s1_T4(i)])
end
 
hour = 0:72;

fig2 = figure('Position',[1,1,800,380]);

yyaxis left
plot(hour,s1_T1,'-g');
hold on;
plot(hour,s1_T2,'-b');
hold on;
plot(hour,s1_T3,'-m');
hold on;
plot(hour,s1_T4,'-k');
hold on;
ylabel('Water Depth [m]');
set(gca, 'YColor', 'k');
% ylim([99.99 100.01]);
% ytickformat('%.2f');
xlabel('Time [Hours]');
% set(gca, 'XDir','reverse')
% xtickformat('%4f');

yyaxis right
plot(hour,s1_all_diff,'--r');
hold on;
ylabel('Maximum Difference [m]');
ylim([-0.001 0.001]);
set(gca, 'YColor', 'k');

lgd = legend({'T1','T2','T3','T4','Max Difference'}); 
lgd.Location = 'southeast';
grid on;
% saveas(fig2,['d:\Projects\RHU\Tests\cases\e02_dflowfm\f101_1D-boundaries\c01_steady-state-flow\doc\figures\e02_f101_c01_Water_Depth_ObsCross.jpg']);


% Get Discharge data
q = qpread(FileInfo, var2,'griddata', t);

X = (s1.X(1:40)-25)/1000;
chainage = flip(X);

q_T1 = q.Val(1:40);
q_T2 = q.Val(41:80);
q_T3 = q.Val(81:120);
q_T4 = q.Val(121:160);

%get the maximum differences of all sub-model at a pressure point location
q_diff = zeros(40,1);
for i = 1:40
    q_diff(i) = max([q_T1(i),q_T2(i),q_T3(i),q_T4(i)]) - min([q_T1(i),q_T2(i),q_T3(i),q_T4(i)])
end
 
fig3 = figure('Position',[1,1,800,380]);

yyaxis left
plot(chainage,q_T1,'-g');
hold on;
plot(chainage,q_T2,'-b');
hold on;
plot(chainage,q_T3,'-m');
hold on;
plot(chainage,q_T4,'-k');
hold on;
ylabel('Discharge [CMS]');
ylim([99.99 100.01]);
ytickformat('%.2f');
set(gca, 'YColor', 'k');
xlabel('Chainage [km]');
set(gca, 'XDir','reverse')
xtickformat('%.1f');

yyaxis right
plot(chainage,q_diff,'--r');
hold on;
ylabel('Maximum Difference [CMS]');
set(gca, 'YColor', 'k');

lgd = legend({'T1','T2','T3','T4','Max Difference'}); 
lgd.Location = 'northeast';
grid on

% saveas(fig3,['d:\Projects\RHU\Tests\cases\e02_dflowfm\f101_1D-boundaries\c01_steady-state-flow\doc\figures\e02_f101_c01_Discharge_EntireLength.jpg']);




% Select outputpath for observation crss sections
f_his = '..\..\DFM_OUTPUT_Boundary\Boundary_his.nc';

FileInfo = qpfopen(f_his)
% 
% Check DataFields for the correct name of the variable (not the variable
% as mentioned in nc-file but as mentioned by quickplot
[DataFields, Dims, Nval ] = qpread(FileInfo);

% Define variabele of interest
var1 = 'cross_section_discharge';
% 
names = qpread(FileInfo, var1, 'stations');
times = qpread(FileInfo, var1, 'times');

obs_crs_T1 = qpread(FileInfo, var1,'griddata',0,1);
obs_crs_T2 = qpread(FileInfo, var1,'griddata',0,2);
obs_crs_T3 = qpread(FileInfo, var1,'griddata',0,3);
obs_crs_T4 = qpread(FileInfo, var1,'griddata',0,4);

q_obs_crs_T1 = obs_crs_T1.Val;
q_obs_crs_T2 = obs_crs_T2.Val;
q_obs_crs_T3 = obs_crs_T3.Val;
q_obs_crs_T4 = obs_crs_T4.Val;

%get the maximum differences of all sub-model at a pressure point location
q_obs_crs_diff = zeros(73,1);
for i = 1:73
    q_obs_crs_diff(i) = max([q_obs_crs_T1(i),q_obs_crs_T2(i),q_obs_crs_T3(i),q_obs_crs_T4(i)]) - min([q_obs_crs_T1(i),q_obs_crs_T2(i),q_obs_crs_T3(i),q_obs_crs_T4(i)])
end
 
hour = 0:72;

fig4 = figure('Position',[1,1,800,380]);

yyaxis left
plot(hour,q_obs_crs_T1,'-g');
hold on;
plot(hour,q_obs_crs_T2,'-b');
hold on;
plot(hour,q_obs_crs_T3,'-m');
hold on;
plot(hour,q_obs_crs_T4,'-k');
hold on;
ylabel('Discharge [CMS]');
set(gca, 'YColor', 'k');
% ylim([99.99 100.01]);
% ytickformat('%.2f');
xlabel('Time [Hours]');
% set(gca, 'XDir','reverse')
% xtickformat('%4f');

yyaxis right
plot(hour,q_obs_crs_diff,'--r');
hold on;
ylabel('Maximum Difference [CMS]');
ylim([-1 1]);
set(gca, 'YColor', 'k');

lgd = legend(names{1},names{2},names{3},names{4}, 'Max Difference');
set(lgd,'Interpreter', 'none')
lgd.Location = 'southeast';
grid on;
% saveas(fig4,['d:\Projects\RHU\Tests\cases\e02_dflowfm\f101_1D-boundaries\c01_steady-state-flow\doc\figures\e02_f101_c01_Discharge_ObsCross.jpg']);





 





