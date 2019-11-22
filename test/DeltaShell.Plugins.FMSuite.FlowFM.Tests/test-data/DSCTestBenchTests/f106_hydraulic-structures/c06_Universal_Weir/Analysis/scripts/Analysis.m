clear all; close all; clc;
 
%% Analysis of testcase https://repos.deltares.nl/repos/DSCTestbench/trunk/cases/e02_dflowfm/f106_hydraulic-structures/c02_Orifice

%% Settings and parameters
format long

% To use the qp plot path. This should be adjusted to your path
run('d:\05_Projects\05_27_FM_testing\openearthtools\Matlab\oetsettings.m')

% Select outputpath of model run
f_map = '..\..\DFM_OUTPUT_Flow1D\Flow1D_his.nc';
FileInfo = qpfopen(f_map);
[DataFields, Dims, Nval] = qpread(FileInfo);

% Define variabele of interest
var1 = 'Discharge through universal weir';
var2 = 'Water level upstream of universal weir';
var3 = 'Water level downstream of universal weir'; 

% Weir Names
Weir = qpread(FileInfo, var1,'stations');

% Get discharge at weir at all timesteps
Q_weir = qpread(FileInfo, var1,'data', 0, 0);

% discharge per network
T1_B1_weir_x55m = Q_weir.Val(:,1);
T2_B1_weir_x55m = Q_weir.Val(:,2);

% up and downstream weir water level
hup_weir  = qpread(FileInfo, var2,'data', 0, 0);
hup = hup_weir.Val(:,1);
hdwn_weir  = qpread(FileInfo, var3,'data', 0, 0);
hdwn = hdwn_weir.Val(:,1); 

% filename = '..\results\Analysis.xlsx';
% xlswrite(filename,hup,'upstream');

% filename = '..\results\Analysis.xlsx';
% xlswrite(filename,hdwn,'downstream');





 





