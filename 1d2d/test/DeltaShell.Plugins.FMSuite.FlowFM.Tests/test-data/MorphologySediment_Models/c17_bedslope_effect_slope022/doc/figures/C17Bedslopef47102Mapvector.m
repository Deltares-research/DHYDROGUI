%Created by Ao_ 23/12/2016
% This script is to plot bed load transport vector map of the FMstructured grid  and D3Dc17 and compare the result with unstructured grid c51
close all
clear all
clc
d3d_qp('close')
try 
    oetroot; 
catch 
    oetsettings;
end 

path1 = fullfile(cd, '..\..\dflowfmoutput\str_map.nc');
path2 = fullfile(cd, '..\..\..\c50_fixed_layer_Unstr\dflowfmoutput\t1-unstr_map.nc'');
path3 = fullfile(cd, '..\..\delft3dv1\trim-str.dat');

%structural grid c17
d3d_qp('openfile', path1 )
d3d_qp('selectfield','bed load transport - nmesh2d_edge: mean')
d3d_qp('hselectiontype','M range and N range')
d3d_qp('editt',16)
d3d_qp('component','vector')
d3d_qp('colour',[ 0 0 1 ])
d3d_qp('quickview')
%ustr-grid c51
d3d_qp('openfile', path2 )
d3d_qp('colour',[ 1 0 0 ])
d3d_qp('addtoplot')
%D3D in c17
d3d_qp('openfile', path3 )
d3d_qp('selectfield','bed load transport')
d3d_qp('colour',[ 0 0 0 ])
d3d_qp('addtoplot')

box on
legend ('FMStr','','','FM-unstr','','','D3D','','','Location','EastOutside')
axis([-3.5 -3.2 8.4 9.6]);
title('bed load transport: Sediment sand 05-Oct-2015 00:04:00-C17-47102')
grid on
d3d_qp('printfigure','C17-bed-Slope-Effect VectorMap.png','PNG file',2,300,1,1,1)
%d3d_qp('closefigure')