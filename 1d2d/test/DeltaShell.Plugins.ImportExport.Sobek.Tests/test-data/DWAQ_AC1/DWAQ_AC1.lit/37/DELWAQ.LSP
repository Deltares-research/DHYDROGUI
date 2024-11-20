                                                                                
 Found -A command line switch                                                   
 ONLY activated processes are switched on                                       


 Model :            Water quality calculation Sobek-WQ      
                                                            


 Run   :                                                    
                    T0: 2010.01.01 00:00:00  (scu=       1s)


 Found -P command line switch                                                   
 
 Using process definition file : D:\Sbk21204\Programs\delwaq\fixed\proc_d
 Version number                :       4.52
 Serial                        : 2012032801
 
 
total number of substances with fractions :  0
# Determining which processes can be switched on                                                    
                                                                                                    
 Input for [DynDepth            ] dynamic calculation of the depth                                  
   Process is activated                                                                             
                                                                                                    
 Input for [TotDepth            ] depth water column                                                
   Process is activated                                                                             
                                                                                                    
 Input for [ResTim              ] residence time                                                    
   Process is activated                                                                             
                                                                                                    
 Input for [CalTau              ] Calculation of bottom friction                                    
   Process is activated                                                                             
                                                                                                    
 Input for [Nitrif_NH4          ] Nitrification of ammonium                                         
   Process is activated                                                                             
                                                                                                    
 Input for [SaturOXY            ] Saturation concentration oxygen                                   
   Process is activated                                                                             
                                                                                                    
 Input for [RearOXY             ] Reaeration of oxygen                                              
   Process is activated                                                                             
                                                                                                    
 Input for [BODCOD              ] Mineralisation BOD and COD                                        
   Process is activated                                                                             
                                                                                                    
 Input for [SedOXYDem           ] Sediment oxygen demand                                            
   Process is activated                                                                             
                                                                                                    
 Input for [S_CBOD51            ] Sedimentation CBOD5 (first pool)                                  
   Process is activated                                                                             
                                                                                                    
 Input for [S_CBOD52            ] Sedimentation CBOD5_2 (second pool)                               
   Process is activated                                                                             
                                                                                                    
 Input for [Prod_TEWOR          ] Production fluxes for TEWOR+                                      
   Process is activated                                                                             
                                                                                                    
                                                                                                    
# determinig the processes to model the substances.                                                 
                                                                                                    
-fluxes for [Continuity          ]                                                                  
 no fluxes found                                                                                    
-dispersion for [Continuity          ]                                                              
 no dispersions found                                                                               
-velocity for [Continuity          ]                                                                
 no velocity found                                                                                  
                                                                                                    
-fluxes for [NH4                 ]                                                                  
 found flux  [dNITRIF             ] nitrification flux                                              
   from proces [Nitrif_NH4          ] Nitrification of ammonium                                     
   process is switched on.                                                                          
 found flux  [dTEWORNH4           ] TEWOR production flux of NH4                                    
   from proces [Prod_TEWOR          ] Production fluxes for TEWOR+                                  
   process is switched on.                                                                          
-dispersion for [NH4                 ]                                                              
 no dispersions found                                                                               
-velocity for [NH4                 ]                                                                
 no velocity found                                                                                  
                                                                                                    
-fluxes for [CBOD5               ]                                                                  
 found flux  [dCBOD5              ] decay flux of CBOD5                                             
   from proces [BODCOD              ] Mineralisation BOD and COD                                    
   process is switched on.                                                                          
 found flux  [dSedBOD5            ] sedimentation flux CBOD5                                        
   from proces [S_CBOD51            ] Sedimentation CBOD5 (first pool)                              
   process is switched on.                                                                          
 found flux  [dTEWORBOD           ] TEWOR production flux of CBOD5                                  
   from proces [Prod_TEWOR          ] Production fluxes for TEWOR+                                  
   process is switched on.                                                                          
-dispersion for [CBOD5               ]                                                              
 no dispersions found                                                                               
-velocity for [CBOD5               ]                                                                
 found velocity [VxSedBOD5           ] sedimentation velocity CBOD5                                 
   from proces [S_CBOD51            ] Sedimentation CBOD5 (first pool)                              
   process is switched on.                                                                          
                                                                                                    
