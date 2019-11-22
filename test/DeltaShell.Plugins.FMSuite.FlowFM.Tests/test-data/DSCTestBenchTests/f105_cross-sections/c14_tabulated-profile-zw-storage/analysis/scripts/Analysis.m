clear all; close all; clc;
 
%% Analysis of testcase https://repos.deltares.nl/repos/DSCTestbench/trunk/cases/e02_dflowfm/f105_cross-sections/c09_tabulated-profile-storage/

%% Settings and parameters
format long

% To use the qp plot path. This should be adjusted to your path
run('d:\Software\OpenEarthTools\oetsettings.m')

% Select outputpath of model run
f_map = '..\..\DFM_OUTPUT_ZW_Storage\ZW_Storage_map.nc';

% Define variabele of interest
var1 = 'Water level';
var2 = 'Discharge through flow link at current time - nmesh1d_edge: sum';

% Open output of model

% coordinates of the connection nodes
X_T1_0 = 2000; X_T1_1000 = 1000; Y_T1 = 0;
X_T2_0 = 2000; X_T2_1750 = 1750; Y_T2 = 250;
X_T3_0 = 2000; X_T3_1000 = 1000; Y_T3 = 500;

FileInfo = qpfopen(f_map);

% Check DataFields for the correct name of the variable (not the variable
% as mentioned in nc-file but as mentioned by quickplot
[DataFields, Dims, Nval ] = qpread(FileInfo);

% Timestep of interest based on 
meta = qpread(FileInfo, var1 ,'size');

% Because we are only interested in last timestep 
t = meta(1);

% Get water level data
s1 = qpread(FileInfo, var1,'griddata', 0);
s1_tm = transpose(s1.Val);
s1_all = [s1.X,s1.Y, s1_tm];
Level = [];

for i=1:t
    s1_T1_0 = s1_all((s1_all(:,1)== X_T1_0 & s1_all(:,2)== Y_T1),i+2);
    s1_T1_1000 = s1_all((s1_all(:,1)== X_T1_1000 & s1_all(:,2)== Y_T1),i+2);
    
    s1_T2_0 = s1_all((s1_all(:,1)== X_T2_0 & s1_all(:,2)== Y_T2),i+2);
    s1_T2_250 = s1_all((s1_all(:,1)== X_T2_1750 & s1_all(:,2)== Y_T2),i+2);
    
    s1_T3_0 = s1_all((s1_all(:,1)== X_T3_0 & s1_all(:,2)== Y_T3),i+2);
    s1_T3_1000 = s1_all((s1_all(:,1)== X_T3_1000 & s1_all(:,2)== Y_T3),i+2);
    
    level = [s1_T1_0,s1_T1_1000,s1_T2_0,s1_T2_250,s1_T3_0,s1_T3_1000];
    Level(i,:) = [i,level];   
end

Point = ["Time Steps","s1_T1_0","s1_T1_1000","s1_T2_0","s1_T2_250","s1_T3_0","s1_T3_1000"];
sT = [Point;num2cell(Level)];

% % Get discharge data
% Q1 = qpread(FileInfo, var2, 'griddata', 0);
% xnodes = Q1.X(Q1.EdgeNodeConnect);
% ynodes = Q1.Y(Q1.EdgeNodeConnect);
% Q1_tm = transpose(Q1.Val);
% Q1comp = [xnodes ynodes Q1_tm];
% 
% % Find the location with Q
% Discharge = [];
% for i=1:t
%     Q1_T1 = Q1comp((Q1comp(:,1)== X_T1 & Q1comp(:,3) == Y_T1),i+4);
%     Q1_T1 = Q1comp((Q1comp(:,1)== X_T1 & Q1comp(:,3) == Y_T1),i+4);
% 
%     Q1_T2 = Q1comp((Q1comp(:,1)== X_T2 & Q1comp(:,3) == Y_T2),i+4);
%     Q1_T2 = Q1comp((Q1comp(:,1)== X_T2 & Q1comp(:,3) == Y_T2),i+4);
% 
%     Q1_T3 = Q1comp((Q1comp(:,1)== X_T3 & Q1comp(:,3) == Y_T3),i+4); 
%     Q1_T3 = Q1comp((Q1comp(:,1)== X_T3 & Q1comp(:,3) == Y_T3),i+4); 
% 
%     discharge = [Q1_T1,Q1_T2,Q1_T3];
%     Discharge(i,:) = [i,discharge];   
% end
% 
% Point = ["Time Step","Q1_T1","Q1_T2","Q1_T3"];
% QT = [Point;num2cell(Discharge)];
% 
% % Select outputpath of model run
% f_his = '..\..\DFM_OUTPUT_YZ_Storage\YZ_Storage_his.nc';
% 
% FileInfo = qpfopen(f_his)
% 
% % Check DataFields for the correct name of the variable (not the variable
% % as mentioned in nc-file but as mentioned by quickplot
% [DataFields, Dims, Nval ] = qpread(FileInfo);
% 
% % Define variabele of interest
% var1 = 'water level (points)'
% 
% sp = qpread(FileInfo, var1, 'griddata', 0,0)
% 
% Waterlevel_obs = sp.Val
% 
% Point = ["Obs_T1_BC","Obs_T1_XY","Obs_T2_BC","Obs_T3_XY"]
% Watelevel_obsT = [Point;num2cell(Waterlevel_obs)];
% 
%  





