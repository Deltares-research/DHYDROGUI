%Created by Ao_ 23/12/2016
% This script is to plot bed load sediment along a centerline of the FMstructured grid  and D3D in c31 folder
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
d3d_qp('openfile', path1)
d3d_qp('selectfield','Time-varying bottom level in flow cell centers - nmesh2d_face: mean')
d3d_qp('hselectiontype','(X,Y) point/path')
d3d_qp('editxy','-4.208, -6.000; -4.208, -5.778; -4.208, -5.556; -4.208, -5.333; -4.208, -5.111; -4.208, -4.889; -4.208, -4.667; -4.208, -4.444; -4.208, -4.222; -4.208, -4.000; -4.208, -3.778; -4.208, -3.556; -4.208, -3.333; -4.208, -3.111; -4.208, -2.889; -4.208, -2.667; -4.208, -2.444; -4.208, -2.222; -4.208, -2.000; -4.209, -1.777; -4.209, -1.555; -4.209, -1.332; -4.210, -1.110; -4.210, -0.887; -4.210, -0.665; -4.209, -0.442; -4.206, -0.221; -4.200, 0.000; -4.188, 0.219; -4.168, 0.436; -4.138, 0.651; -4.097, 0.865; -4.045, 1.076; -3.982, 1.284; -3.908, 1.488; -3.823, 1.689; -3.728, 1.884; -3.623, 2.075; -3.507, 2.260; -3.383, 2.439; -3.249, 2.611; -3.106, 2.776; -2.955, 2.933; -2.795, 3.082; -2.628, 3.223; -2.455, 3.354; -2.274, 3.477; -2.087, 3.590; -1.895, 3.693; -1.697, 3.786; -1.495, 3.869; -1.289, 3.941; -1.080, 4.003; -0.867, 4.053; -0.653, 4.092; -0.436, 4.121; -0.218, 4.138; -0.000, 4.143; 0.218, 4.138; 0.436, 4.121; 0.653, 4.092; 0.867, 4.053; 1.080, 4.003; 1.289, 3.941; 1.495, 3.869; 1.697, 3.786; 1.895, 3.693; 2.087, 3.590; 2.274, 3.477; 2.455, 3.354; 2.628, 3.223; 2.795, 3.082; 2.955, 2.933; 3.106, 2.776; 3.249, 2.611; 3.383, 2.439; 3.507, 2.260; 3.623, 2.075; 3.728, 1.885; 3.823, 1.689; 3.908, 1.488; 3.982, 1.284; 4.045, 1.076; 4.097, 0.865; 4.138, 0.651; 4.168, 0.436; 4.188, 0.219; 4.200, 0.000; 4.206, -0.221; 4.209, -0.443; 4.210, -0.665; 4.210, -0.887; 4.210, -1.110; 4.209, -1.332; 4.209, -1.555; 4.209, -1.777; 4.208, -2.000; 4.208, -2.222; 4.208, -2.444; 4.208, -2.667; 4.208, -2.889; 4.208, -3.111; 4.208, -3.333; 4.208, -3.556; 4.208, -3.778; 4.208, -4.000; 4.208, -4.222; 4.208, -4.444; 4.208, -4.667; 4.208, -4.889; 4.208, -5.111; 4.208, -5.333; 4.208, -5.556; 4.208, -5.778; 4.208, -6.000')
d3d_qp('editt',3)
d3d_qp('colour',[ 0 0 1 ])
d3d_qp('quickview')
% Unstructured Grid loading c55
d3d_qp('openfile', path2 )
d3d_qp('colour',[ 1 0 0 ])
d3d_qp('addtoplot')
% d3d loading c31
d3d_qp('openfile', path3 )
d3d_qp('selectfield','bed level in water level points')
d3d_qp('colour',[ 0 0 0 ])
d3d_qp('addtoplot')


box on
legend ('FMStr','FM-unstr','D3D','Location','Best')
%axis([-3.4 -3.2 4.4 5.4]);
title('Bed Change along the centreline -c31-47116')
grid on
d3d_qp('printfigure','C31_bedChange_centreline-47116.png','PNG file',2,300,1,1,1)
%d3d_qp('closefigure')