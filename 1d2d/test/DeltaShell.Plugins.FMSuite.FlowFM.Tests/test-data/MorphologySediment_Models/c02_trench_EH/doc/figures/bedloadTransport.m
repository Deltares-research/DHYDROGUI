%Created by Ao_ 23/12/2016
% This script is to plot bed load transport of the FMstructured grid and compare the result with unstructured grid and D3D
%
close all
clear all
clc
d3d_qp('close')
try 
    oetroot; 
catch 
    oetsettings;
end 
path1 = fullfile(cd, '..\..\dflowfmoutput\t01_map.nc');
path2 = fullfile(cd, '..\..\delft3d\trim-c02-d3d.dat);
path3 = fullfile(cd, '..\..\..\c48_trench_EH_Unstr\dflowfmoutput\t02Unstr_map.nc');

%structured grid result from c02
d3d_qp('openfile', path1)
d3d_qp('selectfield','bed load transport - nmesh2d_edge: mean')
d3d_qp('editt',31)
d3d_qp('hselectiontype','(X,Y) point/path')
d3d_qp('editxy','0.294, 0.266; 0.605, 0.255; 3.588, 0.249; 7.177, 0.252; 12.892, 0.263; 15.893, 0.249; 19.492, 0.250; 23.978, 0.250; 29.989, 0.253')
d3d_qp('colour',[ 1 0 0 ])
d3d_qp('axestype','X-Val')
d3d_qp('quickview')
% d3d_qp('colour',[ 1 0 0 ])
% d3d_qp('editt',31)
% d3d_qp('addtoplot')

%D3D result from c02
d3d_qp('openfile',path2)
d3d_qp('selectfield','bed load transport')
d3d_qp('colour',[ 0 0 0 ])
d3d_qp('addtoplot')

%Unstructured grid result from c48
d3d_qp('openfile',path3)
d3d_qp('selectfield','bed load transport - nmesh2d_edge: mean')
d3d_qp('colour',[ 0 1 0 ])
d3d_qp('addtoplot')
box on
legend ('bed-FMstr','bed-D3D','bed-FMunstr', 'Location','SouthWest')
%title('18-Feb-2000 00:20:00- C35-dumping-Inside')
grid on
d3d_qp('printfigure','C02-bedloadTransport.png','PNG file',2,300,1,1,1)
%d3d_qp('closefigure')