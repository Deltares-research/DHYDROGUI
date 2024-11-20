                                                                                
 Found -A command line switch                                                   
 ONLY activated processes are switched on                                       


 Model :            Water quality calculation Sobek-WQ      
                                                            


 Run   :                                                    
                    T0: 2010.01.01 00:00:00  (scu=       1s)


 Found -P command line switch                                                   
 
 Using process definition file : D:\Sbk21204\Programs\delwaq\fixed\proc_d
 Version number                :       4.52
 Serial                        : 2008101501
 
# Determining which processes can be switched on                                                    
                                                                                                    
 Input for [DynDepth  ] dynamic calculation of the depth                                            
   Process is activated                                                                             
                                                                                                    
 Input for [TotDepth  ] depth water column                                                          
   Process is activated                                                                             
                                                                                                    
 Input for [ResTim    ] residence time                                                              
   Process is activated                                                                             
                                                                                                    
 Input for [CalTau    ] Calculation of bottom friction                                              
   Process is activated                                                                             
                                                                                                    
 Input for [Nitrif_NH4] Nitrification of ammonium                                                   
   Process is activated                                                                             
                                                                                                    
 Input for [SaturOXY  ] Saturation concentration oxygen                                             
   Process is activated                                                                             
                                                                                                    
 Input for [RearOXY   ] Reaeration of oxygen                                                        
   Process is activated                                                                             
                                                                                                    
 Input for [BODCOD    ] Mineralisation BOD and COD                                                  
   Process is activated                                                                             
                                                                                                    
 Input for [SedOXYDem ] Sediment oxygen demand                                                      
   Process is activated                                                                             
                                                                                                    
 Input for [S_CBOD51  ] Sedimentation CBOD5 (first pool)                                            
   Process is activated                                                                             
                                                                                                    
 Input for [S_CBOD52  ] Sedimentation CBOD5_2 (second pool)                                         
   Process is activated                                                                             
                                                                                                    
 Input for [Prod_TEWOR] Production fluxes for TEWOR+                                                
   Process is activated                                                                             
                                                                                                    
                                                                                                    
# Determinig the processes to model the substances.                             
                                                                                
-Fluxes for [Continuity]                                                        
 No fluxes found                                                                
-Dispersion for [Continuity]                                                    
 No dispersions found                                                           
-Velocity for [Continuity]                                                      
 No velocity found                                                              
                                                                                
-Fluxes for [NH4       ]                                                        
 Found flux  [dNITRIF   ] nitrification flux                                    
   from proces [Nitrif_NH4] Nitrification of ammonium                           
   Process is switched on.                                                      
 Found flux  [dTEWORNH4 ] TEWOR production flux of NH4                          
   from proces [Prod_TEWOR] Production fluxes for TEWOR+                        
   Process is switched on.                                                      
-Dispersion for [NH4       ]                                                    
 No dispersions found                                                           
-Velocity for [NH4       ]                                                      
 No velocity found                                                              
                                                                                
-Fluxes for [CBOD5     ]                                                        
 Found flux  [dCBOD5    ] decay flux of CBOD5                                   
   from proces [BODCOD    ] Mineralisation BOD and COD                          
   Process is switched on.                                                      
 Found flux  [dSedBOD5  ] sedimentation flux CBOD5                              
   from proces [S_CBOD51  ] Sedimentation CBOD5 (first pool)                    
   Process is switched on.                                                      
 Found flux  [dTEWORBOD ] TEWOR production flux of CBOD5                        
   from proces [Prod_TEWOR] Production fluxes for TEWOR+                        
   Process is switched on.                                                      
-Dispersion for [CBOD5     ]                                                    
 No dispersions found                                                           
-Velocity for [CBOD5     ]                                                      
 Found velocity [VxSedBOD5 ] sedimentation velocity CBOD5                       
   from proces [S_CBOD51  ] Sedimentation CBOD5 (first pool)                    
   Process is switched on.                                                      
                                                                                
-Fluxes for [CBOD5_2   ]                                                        
 Found flux  [dCBOD5_2  ] decay flux of CBOD5_2                                 
   from proces [BODCOD    ] Mineralisation BOD and COD                          
   Process is switched on.                                                      
 Found flux  [dSedBOD5_2] sedimentation flux CBOD5_2                            
   from proces [S_CBOD52  ] Sedimentation CBOD5_2 (second pool)                 
   Process is switched on.                                                      
 Found flux  [dTEWORBOD2] TEWOR production flux of CBOD5_2                      
   from proces [Prod_TEWOR] Production fluxes for TEWOR+                        
   Process is switched on.                                                      
