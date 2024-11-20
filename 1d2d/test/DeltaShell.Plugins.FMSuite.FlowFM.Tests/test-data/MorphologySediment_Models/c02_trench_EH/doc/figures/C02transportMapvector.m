Created by Ao_ 23/12/2016
% This script is to plot bed load transport  (vector Map) of the FMstructured grid and compare the result with unstructured grid and D3D
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
ath1 = fullfile(cd, '..\..\dflowfmoutput\t01_map.nc');
path2 = fullfile(cd, '..\..\delft3d\trim-c02-d3d.dat);
path3 = fullfile(cd, '..\..\..\c48_trench_EH_Unstr\dflowfmoutput\t02Unstr_map.nc');

%structured grid result from c02
d3d_qp('openfile',path1)
d3d_qp('selectfield','bed load transport - nmesh2d_edge: mean')
d3d_qp('hselectiontype','M range and N range')
d3d_qp('editt',16)
d3d_qp('component','vector')
d3d_qp('colour',[ 0 0 1 ])
d3d_qp('quickview')
%Unstructured grid result from c48
d3d_qp('openfile',path3)
d3d_qp('colour',[ 1 0 0 ])
d3d_qp('addtoplot')

%D3D result from c02
d3d_qp('openfile','Path2')
d3d_qp('selectfield','bed load transport')
d3d_qp('colour',[ 0 0 0 ])
d3d_qp('addtoplot')
box on
legend ('FMStr','','','FM-unstr','','','D3D','','','Location','SouthOutside')
axis([14 21 0 0.3]);
%title('bed load transport: Sediment sand 05-Oct-2015 00:04:00-C17-47102')
grid on
d3d_qp('printfigure','C02-Uniform sediment VectorMap.png','PNG file',2,300,1,1,1)
%d3d_qp('closefigure')