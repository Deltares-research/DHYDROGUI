%Created by Ao_ 23/12/2016
% This script is to plot depth average velocity along the longitudinal centerline of the FMstructured grid
close all
clear all
clc
d3d_qp('close')
try 
    oetroot; 
catch 
    oetsettings;
end 

path1 = fullfile(cd, '..\..\..\c46_dad_inside_nourishment\DFM_OUTPUT_c46\c46_map.nc');
path2 = fullfile(cd, '..\..\..\c46_dad_inside_nourishment\D3D\trim-c46_nor.dat');

d3d_qp('openfile', path1 )
d3d_qp('selectfield','normal velocity at velocity point - nmesh2d_edge: mean')
d3d_qp('editt',1)
d3d_qp('hselectiontype','(X,Y) point/path')
d3d_qp('editxy','0.294, 0.266; 0.605, 0.255; 3.588, 0.249; 7.177, 0.252; 12.892, 0.263; 15.893, 0.249; 19.492, 0.250; 23.978, 0.250; 29.989, 0.253')
d3d_qp('colour',[ 0 0 1 ])
d3d_qp('axestype','X-Val')
d3d_qp('quickview')
d3d_qp('colour',[ 1 0 0 ])
d3d_qp('editt',401)
d3d_qp('addtoplot')

d3d_qp('openfile',  path2 )
d3d_qp('selectfield','depth averaged velocity')
d3d_qp('allt', 0)
d3d_qp('editt', 401)
d3d_qp('hselectiontype','(X,Y) point/path')
d3d_qp('editxy','0.294, 0.266; 0.605, 0.255; 3.588, 0.249; 7.177, 0.252; 12.892, 0.263; 15.893, 0.249; 19.492, 0.250; 23.978, 0.250; 29.989, 0.253')
d3d_qp('colour',[ 0 0 0 ])
%d3d_qp('linestyle','--')
d3d_qp('addtoplot')
box on
legend ('Initial','FM','D3D', 'Location','SouthWest')
%title('18-Feb-200000:00:00 start time- C46-Nourishment47116')
grid on
d3d_qp('printfigure','C46velocityresult.png','PNG file',2,300,1,1,1)
%d3d_qp('closefigure')