-fluxes for [CBOD5_2             ]                                                                  
 found flux  [dCBOD5_2            ] decay flux of CBOD5_2                                           
   from proces [BODCOD              ] Mineralisation BOD and COD                                    
   process is switched on.                                                                          
 found flux  [dSedBOD5_2          ] sedimentation flux CBOD5_2                                      
   from proces [S_CBOD52            ] Sedimentation CBOD5_2 (second pool)                           
   process is switched on.                                                                          
 found flux  [dTEWORBOD2          ] TEWOR production flux of CBOD5_2                                
   from proces [Prod_TEWOR          ] Production fluxes for TEWOR+                                  
   process is switched on.                                                                          
-dispersion for [CBOD5_2             ]                                                              
 no dispersions found                                                                               
-velocity for [CBOD5_2             ]                                                                
 found velocity [VxSedBOD52          ] sedimentation velocity CBOD5_2                               
   from proces [S_CBOD52            ] Sedimentation CBOD5_2 (second pool)                           
   process is switched on.                                                                          
                                                                                                    
-fluxes for [OXY                 ]                                                                  
 found flux  [dNITRIF             ] nitrification flux                                              
   from proces [Nitrif_NH4          ] Nitrification of ammonium                                     
   process is switched on.                                                                          
 found flux  [dREAROXY            ] reaeration flux of dissolved oxygen                             
   from proces [RearOXY             ] Reaeration of oxygen                                          
   process is switched on.                                                                          
 found flux  [dOxyBODCOD          ] oxygen consumption from decay BOD and COD                       
   from proces [BODCOD              ] Mineralisation BOD and COD                                    
   process is switched on.                                                                          
 found flux  [dOxSOD              ] oxygen consumption from SOD                                     
   from proces [SedOXYDem           ] Sediment oxygen demand                                        
   process is switched on.                                                                          
 found flux  [dTEWOROXY           ] TEWOR production flux of OXY                                    
   from proces [Prod_TEWOR          ] Production fluxes for TEWOR+                                  
   process is switched on.                                                                          
-dispersion for [OXY                 ]                                                              
 no dispersions found                                                                               
-velocity for [OXY                 ]                                                                
 no velocity found                                                                                  
                                                                                                    
# locating processes for requested output                                                           
                                                                                                    
