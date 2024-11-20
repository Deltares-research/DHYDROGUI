%Created by Ao_ 23/12/2016
% This script is to plot bed level along the longitudinal centerline of the FMstructured grid
close all
clear all
clc
d3d_qp('close')
try 
    oetroot; 
catch 
    oetsettings;
end 

path1 = fullfile(cd, '..\..\..\c43_daddumpisdredge_layered_bed\DFM_OUTPUT_test\test_map.nc');

d3d_qp('openfile', path1 )
d3d_qp('selectfield','Time-varying bottom level in flow cell centers - nmesh2d_face: mean')
d3d_qp('editt',1)
d3d_qp('hselectiontype','(X,Y) point/path')
d3d_qp('editxy','0.440, 0.228; 29.948, 0.184')
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
legend ('orginal-bed','bed-withDredging','bed-withoutDredging', 'Location','SouthOutside')
title('18-Feb-2000 00:20:00- C43-dredgeisdump')
grid on
d3d_qp('printfigure','C43-bedComparison-dredgeisdump.png','PNG file',2,300,1,1,1)
%d3d_qp('closefigure')