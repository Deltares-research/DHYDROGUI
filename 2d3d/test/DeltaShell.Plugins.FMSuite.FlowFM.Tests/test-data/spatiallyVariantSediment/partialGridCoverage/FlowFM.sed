[SedimentFileInformation]
    FileCreatedBy         = Deltares, FM-Suite DFlowFM Model Version 1.2.5.37186, DFlow FM Version 1.1.198.48807 
    FileCreationDate      = Fri Apr 14 2017, 15:25:22 
    FileVersion           = 02.00                  
[SedimentOverall]
    Cref                  = 1600                   [kg/m³]   Reference density for hindered settling calculations
[Sediment]
    Name                  = #gouwe#                          Name of sediment fraction
    SedTyp                = sand                             Must be "sand", "mud" or "bedload"
    SedConc               = 0                      [kg/m³]   Initial Concentration
    IniSedThick           = #gouwe_IniSedThick.xyz# [m]       Initial sediment layer thickness at bed
    FacDss                = 1                                Initial suspended sediment diameter
    RhoSol                = 0                      [kg/m³]   Specific density
    TraFrm                = -2                               Integer selecting the transport formula
    CDryB                 = 1600                   [kg/m³]   Dry bed density
    SedDia                = 0.0002                 [m]       Median sediment diameter (D50)
    IopSus                = 0                                Option for determining suspended sediment diameter
    Pangle                = 0                      [degrees] Phase lead angle
    Fpco                  = 1                                Coefficient for phase lag effects
    Subiw                 = 51                               Wave period subdivision
    EpsPar                = False                            Use Van Rijn's parabolic mixing coefficient
    GamTcr                = 1.5                              Coefficient for grain size effect
    SalMax                = 0                      [ppt]     Salinity for saline settling velocity
