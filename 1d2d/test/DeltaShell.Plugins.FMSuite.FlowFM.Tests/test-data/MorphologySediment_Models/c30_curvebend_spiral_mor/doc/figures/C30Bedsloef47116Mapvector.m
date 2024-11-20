%Created by Ao_ 23/12/2016
% This script is to plot secondary flow intensity map of the FMstructured grid  and D3D in c30 folder
% and compare the result with unstructured grid c54 folder
close all
clear all
clc
d3d_qp('close')
try 
    oetroot; 
catch 
    oetsettings;
end 
path1 = fullfile(cd, '..\..\dflowfmoutput\c30smallbend_map.nc');
path2 = fullfile(cd, '..\..\..\c54_curvebend_spiral_mor_Unstr\dflowfmoutput\c30smallbendUnstr_map.nc');
path3 = fullfile(cd, '..\..\delft3d\trim-c30smallbend.dat');

% structured Grid loading c30
d3d_qp('openfile',path1 )
d3d_qp('selectfield','bed load transport - nmesh2d_edge: mean')
d3d_qp('hselectiontype','M range and N range')
d3d_qp('editt',3)
d3d_qp('component','vector')
d3d_qp('colour',[ 0 0 1 ])
d3d_qp('quickview')
% unstructured Grid loading c54
d3d_qp('openfile', path2 )
d3d_qp('colour',[ 1 0 0 ])
d3d_qp('addtoplot')
% d3d loading c30
d3d_qp('openfile',' path3 ')
d3d_qp('colour',[ 0 0 0 ])
d3d_qp('addtoplot')
box on
legend ('FMStr','','','FM-unstr','','','D3D','','','Location','EastOutside')
axis([-4.5 -2 1.5 4]);
title('bed load transport: Sediment sand -C30-47116')
grid on
d3d_qp('printfigure','C30-bedTransport_2flow-VectorMap.png','PNG file',2,300,1,1,1)
%d3d_qp('closefigure')