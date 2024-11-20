 Deltares, DELWAQ Version 5.08.00.64757M, Aug 28 2019, 20:32:30
 Execution start: 2019/11/07 17:39:16 
                                                                                
 found -p command line switch                                                   

 Info: This processes definition file does not contain standard names and units for NetCDF files.

 
 Using process definition file : C:\Program Files (x86)\Deltares\Delft3D FM Suite 2020.01 HMWQ (1.6.0.46550)\plugins\DeltaShell.Dimr\kernels\x64\dwaq\default\proc_def
 Version number                :       5.08
 Serial                        : 2019050601
 
                                                                                
 found -eco command line switch                                                 
 using eco input file:C:\Program Files (x86)\Deltares\Delft3D FM Suite 2020.01 H


 Model :            Water quality calculation               
                                                            


 Run   :                                                    
                    T0: 2001.01.01 00:00:00  (scu=       1s)


# scanning input for old process definitions
 no BLOOM algae were found, switching of eco mode.                              
 found only_active constant                                                     
 only activated processes are switched on                                       
 
total number of substances with fractions :  0
# Determining which processes can be switched on                                                    
                                                                                                    
 Input for [DynDepth            ] dynamic calculation of the depth                                  
   Process is activated                                                                             
                                                                                                    
 Input for [Salinchlor          ] Conversion of salinity into chloride or vv                        
   Process is activated                                                                             
                                                                                                    
 Input for [CalcRadUV           ] UV-Radiation at segment upper and lower boundaries                
   Process is activated                                                                             
                                                                                                    
 Input for [EColiMrt            ] Mortality EColi bacteria                                          
   Process is activated                                                                             
                                                                                                    
                                                                                                    
# determinig the processes to model the substances.                                                 
                                                                                                    
-fluxes for [Salinity            ]                                                                  
 no fluxes found                                                                                    
-dispersion for [Salinity            ]                                                              
 no dispersions found                                                                               
-velocity for [Salinity            ]                                                                
 no velocity found                                                                                  
                                                                                                    
-fluxes for [EColi               ]                                                                  
 found flux  [dMrtEColi           ] mortality flux EColi                                            
   from proces [EColiMrt            ] Mortality EColi bacteria                                      
   process is switched on.                                                                          
-dispersion for [EColi               ]                                                              
 no dispersions found                                                                               
-velocity for [EColi               ]                                                                
 no velocity found                                                                                  
                                                                                                    
# locating processes for requested output                                                           
                                                                                                    
# determining the input for the processes (in reversed order)                                       
                                                                                                    
 Input for [EColiMrt            ] Mortality EColi bacteria                                          
       [EColi               ] E. Coli bacteria                                                      
       Using substance nr   2                                                                       
       [RcMrtEColi          ] first-order mortality rate EColi                                      
       Using constant nr  6 with value: 0.800000                                                    
       [TcMrtEColi          ] temperature coefficient for mortality EColi                           
       using default value:  1.07000                                                                
       [Temp                ] ambient water temperature                                             
       Using constant nr  7 with value:  15.0000                                                    
       [CTMrtEColi          ] critical temperature for mortality EColi                              
       using default value:  2.00000                                                                
       [Cl                  ] Chloride                                                              
       Using output from proces [Salinchlor          ]                                              
       [Rad_uv              ] UV-irradiation at the segment upper-boundary                          
       Using output from proces [CalcRadUV           ]                                              
       [CFRAD               ] conversion factor RAD to mortality rate                               
       using default value: 0.860000E-01                                                            
       [DayL                ] daylength <0-1>                                                       
       Using constant nr  8 with value: 0.580000                                                    
       [FrUvVL              ] fraction UV light in visible light                                    
       using default value: 0.120000                                                                
       [ExtUv               ] total extinction coefficient UV light                                 
       Using constant nr  9 with value:  3.00000                                                    
       [Depth               ] depth of segment                                                      
       Using output from proces [DynDepth            ]                                              
       [SpMrtEColi          ] chloride enhanced mortality rate EColi                                
       using default value: 0.110000E-04                                                            
                                                                                                    
 Input for [CalcRadUV           ] UV-Radiation at segment upper and lower boundaries                
       [ExtUv               ] total extinction coefficient UV light                                 
       Using constant nr  9 with value:  3.00000                                                    
       [Depth               ] depth of segment                                                      
       Using output from proces [DynDepth            ]                                              
       [RadSurf             ] irradiation at the water surface                                      
       Using constant nr 10 with value:  0.00000                                                    
       [a_enh               ] enhancement factor in radiation calculation                           
       using default value:  1.50000                                                                
       [Surf                ] horizontal surface area of a DELWAQ segment                           
       Using parameter nr  1                                                                        
       [SwEmersion          ] switch indicating submersion(0) or emersion(1)                        
       using default value:  0.00000                                                                
       [RadBot_uv           ] UV-irradiation at the segment lower-boundary                          
       Using output from proces [CalcRadUV           ]                                              
       [fRefl               ] fraction of radiation reflected at water surface                      
       using default value:  0.00000                                                                
                                                                                                    
 Input for [Salinchlor          ] Conversion of salinity into chloride or vv                        
       [Salinity            ] Salinity                                                              
       Using substance nr   1                                                                       
       [Cl                  ] Chloride                                                              
       Using output from proces [Salinchlor          ]                                              
       [GtCl                ] Salinity:Chloride ratio in sea water                                  
       using default value:  1.80500                                                                
       [Temp                ] ambient water temperature                                             
       Using constant nr  7 with value:  15.0000                                                    
       [Sal0                ] salinity at zero chloride concentration                               
       using default value: 0.300000E-01                                                            
       [SWSalCl             ] option: 0.0 salinity simulated, 1.0 Cl simultated                     
       using default value:  0.00000                                                                
                                                                                                    
 Input for [DynDepth            ] dynamic calculation of the depth                                  
       [Volume              ] volume of computational cell                                          
       Using DELWAQ volume                                                                          
       [Surf                ] horizontal surface area of a DELWAQ segment                           
       Using parameter nr  1                                                                        
                                                                                                    
# determining the use of the delwaq input                                       
                                                                                
 info: constant [NOTHREADS ] is not used by the proces system                   
 info: constant [DRY_THRESH] is not used by the proces system                   
                                                                                
# locating requested output from active processes                                                   
                                                                                                    
                                                                                                    
