addpath D:\3_Software\_subversion\trunk\tools\matlab\Delft3D-toolbox\netcdf\mexnc
addpath D:\3_Software\_subversion\trunk\tools\matlab\Delft3D-toolbox\netcdf\snctools
FILENAME = 'unstructured_example.net';
NCFILE = 'example.nc';

%% open file
fid=fopen(FILENAME,'r');
fgetl(fid);
N = fscanf(fid,'%f',[1 2]); fgetl(fid);
NNodes = N(1);
NLines = N(2);
fgetl(fid);
XY = fscanf(fid,'%f',[2 NNodes]); fgetl(fid);
fgetl(fid);
LN = fscanf(fid,'%f',[2 NLines]); fclose(fid);

%% plot lines
x = reshape(XY(1,LN),size(LN));
x(3,:) = NaN;
y = reshape(XY(2,LN),size(LN));
y(3,:) = NaN;
plot(x(:),y(:))
axis equal
hold on

%% determine connections and sort them per point based on orientation
C = [LN';fliplr(LN')];
dXY = XY(:,C(:,1))-XY(:,C(:,2));
NodeConn = sortrows([C(:,1) atan2(dXY(2,:),dXY(1,:))' C(:,2)]);
NLinesPerNode = sparse(NodeConn(:,1),1,1,NNodes,1);
Line1OfNode=[1;cumsum(full(NLinesPerNode))+1];
Angle = NodeConn(:,2);
NodeConn = NodeConn(:,[1 3]);

%% determine points of 1D networks
oneD = zeros(NNodes,1);
oneD(NLinesPerNode==1) = 1;
oneDo = [];
while ~isequal(oneDo,oneD)
   oneDo = oneD;
   oneD = min(sparse(NodeConn(:,1),1,oneD(NodeConn(:,2)),NNodes,1)+oneD,1);
end
oneD = oneD==1;
plot(XY(1,oneD),XY(2,oneD),'r.')

%% determine number of volumes in 2D grid by excluding 1D links
Followed = false(NLines*2,1);
Followed(oneD(NodeConn(:,1))) = 1;
NNodes2D = sum(~oneD);
NLines2D = sum(~Followed)/2;
%NVolumes = NLines2D-NNodes2D+1; % assuming that all 2D areas are connected (only one unstructured 2D domain)

%% index 2D lines
Lines2D = find(~oneD(NodeConn(:,1)));

%% determine connectivity of volume edges
VEdgeConn=zeros(NLines*2,2);
for lineIn = 1:2*NLines
   fromPoint = NodeConn(lineIn,1);
   toPoint = NodeConn(lineIn,2);
   LinesOutOfToPoint = NodeConn(Line1OfNode(toPoint)+(0:NLinesPerNode(toPoint)-1),2);
   lineOut = find(LinesOutOfToPoint==fromPoint)+1;
   if lineOut>NLinesPerNode(toPoint)
      lineOut = 1;
   end
   VEdgeConn(lineIn,1:2) = [lineIn Line1OfNode(toPoint)+lineOut-1];
end

%% count number of edges
VEdgeID=zeros(2*NLines,1);
VEdgeID(VEdgeConn(:,1)~=0) = 1:2*NLines;
VEdgeIDo = [];
while ~isequal(VEdgeID,VEdgeIDo)
   VEdgeIDo = VEdgeID;
   VEdgeID = min(VEdgeID,VEdgeID(VEdgeConn(:,2))+0.000001);
end
VEdgeID_ = floor(VEdgeID);
ReID = cumsum(accumarray(VEdgeID_,1,[2*NLines 1])>0);
NSets = ReID(end);

VEdgeID=ReID(VEdgeID_)+VEdgeID-VEdgeID_;
VEdgeID_ = floor(VEdgeID);
NPerVol = accumarray(VEdgeID_,1,[NSets,1]);
Edge1D = oneD(NodeConn(:,1));
Vol2D = ~accumarray(VEdgeID_,Edge1D,[NSets,1]);

%% detect outer edge(s) of 2D domain(s)
VolBnd = sortrows([VEdgeID NodeConn]);
OuterEdge2D = false([NSets 1]);
n = 0;
for i = 1:NSets
   j = n+(1:NPerVol(i));
   nodes_i = VolBnd(j,2);
   if ~oneD(VolBnd(j(1),2)) && clockwise(XY(1,nodes_i),XY(2,nodes_i))>0
      OuterEdge2D(i) = 1;
   end
   n = n + NPerVol(i);
end
Vol2D = Vol2D & ~OuterEdge2D;
NVolumes = sum(Vol2D);
N2DDomains = sum(OuterEdge2D);

%% Plot flux points
% Approximate location. Actual location depends on numerical
% implementation.
XYu = (XY(:,NodeConn(:,1))+XY(:,NodeConn(:,2)))/2;
plot(XYu(1,:),XYu(2,:),'g.')

%% Plot pressure points
% Approximate location. Actual location depends on numerical
% implementation.
XYp = zeros(2,NSets);
n = 0;
for i = 1:NSets
   if Vol2D(i)
      j = n+(1:NPerVol(i));
      nodes_i = VolBnd(j,2);
      XYp(:,i) = mean(XY(:,nodes_i),2);
   end
   n = n + NPerVol(i);
end
plot(XYp(1,:),XYp(2,:),'k+')

%% Collect points in one grid
%
XYl = (XY(:,LN(1,:))+XY(:,LN(2,:)))/2;
XYall = [XY XYp XYl];
offset_XYp = NNodes;
offset_XYl = offset_XYp + NSets;
iP = offset_XYp+(1:NSets);
iE = offset_XYl+(1:NLines);
%
Nquads = sum(NPerVol(Vol2D));
n = 0;
q = 0;
Quads = zeros(Nquads,4);
for i = 1:NSets
   if Vol2D(i)
      jList = n+(1:NPerVol(i));
      for j = jList
         p = VolBnd(j,2);
         lines = find(any(LN==p));
         aLN = LN(:,lines);
         ilines = find(any(ismember(aLN,VolBnd(jList,2)) & aLN~=p));
         q = q+1;
         Quads(q,:) = [offset_XYp+i offset_XYl+lines(ilines(1)) p offset_XYl+lines(ilines(2))];
      end
   end
   n = n + NPerVol(i);
end
p = patch('vertices',XYall','faces',Quads,'cdata',1:size(Quads,1));
set(p,'facecolor','flat','linestyle',':')
colormap(rand(size(Quads,1),3))

quadNodes = unique(Quads(:));
qNode2D = zeros(size(quadNodes));
qNode2D(quadNodes) = 1:length(quadNodes);
XYall2D = XYall(:,quadNodes);
Quads2D = qNode2D(Quads);

%% Construct connectivity array
% 
Node2D=zeros(size(oneD));
Node2D(~oneD) = 1:sum(~oneD);
%
MaxNodesPerVol = max(NPerVol(Vol2D));
Connectivity = zeros(NVolumes,MaxNodesPerVol);
Connectivity(:,1) = NPerVol(Vol2D);
n = 0;
ii = 0;
for i = 1:NSets
    if Vol2D(i)
        ii = ii+1;
        j = n+(1:NPerVol(i));
        nodes_i = VolBnd(j,2);
        Connectivity(ii,1+(1:NPerVol(i))) = Node2D(VolBnd(j,2));
    end
    n = n + NPerVol(i);
end

%% NetCDF output mixed tri, quad, ...
%
if ~isempty(NCFILE)
	nZone = NVolumes;
	nNode = NNodes2D;
	nEdge = NLines2D;
    nTime = 0; % automatic time
	nConnect = MaxNodesPerVol + 1;
    %
    [NCID, status] = mexnc('create',[NCFILE(1:end-3) '_mixedzone.nc'],nc_share_mode);
    %
    [dZone, status] = mexnc('def_dim',NCID,'zone',nZone);
    [dNode, status] = mexnc('def_dim',NCID,'node',nNode);
    [dEdge, status] = mexnc('def_dim',NCID,'edge',nEdge);
    [dTime, status] = mexnc('def_dim',NCID,'time',nTime);
    [dConnect, status] = mexnc('def_dim',NCID,'nConnect',nConnect);
    %
    [status, vGrid1] = definevar(NCID, ...
        'grid1',nc_double,[dConnect dZone], ...
        {'standard_name',nc_char,'connectivity'
        'spatial_dimension',nc_int,2
        'topological_dimension',nc_int,2
        'x_nodal_coordinate',nc_char,'x'
        'y_nodal_coordinate',nc_char,'y'
        'cell_type',nc_char,'nc_mixed'});
    %
    [status, vX] = definevar(NCID, ...
        'x',nc_double,dNode, ...
        {'long_name',nc_char,'nodal x-coordinate'
        'units',nc_char,'meters'
        'grid',nc_char,'grid1'});
    %
    [status, vY] = definevar(NCID, ...
        'y',nc_double,dNode, ...
        {'long_name',nc_char,'nodal y-coordinate'
        'units',nc_char,'meters'
        'grid',nc_char,'grid1'});
    %
    [status, vS1] = definevar(NCID, ...
        'zw',nc_double,[dTime dZone], ...
        {'long_name',nc_char,'water level'
        'units',nc_char,'meters'
        'positive',nc_char,'up'
        'standard_name',nc_char,'sea_surface_elevation'
        'grid',nc_char,'grid1'});
    %
    [status, vDP] = definevar(NCID, ...
        'zb',nc_double,dZone, ...
        {'long_name',nc_char,'bed level'
        'units',nc_char,'meters'
        'positive',nc_char,'up'
        'grid',nc_char,'grid1'});
    %
    [status, vU1] = definevar(NCID, ...
        'u',nc_double,[dTime dEdge], ...
        {'long_name',nc_char,'normal velocity'
        'units',nc_char,'meters s-1'
        'grid',nc_char,'grid1'});
    %
    [status, vT] = definevar(NCID, ...
        'time',nc_double,dTime, ...
        {'long_name',nc_char,'time'
        'units',nc_char,'days since 2008-03-01 00:00:00 GMT'});
    %
    %AVal = 'Deltares';
    %status = mexnc('put_att_text',NCID,-1,'Creator',nc_char,length(AVal),AVal);
    %
    status = mexnc('end_def',NCID);
    %
    status = mexnc('put_var_double',NCID,vGrid1,Connectivity);
    status = mexnc('put_var_double',NCID,vX,XY(1,~oneD));
    status = mexnc('put_var_double',NCID,vY,XY(2,~oneD));
    %
    status = mexnc('close',NCID);
end

%% NetCDF output mixed node type
%
iCenter = qNode2D(iP);
iCenter(iCenter==0) = [];
iEdge = qNode2D(iE);
iEdge(iEdge==0) = [];
%
if ~isempty(NCFILE)
	nZone = size(Quads,1);
	nNode = size(XYall2D,2);
    nCent = NVolumes;
    nCorn = NNodes2D;
    nEdge = NLines2D;
    time = 0; % automatic time
	nConnect = 4 ;
    %
    [NCID, status] = mexnc('create',[NCFILE(1:end-3) '_mixednode.nc'],nc_share_mode);
    %
    [dZone, status] = mexnc('def_dim',NCID,'zone',nZone);
    [dNode, status] = mexnc('def_dim',NCID,'node',nNode);
    [dCent, status] = mexnc('def_dim',NCID,'centerNode',nCent);
    [dCorn, status] = mexnc('def_dim',NCID,'cornerNode',nCorn);
    [dEdge, status] = mexnc('def_dim',NCID,'edgeNode',nEdge);
    [dTime, status] = mexnc('def_dim',NCID,'time',time);
    [dConnect, status] = mexnc('def_dim',NCID,'nConnect',nConnect);
    %
    [status, vGrid1] = definevar(NCID, ...
        'grid1',nc_double,[dConnect dZone], ...
        {'standard_name',nc_char,'connectivity'
        'spatial_dimension',nc_int,2
        'topological_dimension',nc_int,2
        'x_coordinate',nc_char,'x'
        'y_coordinate',nc_char,'y'
        'cell_type',nc_char,'nc_quad'});
    %
    [status, vX] = definevar(NCID, ...
        'x',nc_double,dNode, ...
        {'long_name',nc_char,'x-coordinate'
        'units',nc_char,'meters'
        'grid',nc_char,'grid1'});
    %
    [status, vY] = definevar(NCID, ...
        'y',nc_double,dNode, ...
        {'long_name',nc_char,'y-coordinate'
        'units',nc_char,'meters'
        'grid',nc_char,'grid1'});
    %
    [status, vCS] = definevar(NCID, ...
        'center_stagger',nc_int,dCent, ...
        {'standard_name',nc_char,'stagger_index'
        'long_name',nc_char,'indices of center nodes'
        'grid',nc_char,'grid1'});
    %
    [status, vES] = definevar(NCID, ...
        'edge_stagger',nc_int,dEdge, ...
        {'standard_name',nc_char,'stagger_index'
        'long_name',nc_char,'indices of edge nodes'
        'grid',nc_char,'grid1'});
    %
    [status, vS1] = definevar(NCID, ...
        'zw',nc_double,[dTime dCent], ...
        {'long_name',nc_char,'water level'
        'units',nc_char,'meters'
        'positive',nc_char,'up'
        'standard_name',nc_char,'sea_surface_elevation'
        'grid',nc_char,'grid1'
        'stagger',nc_char,'center_stagger'});
    %
    [status, vDP] = definevar(NCID, ...
        'zb',nc_double,dCent, ...
        {'long_name',nc_char,'bed level'
        'units',nc_char,'meters'
        'positive',nc_char,'up'
        'grid',nc_char,'grid1'
        'stagger',nc_char,'center_stagger'});
    %
    [status, vU1] = definevar(NCID, ...
        'u',nc_double,[dTime dEdge], ...
        {'long_name',nc_char,'normal velocity'
        'units',nc_char,'meters s-1'
        'grid',nc_char,'grid1'
        'stagger',nc_char,'edge_stagger'});
    %
    [status, vT] = definevar(NCID, ...
        'time',nc_double,dTime, ...
        {'long_name',nc_char,'time'
        'units',nc_char,'days since 2008-03-01 00:00:00 GMT'});
    %
    %AVal = 'Deltares';
    %status = mexnc('put_att_text',NCID,-1,'Creator',nc_char,length(AVal),AVal);
    %
    status = mexnc('end_def',NCID);
    %
    status = mexnc('put_var_double',NCID,vGrid1,Quads');
    status = mexnc('put_var_double',NCID,vCS,iCenter);
    status = mexnc('put_var_double',NCID,vES,iEdge);
    status = mexnc('put_var_double',NCID,vX,XYall2D(1,:));
    status = mexnc('put_var_double',NCID,vY,XYall2D(2,:));
    %
    status = mexnc('close',NCID);
end

