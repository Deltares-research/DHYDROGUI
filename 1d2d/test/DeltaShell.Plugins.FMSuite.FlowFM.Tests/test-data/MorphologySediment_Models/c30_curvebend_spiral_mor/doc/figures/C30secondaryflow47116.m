%Created by Ao_ 23/12/2016
% This script is to plot bed load sediment transport maps of the FMstructured grid  and D3D in c30 folder
% and compare the result with unstructured grid c54 folder
close all
clear all
clc
d3d_qp('close')
try 
    oetroot; 
catch 
    oetsettings;
end 

path1 = fullfile(cd, '..\..\dflowfmoutput\c30smallbend_map.nc');
path2 = fullfile(cd, '..\..\..\c54_curvebend_spiral_mor_Unstr\dflowfmoutput\c30smallbendUnstr_map.nc');
path3 = fullfile(cd, '..\..\delft3d\trim-c30smallbend.dat');

% structured Grid loading c30
d3d_qp('openfile', path1 )
d3d_qp('selectfield','bed load transport - nmesh2d_edge: mean')
d3d_qp('component','magnitude')
d3d_qp('colourmap','jet')
d3d_qp('climmode','manual')
d3d_qp('climmin',0)
d3d_qp('climmax',2.5e-06)
d3d_qp('markersize',0.5)
d3d_qp('allt', 0)
d3d_qp('editt', 3)
%d3d_qp('newfigure','3 plots, horizontal - landscape','3 plots, horizontal - landscape')
d3d_qp('newfigure','3 plots, vertical - portrait','3 plots, vertical - portrait')
d3d_qp('figureborderstyle','none')
d3d_qp('figurepapertype','A3','portrait')
%d3d_qp('selectaxes','middle plot')
%d3d_qp('selectaxes','lower plot')
d3d_qp('selectaxes','upper plot')
%d3d_qp('selectaxes','left plot')
%d3d_qp('quickview')
d3d_qp('addtoplot')
box on
grid on
title('C30-FM-str')

% Unstrctural grid looding
d3d_qp('openfile',  path2 )
d3d_qp('allt', 0)
d3d_qp('editt', 3)
d3d_qp('markersize',0.5)
d3d_qp('selectaxes','middle plot')
d3d_qp('addtoplot')
box on
grid on
title('C30-FM-Unstr')

%delft3D simulation looding
d3d_qp('openfile',  path3 )
d3d_qp('selectfield','bed load transport')
d3d_qp('colourmap','jet')
d3d_qp('climmode','manual')
d3d_qp('markersize',0.5)
d3d_qp('climmin',0)
d3d_qp('climmax',2.5e-06)
d3d_qp('allt', 0)
d3d_qp('editt', 3)
d3d_qp('selectaxes','lower plot')
d3d_qp('addtoplot')

box on
grid on
title('C30-D3D')

d3d_qp('printfigure','C30-2flowcoupling-47116.png','PNG file',2,300,1,1,1)
%d3d_qp('closefigure')