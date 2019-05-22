                                                                                
 Found -A command line switch                                                   
 ONLY activated processes are switched on                                       


 Model :            Water quality calculation Sobek-WQ      
                                                            


 Run   :                                                    
                    T0: 1996.01.01 00:00:00  (scu=       1s)


 Found -P command line switch                                                   
 
 Using process definition file : C:\SBKRiver\Programs\delwaq\fixed\proc_d
 Version number                :       4.52
 Serial                        : 2012032801
 
 
total number of substances with fractions :  0
# Determining which processes can be switched on                                                    
                                                                                                    
 Input for [HydDuflow           ] HydDuflow                                                         
   Process is activated                                                                             
                                                                                                    
 Input for [Omive               ] Omive                                                             
   Process is activated                                                                             
                                                                                                    
                                                                                                    
# determinig the processes to model the substances.                                                 
                                                                                                    
-fluxes for [CWTOT               ]                                                                  
 found flux  [D0CWTOT             ] (-Msed+Mres-Mdif1-Mverv-Mafb1+Matm+Mpsad1)/Z                    
   from proces [Omive               ] Omive                                                         
   process is switched on.                                                                          
-dispersion for [CWTOT               ]                                                              
 no dispersions found                                                                               
-velocity for [CWTOT               ]                                                                
 no velocity found                                                                                  
                                                                                                    
-fluxes for [POCW                ]                                                                  
 found flux  [D1POCW              ]  -KHYDW - VSN/Z * POCW                                          
   from proces [Omive               ] Omive                                                         
   process is switched on.                                                                          
 found flux  [D0POCW              ]   KDAOC*ALFA*AOCW + FRES*POCB/(SSB*Z)                           
   from proces [Omive               ] Omive                                                         
   process is switched on.                                                                          
-dispersion for [POCW                ]                                                              
 no dispersions found                                                                               
-velocity for [POCW                ]                                                                
 no velocity found                                                                                  
                                                                                                    
-fluxes for [SSW                 ]                                                                  
 found flux  [D1SSW               ] -Vss/Z * SSW                                                    
   from proces [Omive               ] Omive                                                         
   process is switched on.                                                                          
 found flux  [D0SSW               ] Fres/Z                                                          
   from proces [Omive               ] Omive                                                         
   process is switched on.                                                                          
-dispersion for [SSW                 ]                                                              
 no dispersions found                                                                               
-velocity for [SSW                 ]                                                                
 no velocity found                                                                                  
                                                                                                    
-fluxes for [DOCW                ]                                                                  
 found flux  [D1DOCW              ]  -KDMINW * DOCW                                                 
   from proces [Omive               ] Omive                                                         
   process is switched on.                                                                          
 found flux  [D0DOCW              ]  KHYDW*POCW+(1-ALFA)*KDAOC*AOCW   &                             
   from proces [Omive               ] Omive                                                         
   process is switched on.                                                                          
-dispersion for [DOCW                ]                                                              
 no dispersions found                                                                               
-velocity for [DOCW                ]                                                                
 no velocity found                                                                                  
                                                                                                    
-fluxes for [AOCW                ]                                                                  
 found flux  [D1AOCW              ] -KDAOC-VSP/Z * AOCW                                             
   from proces [Omive               ] Omive                                                         
   process is switched on.                                                                          
 found flux  [D0AOCW              ] PAOC/Z                                                          
   from proces [Omive               ] Omive                                                         
   process is switched on.                                                                          
-dispersion for [AOCW                ]                                                              
 no dispersions found                                                                               
-velocity for [AOCW                ]                                                                
 no velocity found                                                                                  
                                                                                                    
-fluxes for [DOCB                ]                                                                  
 found flux  [D1DOCB              ]  -KDMINB * DOCB / Z                                             
   from proces [Omive               ] Omive                                                         
   process is switched on.                                                                          
 found flux  [D0DOCB              ]  KHYDB*POCB   &                                                 
   from proces [Omive               ] Omive                                                         
   process is switched on.                                                                          
-dispersion for [DOCB                ]                                                              
 no dispersions found                                                                               
-velocity for [DOCB                ]                                                                
 no velocity found                                                                                  
                                                                                                    
-fluxes for [POCB                ]                                                                  
 found flux  [D1POCB              ]  -KHYDB - FRES/(SSB*DZ)-VS/DZ * POCB / Z                        
   from proces [Omive               ] Omive                                                         
   process is switched on.                                                                          
 found flux  [D0POCB              ]   VSN*POCW/DZ+VSP*AOCW/DZ+VR*POCSUB/DZ                          
   from proces [Omive               ] Omive                                                         
   process is switched on.                                                                          
-dispersion for [POCB                ]                                                              
 no dispersions found                                                                               
-velocity for [POCB                ]                                                                
 no velocity found                                                                                  
                                                                                                    
-fluxes for [DOCSUB              ]                                                                  
 no fluxes found                                                                                    
