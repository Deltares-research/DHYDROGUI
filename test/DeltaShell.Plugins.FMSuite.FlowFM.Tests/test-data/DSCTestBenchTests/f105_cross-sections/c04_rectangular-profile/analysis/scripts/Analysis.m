clear all; close all; clc;
 
%% Analysis of testcase https://repos.deltares.nl/repos/DSCTestbench/trunk/cases/e02_dflowfm/f104_1D_numerical-aspects/c01_junction-advection-acceleration-equidistant

%% Settings and parameters
format long

% To use the qp plot path. This should be adjusted to your path
run('d:\05_Projects\05_27_FM_testing\openearthtools\Matlab\oetsettings.m')

% Select outputpath of model run
f_out = '..\..\DFM_OUTPUT_Flow1D\Flow1d_map.nc';

% Define variabele of interest
var1 = 'Water depth at pressure points';
var2 = 'Water level';
var3 = 'flow element center bedlevel (bl)';
% Open output of model
FileInfo = qpfopen(f_out);

% Check DataFields for the correct name of the variable (not the variable
% as mentioned in nc-file but as mentioned by quickplot
[DataFields, Dims, Nval ] = qpread(FileInfo);

% Timestep of interest based on 
meta = qpread(FileInfo, var1 ,'size');

% Because we are only interested in last timestep 
t = meta(1);

% Get water depth data
h1 = qpread(FileInfo, var1,'griddata',t);
h1_tm = h1.Val;
h1_all = [h1.X,h1.Y, h1_tm];

X_All = 20000; Y_T1 = 0;
Y_T2 = 500; Y_T3 = 1000;
Y_T4 = 1500; Y_T5 = 2000;
Y_T6 = 2500; Y_T7 = 3000;
Y_T8 = 3500; Y_T9 = 4000;

h1_T1 = h1_all((h1_all(:,1)== X_All & h1_all(:,2)== Y_T1),3);
h1_T2 = h1_all((h1_all(:,1)== X_All & h1_all(:,2)== Y_T2),3);
h1_T3 = h1_all((h1_all(:,1)== X_All & h1_all(:,2)== Y_T3),3);
h1_T4 = h1_all((h1_all(:,1)== X_All & h1_all(:,2)== Y_T4),3);
h1_T5 = h1_all((h1_all(:,1)== X_All & h1_all(:,2)== Y_T5),3);
h1_T6 = h1_all((h1_all(:,1)== X_All & h1_all(:,2)== Y_T6),3);
h1_T7 = h1_all((h1_all(:,1)== X_All & h1_all(:,2)== Y_T7),3);
h1_T8 = h1_all((h1_all(:,1)== X_All & h1_all(:,2)== Y_T8),3);
h1_T9 = h1_all((h1_all(:,1)== X_All & h1_all(:,2)== Y_T9),3);

D_hydro_Waterdepth = [h1_T1,h1_T2,h1_T3,h1_T4,...
    h1_T5,h1_T6,h1_T7,h1_T8, h1_T9];

Point = ["T1_up","T2_up","T3_up", "T4_up", "T5_up", "T6_up", "T7_up", "T8_up", "T9_up"];
Waterdepth = [Point;num2cell(D_hydro_Waterdepth)];
waterdepth_out = transpose(Waterdepth);

% Get water level data
s1 = qpread(FileInfo, var2,'griddata', t);
s1_tm = s1.Val;
s1_all = [s1.X,s1.Y, s1_tm];

s1_T1 =s1_all((s1_all(:,1)== X_All & s1_all(:,2)== Y_T1),3);
s1_T2 =s1_all((s1_all(:,1)== X_All & s1_all(:,2)== Y_T2),3);
s1_T3 =s1_all((s1_all(:,1)== X_All & s1_all(:,2)== Y_T3),3);
s1_T4 =s1_all((s1_all(:,1)== X_All & s1_all(:,2)== Y_T4),3);
s1_T5 =s1_all((s1_all(:,1)== X_All & s1_all(:,2)== Y_T5),3);
s1_T6 =s1_all((s1_all(:,1)== X_All & s1_all(:,2)== Y_T6),3);
s1_T7 =s1_all((s1_all(:,1)== X_All & s1_all(:,2)== Y_T7),3);
s1_T8 =s1_all((s1_all(:,1)== X_All & s1_all(:,2)== Y_T8),3);
s1_T9 =s1_all((s1_all(:,1)== X_All & s1_all(:,2)== Y_T9),3);

D_hydro_Waterlevel = [s1_T1,s1_T2,s1_T3,s1_T4,...
    s1_T5,s1_T6,s1_T7,s1_T8, s1_T9];

Point = ["T1_up","T2_up","T3_up", "T4_up", "T5_up", "T6_up", "T7_up", "T8_up", "T9_up"];
Waterlevel = [Point;num2cell(D_hydro_Waterlevel)];
Waterlevel_out = transpose(Waterlevel);

filename = 'd:\05_Projects\05_27_FM_testing\testbench\cases\e02_dflowfm\f105_cross-sections\c04_rectangular-profile\analysis\result\D_hydro_waterdepth.xlsx';
xlswrite(filename,waterdepth_out);

filename = 'd:\05_Projects\05_27_FM_testing\testbench\cases\e02_dflowfm\f105_cross-sections\c04_rectangular-profile\analysis\result\D_hydro_waterlevel.xlsx';
xlswrite(filename,Waterlevel_out);
