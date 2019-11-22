clear all; close all; clc;
 
%% Analysis of testcase https://repos.deltares.nl/repos/DSCTestbench/trunk/cases/e02_dflowfm/f104_1D_numerical-aspects/c04_junction-advection-deceleration-non-equidistant

%% Settings and parameters
format long

% To use the qp plot path. This should be adjusted to your path
run('d:\Software\OpenEarthTools\oetsettings.m')

% Select outputpath of model run
f_out = '..\..\DFM_OUTPUT_Flow1D\Flow1d_map.nc';

% Define variabele of interest
var1 = 'Water depth at pressure points';
var2 = 'Water level';
var3 = 'Discharge through flow link at current time - nmesh1d_edge: sum';
var4 = 'Velocity at velocity point, n-component - nmesh1d_edge: mean';

% Location connection node
X_cn = 15000;

% Open output of model
FileInfo = qpfopen(f_out);

% Check DataFields for the correct name of the variable (not the variable
% as mentioned in nc-file but as mentioned by quickplot
[DataFields, Dims, Nval ] = qpread(FileInfo);

% Timestep of interest based on 
meta = qpread(FileInfo, var1 ,'size');

% Because we are only interested in last timestep 
t = meta(1);

% Determine the number of branches 
branch_id = qpread(FileInfo, 'Number of the branch on which the node is located','data',0);
branches = unique(branch_id.Val);
% In total 9  branches were found, each id for each system
% 0 -> T1
% 1,2 --> T2
% 3,4,5 --> T3
% 6,7,8,9 -->T4

% Get water depth data
h1 = qpread(FileInfo, var1,'griddata', t);

h1_T1 = [h1.X(branch_id.Val==0), h1.Val(branch_id.Val==0)];
h1_T2 = [h1.X(branch_id.Val==1), h1.Val(branch_id.Val==1)];
h1_T3 = [h1.X(branch_id.Val==3), h1.Val(branch_id.Val==3)];
h1_T4a = [h1.X(branch_id.Val==6), h1.Val(branch_id.Val==6)];

Depth = [h1_T1(h1_T1(:,1)==X_cn,2), h1_T2(h1_T2(:,1)==X_cn,2),h1_T3(h1_T3(:,1)==X_cn,2), h1_T4a(h1_T4a(:,1)==X_cn,2)];
Point = ["h1_T1","h1_T2","h1_T3","h1_T4"];

hT = table(transpose(Point),transpose(Depth));

% h1 = [h1_T1(h1_T1(:,1)==X_cn,2); h1_T2(h1_T2(:,1)==X_cn,2);...
%     h1_T3(h1_T3(:,1)==X_cn,2); h1_T4a(h1_T4a(:,1)==X_cn,2)];
% h1 = round(h1,6);


% Get water level data
s1 = qpread(FileInfo, var2,'griddata', t);

s1_T1 = [s1.X(branch_id.Val==0), s1.Val(branch_id.Val==0)];
s1_T2 = [s1.X(branch_id.Val==1), s1.Val(branch_id.Val==1)];
s1_T3 = [s1.X(branch_id.Val==3), s1.Val(branch_id.Val==3)];
s1_T4a = [s1.X(branch_id.Val==6), s1.Val(branch_id.Val==6)];

Level = [s1_T1(s1_T1(:,1)==X_cn,2), s1_T2(s1_T2(:,1)==X_cn,2), s1_T3(s1_T3(:,1)==X_cn,2), s1_T4a(s1_T4a(:,1)==X_cn,2)];
Point = ["s1_T1","s1_T2","s1_T3","s1_T4"];

sT = table(transpose(Point),transpose(Level));

% 
% s1 = [s1_T1(s1_T1(:,1)==X_cn,2); s1_T2(s1_T2(:,1)==X_cn,2);...
%     s1_T3(s1_T3(:,1)==X_cn,2); s1_T4a(s1_T4a(:,1)==X_cn,2)];
% s1 = round(s1,6);

% Get discharge data
Q1 = qpread(FileInfo, var3, 'griddata', t);
xnodes = Q1.X(Q1.EdgeNodeConnect);
ynodes = Q1.Y(Q1.EdgeNodeConnect);
Q1comp = [xnodes ynodes Q1.Val];

% the locations of of interest are:
% y=0,2500,11000,25500
% x=1500

% Find the location with Q
Q1_T1_Before = Q1comp(find(Q1comp(:,2)==15000 & Q1comp(:,3) == 0),5);
Q1_T1_After = Q1comp(find(Q1comp(:,1)==15000 & Q1comp(:,3) == 0),5);

Q1_T2_B1 = Q1comp(find(Q1comp(:,2)==15000 & Q1comp(:,3) == 2500),5);
Q1_T2_B2 = Q1comp(find(Q1comp(:,1)==15000 & Q1comp(:,3) == 2500),5);

Q1_T3_B1 = Q1comp(find(Q1comp(:,2)==15000 & Q1comp(:,3) == 11000),5);
Q1_T3_After = Q1comp(find(Q1comp(:,1)==15000 & Q1comp(:,3) == 11000),5);
Q1_T3_B2 = Q1_T3_After(2);
Q1_T3_B3 = Q1_T3_After(1);

Q1_T4_Before = Q1comp(find(Q1comp(:,2)==15000 & Q1comp(:,4) == 25500),5);
Q1_T4_B1 = Q1_T4_Before(1); 
Q1_T4_B4 = Q1_T4_Before(2); 

Q1_T4_After = Q1comp(find(Q1comp(:,1)==15000 & Q1comp(:,3) == 25500),5);
Q1_T4_B2 = Q1_T4_After(2); 
Q1_T4_B3 = Q1_T4_After(1); 

Discharge = [Q1_T1_Before,Q1_T1_After,Q1_T2_B1,Q1_T2_B2,Q1_T3_B1,Q1_T3_B2,Q1_T3_B3,Q1_T4_B1,Q1_T4_B2,Q1_T4_B3,Q1_T4_B4];
Point = ["Q1_T1_Before","Q1_T1_After","Q1_T2_B1","Q1_T2_B2","Q1_T3_B1","Q1_T3_B2","Q1_T3_B3","Q1_T4_B1","Q1_T4_B2","Q1_T4_B3","Q1_T4_B4"];

QT = table(transpose(Point),transpose(Discharge));

% % Get velocity data
u1 = qpread(FileInfo, var4, 'griddata', t);
xnodes = u1.X(u1.EdgeNodeConnect);
ynodes = u1.Y(u1.EdgeNodeConnect);
u1comp = [xnodes ynodes u1.Val];

% the locations of of interest are:
% y=0,2500,11000,25500
% x=1500

% Find the location with u
u1_T1_Before = u1comp(find(u1comp(:,2)==15000 & u1comp(:,3) == 0),5);
u1_T1_After = u1comp(find(u1comp(:,1)==15000 & u1comp(:,3) == 0),5);

u1_T2_B1 = u1comp(find(u1comp(:,2)==15000 & u1comp(:,3) == 2500),5);
u1_T2_B2 = u1comp(find(u1comp(:,1)==15000 & u1comp(:,3) == 2500),5);

u1_T3_B1 = u1comp(find(u1comp(:,2)==15000 & u1comp(:,3) == 11000),5);
u1_T3_After = u1comp(find(u1comp(:,1)==15000 & u1comp(:,3) == 11000),5);
u1_T3_B2 = u1_T3_After(2);
u1_T3_B3 = u1_T3_After(1);

u1_T4_Before = u1comp(find(u1comp(:,2)==15000 & u1comp(:,4) == 25500),5);
u1_T4_B1 = u1_T4_Before(1); 
u1_T4_B4 = u1_T4_Before(2); 

u1_T4_After = u1comp(find(u1comp(:,1)==15000 & u1comp(:,3) == 25500),5);
u1_T4_B2 = u1_T4_After(2); 
u1_T4_B3 = u1_T4_After(1); 

Velocity = [u1_T1_Before,u1_T1_After,u1_T2_B1,u1_T2_B2,u1_T3_B1,u1_T3_B2,u1_T3_B3,u1_T4_B1,u1_T4_B2,u1_T4_B3,u1_T4_B4];
Point = ["u1_T1_Before","u1_T1_After","u1_T2_B1","u1_T2_B2","u1_T3_B1","u1_T3_B2","u1_T3_B3","u1_T4_B1","u1_T4_B2","u1_T4_B3","u1_T4_B4"];

uT = table(transpose(Point),transpose(Velocity));










