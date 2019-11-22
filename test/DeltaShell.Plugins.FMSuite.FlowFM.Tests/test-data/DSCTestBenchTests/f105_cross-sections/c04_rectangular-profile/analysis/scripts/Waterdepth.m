clear all; close all; clc;
 
%% Analysis of testcase https://repos.deltares.nl/repos/DSCTestbench/trunk/cases/e02_dflowfm/f105_cross-sections/c16_lumped_YZ-profile

%% Settings and parameters
format long

% To use the qp plot path. This should be adjusted to your path
run('d:\05_Projects\05_27_FM_testing\openearthtools\Matlab\oetsettings.m')

% Select outputpath of model run
f_out = '..\..\DFM_OUTPUT_Flow1D\Flow1D_map.nc';

% Define variabele of interest
var1 = 'Water depth at pressure points';

% Open output of model
FileInfo = qpfopen(f_out);

% Check DataFields for the correct name of the variable (not the variable
% as mentioned in nc-file but as mentioned by quickplot
[DataFields, Dims, Nval] = qpread(FileInfo);

% Timestep of interest based on 
meta = qpread(FileInfo, var1 ,'size');

% Because we are only interested in last timestep 
t = meta(1);

% Get water depth data
h1 = qpread(FileInfo, var1,'griddata', t);

h1_T1 = h1.Val(1:401);
h1_T2 = h1.Val(402:802);
h1_T3 = h1.Val(803:1203);
h1_T4 = h1.Val(1204:1604);
h1_T5 = h1.Val(1605:2005);
h1_T6 = h1.Val(2006:2406);
h1_T7 = h1.Val(2407:2807);
h1_T8 = h1.Val(2808:3208);
h1_T9 = h1.Val(3209:3609);

X = h1.X(1:401)/1000;
chainage =flip(X);

fig1 = figure('Position',[1,1,800,380]);
plot(chainage,h1_T1);
hold on;
plot(chainage,h1_T2);
hold on;
plot(chainage,h1_T3);
hold on;
plot(chainage,h1_T4);
hold on;
plot(chainage,h1_T5);
hold on;
plot(chainage,h1_T6);
hold on;
plot(chainage,h1_T7);
hold on;
plot(chainage,h1_T8);
hold on;
plot(chainage,h1_T9);
hold on;
ylabel('Water Depth [m]');
ytickformat('%.4f');
xlabel('Chainage [km]');
set(gca, 'XDir','reverse')
lgd = legend({'T1','T2','T3','T5','T6','T7','T8','T9'}); 
lgd.Location = 'southeast';
grid on
saveas(fig1,'d:\05_Projects\05_27_FM_testing\testbench\cases\e02_dflowfm\f105_cross-sections\c04_rectangular-profile\doc\figures\e02_f105_c04_Water_Depth_EntireLength.jpg')
