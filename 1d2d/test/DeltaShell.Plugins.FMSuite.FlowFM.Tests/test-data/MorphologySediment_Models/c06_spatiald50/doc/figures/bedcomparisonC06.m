%Created by Ao_ 23/12/2016
% This script is to plot the bed level of the FMstructured grid and compare the result with unstructured grid
% The tstop in the mdu has to be changed to "TStop = 8.0000000e+03 "" and run it
close all
clear all
clc
try 
    oetroot; 
catch 
    oetsettings;
end 
path1 = fullfile(cd, '..\..\dflowfmoutput\da3_map.nc);
path2 = fullfile(cd, '..\..\..\c49_spatiald50_Unstr\dflowfmoutput\da3_map.nc');
%str from c06
d3d_qp('openfile',path1)
d3d_qp('selectfield','Time-varying bottom level in flow cell centers - nmesh2d_face: mean')
d3d_qp('editt',1)
d3d_qp('hselectiontype','(X,Y) point/path')
d3d_qp('editxy','48.135, 43.552; 9983.656, 43.552')
d3d_qp('colour',[ 0 0 1 ])
d3d_qp('axestype','X-Val')
d3d_qp('quickview')
d3d_qp('colour',[ 1 0 0 ])
d3d_qp('editt',135)
d3d_qp('addtoplot')

%unstr from c49
d3d_qp('openfile',path2)
d3d_qp('selectfield','Time-varying bottom level in flow cell centers - nmesh2d_face: mean')
d3d_qp('colour',[ 0 0 0 ])
d3d_qp('addtoplot')
box on
legend ('orginal-bed','bed-strGrid','bed-UnstrGrid', 'Location','SouthEast')
%title('18-Feb-2000 00:20:00- C06 varying sediment')
grid on
d3d_qp('printfigure','C06-bedComparison-varyingSediment.png','PNG file',2,300,1,1,1)
%d3d_qp('closefigure')