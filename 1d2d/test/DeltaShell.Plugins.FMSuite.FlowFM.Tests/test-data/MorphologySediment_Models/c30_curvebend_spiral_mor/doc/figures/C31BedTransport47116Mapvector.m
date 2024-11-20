%Created by Ao_ 23/12/2016
% This script is to plot bed load sediment transport vector map of the FMstructured grid  and D3D in c31 folder
% and compare the result with unstructured grid c55 folder
close all
clear all
clc
d3d_qp('close')
try 
    oetroot; 
catch 
    oetsettings;
end 
path1 = fullfile(cd, '..\..\..\c31_curvebend_spiral_mor_Bedup\dflowfmoutput\c31smallbend_map.nc');
path2 = fullfile(cd, '..\..\..\c55_curvebend_spiral_mor_Bedup_Unstr\dflowfmoutput\c31smallbendUnstr_map.nc');
path3 = fullfile(cd, '..\..\..\c31_curvebend_spiral_mor_Bedup\delft3d\trim-c31smallbend.dat');

% structured Grid loading c31
d3d_qp('openfile', path1 )
d3d_qp('selectfield','bed load transport - nmesh2d_edge: mean')
d3d_qp('hselectiontype','M range and N range')
d3d_qp('editt',3)
d3d_qp('component','vector')
d3d_qp('colour',[ 0 0 1 ])
d3d_qp('quickview')
% Unstructured Grid loading c55
d3d_qp('openfile', path2 )
d3d_qp('colour',[ 1 0 0 ])
d3d_qp('addtoplot')
% d3d loading c31
d3d_qp('openfile', path3 )
d3d_qp('colour',[ 0 0 0 ])
d3d_qp('addtoplot')
box on
legend ('FMStr','','','FM-unstr','','','D3D','','','Location','EastOutside')
axis([-4.5 -2 1.5 4]);
title('bed load transport: Sediment sand -C30-47116')
grid on
d3d_qp('printfigure','C30-bedTransport_2flow-VectorMap.png','PNG file',2,300,1,1,1)
%d3d_qp('closefigure')