-Dispersion for [CBOD5_2   ]                                                    
 No dispersions found                                                           
-Velocity for [CBOD5_2   ]                                                      
 Found velocity [VxSedBOD52] sedimentation velocity CBOD5_2                     
   from proces [S_CBOD52  ] Sedimentation CBOD5_2 (second pool)                 
   Process is switched on.                                                      
                                                                                
-Fluxes for [OXY       ]                                                        
 Found flux  [dNITRIF   ] nitrification flux                                    
   from proces [Nitrif_NH4] Nitrification of ammonium                           
   Process is switched on.                                                      
 Found flux  [dREAROXY  ] reaeration flux of dissolved oxygen                   
   from proces [RearOXY   ] Reaeration of oxygen                                
   Process is switched on.                                                      
 Found flux  [dOxyBODCOD] oxygen consumption from decay BOD and COD             
   from proces [BODCOD    ] Mineralisation BOD and COD                          
   Process is switched on.                                                      
 Found flux  [dOxSOD    ] oxygen consumption from SOD                           
   from proces [SedOXYDem ] Sediment oxygen demand                              
   Process is switched on.                                                      
 Found flux  [dTEWOROXY ] TEWOR production flux of OXY                          
   from proces [Prod_TEWOR] Production fluxes for TEWOR+                        
   Process is switched on.                                                      
-Dispersion for [OXY       ]                                                    
 No dispersions found                                                           
-Velocity for [OXY       ]                                                      
 No velocity found                                                              
                                                                                
# Locating processes for requested output                                       
                                                                                
