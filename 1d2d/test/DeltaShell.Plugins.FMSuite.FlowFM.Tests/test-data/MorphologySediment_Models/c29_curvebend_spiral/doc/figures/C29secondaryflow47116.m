%Created by Ao_ 23/12/2016
% This script is to plot secondary flow intensity map of the FMstructured grid  and D3D in c29 folder
% and compare the result with unstructured grid c53 folder
close all
clear all
clc
%d3d_qp('close')
try 
    oetroot; 
catch 
    oetsettings;
end 
path1 = fullfile(cd, '..\..\dflowfmoutput\smallbend_map.nc');
path2 = fullfile(cd, '..\..\..\c53_curvebend_spiral_Unstr\dflowfmoutput\smallbend_unstr_map.nc');
path3 = fullfile(cd, '..\..\delft3d\trim-smallbend.dat');

% structured Grid loading c29
d3d_qp('openfile', path1)
d3d_qp('selectfield','Spiral flow intensity - nmesh2d_face: mean')
d3d_qp('colourmap','jet')
d3d_qp('climmode','manual')
d3d_qp('climmin',0)
d3d_qp('climmax',0.018)
d3d_qp('allt', 0)
d3d_qp('editt', 3)
%d3d_qp('newfigure','3 plots, horizontal - landscape','3 plots, horizontal - landscape')
d3d_qp('newfigure','3 plots, vertical - portrait','3 plots, vertical - portrait')
%d3d_qp('selectaxes','middle plot')
%d3d_qp('selectaxes','lower plot')
d3d_qp('selectaxes','upper plot')
%d3d_qp('selectaxes','left plot')
%d3d_qp('quickview')
d3d_qp('addtoplot')

box on
grid on
title('C29-FM-str')
% Unstrctural grid looding
d3d_qp('openfile', path2 )
d3d_qp('allt', 0)
d3d_qp('editt', 3)
d3d_qp('selectaxes','middle plot')
d3d_qp('addtoplot')
box on
grid on
title('C29-FM-Unstr')
%delft3D simulation looding
d3d_qp('openfile', path3 )
d3d_qp('selectfield','secondary flow')
d3d_qp('colourmap','jet')
d3d_qp('climmode','manual')
d3d_qp('climmin',0)
d3d_qp('climmax',0.018)
d3d_qp('allt', 0)
d3d_qp('editt', 3)
%d3d_qp('newfigure','3 plots, horizontal - landscape','3 plots, horizontal - landscape')
%d3d_qp('newfigure','3 plots, vertical - portrait','3 plots, vertical - portrait')
d3d_qp('selectaxes','lower plot')
d3d_qp('addtoplot')

box on
%legend ('FMStr','FM-unstr','D3D', 'Location','SouthOutside')
title('C29-D3D')
grid on
d3d_qp('printfigure','C29-2flow-47116.png','PNG file',2,300,1,1,1)
%d3d_qp('closefigure')