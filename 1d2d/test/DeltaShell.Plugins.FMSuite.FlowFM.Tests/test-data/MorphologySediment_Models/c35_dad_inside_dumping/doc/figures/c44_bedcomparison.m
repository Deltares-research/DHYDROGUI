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
path1 = fullfile(cd, '..\..\..\c44_dad_sandmining_layered_bed\DFM_OUTPUT_da3\da3_map.nc');

d3d_qp('openfile', path1 )
d3d_qp('selectfield','Time-varying bottom level in flow cell centers - nmesh2d_face: mean')
d3d_qp('editt',1)
d3d_qp('hselectiontype','(X,Y) point/path')
d3d_qp('editxy','48.135, 43.552; 9983.656, 43.552')
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
title('18-Feb-2000 00:20:00- C44-sandmining')
grid on
d3d_qp('printfigure','C44-bedComparison-sandmining.png','PNG file',2,300,1,1,1)
%d3d_qp('closefigure')