# Determining the input for the processes (in reversed order)                   
                                                                                
 Input for [Prod_TEWOR] Production fluxes for TEWOR+                            
       [fTEWORBOD           ] TEWOR production flux of CBOD5_2                  
       Using constant nr 18                                                     
       [fTEWORBOD2          ] TEWOR production flux of CBOD5_2                  
       Using default value: 0.00000                                             
       [fTEWORBOD3          ] TEWOR production flux of CBOD5_3                  
       Using default value: 0.00000                                             
       [fTEWORCOD           ] TEWOR production flux of COD_Cr                   
       Using default value: 0.00000                                             
       [fTEWOROXY           ] TEWOR production flux of OXY                      
       Using constant nr 19                                                     
       [fTEWORORGN          ] TEWOR production flux of Org-N                    
       Using default value: 0.00000                                             
       [fTEWORNH4           ] TEWOR production flux of NH4                      
       Using constant nr 20                                                     
       [fTEWORNO3           ] TEWOR production flux of NO3                      
       Using default value: 0.00000                                             
       [fTEWOROON           ] TEWOR production flux of OON                      
       Using default value: 0.00000                                             
       [fTEWORECol          ] TEWOR production flux of EColi                    
       Using default value: 0.00000                                             
                                                                                
 Input for [S_CBOD52  ] Sedimentation CBOD5_2 (second pool)                     
       [CBOD5_2             ] carbonaceous BOD (second pool) at 5 days          
       Using substance nr   4                                                   
       [ZSedBOD5_2          ] zeroth-order sedimentation flux CBOD5_2           
       Using default value: 0.00000                                             
       [VSedBOD5_2          ] sedimentation velocity CBOD5_2                    
       Using constant nr 31                                                     
       [Tau                 ] total bottom shear stress                         
       Using output from proces [CalTau    ]                                    
       [TaucSBOD            ] critical shear stress for sedimentation BOD       
       Using default value:0.100000E+13                                         
       [Depth               ] depth of segment                                  
       Using output from proces [DynDepth  ]                                    
       [DELT                ] timestep for processes                            
       Using DELWAQ timestep in days                                            
       [MinDepth            ] minimum waterdepth for sedimentation              
       Using default value:0.100000                                             
       [VSedBOD5_2          ] sedimentation velocity CBOD5_2                    
       Using constant nr 31                                                     
                                                                                
 Input for [S_CBOD51  ] Sedimentation CBOD5 (first pool)                        
       [CBOD5               ] carbonaceous BOD (first pool) at 5 days           
       Using substance nr   3                                                   
       [ZSedBOD5            ] zeroth-order sedimentation flux CBOD5             
       Using default value: 0.00000                                             
       [VSedBOD5            ] sedimentation velocity CBOD5                      
       Using constant nr 30                                                     
       [Tau                 ] total bottom shear stress                         
       Using output from proces [CalTau    ]                                    
       [TaucSBOD            ] critical shear stress for sedimentation BOD       
       Using default value:0.100000E+13                                         
       [Depth               ] depth of segment                                  
       Using output from proces [DynDepth  ]                                    
       [DELT                ] timestep for processes                            
       Using DELWAQ timestep in days                                            
       [MinDepth            ] minimum waterdepth for sedimentation              
       Using default value:0.100000                                             
       [VSedBOD5            ] sedimentation velocity CBOD5                      
       Using constant nr 30                                                     
                                                                                
 Input for [SedOXYDem ] Sediment oxygen demand                                  
       [fSODaut             ] autonomous SOD (no effect SOD stat.var)           
       Using default value: 0.00000                                             
       [fSOD                ] zeroth-order sediment oxygen demand flux          
       Using constant nr 36                                                     
       [Depth               ] depth of segment                                  
       Using output from proces [DynDepth  ]                                    
       [SOD                 ] Sediment oxygen demand (SOD)                      
       Using default value: 0.00000                                             
       [RcSOD               ] decay rate SOD at 20 oC                           
       Using constant nr 37                                                     
       [TcSOD               ] temperature coefficient decay SOD                 
       Using default value: 1.04000                                             
       [Temp                ] ambient water temperature                         
       Using constant nr 40                                                     
       [Volume              ] volume of computational cell                      
       Using DELWAQ volume                                                      
       [SwCH4bub            ] switch (1=include CH4 bubbles, 0=not)             
       Using default value: 0.00000                                             
       [HSED                ] Total sediment thickness                          
       Using default value:0.100000                                             
       [KAPC                ] constant                                          
       Using default value: 1.60000                                             
       [thetak              ] temperature constant                              
       Using default value: 1.07900                                             
       [edwcsd              ] diffusion coefficient                             
       Using default value:0.250000E-03                                         
       [diamb               ] Diameter of methane bubbles                       
       Using default value: 1.00000                                             
       [OXY                 ] Dissolved Oxygen                                  
       Using substance nr   5                                                   
       [kappad              ] transfer coefficient                              
       Using default value:0.300000E-02                                         
       [dMinDetCS1          ] mineralisation flux DetCS1                        
       Using default value: 0.00000                                             
       [dMinDetCS2          ] mineralisation flux DetCS2                        
       Using default value: 0.00000                                             
       [dMinOOCS1           ] mineralisation flux OOCS1                         
       Using default value: 0.00000                                             
       [dMinOOCS2           ] mineralisation flux OOCS2                         
       Using default value: 0.00000                                             
       [TotalDepth          ] total depth water column                          
       Using output from proces [TotDepth  ]                                    
       [COXSOD              ] critical oxygen concentration for SOD decay       
       Using default value: 0.00000                                             
       [OOXSOD              ] optimum oxygen concentration for SOD decay        
       Using default value: 2.00000                                             
                                                                                
 Input for [BODCOD    ] Mineralisation BOD and COD                              
       [SwOXYDem            ] switch oxygen consumption(0=BOD, 1=COD, 2=both)   
       Using default value: 0.00000                                             
       [CBOD5               ] carbonaceous BOD (first pool) at 5 days           
       Using substance nr   3                                                   
       [CBOD5_2             ] carbonaceous BOD (second pool) at 5 days          
       Using substance nr   4                                                   
       [CBODu               ] carbonaceous BOD (first pool) ultimate            
       Using default value: 0.00000                                             
       [CBODu_2             ] carbonaceous BOD (second pool) ultimate           
       Using default value: 0.00000                                             
       [COD_Cr              ] COD concentration by the Cr-method                
       Using default value: 0.00000                                             
       [COD_Mn              ] COD concentration by the Mn-method                
       Using default value: 0.00000                                             
       [NBOD5               ] nitrogenous BOD at 5 days                         
       Using default value: 0.00000                                             
       [NBODu               ] nitrogenous BOD ultimate                          
       Using default value: 0.00000                                             
       [RcBOD               ] decay rate BOD (first pool) at 20 oC              
       Using constant nr 21                                                     
       [RcBOD_2             ] decay rate BOD (second pool) at 20 oC             
       Using constant nr 22                                                     
       [RcCOD               ] decay rate COD at 20 oC                           
       Using default value:0.500000E-01                                         
       [RcBODN              ] first-order mineralisation rate BODN              
       Using default value:0.300000                                             
       [TcBOD               ] temperature coefficient decay BOD                 
       Using constant nr 23                                                     
       [TcCOD               ] temperature coefficient decay COD                 
       Using default value: 1.02000                                             
       [TcBODN              ] temperature coefficient decay BODN                
       Using default value: 1.08000                                             
       [Temp                ] ambient water temperature                         
       Using constant nr 40                                                     
       [OXY                 ] Dissolved Oxygen                                  
       Using substance nr   5                                                   
       [COXBOD              ] critical oxygen concentration for BOD decay       
       Using constant nr 24                                                     
       [OOXBOD              ] optimum oxygen concentration for BOD decay        
       Using constant nr 25                                                     
       [CFLBOD              ] oxygen function level for oxygen below COXBOD     
       Using constant nr 26                                                     
       [CurvBOD             ] curvature of DO function for mineralisation BOD   
       Using default value: 0.00000                                             
       [LAgeFun             ] lower value of age function BOD decay             
       Using default value: 1.00000                                             
       [UAgeFun             ] upper value of age function BOD decay             
       Using default value: 1.00000                                             
       [LAgeIndx            ] lower value of age index BOD decay                
       Using default value: 2.00000                                             
       [UAgeIndx            ] upper value of age index BOD decay                
       Using default value: 3.00000                                             
       [Phyt                ] total carbon in phytoplankton                     
       Using default value: 0.00000                                             
       [BOD5/uPHYT          ] BOD5:BODu ratio in phytoplankton                  
       Using default value:0.600000                                             
       [AlgFrBOD            ] fraction algae contributing to BOD-inf            
       Using default value:0.500000                                             
       [OXCCF               ] O2:C ratio in mineralisation                      
       Using default value: 2.67000                                             
       [POCnoa              ] total POC (no algae)                              
       Using default value: 0.00000                                             
       [BOD5/infPO          ] BOD5:BODu ratio in POC                            
       Using default value:0.600000                                             
       [POCFrBOD            ] fraction of POC contributing to BOD-inf           
       Using default value: 1.00000                                             
       [EffCOD_Cr           ] efficiency of Cr method for COD                   
       Using default value:0.900000                                             
       [EffCOD_Mn           ] efficiency of Mn method for COD                   
       Using default value:0.500000                                             
       [AMCCF               ] amount oxygen used for nitrogen in miner.         
       Using default value:0.550000                                             
                                                                                
 Input for [RearOXY   ] Reaeration of oxygen                                    
       [OXY                 ] Dissolved Oxygen                                  
       Using substance nr   5                                                   
       [Depth               ] depth of segment                                  
       Using output from proces [DynDepth  ]                                    
       [Temp                ] ambient water temperature                         
       Using constant nr 40                                                     
       [Velocity            ] horizontal flow velocity                          
       Using segment function nr  3                                             
       [VWind               ] wind speed                                        
       Using constant nr 38                                                     
       [SWRear              ] switch for oxygen reaeration formulation (1-12)   
       Using constant nr 33                                                     
       [KLRear              ] reaeration transfer coefficient                   
       Using constant nr 34                                                     
       [TCRear              ] temperature coefficient for rearation             
       Using function nr  2                                                     
       [DELT                ] timestep for processes                            
       Using DELWAQ timestep in days                                            
       [SaturOXY            ] saturation concentration                          
       Using output from proces [SaturOXY  ]                                    
       [Salinity            ] Salinity                                          
       Using function nr  1                                                     
       [TotalDepth          ] total depth water column                          
       Using output from proces [TotDepth  ]                                    
       [fcover              ] fraction of water surface covered <0-1>           
       Using default value: 0.00000                                             
       [KLRearMax           ] maximum KLREAR oxygen for temp. correction        
       Using default value: 1000.00                                             
       [KLRearMin           ] minimum rearation transfer coefficient oxygen     
       Using default value:0.200000                                             
                                                                                
 Input for [SaturOXY  ] Saturation concentration oxygen                         
       [Cl                  ] Chloride                                          
       Using default value: 20000.0                                             
       [Temp                ] ambient water temperature                         
       Using constant nr 40                                                     
       [SWSatOXY            ] switch for oxygen saturation formulation (1, 2)   
       Using default value: 1.00000                                             
       [Salinity            ] Salinity                                          
       Using function nr  1                                                     
                                                                                
 Input for [Nitrif_NH4] Nitrification of ammonium                               
       [ZNit                ] zeroth-order nitrification flux                   
       Using default value: 0.00000                                             
       [NH4                 ] Ammonium (NH4)                                    
       Using substance nr   2                                                   
       [RcNit20             ] MM- nitrification rate at 20 oC                   
       Using default value:0.100000                                             
       [TcNit               ] temperature coefficient for nitrification         
       Using constant nr 14                                                     
       [OXY                 ] Dissolved Oxygen                                  
       Using substance nr   5                                                   
       [KsAmNit             ] half saturation constant for ammonium cons.       
       Using default value:0.500000                                             
       [KsOxNit             ] half saturation constant for DO cons.             
       Using default value: 1.00000                                             
       [Temp                ] ambient water temperature                         
       Using constant nr 40                                                     
       [CTNit               ] critical temperature for nitrification            
       Using default value: 3.00000                                             
       [Rc0NitOx            ] zero-order nitrification rate at neg. DO          
       Using default value: 0.00000                                             
       [COXNIT              ] critical oxygen concentration for nitrification   
       Using constant nr 15                                                     
       [Poros               ] volumetric porosity                               
       Using default value: 1.00000                                             
       [SWVnNit             ] switch for old (0), new (1), TEWOR (2) version    
       Using default value: 0.00000                                             
       [RcNit               ] first-order nitrification rate                    
       Using constant nr 16                                                     
       [OOXNIT              ] optimum oxygen concentration for nitrification    
       Using constant nr 17                                                     
       [CFLNIT              ] oxygen function level for oxygen below COXNIT     
       Using default value: 0.00000                                             
       [CurvNit             ] curvature of DO function for nitrification        
       Using default value: 0.00000                                             
                                                                                
 Input for [CalTau    ] Calculation of bottom friction                          
       [WaveHeight          ] calculated height of a wind induced wave          
       Using default value: 0.00000                                             
       [WaveLength          ] calculated length of a wind induced wave          
       Using default value: 0.00000                                             
       [WavePeriod          ] calculated period of a wind induced wave          
       Using default value: 0.00000                                             
       [TauShip             ] bottom shear stress by ship movement              
       Using default value: 0.00000                                             
       [SWTauVeloc          ] Switch Tauveloc (1=calculate|2=TauFlow)           
       Using default value: 1.00000                                             
       [TauFlow             ] bottom shear stress by FLOW                       
       Using default value: 0.00000                                             
       [Velocity            ] horizontal flow velocity                          
       Using segment function nr  3                                             
       [CHEZY               ] Chezy coefficient                                 
       Using segment function nr  1                                             
       [TotalDepth          ] total depth water column                          
       Using output from proces [TotDepth  ]                                    
       [SWTau               ] switch <1=Tamminga|2=Swart|3=Soulsby>             
       Using default value: 1.00000                                             
       [Depth               ] depth of segment                                  
       Using output from proces [DynDepth  ]                                    
                                                                                
 Input for [ResTim    ] residence time                                          
       [Volume              ] volume of computational cell                      
       Using DELWAQ volume                                                      
       [WSNoseg5            ] workspace array no. 5                             
       Using output from proces [ResTim    ]                                    
       [Flow                ] flow rate                                         
       Using DELWAQ flow                                                        
                                                                                
 Input for [TotDepth  ] depth water column                                      
       [Depth               ] depth of segment                                  
       Using output from proces [DynDepth  ]                                    
                                                                                
 Input for [DynDepth  ] dynamic calculation of the depth                        
       [Volume              ] volume of computational cell                      
       Using DELWAQ volume                                                      
       [Surf                ] horizontal surface area of a DELWAQ segment       
       Using segment function nr  2                                             
                                                                                
# Determining the use of the DELWAQ input                                       
                                                                                
 INFO: Constant [Iteration ] is NOT used by the PROCES system                   
 INFO: Constant [O2FuncBOD ] is NOT used by the PROCES system                   
 INFO: Constant [BOD5      ] is NOT used by the PROCES system                   
 INFO: Constant [BODu      ] is NOT used by the PROCES system                   
 INFO: Constant [Salinity  ] is NOT used by the PROCES system                   
 INFO: Constant [TCRear    ] is NOT used by the PROCES system                   
 INFO: Constant [WINDDIR   ] is NOT used by the PROCES system                   
 INFO: Constant [RAD       ] is NOT used by the PROCES system                   
 INFO: Parameter [Xseg      ] is NOT used by the PROCES system                  
 INFO: Parameter [Yseg      ] is NOT used by the PROCES system                  
 INFO: Segment function [Width     ] is NOT used by the PROCES system           
                                                                                
# Locating requested output from active processes                               
                                                                                
 Output [ResTim              ] from proces [ResTim    ]                         
 Output [SatPercOXY          ] from proces [RearOXY   ]                         
 Output [SaturOXY            ] from proces [SaturOXY  ]                         
                                                                                