-dispersion for [DOCSUB              ]                                                              
 no dispersions found                                                                               
-velocity for [DOCSUB              ]                                                                
 no velocity found                                                                                  
                                                                                                    
-fluxes for [POCSUB              ]                                                                  
 no fluxes found                                                                                    
-dispersion for [POCSUB              ]                                                              
 no dispersions found                                                                               
-velocity for [POCSUB              ]                                                                
 no velocity found                                                                                  
                                                                                                    
-fluxes for [CBTOT               ]                                                                  
 found flux  [D0CBTOT             ] ( Msed-Mres+Mdif1-Mdif2-Mafb2+Mpsad2-Mpsad1)/DZ                 
   from proces [Omive               ] Omive                                                         
   process is switched on.                                                                          
-dispersion for [CBTOT               ]                                                              
 no dispersions found                                                                               
-velocity for [CBTOT               ]                                                                
 no velocity found                                                                                  
                                                                                                    
-fluxes for [CSUBT               ]                                                                  
 no fluxes found                                                                                    
-dispersion for [CSUBT               ]                                                              
 no dispersions found                                                                               
-velocity for [CSUBT               ]                                                                
 no velocity found                                                                                  
                                                                                                    
# locating processes for requested output                                                           
                                                                                                    
# determining the input for the processes (in reversed order)                                       
                                                                                                    
 Input for [Omive               ] Omive                                                             
       [CWTOT               ] CONCENTRATIE MICRO-VERONTREINIGING                                    
       Using substance nr   1                                                                       
       [POCW                ] PARTICULAIR ORGANISCHE C WATERKOLOM                                   
       Using substance nr   2                                                                       
       [SSW                 ] Zwevende stof waterkolom                                              
       Using substance nr   3                                                                       
       [DOCW                ] OPGELOST ORGANISCH C WATERKOLOM                                       
       Using substance nr   4                                                                       
       [AOCW                ] ALGEN KOOLSTOF                                                        
       Using substance nr   5                                                                       
       [DOCB                ] OPGELOST ORGANISCH C TOPLAAG                                          
       Using substance nr   6                                                                       
       [POCB                ] PARTICULAIR ORGANISCH C TOPLAAG                                       
       Using substance nr   7                                                                       
       [DOCSUB              ] OPGELOST ORGANISCH C SEDIMENT ONDERLAAG                               
       Using substance nr   8                                                                       
       [POCSUB              ] PARTICULAIR ORGANISCH C SEDIMENT ONDERLAAG                            
       Using substance nr   9                                                                       
       [CBTOT               ] MICRO-VERONTREINIGING TOPLAAG                                         
       Using substance nr  10                                                                       
       [CSUBT               ] MICRO-VERONTREINIGING SEDIMENT ONDERLAAG                              
       Using substance nr  11                                                                       
       [ALFA                ] FRACTIE POC DIE VRIJKOMT AFSTERVING ALGEN                             
       Using constant nr  4 with value:0.500000                                                     
       [DZ                  ] Dikte van de sedimenttoplaag                                          
       Using constant nr  5 with value:0.100000                                                     
       [EZ                  ] Mechanische dispersie water sediment                                  
       Using constant nr  6 with value:0.500000E-04                                                 
       [FC                  ] VERHOUDING C/DROGE STOF ALGEN EN DETRITUS                             
       Using constant nr  7 with value:0.520000                                                     
       [KPAOC               ] PARTITIECOEFFICIENT AOC                                               
       Using constant nr  8 with value:0.490000E-02                                                 
       [KPDOC               ] PARTITIECOEFFICIENT DOC                                               
       Using constant nr  9 with value:0.500000E-02                                                 
       [KPPOC               ] PARTITIECOEFFICIENT POC                                               
       Using constant nr 10 with value:0.490000E-02                                                 
       [KDAOC               ] SNELHEIDSCONSTANTE AFSTERVING ALGEN                                   
       Using constant nr 11 with value:0.500000E-01                                                 
       [KDC                 ] SNELHEIDSCONSTANTE AFBRAAK MICRO WATER                                
       Using constant nr 12 with value:0.500000E-03                                                 
       [KHYDW               ] SNELHEIDSCONSTANTE HYDROLYSE POC WATER                                
       Using constant nr 13 with value:0.100000E-01                                                 
       [KHYDB               ] SNELHEIDSCONSTANTE HYDROLYSE POC SEDIMENT                             
       Using constant nr 14 with value:0.500000E-04                                                 
       [KDMINW              ] SNELHEIDSCONSTANTE MINERALISATIE WATER                                
       Using constant nr 15 with value:0.400000E-01                                                 
       [KDMINB              ] SNELHEIDSCONSTANTE MINERALISATIE SEDIMENT                             
       Using constant nr 16 with value:0.150000E-01                                                 
       [KL                  ] STOFOVERDRACHTSCONSTANTE MICRO                                        
       Using constant nr 17 with value: 1.92000                                                     
       [POR                 ] Porositeit sediment                                                   
       Using constant nr 18 with value:0.800000                                                     
       [RHOANO              ] Dichtheid anorganische stof                                           
       Using constant nr 19 with value:0.260000E+07                                                 
       [RHOORG              ] Dichtheid organische stof                                             
       Using constant nr 20 with value:0.100000E+07                                                 
       [Vsn                 ] Sedimentatiesnelheid organisch N en P                                 
       Using constant nr 21 with value:0.700000                                                     
       [VSP                 ] SEDIMENTATIESNELHEID ALGEN                                            
       Using constant nr 22 with value:0.400000                                                     
       [Vss                 ] Sedimentatiesnelheid zwevende stof                                    
       Using constant nr 23 with value: 1.50000                                                     
       [Fres                ] Resuspensie flux                                                      
       Using constant nr 24 with value: 15.0000                                                     
       [Fatm                ] DIFFUSE BRON MICRO-VERONTREINIGING                                    
       Using constant nr 25 with value: 0.00000                                                     
       [PAOC                ] AOC PRODUCTIESNELHEID                                                 
       Using function nr  1                                                                         
       [Z                   ] water depth                                                           
       Using output from proces [HydDuflow           ]                                              
                                                                                                    
 Input for [HydDuflow           ] HydDuflow                                                         
       [Volume              ] volume of computational cell                                          
       Using DELWAQ volume                                                                          
       [Surf                ] horizontal surface area of a DELWAQ segment                           
       Using segment function nr  2                                                                 
       [DELT                ] timestep for processes                                                
       Using DELWAQ timestep in days                                                                
       [Width               ] total width                                                           
       Using segment function nr  4                                                                 
       [VWind               ] wind speed                                                            
       Using constant nr 27 with value: 0.00000                                                     
       [WindDir             ] actual wind direction                                                 
       Using constant nr 28 with value: 241.000                                                     
       [Velocity            ] horizontal flow velocity                                              
       Using segment function nr  3                                                                 
                                                                                                    
