
%Discharge
Q_universal_weir = 'd:\05_Projects\05_27_FM_testing\testbench\cases\e02_dflowfm\f106_hydraulic-structures\c06_Universal_Weir\Analysis\SOBEK_model\universal_weir.dsproj_data\Flow1D_output\dflow1d\output\structures.nc';

FileInfo = qpfopen(Q_universal_weir);
[DataFields, Dims, Nval] = qpread(FileInfo);


var1 = 'Structure discharge';
var2 = 'Water level upstream of structure wrt branch direction'; 
var3 = 'Water level downstream of structure wrt branch direction'; 

% simple weir  
SOBEK_Q_WE_universal_weir = qpread(FileInfo, var1, 'data', 0 , 1);
SOBEK_Q_WE_universal_weir_out = SOBEK_Q_WE_universal_weir.Val(:,1);
SOBEK_Q_EW_universal_weir = qpread(FileInfo, var1, 'data', 0 , 2);
SOBEK_Q_EW_universal_weir_out = SOBEK_Q_EW_universal_weir.Val(:,1);

hup_weir = qpread(FileInfo, var2, 'data', 0 , 1);
hup = hup_weir.Val(:,1);
hdwn_weir = qpread(FileInfo, var3, 'data', 0 , 1);
hdwn = hdwn_weir.Val(:,1);


