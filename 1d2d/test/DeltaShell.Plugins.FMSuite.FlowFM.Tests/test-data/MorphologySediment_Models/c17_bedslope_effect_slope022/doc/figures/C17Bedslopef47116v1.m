%Created by Ao_ 23/12/2016
% This script is to plot bed load transport along longitudinal cross-section of the FMstructured grid  and D3D in c17 folder
% and compare the result with unstructured grid c51 folder

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
d3d_qp('openfile', path1)
%d3d_qp('selectfield','Time-varying bottom level in flow cell centers - nmesh2d_face: mean')
d3d_qp('selectfield','bed load transport - nmesh2d_edge: mean')
d3d_qp('component','magnitude')
d3d_qp('axestype','X-Val')
d3d_qp('editt',16)
d3d_qp('hselectiontype','(X,Y) point/path')
d3d_qp('editxy','-4.096, 2.6; -2.488, 2.6')
d3d_qp('colour',[ 0 0 1 ])
d3d_qp('axestype','X-Val')
d3d_qp('quickview')
% d3d_qp('colour',[ 1 0 0 ])
% d3d_qp('editt',3)
% d3d_qp('addtoplot')

%ustr-grid c51
d3d_qp('openfile',  path2 )
%d3d_qp('selectfield', 'bed level in water level points')
d3d_qp('selectfield','bed load transport - nmesh2d_edge: mean')
d3d_qp('component','magnitude')
d3d_qp('axestype','X-Val')
d3d_qp('allt', 0)
d3d_qp('editt', 16)
d3d_qp('hselectiontype','(X,Y) point/path')
d3d_qp('editxy','-4.096, 2.6; -2.488, 2.6')
d3d_qp('colour',[ 0 0 0 ])
%d3d_qp('linestyle','--')
d3d_qp('addtoplot')

%D3D in c17
d3d_qp('openfile',  path3 )
%d3d_qp('selectfield', 'bed level in water level points')
d3d_qp('selectfield','bed load transport')
d3d_qp('component','magnitude')
d3d_qp('axestype','X-Val')
d3d_qp('allt', 0)
d3d_qp('editt', 16)
d3d_qp('hselectiontype','(X,Y) point/path')
d3d_qp('editxy','-4.096, 2.6; -2.488, 2.6')
d3d_qp('colour',[ 1 0 1 ])
%d3d_qp('linestyle','--')
d3d_qp('addtoplot')
box on
legend ('FMStr','FM-unstr','D3D', 'Location','NorthEast')
%title('18-Feb-2000 00:20:00- C17-bed-Slope-Effect47102')
grid on
d3d_qp('printfigure','C17-bed-Slope-Effect47116.png','PNG file',2,300,1,1,1)
%d3d_qp('closefigure')