# determining the use of the delwaq input                                       
                                                                                
 info: constant [Iteration ] is not used by the proces system                   
 info: constant [PAOC      ] is not used by the proces system                   
 info: constant [NATTEMP   ] is not used by the proces system                   
 info: constant [RAD       ] is not used by the proces system                   
 info: parameter [Xseg      ] is not used by the proces system                  
 info: parameter [Yseg      ] is not used by the proces system                  
 info: segment function [Chezy     ] is not used by the proces system           
                                                                                
# locating requested output from active processes                                                   
                                                                                                    
 warning: output [Temp                ] not located                                                 
 output [CBDIS               ] from proces [Omive     ]                                             
 output [CBDOC               ] from proces [Omive     ]                                             
 output [CBPOC               ] from proces [Omive     ]                                             
 output [CIW                 ] from proces [Omive     ]                                             
 output [CSSB                ] from proces [Omive     ]                                             
 output [CSSW                ] from proces [Omive     ]                                             
 output [CSUBDIS             ] from proces [Omive     ]                                             
 output [CSUBDOC             ] from proces [Omive     ]                                             
 output [CWAOC               ] from proces [Omive     ]                                             
 output [CWDIS               ] from proces [Omive     ]                                             
 output [CWDOC               ] from proces [Omive     ]                                             
 output [CWPOC               ] from proces [Omive     ]                                             
 output [dt                  ] from proces [HydDuflow ]                                             
 output [EZ2                 ] from proces [Omive     ]                                             
 output [FCSSB               ] from proces [Omive     ]                                             
 output [FCSSW               ] from proces [Omive     ]                                             
 output [FOMB                ] from proces [Omive     ]                                             
 output [FOMW                ] from proces [Omive     ]                                             
 output [FSPOC               ] from proces [Omive     ]                                             
 output [FSSB                ] from proces [Omive     ]                                             
 output [FSSW                ] from proces [Omive     ]                                             
 output [Mafb1               ] from proces [Omive     ]                                             
 output [Mafb2               ] from proces [Omive     ]                                             
 output [Matm                ] from proces [Omive     ]                                             
 output [Mdif1               ] from proces [Omive     ]                                             
 output [Mdif2               ] from proces [Omive     ]                                             
 output [Mpsad1              ] from proces [Omive     ]                                             
 output [Mpsad2              ] from proces [Omive     ]                                             
 output [Mres                ] from proces [Omive     ]                                             
 output [Msed                ] from proces [Omive     ]                                             
 output [Mverv               ] from proces [Omive     ]                                             
 output [RHOSSB              ] from proces [Omive     ]                                             
 output [RHOSSW              ] from proces [Omive     ]                                             
 output [SSB                 ] from proces [Omive     ]                                             
 output [VR                  ] from proces [Omive     ]                                             
 output [VS                  ] from proces [Omive     ]                                             
 output [Z                   ] from proces [HydDuflow ]                                             
                                                                                                    
