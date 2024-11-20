%Created by Ao_ 23/12/2016
% This script is to plot secondary flow intensity along longitudinal cross-section of the FMstructured grid  and D3D in c29 folder
% and compare the result with unstructured grid c53 folder
close all
clear all
clc
d3d_qp('close')
try 
    oetroot; 
catch 
    oetsettings;
end 

path1 = fullfile(cd, '..\..\dflowfmoutput\smallbend_map.nc');
path2 = fullfile(cd, '..\..\..\c53_curvebend_spiral_Unstr\dflowfmoutput\smallbend_unstr_map.nc');
path3 = fullfile(cd, '..\..\delft3d\trim-smallbend.dat');
%Str c29
d3d_qp('openfile', path1 )
d3d_qp('selectfield','Spiral flow intensity - nmesh2d_face: mean')
d3d_qp('hselectiontype','(X,Y) point/path')
d3d_qp('editxy','-4.255, -5.997; -4.248, -4.884; -4.252, -0.442; -4.247, -0.213; -4.251, -0.003; -4.231, 0.228; -4.217, 0.441; -4.135, 0.882; -4.079, 1.088; -4.030, 1.301; -3.952, 1.510; -3.859, 1.704; -3.768, 1.898; -3.665, 2.096; -3.407, 2.471; -2.979, 2.961; -2.665, 3.244; -2.108, 3.620; -1.511, 3.908; -0.872, 4.097; -0.223, 4.177; 0.435, 4.155; 0.683, 4.130; 1.231, 4.005; 1.761, 3.807; 2.157, 3.597; 2.590, 3.301; 2.976, 2.963; 3.274, 2.639; 3.661, 2.096; 3.772, 1.903; 3.953, 1.498; 4.079, 1.096; 4.183, 0.660; 4.225, 0.232; 4.252, -0.212; 4.250, -5.994')
d3d_qp('editt',3)
d3d_qp('colour',[ 0 0 1 ])
d3d_qp('quickview')
%Unstr c53
d3d_qp('openfile',  Path2 )
d3d_qp('colour',[ 1 0 0 ])
d3d_qp('addtoplot')
 % d3d c29
d3d_qp('openfile',  path3)
d3d_qp('selectfield','secondary flow')
d3d_qp('colour',[ 0 0 0 ])
d3d_qp('addtoplot')


box on
legend ('FMStr','FM-unstr','D3D','Location','Best')
%axis([-3.4 -3.2 4.4 5.4]);
title('Seconday flow along the centreline -c29-47116')
grid on
d3d_qp('printfigure','C29Seconday flow-47116.png','PNG file',2,300,1,1,1)
%d3d_qp('closefigure')