# determining the input for the processes (in reversed order)                                       
                                                                                                    
 Input for [Prod_TEWOR          ] Production fluxes for TEWOR+                                      
       [fTEWORBOD           ] TEWOR production flux of CBOD5_2                                      
       Using constant nr 18 with value: 0.00000                                                     
       [fTEWORBOD2          ] TEWOR production flux of CBOD5_2                                      
       using default value: 0.00000                                                                 
       [fTEWORBOD3          ] TEWOR production flux of CBOD5_3                                      
       using default value: 0.00000                                                                 
       [fTEWORCOD           ] TEWOR production flux of COD_Cr                                       
       using default value: 0.00000                                                                 
       [fTEWOROXY           ] TEWOR production flux of OXY                                          
       Using constant nr 19 with value: 0.00000                                                     
       [fTEWORORGN          ] TEWOR production flux of Org-N                                        
       using default value: 0.00000                                                                 
       [fTEWORNH4           ] TEWOR production flux of NH4                                          
       Using constant nr 20 with value: 0.00000                                                     
       [fTEWORNO3           ] TEWOR production flux of NO3                                          
       using default value: 0.00000                                                                 
       [fTEWOROON           ] TEWOR production flux of OON                                          
       using default value: 0.00000                                                                 
       [fTEWORECol          ] TEWOR production flux of EColi                                        
       using default value: 0.00000                                                                 
                                                                                                    
 Input for [S_CBOD52            ] Sedimentation CBOD5_2 (second pool)                               
       [CBOD5_2             ] carbonaceous BOD (second pool) at 5 days                              
       Using substance nr   4                                                                       
       [ZSedBOD5_2          ] zeroth-order sedimentation flux CBOD5_2                               
       using default value: 0.00000                                                                 
       [VSedBOD5_2          ] sedimentation velocity CBOD5_2                                        
       Using constant nr 31 with value:0.100000                                                     
       [Tau                 ] total bottom shear stress                                             
       Using output from proces [CalTau              ]                                              
       [TaucSBOD            ] critical shear stress for sedimentation BOD                           
       using default value:0.100000E+13                                                             
       [Depth               ] depth of segment                                                      
       Using output from proces [DynDepth            ]                                              
       [DELT                ] timestep for processes                                                
       Using DELWAQ timestep in days                                                                
       [MinDepth            ] minimum waterdepth for sedimentation                                  
       using default value:0.100000                                                                 
       [VSedBOD5_2          ] sedimentation velocity CBOD5_2                                        
       Using constant nr 31 with value:0.100000                                                     
                                                                                                    
 Input for [S_CBOD51            ] Sedimentation CBOD5 (first pool)                                  
       [CBOD5               ] carbonaceous BOD (first pool) at 5 days                               
       Using substance nr   3                                                                       
       [ZSedBOD5            ] zeroth-order sedimentation flux CBOD5                                 
       using default value: 0.00000                                                                 
       [VSedBOD5            ] sedimentation velocity CBOD5                                          
       Using constant nr 30 with value:0.100000                                                     
       [Tau                 ] total bottom shear stress                                             
       Using output from proces [CalTau              ]                                              
       [TaucSBOD            ] critical shear stress for sedimentation BOD                           
       using default value:0.100000E+13                                                             
       [Depth               ] depth of segment                                                      
       Using output from proces [DynDepth            ]                                              
       [DELT                ] timestep for processes                                                
       Using DELWAQ timestep in days                                                                
       [MinDepth            ] minimum waterdepth for sedimentation                                  
       using default value:0.100000                                                                 
       [VSedBOD5            ] sedimentation velocity CBOD5                                          
       Using constant nr 30 with value:0.100000                                                     
                                                                                                    
 Input for [SedOXYDem           ] Sediment oxygen demand                                            
       [fSODaut             ] autonomous SOD (no effect SOD stat.var)                               
       using default value: 0.00000                                                                 
       [fSOD                ] zeroth-order sediment oxygen demand flux                              
       Using constant nr 36 with value: 0.00000                                                     
       [Depth               ] depth of segment                                                      
       Using output from proces [DynDepth            ]                                              
       [SOD                 ] Sediment oxygen demand (SOD)                                          
       using default value: 0.00000                                                                 
       [RcSOD               ] decay rate SOD at 20 oC                                               
       Using constant nr 37 with value:0.112000                                                     
       [TcSOD               ] temperature coefficient decay SOD                                     
       using default value: 1.04000                                                                 
       [Temp                ] ambient water temperature                                             
       Using constant nr 40 with value: 18.0000                                                     
       [Volume              ] volume of computational cell                                          
       Using DELWAQ volume                                                                          
       [SwCH4bub            ] switch (1=include CH4 bubbles, 0=not)                                 
       using default value: 0.00000                                                                 
       [HSED                ] Total sediment thickness                                              
       using default value:0.100000                                                                 
       [KAPC                ] constant                                                              
       using default value: 1.60000                                                                 
       [thetak              ] temperature constant                                                  
       using default value: 1.07900                                                                 
       [edwcsd              ] diffusion coefficient                                                 
       using default value:0.250000E-03                                                             
       [diamb               ] Diameter of methane bubbles                                           
       using default value: 1.00000                                                                 
       [OXY                 ] Dissolved Oxygen                                                      
       Using substance nr   5                                                                       
       [kappad              ] transfer coefficient                                                  
       using default value:0.300000E-02                                                             
       [dMinDetCS1          ] mineralisation flux DetCS1                                            
       using default value: 0.00000                                                                 
       [dMinDetCS2          ] mineralisation flux DetCS2                                            
       using default value: 0.00000                                                                 
       [dMinOOCS1           ] mineralisation flux OOCS1                                             
       using default value: 0.00000                                                                 
       [dMinOOCS2           ] mineralisation flux OOCS2                                             
       using default value: 0.00000                                                                 
       [TotalDepth          ] total depth water column                                              
       Using output from proces [TotDepth            ]                                              
       [COXSOD              ] critical oxygen concentration for SOD decay                           
       using default value: 0.00000                                                                 
       [OOXSOD              ] optimum oxygen concentration for SOD decay                            
       using default value: 2.00000                                                                 
                                                                                                    
 Input for [BODCOD              ] Mineralisation BOD and COD                                        
       [SwOXYDem            ] switch oxygen consumption(0=BOD, 1=COD, 2=both)                       
       using default value: 0.00000                                                                 
       [CBOD5               ] carbonaceous BOD (first pool) at 5 days                               
       Using substance nr   3                                                                       
       [CBOD5_2             ] carbonaceous BOD (second pool) at 5 days                              
       Using substance nr   4                                                                       
       [CBODu               ] carbonaceous BOD (first pool) ultimate                                
       using default value: 0.00000                                                                 
       [CBODu_2             ] carbonaceous BOD (second pool) ultimate                               
       using default value: 0.00000                                                                 
       [COD_Cr              ] COD concentration by the Cr-method                                    
       using default value: 0.00000                                                                 
       [COD_Mn              ] COD concentration by the Mn-method                                    
       using default value: 0.00000                                                                 
       [NBOD5               ] nitrogenous BOD at 5 days                                             
       using default value: 0.00000                                                                 
       [NBODu               ] nitrogenous BOD ultimate                                              
       using default value: 0.00000                                                                 
       [RcBOD               ] decay rate BOD (first pool) at 20 oC                                  
       Using constant nr 21 with value:0.300000                                                     
       [RcBOD_2             ] decay rate BOD (second pool) at 20 oC                                 
       Using constant nr 22 with value:0.150000                                                     
       [RcCOD               ] decay rate COD at 20 oC                                               
       using default value:0.500000E-01                                                             
       [RcBODN              ] first-order mineralisation rate BODN                                  
       using default value:0.300000                                                                 
       [TcBOD               ] temperature coefficient decay BOD                                     
       Using constant nr 23 with value: 1.04000                                                     
       [TcCOD               ] temperature coefficient decay COD                                     
       using default value: 1.02000                                                                 
       [TcBODN              ] temperature coefficient decay BODN                                    
       using default value: 1.08000                                                                 
       [Temp                ] ambient water temperature                                             
       Using constant nr 40 with value: 18.0000                                                     
       [OXY                 ] Dissolved Oxygen                                                      
       Using substance nr   5                                                                       
       [COXBOD              ] critical oxygen concentration for BOD decay                           
       Using constant nr 24 with value: 1.00000                                                     
       [OOXBOD              ] optimum oxygen concentration for BOD decay                            
       Using constant nr 25 with value: 5.00000                                                     
       [CFLBOD              ] oxygen function level for oxygen below COXBOD                         
       Using constant nr 26 with value:0.300000                                                     
       [CurvBOD             ] curvature of DO function for mineralisation BOD                       
       using default value: 0.00000                                                                 
       [LAgeFun             ] lower value of age function BOD decay                                 
       using default value: 1.00000                                                                 
       [UAgeFun             ] upper value of age function BOD decay                                 
       using default value: 1.00000                                                                 
       [LAgeIndx            ] lower value of age index BOD decay                                    
       using default value: 2.00000                                                                 
       [UAgeIndx            ] upper value of age index BOD decay                                    
       using default value: 3.00000                                                                 
       [Phyt                ] total carbon in phytoplankton                                         
       using default value: 0.00000                                                                 
       [BOD5/uPHYT          ] BOD5:BODu ratio in phytoplankton                                      
       using default value:0.600000                                                                 
       [AlgFrBOD            ] fraction algae contributing to BOD-inf                                
       using default value:0.500000                                                                 
       [OXCCF               ] O2:C ratio in mineralisation                                          
       using default value: 2.67000                                                                 
       [POCnoa              ] total POC (no algae)                                                  
       using default value: 0.00000                                                                 
       [BOD5/infPO          ] BOD5:BODu ratio in POC                                                
       using default value:0.600000                                                                 
       [POCFrBOD            ] fraction of POC contributing to BOD-inf                               
       using default value: 1.00000                                                                 
       [EffCOD_Cr           ] efficiency of Cr method for COD                                       
       using default value:0.900000                                                                 
       [EffCOD_Mn           ] efficiency of Mn method for COD                                       
       using default value:0.500000                                                                 
       [AMCCF               ] amount oxygen used for nitrogen in miner.                             
       using default value:0.550000                                                                 
                                                                                                    
 Input for [RearOXY             ] Reaeration of oxygen                                              
       [OXY                 ] Dissolved Oxygen                                                      
       Using substance nr   5                                                                       
       [Depth               ] depth of segment                                                      
       Using output from proces [DynDepth            ]                                              
       [Temp                ] ambient water temperature                                             
       Using constant nr 40 with value: 18.0000                                                     
       [Velocity            ] horizontal flow velocity                                              
       Using segment function nr  3                                                                 
       [VWind               ] wind speed                                                            
       Using constant nr 38 with value: 3.00000                                                     
       [SWRear              ] switch for oxygen reaeration formulation (1-13)                       
       Using constant nr 33 with value: 1.00000                                                     
       [KLRear              ] reaeration transfer coefficient                                       
       Using constant nr 34 with value: 1.00000                                                     
       [TCRear              ] temperature coefficient for rearation                                 
       Using function nr  2                                                                         
       [DELT                ] timestep for processes                                                
       Using DELWAQ timestep in days                                                                
       [SaturOXY            ] saturation concentration                                              
       Using output from proces [SaturOXY            ]                                              
       [Salinity            ] Salinity                                                              
       Using function nr  1                                                                         
       [TotalDepth          ] total depth water column                                              
       Using output from proces [TotDepth            ]                                              
       [fcover              ] fraction of water surface covered <0-1>                               
       using default value: 0.00000                                                                 
       [KLRearMax           ] maximum KLREAR oxygen for temp. correction                            
       using default value: 1000.00                                                                 
       [KLRearMin           ] minimum rearation transfer coefficient oxygen                         
       using default value:0.200000                                                                 
       [Rain                ] rainfall rate                                                         
       using default value: 0.00000                                                                 
       [coefAOxy            ] gas transfer Oxy coefficient transmission                             
       using default value: 1.66000                                                                 
       [coefB1Oxy           ] gas transfer O2 coefficient wind scale 1                              
       using default value:0.260000                                                                 
       [coefB2Oxy           ] gas transfer O2 coefficient wind scale 2                              
       using default value: 1.00000                                                                 
       [coefC1Oxy           ] gas transfer O2 coefficient rain scale 1                              
       using default value:0.660000                                                                 
       [coefC2Oxy           ] gas transfer O2 coefficient rain scale 2                              
       using default value: 1.00000                                                                 
       [coefD1Oxy           ] fresh water coefficient1 for Schmidt nr Oxy                           
       using default value: 1800.06                                                                 
       [coefD2Oxy           ] fresh water coefficient2 for Schmidt nr Oxy                           
       using default value: 120.100                                                                 
       [coefD3Oxy           ] fresh water coefficient3 for Schmidt nr Oxy                           
       using default value: 3.78180                                                                 
       [coefD4Oxy           ] fresh water coefficient4 for Schmidt nr Oxy                           
       using default value:0.476080E-01                                                             
                                                                                                    
 Input for [SaturOXY            ] Saturation concentration oxygen                                   
       [Cl                  ] Chloride                                                              
       using default value: 20000.0                                                                 
       [Temp                ] ambient water temperature                                             
       Using constant nr 40 with value: 18.0000                                                     
       [SWSatOXY            ] switch for oxygen saturation formulation (1, 2)                       
       using default value: 1.00000                                                                 
       [Salinity            ] Salinity                                                              
       Using function nr  1                                                                         
                                                                                                    
 Input for [Nitrif_NH4          ] Nitrification of ammonium                                         
       [ZNit                ] zeroth-order nitrification flux                                       
       using default value: 0.00000                                                                 
       [NH4                 ] Ammonium (NH4)                                                        
       Using substance nr   2                                                                       
       [RcNit20             ] MM- nitrification rate at 20 oC                                       
       using default value:0.100000                                                                 
       [TcNit               ] temperature coefficient for nitrification                             
       Using constant nr 14 with value: 1.06380                                                     
       [OXY                 ] Dissolved Oxygen                                                      
       Using substance nr   5                                                                       
       [KsAmNit             ] half saturation constant for ammonium cons.                           
       using default value:0.500000                                                                 
       [KsOxNit             ] half saturation constant for DO cons.                                 
       using default value: 1.00000                                                                 
       [Temp                ] ambient water temperature                                             
       Using constant nr 40 with value: 18.0000                                                     
       [CTNit               ] critical temperature for nitrification                                
       using default value: 3.00000                                                                 
       [Rc0NitOx            ] zero-order nitrification rate at neg. DO                              
       using default value: 0.00000                                                                 
       [COXNIT              ] critical oxygen concentration for nitrification                       
       Using constant nr 15 with value: 1.00000                                                     
       [Poros               ] volumetric porosity                                                   
       using default value: 1.00000                                                                 
       [SWVnNit             ] switch for old (0), new (1), TEWOR (2) version                        
       using default value: 0.00000                                                                 
       [RcNit               ] first-order nitrification rate                                        
       Using constant nr 16 with value:0.100000                                                     
       [OOXNIT              ] optimum oxygen concentration for nitrification                        
       Using constant nr 17 with value: 5.00000                                                     
       [CFLNIT              ] oxygen function level for oxygen below COXNIT                         
       using default value: 0.00000                                                                 
       [CurvNit             ] curvature of DO function for nitrification                            
       using default value: 0.00000                                                                 
       [DELT                ] timestep for processes                                                
       Using DELWAQ timestep in days                                                                
                                                                                                    
 Input for [CalTau              ] Calculation of bottom friction                                    
       [WaveHeight          ] calculated height of a wind induced wave                              
       using default value: 0.00000                                                                 
       [WaveLength          ] calculated length of a wind induced wave                              
       using default value: 0.00000                                                                 
       [WavePeriod          ] calculated period of a wind induced wave                              
       using default value: 0.00000                                                                 
       [TauShip             ] bottom shear stress by ship movement                                  
       using default value: 0.00000                                                                 
       [SWTauVeloc          ] Switch Tauveloc (1=calculate|2=TauFlow)                               
       using default value: 1.00000                                                                 
       [TauFlow             ] bottom shear stress by FLOW                                           
       using default value: 0.00000                                                                 
       [Velocity            ] horizontal flow velocity                                              
       Using segment function nr  3                                                                 
       [CHEZY               ] Chezy coefficient                                                     
       Using segment function nr  1                                                                 
       [TotalDepth          ] total depth water column                                              
       Using output from proces [TotDepth            ]                                              
       [SWTau               ] switch <1=Tamminga|2=Swart|3=Soulsby>                                 
       using default value: 1.00000                                                                 
       [Depth               ] depth of segment                                                      
       Using output from proces [DynDepth            ]                                              
                                                                                                    
 Input for [ResTim              ] residence time                                                    
       [Volume              ] volume of computational cell                                          
       Using DELWAQ volume                                                                          
       [WSNoseg5            ] workspace array no. 5                                                 
       Using output from proces [ResTim              ]                                              
       [Flow                ] flow rate                                                             
       Using DELWAQ flow                                                                            
                                                                                                    
 Input for [TotDepth            ] depth water column                                                
       [Depth               ] depth of segment                                                      
       Using output from proces [DynDepth            ]                                              
       [Surf                ] horizontal surface area of a DELWAQ segment                           
       Using segment function nr  2                                                                 
                                                                                                    
 Input for [DynDepth            ] dynamic calculation of the depth                                  
       [Volume              ] volume of computational cell                                          
       Using DELWAQ volume                                                                          
       [Surf                ] horizontal surface area of a DELWAQ segment                           
       Using segment function nr  2                                                                 
                                                                                                    
# determining the use of the delwaq input                                       
                                                                                
 info: constant [Iteration ] is not used by the proces system                   
 info: constant [O2FuncBOD ] is not used by the proces system                   
 info: constant [BOD5      ] is not used by the proces system                   
 info: constant [BODu      ] is not used by the proces system                   
 info: constant [Salinity  ] is not used by the proces system                   
 info: constant [TCRear    ] is not used by the proces system                   
 info: constant [WINDDIR   ] is not used by the proces system                   
 info: constant [RAD       ] is not used by the proces system                   
 info: parameter [Xseg      ] is not used by the proces system                  
 info: parameter [Yseg      ] is not used by the proces system                  
 info: segment function [Width     ] is not used by the proces system           
                                                                                
# locating requested output from active processes                                                   
                                                                                                    
 output [ResTim              ] from proces [ResTim    ]                                             
 output [SatPercOXY          ] from proces [RearOXY   ]                                             
 output [SaturOXY            ] from proces [SaturOXY  ]                                             
                                                                                                    
