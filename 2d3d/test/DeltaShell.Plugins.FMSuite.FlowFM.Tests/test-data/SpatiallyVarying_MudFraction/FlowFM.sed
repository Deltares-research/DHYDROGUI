[SedimentFileInformation]
    FileCreatedBy         = Deltares, FM-Suite DFlowFM Model Version 1.3.2.37877, DFlow FM Version 1.1.192.51419 
    FileCreationDate      = Fri Jul 14 2017, 13:44:50 
    FileVersion           = 02.00                  
[SedimentOverall]
    Cref                  = 1600                   [kg/m³]   Reference density for hindered settling calculations
[Sediment]
    Name                  = #mudFraction#                    Name of sediment fraction
    SedTyp                = mud                              Must be "sand", "mud" or "bedload"
    IniSedThick           = #mudFraction_IniSedThick.xyz# [m]       Initial sediment layer thickness at bed
    FacDss                = 1                                Factor for suspended sediment diameter
    RhoSol                = 2650                   [kg/m³]   Specific density
    TraFrm                = -3                               Integer selecting the transport formula
    CDryB                 = 500                    [kg/m³]   Dry bed density
    SalMax                = 0                      [ppt]     Salinity for saline settling velocity
    WS0                   = 0.00025                [m/s]     Settling velocity fresh water
    WSM                   = 0.00025                [m/s]     Settling velocity saline water
    EroPar                = 0.0001                 [kg/m²s]  Erosion parameter
    TcrSed                = #mudFraction_TcrSed.xyz# [N/m²]    Critical stress for sedimentation
    TcrEro                = #mudFraction_TcrEro.xyz# [N/m²]    Critical stress for erosion
    TcrFluff              = 4.94065645841247E-324  [N/m²]    Critical stress for fluff layer erosion
