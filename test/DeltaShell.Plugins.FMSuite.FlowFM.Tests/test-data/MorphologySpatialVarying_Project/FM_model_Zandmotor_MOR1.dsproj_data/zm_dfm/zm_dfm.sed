[SedimentFileInformation]
    FileCreatedBy         = Deltares, FM-Suite DFlowFM Model Version 1.3.1.37789, DFlow FM Version 1.1.192.51191 
    FileCreationDate      = Wed Jul 05 2017, 15:13:51 
    FileVersion           = 02.00                  
[SedimentOverall]
    Cref                  = 1000000                [kg/m³]   Reference density for hindered settling calculations
    IopSus                = 0                      
[Sediment]
    Name                  = #Sediment_sand#                  Name of sediment fraction
    SedTyp                = mud                              Must be "sand", "mud" or "bedload"
    SedConc               = #Sediment_sand_SedConc.xyz# [kg/m³]   Initial Concentration
    IniSedThick           = 1                      [m]       Initial sediment layer thickness at bed
    FacDss                = 1                                Factor for suspended sediment diameter
    RhoSol                = 2650                   [kg/m³]   Specific density
    TraFrm                = -3                               Integer selecting the transport formula
    CDryB                 = 500                    [kg/m³]   Dry bed density
    SalMax                = 0                      [ppt]     Salinity for saline settling velocity
    WS0                   = 0.00025                [m/s]     Settling velocity fresh water
    WSM                   = 0.00025                [m/s]     Settling velocity saline water
    EroPar                = 0.0001                 [kg/m²s]  Erosion parameter
    TcrSed                = 1000                   [N/m²]    Critical stress for sedimentation
    TcrEro                = 0.005                  [N/m²]    Critical stress for erosion
    TcrFluff              = 0.05                   [N/m²]    Critical stress for fluff layer erosion
