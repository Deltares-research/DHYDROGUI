 Deltares, DELWAQ Version 5.08.00.8923, Jun 07 2018, 09:57:42
 Execution start: 2018/06/15 13:29:30 
                                                                                
 found -p command line switch                                                   

 Info: This processes definition file does not contain standard names and units for NetCDF files.

 
 Using process definition file : D:\repositories\Branches\WAQ_Sprint\bin\Debug\plugins\DeltaShell.Dimr\kernels\x64\dwaq\default\proc_def
 Version number                :       5.08
 Serial                        : 2018052301
 
                                                                                
 found -eco command line switch                                                 
 using eco input file:D:\repositories\Branches\WAQ_Sprint\bin\Debug\plugins\Delt


 Model :            Water quality calculation               
                                                            


 Run   :                                                    
                    T0: 2001.01.01 00:00:00  (scu=       1s)


# scanning input for old process definitions
 found only_active constant                                                     
 only activated processes are switched on                                       
 
total number of substances with fractions :  0
# Determining which processes can be switched on                                                    
                                                                                                    
 Input for [DynDepth            ] dynamic calculation of the depth                                  
   Process is activated                                                                             
                                                                                                    
 Input for [TotDepth            ] depth water column                                                
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
                                                                                                    
 Input for [PosOXY              ] Positive oxygen concentration                                     
   Process is activated                                                                             
                                                                                                    
                                                                                                    
# determinig the processes to model the substances.                                                 
                                                                                                    
-fluxes for [NH4                 ]                                                                  
 found flux  [dNITRIF             ] nitrification flux                                              
   from proces [Nitrif_NH4          ] Nitrification of ammonium                                     
   process is switched on.                                                                          
-dispersion for [NH4                 ]                                                              
 no dispersions found                                                                               
-velocity for [NH4                 ]                                                                
 no velocity found                                                                                  
                                                                                                    
-fluxes for [CBOD5               ]                                                                  
 found flux  [dCBOD5              ] decay flux of CBOD5                                             
   from proces [BODCOD              ] Mineralisation BOD and COD                                    
   process is switched on.                                                                          
-dispersion for [CBOD5               ]                                                              
 no dispersions found                                                                               
-velocity for [CBOD5               ]                                                                
 no velocity found                                                                                  
                                                                                                    
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
-dispersion for [OXY                 ]                                                              
 no dispersions found                                                                               
-velocity for [OXY                 ]                                                                
 no velocity found                                                                                  
                                                                                                    
-fluxes for [SOD                 ]                                                                  
 found flux  [dSOD                ] decay flux of SOD                                               
   from proces [SedOXYDem           ] Sediment oxygen demand                                        
   process is switched on.                                                                          
-dispersion for [SOD                 ]                                                              
 no dispersions found                                                                               
-velocity for [SOD                 ]                                                                
 no velocity found                                                                                  
                                                                                                    
# locating processes for requested output                                                           
                                                                                                    
