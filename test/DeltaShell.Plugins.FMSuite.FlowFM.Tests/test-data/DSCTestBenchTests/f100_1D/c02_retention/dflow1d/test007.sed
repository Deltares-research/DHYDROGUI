[SedimentFileInformation]
   FileCreatedBy    = Delft3D-FLOW-GUI, Version: 3.1804
   FileCreationDate = 19-12-2003,  8:50:45
   FileVersion      = 02.00
[SedimentOverall]
   Cref             = 1600.0                  [kg/m3 ] CSoil Reference density for hindered settling calculations
   IopSus           = 0                       [  -   ] 1: Suspended sediment size is calculated dependent on d50
[Sediment]                                    
   Name             = #Sediment1#             [  -   ] Name as specified in NamC in md-file
   SedTyp           = bedload                 [  -   ] Must be "sand" or "mud" (or "bedload" for non-constituent fractions)
   RhoSol           = 2650.0                  [kg/m3 ] Density
   SedD50           = 2.0000000e-004          [  m   ] Sand only: sediment diameter
   CDryB            = 1600.0                  [kg/m3 ] Dry bed density
   IniSedThick      = 5.0000000e+000          [m]      Initial sediment layer thickness at bed (uniform value or filename)
   FacDss           = 1.0                     [-]      FacDss * SedDia = Initial suspended sediment diameter. Range [0.6 - 1.0]
   TraFrm           = #engelund-hansen.tra#   [ - ]    Sediment transport formula (relative path to sed) 