d3d_qp('openfile','D:\niesten\Projects\1000202-001_FMtesting\thindams_IN\c10_fixed_layer\dflowfmoutput\t1_map.nc')
d3d_qp('selectfield','Time-varying bottom level in flow cell centers - nmesh2d_face: mean')
d3d_qp('colour',[ 1 0 0 ])
d3d_qp('hselectiontype','(X,Y) point/path')
d3d_qp('editxy','1.000, 0.900; 12.000, 0.900')

for t=[1,10,20,30,40,50,60]
	d3d_qp('editt',t)
	d3d_qp('quickview')
end
