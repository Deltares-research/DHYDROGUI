[SedimentFileInformation]
   FileCreatedBy    = Delft3D-FLOW-GUI, Version: 3.39.25         
   FileCreationDate = June 2007         
   FileVersion      = 02.00                        
[SedimentOverall]
   Cref             = 1600      [kg/m3]            CSoil Reference density for hindered settling calculations
   IopSus           = 0                            If Iopsus = 1: susp. sediment size depends on local flow and wave conditions
[Sediment]
   Name             = #sediment 1#       Name of sediment fraction
   SedTyp           = bedload                      Must be "sand", "mud" or "bedload"
   RhoSol           = 2650                [kg/m3]  Specific density
   SedMinDia        = 0.3400-003          [m]      Minimum sediment diameter    
   SedMaxDia        = 0.6000-003          [m]      Maximum sediment diameter    
   CDryB            = 1600                [kg/m3]  Dry bed density
   FacDSS           = 1                   [-]      FacDss * SedDia = Initial suspended sediment diameter. Range [0.6 - 1.0]
   IniSedThick      = #t2_sdb_at_cc2.xyz# [m]      Initial sediment layer thickness at bed (uniform value or filename)
   Trafrm           = 1
   ACAL             = 1.21
[Sediment]
   Name             = #sediment 2#       Name of sediment fraction
   SedTyp           = bedload                      Must be "sand", "mud" or "bedload"
   RhoSol           = 2650                [kg/m3]  Specific density
   SedMinDia        = 8.0000-003          [m]      Minimum sediment diameter    
   SedMaxDia        = 1.6000-002          [m]      Maximum sediment diameter    
   CDryB            = 1600                [kg/m3]  Dry bed density
   FacDSS           = 1                   [-]      FacDss * SedDia = Initial suspended sediment diameter. Range [0.6 - 1.0]
   IniSedThick      = 0                   [m]      Initial sediment layer thickness at bed (uniform value or filename)
   Trafrm           = 1
   ACAL             = 1.21
