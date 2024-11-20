%Created by Ao_ 23/12/2016
% This script is to plot bed level along the longitudinal centerline of the FMstructured grid 
close all
clear all
clc
try 
    oetroot; 
catch 
    oetsettings;
end 

path1 = fullfile(cd, '..\..\..\c37_dad_2dredge1dump\DFM_OUTPUT_c37\c37_map.nc');
d3d_qp('openfile', path1 )
d3d_qp('selectfield','Time-varying bottom level in flow cell centers - nmesh2d_face: mean')
d3d_qp('editt',1)
d3d_qp('hselectiontype','(X,Y) point/path')
d3d_qp('editxy','0.294, 0.266; 0.605, 0.255; 3.588, 0.249; 7.177, 0.252; 12.892, 0.263; 15.893, 0.249; 19.492, 0.250; 23.978, 0.250; 29.989, 0.253')
d3d_qp('colour',[ 0 0 1 ])
d3d_qp('axestype','X-Val')
d3d_qp('quickview')
d3d_qp('colour',[ 1 0 0 ])
d3d_qp('editt',64)
d3d_qp('addtoplot')
d3d_qp('openfile', path1 )
d3d_qp('selectfield','Time-varying bottom level in flow cell centers - nmesh2d_face: mean')
d3d_qp('colour',[ 0 0 0 ])
d3d_qp('addtoplot')
box on
legend ('orginal-bed','bed-withDredging','bed-withoutDredging', 'Location','SouthWest')
title('18-Feb-2000 00:20:00- C37-dredge(2)-dump(1)')
grid on
d3d_qp('printfigure','C37-bedComparison-Dredge(2)-dump(1).png','PNG file',2,300,1,1,1)
%d3d_qp('closefigure')