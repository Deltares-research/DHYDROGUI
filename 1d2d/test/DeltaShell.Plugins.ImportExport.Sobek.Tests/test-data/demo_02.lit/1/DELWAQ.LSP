                                                                                
 Found -A command line switch                                                   
 ONLY activated processes are switched on                                       


 Model :            Water quality calculation Sobek-WQ      
                                                            


 Run   :                                                    
                    T0: 1996.01.01 00:00:00  (scu=       1s)


 Found -P command line switch                                                   
 
 Using process definition file : C:\SBKRiver\Programs\delwaq\fixed\proc_d
 Version number                :       4.52
 Serial                        : 2011111111
 
# Determining which processes can be switched on                                                    
                                                                                                    
 Input for [HydDuflow ] HydDuflow                                                                   
   Process is activated                                                                             
                                                                                                    
 Input for [DECAY2    ] DECAY2                                                                      
   Process is activated                                                                             
                                                                                                    
                                                                                                    
# Determinig the processes to model the substances.                             
                                                                                
-Fluxes for [C         ]                                                        
 Found flux  [D1C       ] -Kd * C                                               
   from proces [DECAY2    ] DECAY2                                              
   Process is switched on.                                                      
 Found flux  [D0C       ] Sd/z                                                  
   from proces [DECAY2    ] DECAY2                                              
   Process is switched on.                                                      
-Dispersion for [C         ]                                                    
 No dispersions found                                                           
-Velocity for [C         ]                                                      
 No velocity found                                                              
                                                                                
# Locating processes for requested output                                       
                                                                                
# Determining the input for the processes (in reversed order)                   
                                                                                
 Input for [DECAY2    ] DECAY2                                                  
       [C                   ] Concentratie verontreiniging                      
       Using substance nr   1                                                   
       [Kd                  ] Snelheidsconstante verwijdering                   
       Using constant nr  4                                                     
       [Sd                  ] Gedistribueerde bron                              
       Using constant nr  5                                                     
       [Z                   ] water depth                                       
       Using output from proces [HydDuflow ]                                    
                                                                                
 Input for [HydDuflow ] HydDuflow                                               
       [Volume              ] volume of computational cell                      
       Using DELWAQ volume                                                      
       [Surf                ] horizontal surface area of a DELWAQ segment       
       Using segment function nr  2                                             
       [DELT                ] timestep for processes                            
       Using DELWAQ timestep in days                                            
                                                                                
# Determining the use of the DELWAQ input                                       
                                                                                
 INFO: Constant [Iteration ] is NOT used by the PROCES system                   
 INFO: Constant [VWIND     ] is NOT used by the PROCES system                   
 INFO: Constant [WINDDIR   ] is NOT used by the PROCES system                   
 INFO: Constant [NATTEMP   ] is NOT used by the PROCES system                   
 INFO: Constant [RAD       ] is NOT used by the PROCES system                   
 INFO: Parameter [Xseg      ] is NOT used by the PROCES system                  
 INFO: Parameter [Yseg      ] is NOT used by the PROCES system                  
 INFO: Segment function [Chezy     ] is NOT used by the PROCES system           
 INFO: Segment function [Velocity  ] is NOT used by the PROCES system           
 INFO: Segment function [Width     ] is NOT used by the PROCES system           
                                                                                
# Locating requested output from active processes                               
                                                                                
 WARNING: Output [Temp                ] not located                             
 Output [dt                  ] from proces [HydDuflow ]                         
 Output [LogN                ] from proces [DECAY2    ]                         
 Output [N                   ] from proces [DECAY2    ]                         
 Output [Z                   ] from proces [HydDuflow ]                         
                                                                                