# determining the input for the processes (in reversed order)                                       
                                                                                                    
 Input for [PosOXY              ] Positive oxygen concentration                                     
       [OXY                 ] Dissolved Oxygen                                                      
       Using substance nr   3                                                                       
                                                                                                    
 Input for [SedOXYDem           ] Sediment oxygen demand                                            
       [fSODaut             ] autonomous SOD (no effect SOD stat.var)                               
       using default value:  0.00000                                                                
       [fSOD                ] zeroth-order sediment oxygen demand flux                              
       Using constant nr 20 with value:  0.00000                                                    
       [Depth               ] depth of segment                                                      
       Using output from proces [DynDepth            ]                                              
       [SOD                 ] Sediment oxygen demand (SOD)                                          
       Using substance nr   4                                                                       
       [RcSOD               ] decay rate SOD at 20 oC                                               
       Using constant nr 21 with value: 0.100000                                                    
       [TcSOD               ] temperature coefficient decay SOD                                     
       using default value:  1.04000                                                                
       [Temp                ] ambient water temperature                                             
       Using constant nr 22 with value:  15.0000                                                    
       [Volume              ] volume of computational cell                                          
       Using DELWAQ volume                                                                          
       [SwCH4bub            ] switch (1=include CH4 bubbles, 0=not)                                 
       using default value:  0.00000                                                                
       [HSED                ] Total sediment thickness                                              
       using default value: 0.100000                                                                
       [KAPC                ] constant                                                              
       using default value:  1.60000                                                                
       [thetak              ] temperature constant                                                  
       using default value:  1.07900                                                                
       [edwcsd              ] diffusion coefficient                                                 
       using default value: 0.250000E-03                                                            
       [diamb               ] Diameter of methane bubbles                                           
       using default value:  1.00000                                                                
       [OXY                 ] Dissolved Oxygen                                                      
       Using substance nr   3                                                                       
       [kappad              ] transfer coefficient                                                  
       using default value: 0.300000E-02                                                            
       [dMinDetCS1          ] mineralisation flux DetCS1                                            
       using default value:  0.00000                                                                
       [dMinDetCS2          ] mineralisation flux DetCS2                                            
       using default value:  0.00000                                                                
       [dMinOOCS1           ] mineralisation flux OOCS1                                             
       using default value:  0.00000                                                                
       [dMinOOCS2           ] mineralisation flux OOCS2                                             
       using default value:  0.00000                                                                
       [TotalDepth          ] total depth water column                                              
       Using output from proces [TotDepth            ]                                              
       [COXSOD              ] critical oxygen concentration for SOD decay                           
       using default value:  0.00000                                                                
       [OOXSOD              ] optimum oxygen concentration for SOD decay                            
       using default value:  2.00000                                                                
                                                                                                    
 Input for [BODCOD              ] Mineralisation BOD and COD                                        
       [SwOXYDem            ] switch oxygen consumption(0=BOD, 1=COD, 2=both)                       
       using default value:  0.00000                                                                
       [CBOD5               ] carbonaceous BOD (first pool) at 5 days                               
       Using substance nr   2                                                                       
       [CBOD5_2             ] carbonaceous BOD (second pool) at 5 days                              
       using default value:  0.00000                                                                
       [CBODu               ] carbonaceous BOD (first pool) ultimate                                
       using default value:  0.00000                                                                
       [CBODu_2             ] carbonaceous BOD (second pool) ultimate                               
       using default value:  0.00000                                                                
       [COD_Cr              ] COD concentration by the Cr-method                                    
       using default value:  0.00000                                                                
       [COD_Mn              ] COD concentration by the Mn-method                                    
       using default value:  0.00000                                                                
       [NBOD5               ] nitrogenous BOD at 5 days                                             
       using default value:  0.00000                                                                
       [NBODu               ] nitrogenous BOD ultimate                                              
       using default value:  0.00000                                                                
       [RcBOD               ] decay rate BOD (first pool) at 20 oC                                  
       Using constant nr 11 with value: 0.300000                                                    
       [RcBOD_2             ] decay rate BOD (second pool) at 20 oC                                 
       using default value: 0.150000                                                                
       [RcCOD               ] decay rate COD at 20 oC                                               
       using default value: 0.500000E-01                                                            
       [RcBODN              ] first-order mineralisation rate BODN                                  
       using default value: 0.300000                                                                
       [TcBOD               ] temperature coefficient decay BOD                                     
       using default value:  1.04000                                                                
       [TcCOD               ] temperature coefficient decay COD                                     
       using default value:  1.02000                                                                
       [TcBODN              ] temperature coefficient decay BODN                                    
       using default value:  1.08000                                                                
       [Temp                ] ambient water temperature                                             
       Using constant nr 22 with value:  15.0000                                                    
       [OXY                 ] Dissolved Oxygen                                                      
       Using substance nr   3                                                                       
       [COXBOD              ] critical oxygen concentration for BOD decay                           
       Using constant nr 12 with value:  1.00000                                                    
       [OOXBOD              ] optimum oxygen concentration for BOD decay                            
       Using constant nr 13 with value:  5.00000                                                    
       [CFLBOD              ] oxygen function level for oxygen below COXBOD                         
       Using constant nr 14 with value: 0.300000                                                    
       [CurvBOD             ] curvature of DO function for mineralisation BOD                       
       using default value:  0.00000                                                                
       [LAgeFun             ] lower value of age function BOD decay                                 
       using default value:  1.00000                                                                
       [UAgeFun             ] upper value of age function BOD decay                                 
       using default value:  1.00000                                                                
       [LAgeIndx            ] lower value of age index BOD decay                                    
       using default value:  2.00000                                                                
       [UAgeIndx            ] upper value of age index BOD decay                                    
       using default value:  3.00000                                                                
       [Phyt                ] total carbon in phytoplankton                                         
       using default value:  0.00000                                                                
       [BOD5/uPHYT          ] BOD5:BODu ratio in phytoplankton                                      
       using default value: 0.600000                                                                
       [AlgFrBOD            ] fraction algae contributing to BOD-inf                                
       using default value: 0.500000                                                                
       [OXCCF               ] O2:C ratio in mineralisation                                          
       using default value:  2.67000                                                                
       [POCnoa              ] total POC (no algae)                                                  
       using default value:  0.00000                                                                
       [BOD5/infPO          ] BOD5:BODu ratio in POC                                                
       using default value: 0.600000                                                                
       [POCFrBOD            ] fraction of POC contributing to BOD-inf                               
       using default value:  1.00000                                                                
       [EffCOD_Cr           ] efficiency of Cr method for COD                                       
       using default value: 0.900000                                                                
       [EffCOD_Mn           ] efficiency of Mn method for COD                                       
       using default value: 0.500000                                                                
       [AMCCF               ] amount oxygen used for nitrogen in miner.                             
       using default value: 0.550000                                                                
                                                                                                    
 Input for [RearOXY             ] Reaeration of oxygen                                              
       [OXY                 ] Dissolved Oxygen                                                      
       Using substance nr   3                                                                       
       [Depth               ] depth of segment                                                      
       Using output from proces [DynDepth            ]                                              
       [Temp                ] ambient water temperature                                             
       Using constant nr 22 with value:  15.0000                                                    
       [Velocity            ] horizontal flow velocity                                              
       using default value: 0.500000                                                                
       [VWind               ] wind speed                                                            
       Using constant nr 23 with value:  3.00000                                                    
       [SWRear              ] switch for oxygen reaeration formulation (1-13)                       
       Using constant nr 18 with value:  1.00000                                                    
       [KLRear              ] reaeration transfer coefficient                                       
       Using constant nr 19 with value:  1.00000                                                    
       [TCRear              ] temperature coefficient for rearation                                 
       using default value:  1.01600                                                                
       [DELT                ] timestep for processes                                                
       Using DELWAQ timestep in days                                                                
       [SaturOXY            ] saturation concentration                                              
       Using output from proces [SaturOXY            ]                                              
       [Salinity            ] Salinity                                                              
       using default value:  35.0000                                                                
       [TotalDepth          ] total depth water column                                              
       Using output from proces [TotDepth            ]                                              
       [fcover              ] fraction of water surface covered <0-1>                               
       using default value:  0.00000                                                                
       [KLRearMax           ] maximum KLREAR oxygen for temp. correction                            
       using default value:  1000.00                                                                
       [KLRearMin           ] minimum rearation transfer coefficient oxygen                         
       using default value: 0.200000                                                                
       [Rain                ] rainfall rate                                                         
       using default value:  0.00000                                                                
       [coefAOxy            ] gas transfer Oxy coefficient transmission                             
       using default value:  1.66000                                                                
       [coefB1Oxy           ] gas transfer O2 coefficient wind scale 1                              
       using default value: 0.260000                                                                
       [coefB2Oxy           ] gas transfer O2 coefficient wind scale 2                              
       using default value:  1.00000                                                                
       [coefC1Oxy           ] gas transfer O2 coefficient rain scale 1                              
       using default value: 0.660000                                                                
       [coefC2Oxy           ] gas transfer O2 coefficient rain scale 2                              
       using default value:  1.00000                                                                
       [coefD1Oxy           ] fresh water coefficient1 for Schmidt nr Oxy                           
       using default value:  1800.06                                                                
       [coefD2Oxy           ] fresh water coefficient2 for Schmidt nr Oxy                           
       using default value:  120.100                                                                
       [coefD3Oxy           ] fresh water coefficient3 for Schmidt nr Oxy                           
       using default value:  3.78180                                                                
       [coefD4Oxy           ] fresh water coefficient4 for Schmidt nr Oxy                           
       using default value: 0.476080E-01                                                            
                                                                                                    
 Input for [SaturOXY            ] Saturation concentration oxygen                                   
       [Cl                  ] Chloride                                                              
       using default value:  20000.0                                                                
       [Temp                ] ambient water temperature                                             
       Using constant nr 22 with value:  15.0000                                                    
       [SWSatOXY            ] switch for oxygen saturation formulation (1, 2)                       
       using default value:  1.00000                                                                
       [Salinity            ] Salinity                                                              
       using default value:  35.0000                                                                
                                                                                                    
 Input for [Nitrif_NH4          ] Nitrification of ammonium                                         
       [ZNit                ] zeroth-order nitrification flux                                       
       using default value:  0.00000                                                                
       [NH4                 ] Ammonium (NH4)                                                        
       Using substance nr   1                                                                       
       [RcNit20             ] MM- nitrification rate at 20 oC                                       
       using default value: 0.100000                                                                
       [TcNit               ] temperature coefficient for nitrification                             
       using default value:  1.07000                                                                
       [OXY                 ] Dissolved Oxygen                                                      
       Using substance nr   3                                                                       
       [KsAmNit             ] half saturation constant for ammonium cons.                           
       using default value: 0.500000                                                                
       [KsOxNit             ] half saturation constant for DO cons.                                 
       using default value:  1.00000                                                                
       [Temp                ] ambient water temperature                                             
       Using constant nr 22 with value:  15.0000                                                    
       [CTNit               ] critical temperature for nitrification                                
       using default value:  3.00000                                                                
       [Rc0NitOx            ] zero-order nitrification rate at neg. DO                              
       using default value:  0.00000                                                                
       [COXNIT              ] critical oxygen concentration for nitrification                       
       using default value:  1.00000                                                                
       [Poros               ] volumetric porosity                                                   
       using default value:  1.00000                                                                
       [SWVnNit             ] switch for old (0), new (1), TEWOR (2) version                        
       using default value:  0.00000                                                                
       [RcNit               ] first-order nitrification rate                                        
       Using constant nr 10 with value: 0.100000                                                    
       [OOXNIT              ] optimum oxygen concentration for nitrification                        
       using default value:  5.00000                                                                
       [CFLNIT              ] oxygen function level for oxygen below COXNIT                         
       using default value:  0.00000                                                                
       [CurvNit             ] curvature of DO function for nitrification                            
       using default value:  0.00000                                                                
       [DELT                ] timestep for processes                                                
       Using DELWAQ timestep in days                                                                
                                                                                                    
 Input for [TotDepth            ] depth water column                                                
       [Depth               ] depth of segment                                                      
       Using output from proces [DynDepth            ]                                              
       [Surf                ] horizontal surface area of a DELWAQ segment                           
       Using parameter nr  1                                                                        
                                                                                                    
 Input for [DynDepth            ] dynamic calculation of the depth                                  
       [Volume              ] volume of computational cell                                          
       Using DELWAQ volume                                                                          
       [Surf                ] horizontal surface area of a DELWAQ segment                           
       Using parameter nr  1                                                                        
                                                                                                    
# determining the use of the delwaq input                                       
                                                                                
 info: constant [O2FuncBOD ] is not used by the proces system                   
 info: constant [BOD5      ] is not used by the proces system                   
 info: constant [BODu      ] is not used by the proces system                   
 info: constant [CLOSE_ERR ] is not used by the proces system                   
 info: constant [NOTHREADS ] is not used by the proces system                   
 info: constant [DRY_THRESH] is not used by the proces system                   
 info: constant [maxiter   ] is not used by the proces system                   
 info: constant [tolerance ] is not used by the proces system                   
 info: constant [iteration ] is not used by the proces system                   
                                                                                
# locating requested output from active processes                                                   
                                                                                                    
 output [DO                  ] from proces [PosOXY    ]                                             
                                                                                                    
