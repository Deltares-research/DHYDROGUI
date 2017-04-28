                                                                                
 found -p command line switch                                                   
 
 Using process definition file : D:\NGHS\release\Products\NGHS\src\DeltaShell.Plugins.DelftModels.WaterQualityModel\waq_kernel\Data\Default\proc_def
 Version number                :       5.03
 Serial                        : 2015061701
 
                                                                                
 found -eco command line switch                                                 
 using eco input file:D:\NGHS\release\Products\NGHS\bin\Debug\plugins\DeltaShell


 Model :            Water quality calculation               
                                                            


 Run   :                                                    
                    T0: 2014.01.01 00:00:00  (scu=       1s)


# scanning input for old process definitions
 found only_active constant                                                     
 only activated processes are switched on                                       
 
total number of substances with fractions :  0
# Determining which processes can be switched on                                                    
                                                                                                    
 Input for [DynDepth            ] dynamic calculation of the depth                                  
   Process is activated                                                                             
                                                                                                    
 Input for [EXTINABVL           ] Extinction of light by algae (Bloom)                              
   Process is activated                                                                             
                                                                                                    
 Input for [Extinc_VLG          ] Extinction of visible-light (370-680nm) DLWQ-G                    
   Process is activated                                                                             
                                                                                                    
 Input for [CalcRad             ] Radiation at segment upper and lower boundaries                   
   Process is activated                                                                             
                                                                                                    
 Input for [Daylength           ] Daylength calculation                                             
   Process is activated                                                                             
                                                                                                    
 Input for [DepAve              ] Average depth for Bloom step                                      
   Process is activated                                                                             
                                                                                                    
 Input for [BLOOM               ] BLOOM II algae module                                             
   Process is activated                                                                             
                                                                                                    
 Input for [Phy_Blo             ] Computation of phytoplankton output - Bloom                       
   Process is activated                                                                             
                                                                                                    
 Input for [Compos              ] Composition                                                       
   Process is activated                                                                             
                                                                                                    
 Input for [DecFast             ] Mineralization fast decomp. detritus POC1                         
   Process is activated                                                                             
                                                                                                    
 Input for [DisSi               ] Dissolution of Si in opal (SWITCH defaults)                       
   Process is activated                                                                             
                                                                                                    
                                                                                                    
# determinig the processes to model the substances.                                                 
                                                                                                    
-fluxes for [Continuity          ]                                                                  
 no fluxes found                                                                                    
-dispersion for [Continuity          ]                                                              
 no dispersions found                                                                               
-velocity for [Continuity          ]                                                                
 no velocity found                                                                                  
                                                                                                    
-fluxes for [NH4                 ]                                                                  
 found flux  [dNaut               ] autolysis flux of nitrogen                                      
   from proces [BLOOM               ] BLOOM II algae module                                         
   process is switched on.                                                                          
 found flux  [dNH4Upt             ] NH4 uptake by algae growth                                      
   from proces [BLOOM               ] BLOOM II algae module                                         
   process is switched on.                                                                          
 found flux  [dMinPON1            ] mineralization flux PON1 to NH4                                 
   from proces [DecFast             ] Mineralization fast decomp. detritus POC1                     
   process is switched on.                                                                          
-dispersion for [NH4                 ]                                                              
 no dispersions found                                                                               
-velocity for [NH4                 ]                                                                
 no velocity found                                                                                  
                                                                                                    
-fluxes for [NO3                 ]                                                                  
 found flux  [dNO3Upt             ] uptake of NO3 by algae growth                                   
   from proces [BLOOM               ] BLOOM II algae module                                         
   process is switched on.                                                                          
-dispersion for [NO3                 ]                                                              
 no dispersions found                                                                               
-velocity for [NO3                 ]                                                                
 no velocity found                                                                                  
                                                                                                    
-fluxes for [PO4                 ]                                                                  
 found flux  [dPaut               ] autolysis flux of PO4                                           
   from proces [BLOOM               ] BLOOM II algae module                                         
   process is switched on.                                                                          
 found flux  [dPO4Upt             ] PO4 uptake by algae growth                                      
   from proces [BLOOM               ] BLOOM II algae module                                         
   process is switched on.                                                                          
 found flux  [dMinPOP1            ] mineralization flux POP1 to PO4                                 
   from proces [DecFast             ] Mineralization fast decomp. detritus POC1                     
   process is switched on.                                                                          
-dispersion for [PO4                 ]                                                              
 no dispersions found                                                                               
-velocity for [PO4                 ]                                                                
 no velocity found                                                                                  
                                                                                                    
-fluxes for [Si                  ]                                                                  
 found flux  [dSIaut              ] autolysis flux of silicate                                      
   from proces [BLOOM               ] BLOOM II algae module                                         
   process is switched on.                                                                          
 found flux  [dSIUpt              ] Si uptake by algae growth                                       
   from proces [BLOOM               ] BLOOM II algae module                                         
   process is switched on.                                                                          
 found flux  [dDissolSi           ] dissolution flux Opal to Si                                     
   from proces [DisSi               ] Dissolution of Si in opal (SWITCH defaults)                   
   process is switched on.                                                                          
-dispersion for [Si                  ]                                                              
 no dispersions found                                                                               
-velocity for [Si                  ]                                                                
 no velocity found                                                                                  
                                                                                                    
-fluxes for [Opal                ]                                                                  
 found flux  [dDetSiMort          ] production of DetSi by mortality                                
   from proces [BLOOM               ] BLOOM II algae module                                         
   process is switched on.                                                                          
 found flux  [dOOSiMort           ] production of OOSi by mortality                                 
   from proces [BLOOM               ] BLOOM II algae module                                         
   process is switched on.                                                                          
 found flux  [dDissolSi           ] dissolution flux Opal to Si                                     
   from proces [DisSi               ] Dissolution of Si in opal (SWITCH defaults)                   
   process is switched on.                                                                          
-dispersion for [Opal                ]                                                              
 no dispersions found                                                                               
-velocity for [Opal                ]                                                                
 no velocity found                                                                                  
                                                                                                    
-fluxes for [POC1                ]                                                                  
 found flux  [dDetCMort           ] production of DetC by mortality                                 
   from proces [BLOOM               ] BLOOM II algae module                                         
   process is switched on.                                                                          
 found flux  [dDetCUpt            ] uptake of DetC by heterotroph algae growth                      
   from proces [BLOOM               ] BLOOM II algae module                                         
   process is switched on.                                                                          
 found flux  [dCnvPPOC1           ] conversion flux POC1 to POC2                                    
   from proces [DecFast             ] Mineralization fast decomp. detritus POC1                     
   process is switched on.                                                                          
 found flux  [dCnvDPOC1           ] conversion flux POC1 to DOC                                     
   from proces [DecFast             ] Mineralization fast decomp. detritus POC1                     
   process is switched on.                                                                          
 found flux  [dMinPOC1G           ] mineralization flux POC1 to CO2                                 
   from proces [DecFast             ] Mineralization fast decomp. detritus POC1                     
   process is switched on.                                                                          
-dispersion for [POC1                ]                                                              
 no dispersions found                                                                               
-velocity for [POC1                ]                                                                
 no velocity found                                                                                  
                                                                                                    
-fluxes for [PON1                ]                                                                  
 found flux  [dDetNMort           ] production of DetN by mortality                                 
   from proces [BLOOM               ] BLOOM II algae module                                         
   process is switched on.                                                                          
 found flux  [dDetNUpt            ] uptake of DetN by heterotroph algae growth                      
   from proces [BLOOM               ] BLOOM II algae module                                         
   process is switched on.                                                                          
 found flux  [dCnvPPON1           ] conversion flux PON1 to PON2                                    
   from proces [DecFast             ] Mineralization fast decomp. detritus POC1                     
   process is switched on.                                                                          
 found flux  [dCnvDPON1           ] conversion flux PON1 to DON                                     
   from proces [DecFast             ] Mineralization fast decomp. detritus POC1                     
   process is switched on.                                                                          
 found flux  [dMinPON1            ] mineralization flux PON1 to NH4                                 
   from proces [DecFast             ] Mineralization fast decomp. detritus POC1                     
   process is switched on.                                                                          
-dispersion for [PON1                ]                                                              
 no dispersions found                                                                               
-velocity for [PON1                ]                                                                
 no velocity found                                                                                  
                                                                                                    
-fluxes for [POP1                ]                                                                  
 found flux  [dDetPMort           ] production of DetP by mortality                                 
   from proces [BLOOM               ] BLOOM II algae module                                         
   process is switched on.                                                                          
 found flux  [dDetPUpt            ] uptake of DetP by heterotroph algae growth                      
   from proces [BLOOM               ] BLOOM II algae module                                         
   process is switched on.                                                                          
 found flux  [dCnvPPOP1           ] conversion flux POP1 to POP2                                    
   from proces [DecFast             ] Mineralization fast decomp. detritus POC1                     
   process is switched on.                                                                          
 found flux  [dCnvDPOP1           ] conversion flux POP1 to DOP                                     
   from proces [DecFast             ] Mineralization fast decomp. detritus POC1                     
   process is switched on.                                                                          
 found flux  [dMinPOP1            ] mineralization flux POP1 to PO4                                 
   from proces [DecFast             ] Mineralization fast decomp. detritus POC1                     
   process is switched on.                                                                          
-dispersion for [POP1                ]                                                              
 no dispersions found                                                                               
-velocity for [POP1                ]                                                                
 no velocity found                                                                                  
                                                                                                    
-fluxes for [POS1                ]                                                                  
 found flux  [dDetCMort           ] production of DetC by mortality                                 
   from proces [BLOOM               ] BLOOM II algae module                                         
   process is switched on.                                                                          
 found flux  [dCnvPPOS1           ] conversion flux POS1 to POS2                                    
   from proces [DecFast             ] Mineralization fast decomp. detritus POC1                     
   process is switched on.                                                                          
 found flux  [dCnvDPOS1           ] conversion flux POS1 to DOS                                     
   from proces [DecFast             ] Mineralization fast decomp. detritus POC1                     
   process is switched on.                                                                          
 found flux  [dMinPOS1            ] mineralization flux POS1 to SUD                                 
   from proces [DecFast             ] Mineralization fast decomp. detritus POC1                     
   process is switched on.                                                                          
-dispersion for [POS1                ]                                                              
 no dispersions found                                                                               
-velocity for [POS1                ]                                                                
 no velocity found                                                                                  
                                                                                                    
-fluxes for [APHANIZO_E          ]                                                                  
 found flux  [dProdAlg07          ] primary production of algae type 07                             
   from proces [BLOOM               ] BLOOM II algae module                                         
   process is switched on.                                                                          
 found flux  [dMortAlg07          ] mortality of algae type 07                                      
   from proces [BLOOM               ] BLOOM II algae module                                         
   process is switched on.                                                                          
-dispersion for [APHANIZO_E          ]                                                              
 no dispersions found                                                                               
-velocity for [APHANIZO_E          ]                                                                
 no velocity found                                                                                  
                                                                                                    
-fluxes for [APHANIZO_N          ]                                                                  
 found flux  [dProdAlg08          ] primary production of algae type 08                             
   from proces [BLOOM               ] BLOOM II algae module                                         
   process is switched on.                                                                          
 found flux  [dMortAlg08          ] mortality of algae type 08                                      
   from proces [BLOOM               ] BLOOM II algae module                                         
   process is switched on.                                                                          
-dispersion for [APHANIZO_N          ]                                                              
 no dispersions found                                                                               
-velocity for [APHANIZO_N          ]                                                                
 no velocity found                                                                                  
                                                                                                    
-fluxes for [APHANIZO_P          ]                                                                  
 found flux  [dProdAlg09          ] primary production of algae type 09                             
   from proces [BLOOM               ] BLOOM II algae module                                         
   process is switched on.                                                                          
 found flux  [dMortAlg09          ] mortality of algae type 09                                      
   from proces [BLOOM               ] BLOOM II algae module                                         
   process is switched on.                                                                          
-dispersion for [APHANIZO_P          ]                                                              
 no dispersions found                                                                               
-velocity for [APHANIZO_P          ]                                                                
 no velocity found                                                                                  
                                                                                                    
-fluxes for [FDIATOMS_E          ]                                                                  
 found flux  [dProdAlg01          ] primary production of algae type 01                             
   from proces [BLOOM               ] BLOOM II algae module                                         
   process is switched on.                                                                          
 found flux  [dMortAlg01          ] mortality of algae type 01                                      
   from proces [BLOOM               ] BLOOM II algae module                                         
   process is switched on.                                                                          
-dispersion for [FDIATOMS_E          ]                                                              
 no dispersions found                                                                               
-velocity for [FDIATOMS_E          ]                                                                
 no velocity found                                                                                  
                                                                                                    
-fluxes for [FDIATOMS_P          ]                                                                  
 found flux  [dProdAlg02          ] primary production of algae type 02                             
   from proces [BLOOM               ] BLOOM II algae module                                         
   process is switched on.                                                                          
 found flux  [dMortAlg02          ] mortality of algae type 02                                      
   from proces [BLOOM               ] BLOOM II algae module                                         
   process is switched on.                                                                          
-dispersion for [FDIATOMS_P          ]                                                              
 no dispersions found                                                                               
-velocity for [FDIATOMS_P          ]                                                                
 no velocity found                                                                                  
                                                                                                    
-fluxes for [FFLAGELA            ]                                                                  
 found flux  [dProdAlg03          ] primary production of algae type 03                             
   from proces [BLOOM               ] BLOOM II algae module                                         
   process is switched on.                                                                          
 found flux  [dMortAlg03          ] mortality of algae type 03                                      
   from proces [BLOOM               ] BLOOM II algae module                                         
   process is switched on.                                                                          
-dispersion for [FFLAGELA            ]                                                              
 no dispersions found                                                                               
-velocity for [FFLAGELA            ]                                                                
 no velocity found                                                                                  
                                                                                                    
-fluxes for [GREENS_E            ]                                                                  
 found flux  [dProdAlg04          ] primary production of algae type 04                             
   from proces [BLOOM               ] BLOOM II algae module                                         
   process is switched on.                                                                          
 found flux  [dMortAlg04          ] mortality of algae type 04                                      
   from proces [BLOOM               ] BLOOM II algae module                                         
   process is switched on.                                                                          
-dispersion for [GREENS_E            ]                                                              
 no dispersions found                                                                               
-velocity for [GREENS_E            ]                                                                
 no velocity found                                                                                  
                                                                                                    
-fluxes for [GREENS_N            ]                                                                  
 found flux  [dProdAlg05          ] primary production of algae type 05                             
   from proces [BLOOM               ] BLOOM II algae module                                         
   process is switched on.                                                                          
 found flux  [dMortAlg05          ] mortality of algae type 05                                      
   from proces [BLOOM               ] BLOOM II algae module                                         
   process is switched on.                                                                          
-dispersion for [GREENS_N            ]                                                              
 no dispersions found                                                                               
-velocity for [GREENS_N            ]                                                                
 no velocity found                                                                                  
                                                                                                    
-fluxes for [GREENS_P            ]                                                                  
 found flux  [dProdAlg06          ] primary production of algae type 06                             
   from proces [BLOOM               ] BLOOM II algae module                                         
   process is switched on.                                                                          
 found flux  [dMortAlg06          ] mortality of algae type 06                                      
   from proces [BLOOM               ] BLOOM II algae module                                         
   process is switched on.                                                                          
-dispersion for [GREENS_P            ]                                                              
 no dispersions found                                                                               
-velocity for [GREENS_P            ]                                                                
 no velocity found                                                                                  
                                                                                                    
-fluxes for [OSCILAT_E           ]                                                                  
 found flux  [dProdAlg10          ] primary production of algae type 10                             
   from proces [BLOOM               ] BLOOM II algae module                                         
   process is switched on.                                                                          
 found flux  [dMortAlg10          ] mortality of algae type 10                                      
   from proces [BLOOM               ] BLOOM II algae module                                         
   process is switched on.                                                                          
-dispersion for [OSCILAT_E           ]                                                              
 no dispersions found                                                                               
-velocity for [OSCILAT_E           ]                                                                
 no velocity found                                                                                  
                                                                                                    
-fluxes for [OSCILAT_N           ]                                                                  
 found flux  [dProdAlg11          ] primary production of algae type 11                             
   from proces [BLOOM               ] BLOOM II algae module                                         
   process is switched on.                                                                          
 found flux  [dMortAlg11          ] mortality of algae type 11                                      
   from proces [BLOOM               ] BLOOM II algae module                                         
   process is switched on.                                                                          
-dispersion for [OSCILAT_N           ]                                                              
 no dispersions found                                                                               
-velocity for [OSCILAT_N           ]                                                                
 no velocity found                                                                                  
                                                                                                    
-fluxes for [OSCILAT_P           ]                                                                  
 found flux  [dProdAlg12          ] primary production of algae type 12                             
   from proces [BLOOM               ] BLOOM II algae module                                         
   process is switched on.                                                                          
 found flux  [dMortAlg12          ] mortality of algae type 12                                      
   from proces [BLOOM               ] BLOOM II algae module                                         
   process is switched on.                                                                          
-dispersion for [OSCILAT_P           ]                                                              
 no dispersions found                                                                               
-velocity for [OSCILAT_P           ]                                                                
 no velocity found                                                                                  
                                                                                                    
# locating processes for requested output                                                           
                                                                                                    
# determining the input for the processes (in reversed order)                                       
                                                                                                    
 Input for [DisSi               ] Dissolution of Si in opal (SWITCH defaults)                       
       [Si                  ] dissolved Silica (Si)                                                 
       Using substance nr   5                                                                       
       [Opal                ] Opal-Si                                                               
       Using substance nr   6                                                                       
       [Ceq_disSi           ] Saturation concentration of Si with opal                              
       using default value: 10.00000                                                                
       [RCdisSi20           ] 2nd order dissolution rate SiO2 at 20 oC                              
       using default value: 0.100000E-03                                                            
       [TCdisSi             ] temperature dependency dissolution Si                                 
       using default value: 1.000000                                                                
       [Temp                ] ambient water temperature                                             
       Using constant nr 15 with value:  15.0000                                                    
       [Poros               ] volumetric porosity                                                   
       using default value: 1.000000                                                                
       [SWDisSi             ] option: 0.0 2nd order diss., 1.0 1st order diss.                      
       using default value:  0.00000                                                                
                                                                                                    
 Input for [DecFast             ] Mineralization fast decomp. detritus POC1                         
       [POC1                ] POC1 (fast decomposing fraction)                                      
       Using substance nr   7                                                                       
       [PON1                ] PON1 (fast decomposing fraction)                                      
       Using substance nr   8                                                                       
       [POP1                ] POP1 (fast decomposing fraction)                                      
       Using substance nr   9                                                                       
       [POS1                ] POS1 (fast decomposing fraction)                                      
       Using substance nr  10                                                                       
       [IdDet1              ] identifier for detritus group POC1, POC2, POC3                        
       using default value: 1.000000                                                                
       [ku_dFdcC20          ] upper limit mineralization rate fast detr-C                           
       Using constant nr 53 with value: 0.180000                                                    
       [kl_dFdcC20          ] lower limit mineralization rate fast detr-C                           
       Using constant nr 54 with value: 0.120000                                                    
       [ku_dFdcN20          ] upper limit mineralization rate fast detr-N                           
       Using constant nr 55 with value: 0.180000                                                    
       [kl_dFdcN20          ] lower limit mineralization rate fast detr-N                           
       Using constant nr 56 with value: 0.120000                                                    
       [ku_dFdcP20          ] upper limit mineralization rate fast detr-P                           
       Using constant nr 57 with value: 0.180000                                                    
       [kl_dFdcP20          ] lower limit mineralization rate fast detr-P                           
       Using constant nr 58 with value: 0.120000                                                    
       [kT_dec              ] temperature coefficient for decomposition                             
       Using constant nr 59 with value:  1.04700                                                    
       [Temp                ] ambient water temperature                                             
       Using constant nr 15 with value:  15.0000                                                    
       [a_dNpr              ] target N:C ratio in refractory detritus                               
       using default value: 0.500000E-01                                                            
       [a_dPpr              ] target P:C ratio in refractory detritus                               
       using default value: 0.500000E-02                                                            
       [a_dSpr              ] target S:C ratio in refractory detritus                               
       using default value: 0.500000E-02                                                            
       [al_dNf              ] lower limit N:C ratio in fast decomp.  detr                           
       using default value: 0.100000E+00                                                            
       [al_dPf              ] lower limit P:C ratio in fast decomp.  detr                           
       using default value: 0.100000E-01                                                            
       [au_dNf              ] upper limit N:C ratio in fast decomp.  detr                           
       using default value: 0.150000                                                                
       [au_dPf              ] upper limit P:C ratio in fast decomp.  detr                           
       using default value: 0.150000E-01                                                            
       [OXY                 ] Dissolved Oxygen                                                      
       using default value: 10.00000                                                                
       [NO3                 ] Nitrate (NO3)                                                         
       Using substance nr   3                                                                       
       [b_ni                ] attenuation factor decomp. in denitrifying zone                       
       using default value: 1.000000                                                                
       [b_su                ] attenuation factor decomp. in sulphate red.zone                       
       using default value: 1.000000                                                                
       [b_poc1poc2          ] fraction POC1 converted to POC2                                       
       using default value:  0.00000                                                                
       [b_poc1doc           ] fraction POC1 converted to DOC                                        
       using default value:  0.00000                                                                
       [SWOMDec             ] option: 0.0 for stripping, 1.0 for different rates                    
       using default value:  0.00000                                                                
                                                                                                    
 Input for [Compos              ] Composition                                                       
       [NO3                 ] Nitrate (NO3)                                                         
       Using substance nr   3                                                                       
       [NH4                 ] Ammonium (NH4)                                                        
       Using substance nr   2                                                                       
       [PO4                 ] Ortho-Phosphate (PO4)                                                 
       Using substance nr   4                                                                       
       [Si                  ] dissolved Silica (Si)                                                 
       Using substance nr   5                                                                       
       [IM1                 ] inorganic matter (IM1)                                                
       using default value:  0.00000                                                                
       [IM2                 ] inorganic matter (IM2)                                                
       using default value:  0.00000                                                                
       [IM3                 ] inorganic matter (IM3)                                                
       using default value:  0.00000                                                                
       [Phyt                ] total carbon in phytoplankton                                         
       Using output from proces [Phy_Blo             ]                                              
       [AlgN                ] total nitrogen in algae                                               
       Using output from proces [Phy_Blo             ]                                              
       [AlgP                ] total phosphorus in algae                                             
       Using output from proces [Phy_Blo             ]                                              
       [AlgSi               ] total silica in algae                                                 
       Using output from proces [Phy_Blo             ]                                              
       [AlgDM               ] total DM in algae                                                     
       Using output from proces [Phy_Blo             ]                                              
       [POC1                ] POC1 (fast decomposing fraction)                                      
       Using substance nr   7                                                                       
       [POC2                ] POC2 (medium decomposing fraction)                                    
       using default value:  0.00000                                                                
       [POC3                ] POC3 (slow decomposing fraction)                                      
       using default value:  0.00000                                                                
       [POC4                ] POC4 (particulate refractory fraction)                                
       using default value:  0.00000                                                                
       [PON1                ] PON1 (fast decomposing fraction)                                      
       Using substance nr   8                                                                       
       [DOC                 ] Dissolved Organic Carbon (DOC)                                        
       using default value:  0.00000                                                                
       [DON                 ] Dissolved Organic Nitrogen (DON)                                      
       using default value:  0.00000                                                                
       [DOP                 ] Dissolved Organic Phosphorus (DOP)                                    
       using default value:  0.00000                                                                
       [DOS                 ] Dissolved Organic Sulphur (DOS)                                       
       using default value:  0.00000                                                                
       [AAP                 ] adsorbed ortho phosphate                                              
       using default value:  0.00000                                                                
       [VIVP                ] Vivianite-P                                                           
       using default value:  0.00000                                                                
       [APATP               ] Apatite-P                                                             
       using default value:  0.00000                                                                
       [DMCFIM1             ] DM:C ratio IM1                                                        
       using default value: 1.000000                                                                
       [DMCFIM2             ] DM:C ratio IM2                                                        
       using default value: 1.000000                                                                
       [DMCFIM3             ] DM:C ratio IM3                                                        
       using default value: 1.000000                                                                
       [PON2                ] PON2 (medium decomposing fraction)                                    
       using default value:  0.00000                                                                
       [PON3                ] PON3 (slow decomposing fraction)                                      
       using default value:  0.00000                                                                
       [PON4                ] PON4 (particulate refractory fraction)                                
       using default value:  0.00000                                                                
       [POP1                ] POP1 (fast decomposing fraction)                                      
       Using substance nr   9                                                                       
       [POP2                ] POP2 (medium decomposing fraction)                                    
       using default value:  0.00000                                                                
       [POP3                ] POP3 (slow decomposing fraction)                                      
       using default value:  0.00000                                                                
       [POP4                ] POP4 (particulate refractory fraction)                                
       using default value:  0.00000                                                                
       [POS1                ] POS1 (fast decomposing fraction)                                      
       Using substance nr  10                                                                       
       [POS2                ] POS2 (medium decomposing fraction)                                    
       using default value:  0.00000                                                                
       [POS3                ] POS3 (slow decomposing  fraction)                                     
       using default value:  0.00000                                                                
       [POS4                ] POS4 (particulate refractory fraction)                                
       using default value:  0.00000                                                                
       [Opal                ] Opal-Si                                                               
       Using substance nr   6                                                                       
       [DmCfPOC1            ] DM:C ratio POC1                                                       
       using default value:  2.50000                                                                
       [DmCfPOC2            ] DM:C ratio POC2                                                       
       using default value:  2.50000                                                                
       [DmCfPOC3            ] DM:C ratio POC3                                                       
       using default value:  2.50000                                                                
       [DmCfPOC4            ] DM:C ratio POC4                                                       
       using default value:  2.50000                                                                
                                                                                                    
 Input for [Phy_Blo             ] Computation of phytoplankton output - Bloom                       
       [NAlgBloom           ] number of algae types in BLOOM                                        
       using default value:  30.0000                                                                
       [Depth               ] depth of segment                                                      
       Using output from proces [DynDepth            ]                                              
       [FDIATOMS_E          ] concentration of algae type 1                                         
       Using substance nr  14                                                                       
       [FDIATOMS_P          ] concentration of algae type 2                                         
       Using substance nr  15                                                                       
       [FFLAGELA            ] concentration of algae type 3                                         
       Using substance nr  16                                                                       
       [GREENS_E            ] concentration of algae type 4                                         
       Using substance nr  17                                                                       
       [GREENS_N            ] concentration of algae type 5                                         
       Using substance nr  18                                                                       
       [GREENS_P            ] concentration of algae type 6                                         
       Using substance nr  19                                                                       
       [APHANIZO_E          ] concentration of algae type 7                                         
       Using substance nr  11                                                                       
       [APHANIZO_N          ] concentration of algae type 8                                         
       Using substance nr  12                                                                       
       [APHANIZO_P          ] concentration of algae type 9                                         
       Using substance nr  13                                                                       
       [OSCILAT_E           ] concentration of algae type 10                                        
       Using substance nr  20                                                                       
       [OSCILAT_N           ] concentration of algae type 11                                        
       Using substance nr  21                                                                       
       [OSCILAT_P           ] concentration of algae type 12                                        
       Using substance nr  22                                                                       
       [BLOOMALG13          ] concentration of algae type 13                                        
       using default value: -101.000                                                                
       [BLOOMALG14          ] concentration of algae type 14                                        
       using default value: -101.000                                                                
       [BLOOMALG15          ] concentration of algae type 15                                        
       using default value: -101.000                                                                
       [BLOOMALG16          ] concentration of algae type 16                                        
       using default value: -101.000                                                                
       [BLOOMALG17          ] concentration of algae type 17                                        
       using default value: -101.000                                                                
       [BLOOMALG18          ] concentration of algae type 18                                        
       using default value: -101.000                                                                
       [BLOOMALG19          ] concentration of algae type 19                                        
       using default value: -101.000                                                                
       [BLOOMALG20          ] concentration of algae type 20                                        
       using default value: -101.000                                                                
       [BLOOMALG21          ] concentration of algae type 21                                        
       using default value: -101.000                                                                
       [BLOOMALG22          ] concentration of algae type 22                                        
       using default value: -101.000                                                                
       [BLOOMALG23          ] concentration of algae type 23                                        
       using default value: -101.000                                                                
       [BLOOMALG24          ] concentration of algae type 24                                        
       using default value: -101.000                                                                
       [BLOOMALG25          ] concentration of algae type 25                                        
       using default value: -101.000                                                                
       [BLOOMALG26          ] concentration of algae type 26                                        
       using default value: -101.000                                                                
       [BLOOMALG27          ] concentration of algae type 27                                        
       using default value: -101.000                                                                
       [BLOOMALG28          ] concentration of algae type 28                                        
       using default value: -101.000                                                                
       [BLOOMALG29          ] concentration of algae type 29                                        
       using default value: -101.000                                                                
       [BLOOMALG30          ] concentration of algae type 30                                        
       using default value: -101.000                                                                
       [SpecAlg01           ] number of the group for algae type 01                                 
       using default value: 1.000000                                                                
       [SpecAlg02           ] number of the group for algae type 02                                 
       using default value: 1.000000                                                                
       [SpecAlg03           ] number of the group for algae type 03                                 
       using default value:  2.00000                                                                
       [SpecAlg04           ] number of the group for algae type 04                                 
       using default value:  3.00000                                                                
       [SpecAlg05           ] number of the group for algae type 05                                 
       using default value:  3.00000                                                                
       [SpecAlg06           ] number of the group for algae type 06                                 
       using default value:  3.00000                                                                
       [SpecAlg07           ] number of the group for algae type 07                                 
       using default value:  4.00000                                                                
       [SpecAlg08           ] number of the group for algae type 08                                 
       using default value:  4.00000                                                                
       [SpecAlg09           ] number of the group for algae type 09                                 
       using default value:  4.00000                                                                
       [SpecAlg10           ] number of the group for algae type 10                                 
       using default value:  5.00000                                                                
       [SpecAlg11           ] number of the group for algae type 11                                 
       using default value:  5.00000                                                                
       [SpecAlg12           ] number of the group for algae type 12                                 
       using default value:  5.00000                                                                
       [SpecAlg13           ] number of the group for algae type 13                                 
       using default value:  0.00000                                                                
       [SpecAlg14           ] number of the group for algae type 14                                 
       using default value:  0.00000                                                                
       [SpecAlg15           ] number of the group for algae type 15                                 
       using default value:  0.00000                                                                
       [SpecAlg16           ] number of the group for algae type 16                                 
       using default value:  0.00000                                                                
       [SpecAlg17           ] number of the group for algae type 17                                 
       using default value:  0.00000                                                                
       [SpecAlg18           ] number of the group for algae type 18                                 
       using default value:  0.00000                                                                
       [SpecAlg19           ] number of the group for algae type 19                                 
       using default value:  0.00000                                                                
       [SpecAlg20           ] number of the group for algae type 20                                 
       using default value:  0.00000                                                                
       [SpecAlg21           ] number of the group for algae type 21                                 
       using default value:  0.00000                                                                
       [SpecAlg22           ] number of the group for algae type 22                                 
       using default value:  0.00000                                                                
       [SpecAlg23           ] number of the group for algae type 23                                 
       using default value:  0.00000                                                                
       [SpecAlg24           ] number of the group for algae type 24                                 
       using default value:  0.00000                                                                
       [SpecAlg25           ] number of the group for algae type 25                                 
       using default value:  0.00000                                                                
       [SpecAlg26           ] number of the group for algae type 26                                 
       using default value:  0.00000                                                                
       [SpecAlg27           ] number of the group for algae type 27                                 
       using default value:  0.00000                                                                
       [SpecAlg28           ] number of the group for algae type 28                                 
       using default value:  0.00000                                                                
       [SpecAlg29           ] number of the group for algae type 29                                 
       using default value:  0.00000                                                                
       [SpecAlg30           ] number of the group for algae type 30                                 
       using default value:  0.00000                                                                
       [NCRFDI_E            ] N:C ratio algae type 01                                               
       using default value: 0.210000                                                                
       [NCRFDI_P            ] N:C ratio algae type 02                                               
       using default value: 0.188000                                                                
       [NCRFFL_E            ] N:C ratio algae type 03                                               
       using default value: 0.275000                                                                
       [NCRGRE_E            ] N:C ratio algae type 04                                               
       using default value: 0.275000                                                                
       [NCRGRE_N            ] N:C ratio algae type 05                                               
       using default value: 0.175000                                                                
       [NCRGRE_P            ] N:C ratio algae type 06                                               
       using default value: 0.200000                                                                
       [NCRAPH_E            ] N:C ratio algae type 07                                               
       using default value: 0.220000                                                                
       [NCRAPH_N            ] N:C ratio algae type 08                                               
       using default value: 0.125000                                                                
       [NCRAPH_P            ] N:C ratio algae type 09                                               
       using default value: 0.170000                                                                
       [NCROSC_E            ] N:C ratio algae type 10                                               
       using default value: 0.225000                                                                
       [NCROSC_N            ] N:C ratio algae type 11                                               
       using default value: 0.125000                                                                
       [NCROSC_P            ] N:C ratio algae type 12                                               
       using default value: 0.150000                                                                
       [NCRAlg13            ] N:C ratio algae type 13                                               
       using default value: 0.200000                                                                
       [NCRAlg14            ] N:C ratio algae type 14                                               
       using default value: 0.200000                                                                
       [NCRAlg15            ] N:C ratio algae type 15                                               
       using default value: 0.200000                                                                
       [NCRAlg16            ] N:C ratio algae type 16                                               
       using default value: 0.200000                                                                
       [NCRAlg17            ] N:C ratio algae type 17                                               
       using default value: 0.200000                                                                
       [NCRAlg18            ] N:C ratio algae type 18                                               
       using default value: 0.200000                                                                
       [NCRAlg19            ] N:C ratio algae type 19                                               
       using default value: 0.200000                                                                
       [NCRAlg20            ] N:C ratio algae type 20                                               
       using default value: 0.200000                                                                
       [NCRAlg21            ] N:C ratio algae type 21                                               
       using default value: 0.200000                                                                
       [NCRAlg22            ] N:C ratio algae type 22                                               
       using default value: 0.200000                                                                
       [NCRAlg23            ] N:C ratio algae type 23                                               
       using default value: 0.200000                                                                
       [NCRAlg24            ] N:C ratio algae type 24                                               
       using default value: 0.200000                                                                
       [NCRAlg25            ] N:C ratio algae type 25                                               
       using default value: 0.200000                                                                
       [NCRAlg26            ] N:C ratio algae type 26                                               
       using default value: 0.200000                                                                
       [NCRAlg27            ] N:C ratio algae type 27                                               
       using default value: 0.200000                                                                
       [NCRAlg28            ] N:C ratio algae type 28                                               
       using default value: 0.200000                                                                
       [NCRAlg29            ] N:C ratio algae type 29                                               
       using default value: 0.200000                                                                
       [NCRAlg30            ] N:C ratio algae type 30                                               
       using default value: 0.200000                                                                
       [PCRFDI_E            ] P:C ratio algae type 01                                               
       using default value: 0.180000E-01                                                            
       [PCRFDI_P            ] P:C ratio algae type 02                                               
       using default value: 0.113000E-01                                                            
       [PCRFFL_E            ] P:C ratio algae type 03                                               
       using default value: 0.180000E-01                                                            
       [PCRGRE_E            ] P:C ratio algae type 04                                               
       using default value: 0.238000E-01                                                            
       [PCRGRE_N            ] P:C ratio algae type 05                                               
       using default value: 0.150000E-01                                                            
       [PCRGRE_P            ] P:C ratio algae type 06                                               
       using default value: 0.125000E-01                                                            
       [PCRAPH_E            ] P:C ratio algae type 07                                               
       using default value: 0.125000E-01                                                            
       [PCRAPH_N            ] P:C ratio algae type 08                                               
       using default value: 0.125000E-01                                                            
       [PCRAPH_P            ] P:C ratio algae type 09                                               
       using default value: 0.880000E-02                                                            
       [PCROSC_E            ] P:C ratio algae type 10                                               
       using default value: 0.188000E-01                                                            
       [PCROSC_N            ] P:C ratio algae type 11                                               
       using default value: 0.138000E-01                                                            
       [PCROSC_P            ] P:C ratio algae type 12                                               
       using default value: 0.113000E-01                                                            
       [PCRAlg13            ] P:C ratio algae type 13                                               
       using default value: 0.200000E-01                                                            
       [PCRAlg14            ] P:C ratio algae type 14                                               
       using default value: 0.200000E-01                                                            
       [PCRAlg15            ] P:C ratio algae type 15                                               
       using default value: 0.200000E-01                                                            
       [PCRAlg16            ] P:C ratio algae type 16                                               
       using default value: 0.200000E-01                                                            
       [PCRAlg17            ] P:C ratio algae type 17                                               
       using default value: 0.200000E-01                                                            
       [PCRAlg18            ] P:C ratio algae type 18                                               
       using default value: 0.200000E-01                                                            
       [PCRAlg19            ] P:C ratio algae type 19                                               
       using default value: 0.200000E-01                                                            
       [PCRAlg20            ] P:C ratio algae type 20                                               
       using default value: 0.200000E-01                                                            
       [PCRAlg21            ] P:C ratio algae type 21                                               
       using default value: 0.200000E-01                                                            
       [PCRAlg22            ] P:C ratio algae type 22                                               
       using default value: 0.200000E-01                                                            
       [PCRAlg23            ] P:C ratio algae type 23                                               
       using default value: 0.200000E-01                                                            
       [PCRAlg24            ] P:C ratio algae type 24                                               
       using default value: 0.200000E-01                                                            
       [PCRAlg25            ] P:C ratio algae type 25                                               
       using default value: 0.200000E-01                                                            
       [PCRAlg26            ] P:C ratio algae type 26                                               
       using default value: 0.200000E-01                                                            
       [PCRAlg27            ] P:C ratio algae type 27                                               
       using default value: 0.200000E-01                                                            
       [PCRAlg28            ] P:C ratio algae type 28                                               
       using default value: 0.200000E-01                                                            
       [PCRAlg29            ] P:C ratio algae type 29                                               
       using default value: 0.200000E-01                                                            
       [PCRAlg30            ] P:C ratio algae type 30                                               
       using default value: 0.200000E-01                                                            
       [SCRFDI_E            ] Si:C ratio algae type 01                                              
       using default value: 0.660000                                                                
       [SCRFDI_P            ] Si:C ratio algae type 02                                              
       using default value: 0.550000                                                                
       [SCRFFL_E            ] Si:C ratio algae type 03                                              
       using default value: 0.180000E-02                                                            
       [SCRGRE_E            ] Si:C ratio algae type 04                                              
       using default value: 0.180000E-02                                                            
       [SCRGRE_N            ] Si:C ratio algae type 05                                              
       using default value: 0.180000E-02                                                            
       [SCRGRE_P            ] Si:C ratio algae type 06                                              
       using default value: 0.180000E-02                                                            
       [SCRAPH_E            ] Si:C ratio algae type 07                                              
       using default value: 0.180000E-02                                                            
       [SCRAPH_N            ] Si:C ratio algae type 08                                              
       using default value: 0.180000E-02                                                            
       [SCRAPH_P            ] Si:C ratio algae type 09                                              
       using default value: 0.180000E-02                                                            
       [SCROSC_E            ] Si:C ratio algae type 10                                              
       using default value: 0.180000E-02                                                            
       [SCROSC_N            ] Si:C ratio algae type 11                                              
       using default value: 0.180000E-02                                                            
       [SCROSC_P            ] Si:C ratio algae type 12                                              
       using default value: 0.180000E-02                                                            
       [SCRAlg13            ] Si:C ratio algae type 13                                              
       using default value: 0.200000E-02                                                            
       [SCRAlg14            ] Si:C ratio algae type 14                                              
       using default value: 0.200000E-02                                                            
       [SCRAlg15            ] Si:C ratio algae type 15                                              
       using default value: 0.200000E-02                                                            
       [SCRAlg16            ] Si:C ratio algae type 16                                              
       using default value: 0.200000E-02                                                            
       [SCRAlg17            ] Si:C ratio algae type 17                                              
       using default value: 0.200000E-02                                                            
       [SCRAlg18            ] Si:C ratio algae type 18                                              
       using default value: 0.200000E-02                                                            
       [SCRAlg19            ] Si:C ratio algae type 19                                              
       using default value: 0.200000E-02                                                            
       [SCRAlg20            ] Si:C ratio algae type 20                                              
       using default value: 0.200000E-02                                                            
       [SCRAlg21            ] Si:C ratio algae type 21                                              
       using default value: 0.200000E-02                                                            
       [SCRAlg22            ] Si:C ratio algae type 22                                              
       using default value: 0.200000E-02                                                            
       [SCRAlg23            ] Si:C ratio algae type 23                                              
       using default value: 0.200000E-02                                                            
       [SCRAlg24            ] Si:C ratio algae type 24                                              
       using default value: 0.200000E-02                                                            
       [SCRAlg25            ] Si:C ratio algae type 25                                              
       using default value: 0.200000E-02                                                            
       [SCRAlg26            ] Si:C ratio algae type 26                                              
       using default value: 0.200000E-02                                                            
       [SCRAlg27            ] Si:C ratio algae type 27                                              
       using default value: 0.200000E-02                                                            
       [SCRAlg28            ] Si:C ratio algae type 28                                              
       using default value: 0.200000E-02                                                            
       [SCRAlg29            ] Si:C ratio algae type 29                                              
       using default value: 0.200000E-02                                                            
       [SCRAlg30            ] Si:C ratio algae type 30                                              
       using default value: 0.200000E-02                                                            
       [DMCFFDI_E           ] DM:C ratio algae type 01                                              
       using default value:  3.00000                                                                
       [DMCFFDI_P           ] DM:C ratio algae type 02                                              
       using default value:  2.50000                                                                
       [DMCFFFL_E           ] DM:C ratio algae type 03                                              
       using default value:  2.50000                                                                
       [DMCFGRE_E           ] DM:C ratio algae type 04                                              
       using default value:  2.50000                                                                
       [DMCFGRE_N           ] DM:C ratio algae type 05                                              
       using default value:  2.50000                                                                
       [DMCFGRE_P           ] DM:C ratio algae type 06                                              
       using default value:  2.50000                                                                
       [DMCFAPH_E           ] DM:C ratio algae type 07                                              
       using default value:  2.50000                                                                
       [DMCFAPH_N           ] DM:C ratio algae type 08                                              
       using default value:  2.50000                                                                
       [DMCFAPH_P           ] DM:C ratio algae type 09                                              
       using default value:  2.50000                                                                
       [DMCFOSC_E           ] DM:C ratio algae type 10                                              
       using default value:  2.50000                                                                
       [DMCFOSC_N           ] DM:C ratio algae type 11                                              
       using default value:  2.50000                                                                
       [DMCFOSC_P           ] DM:C ratio algae type 12                                              
       using default value:  2.50000                                                                
       [DMCFAlg13           ] DM:C ratio algae type 13                                              
       using default value:  2.50000                                                                
       [DMCFAlg14           ] DM:C ratio algae type 14                                              
       using default value:  2.50000                                                                
       [DMCFAlg15           ] DM:C ratio algae type 15                                              
       using default value:  2.50000                                                                
       [DMCFAlg16           ] DM:C ratio algae type 16                                              
       using default value:  2.50000                                                                
       [DMCFAlg17           ] DM:C ratio algae type 17                                              
       using default value:  2.50000                                                                
       [DMCFAlg18           ] DM:C ratio algae type 18                                              
       using default value:  2.50000                                                                
       [DMCFAlg19           ] DM:C ratio algae type 19                                              
       using default value:  2.50000                                                                
       [DMCFAlg20           ] DM:C ratio algae type 20                                              
       using default value:  2.50000                                                                
       [DMCFAlg21           ] DM:C ratio algae type 21                                              
       using default value:  2.50000                                                                
       [DMCFAlg22           ] DM:C ratio algae type 22                                              
       using default value:  2.50000                                                                
       [DMCFAlg23           ] DM:C ratio algae type 23                                              
       using default value:  2.50000                                                                
       [DMCFAlg24           ] DM:C ratio algae type 24                                              
       using default value:  2.50000                                                                
       [DMCFAlg25           ] DM:C ratio algae type 25                                              
       using default value:  2.50000                                                                
       [DMCFAlg26           ] DM:C ratio algae type 26                                              
       using default value:  2.50000                                                                
       [DMCFAlg27           ] DM:C ratio algae type 27                                              
       using default value:  2.50000                                                                
       [DMCFAlg28           ] DM:C ratio algae type 28                                              
       using default value:  2.50000                                                                
       [DMCFAlg29           ] DM:C ratio algae type 29                                              
       using default value:  2.50000                                                                
       [DMCFAlg30           ] DM:C ratio algae type 30                                              
       using default value:  2.50000                                                                
       [CHLACFDI_E          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.400000E-01                                                            
       [CHLACFDI_P          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.250000E-01                                                            
       [CHLACFFL_E          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.290000E-01                                                            
       [CHLACGRE_E          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.330000E-01                                                            
       [CHLACGRE_N          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.250000E-01                                                            
       [CHLACGRE_P          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.250000E-01                                                            
       [CHLACAPH_E          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.330000E-01                                                            
       [CHLACAPH_N          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.250000E-01                                                            
       [CHLACAPH_P          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.250000E-01                                                            
       [CHLACOSC_E          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.330000E-01                                                            
       [CHLACOSC_N          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.200000E-01                                                            
       [CHLACOSC_P          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.200000E-01                                                            
       [ChlaCAlg13          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.300000E-01                                                            
       [ChlaCAlg14          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.300000E-01                                                            
       [ChlaCAlg15          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.300000E-01                                                            
       [ChlaCAlg16          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.300000E-01                                                            
       [ChlaCAlg17          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.300000E-01                                                            
       [ChlaCAlg18          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.300000E-01                                                            
       [ChlaCAlg19          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.300000E-01                                                            
       [ChlaCAlg20          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.300000E-01                                                            
       [ChlaCAlg21          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.300000E-01                                                            
       [ChlaCAlg22          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.300000E-01                                                            
       [ChlaCAlg23          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.300000E-01                                                            
       [ChlaCAlg24          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.300000E-01                                                            
       [ChlaCAlg25          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.300000E-01                                                            
       [ChlaCAlg26          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.300000E-01                                                            
       [ChlaCAlg27          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.300000E-01                                                            
       [ChlaCAlg28          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.300000E-01                                                            
       [ChlaCAlg29          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.300000E-01                                                            
       [ChlaCAlg30          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.300000E-01                                                            
       [XNCRFDI_E           ] N:C ratio for heterotrophic algae type 01                             
       using default value:-1.000000                                                                
       [XNCRFDI_P           ] N:C ratio for heterotrophic algae type 02                             
       using default value:-1.000000                                                                
       [XNCRFFL_E           ] N:C ratio for heterotrophic algae type 03                             
       using default value:-1.000000                                                                
       [XNCRGRE_E           ] N:C ratio for heterotrophic algae type 04                             
       using default value:-1.000000                                                                
       [XNCRGRE_N           ] N:C ratio for heterotrophic algae type 05                             
       using default value:-1.000000                                                                
       [XNCRGRE_P           ] N:C ratio for heterotrophic algae type 06                             
       using default value:-1.000000                                                                
       [XNCRAPH_E           ] N:C ratio for heterotrophic algae type 07                             
       using default value:-1.000000                                                                
       [XNCRAPH_N           ] N:C ratio for heterotrophic algae type 08                             
       using default value:-1.000000                                                                
       [XNCRAPH_P           ] N:C ratio for heterotrophic algae type 09                             
       using default value:-1.000000                                                                
       [XNCROSC_E           ] N:C ratio for heterotrophic algae type 10                             
       using default value:-1.000000                                                                
       [XNCROSC_N           ] N:C ratio for heterotrophic algae type 11                             
       using default value:-1.000000                                                                
       [XNCROSC_P           ] N:C ratio for heterotrophic algae type 12                             
       using default value:-1.000000                                                                
       [XNCRAlg13           ] N:C ratio for heterotrophic algae type 13                             
       using default value:-1.000000                                                                
       [XNCRAlg14           ] N:C ratio for heterotrophic algae type 14                             
       using default value:-1.000000                                                                
       [XNCRAlg15           ] N:C ratio for heterotrophic algae type 15                             
       using default value:-1.000000                                                                
       [XNCRAlg16           ] N:C ratio for heterotrophic algae type 16                             
       using default value:-1.000000                                                                
       [XNCRAlg17           ] N:C ratio for heterotrophic algae type 17                             
       using default value:-1.000000                                                                
       [XNCRAlg18           ] N:C ratio for heterotrophic algae type 18                             
       using default value:-1.000000                                                                
       [XNCRAlg19           ] N:C ratio for heterotrophic algae type 19                             
       using default value:-1.000000                                                                
       [XNCRAlg20           ] N:C ratio for heterotrophic algae type 20                             
       using default value:-1.000000                                                                
       [XNCRAlg21           ] N:C ratio for heterotrophic algae type 21                             
       using default value:-1.000000                                                                
       [XNCRAlg22           ] N:C ratio for heterotrophic algae type 22                             
       using default value:-1.000000                                                                
       [XNCRAlg23           ] N:C ratio for heterotrophic algae type 23                             
       using default value:-1.000000                                                                
       [XNCRAlg24           ] N:C ratio for heterotrophic algae type 24                             
       using default value:-1.000000                                                                
       [XNCRAlg25           ] N:C ratio for heterotrophic algae type 25                             
       using default value:-1.000000                                                                
       [XNCRAlg26           ] N:C ratio for heterotrophic algae type 26                             
       using default value:-1.000000                                                                
       [XNCRAlg27           ] N:C ratio for heterotrophic algae type 27                             
       using default value:-1.000000                                                                
       [XNCRAlg28           ] N:C ratio for heterotrophic algae type 28                             
       using default value:-1.000000                                                                
       [XNCRAlg29           ] N:C ratio for heterotrophic algae type 29                             
       using default value:-1.000000                                                                
       [XNCRAlg30           ] N:C ratio for heterotrophic algae type 30                             
       using default value:-1.000000                                                                
       [XPCRFDI_E           ] P:C ratio for heterotrophic algae type 01                             
       using default value:-1.000000                                                                
       [XPCRFDI_P           ] P:C ratio for heterotrophic algae type 02                             
       using default value:-1.000000                                                                
       [XPCRFFL_E           ] P:C ratio for heterotrophic algae type 03                             
       using default value:-1.000000                                                                
       [XPCRGRE_E           ] P:C ratio for heterotrophic algae type 04                             
       using default value:-1.000000                                                                
       [XPCRGRE_N           ] P:C ratio for heterotrophic algae type 05                             
       using default value:-1.000000                                                                
       [XPCRGRE_P           ] P:C ratio for heterotrophic algae type 06                             
       using default value:-1.000000                                                                
       [XPCRAPH_E           ] P:C ratio for heterotrophic algae type 07                             
       using default value:-1.000000                                                                
       [XPCRAPH_N           ] P:C ratio for heterotrophic algae type 08                             
       using default value:-1.000000                                                                
       [XPCRAPH_P           ] P:C ratio for heterotrophic algae type 09                             
       using default value:-1.000000                                                                
       [XPCROSC_E           ] P:C ratio for heterotrophic algae type 10                             
       using default value:-1.000000                                                                
       [XPCROSC_N           ] P:C ratio for heterotrophic algae type 11                             
       using default value:-1.000000                                                                
       [XPCROSC_P           ] P:C ratio for heterotrophic algae type 12                             
       using default value:-1.000000                                                                
       [XPCRAlg13           ] P:C ratio for heterotrophic algae type 13                             
       using default value:-1.000000                                                                
       [XPCRAlg14           ] P:C ratio for heterotrophic algae type 14                             
       using default value:-1.000000                                                                
       [XPCRAlg15           ] P:C ratio for heterotrophic algae type 15                             
       using default value:-1.000000                                                                
       [XPCRAlg16           ] P:C ratio for heterotrophic algae type 16                             
       using default value:-1.000000                                                                
       [XPCRAlg17           ] P:C ratio for heterotrophic algae type 17                             
       using default value:-1.000000                                                                
       [XPCRAlg18           ] P:C ratio for heterotrophic algae type 18                             
       using default value:-1.000000                                                                
       [XPCRAlg19           ] P:C ratio for heterotrophic algae type 19                             
       using default value:-1.000000                                                                
       [XPCRAlg20           ] P:C ratio for heterotrophic algae type 20                             
       using default value:-1.000000                                                                
       [XPCRAlg21           ] P:C ratio for heterotrophic algae type 21                             
       using default value:-1.000000                                                                
       [XPCRAlg22           ] P:C ratio for heterotrophic algae type 22                             
       using default value:-1.000000                                                                
       [XPCRAlg23           ] P:C ratio for heterotrophic algae type 23                             
       using default value:-1.000000                                                                
       [XPCRAlg24           ] P:C ratio for heterotrophic algae type 24                             
       using default value:-1.000000                                                                
       [XPCRAlg25           ] P:C ratio for heterotrophic algae type 25                             
       using default value:-1.000000                                                                
       [XPCRAlg26           ] P:C ratio for heterotrophic algae type 26                             
       using default value:-1.000000                                                                
       [XPCRAlg27           ] P:C ratio for heterotrophic algae type 27                             
       using default value:-1.000000                                                                
       [XPCRAlg28           ] P:C ratio for heterotrophic algae type 28                             
       using default value:-1.000000                                                                
       [XPCRAlg29           ] P:C ratio for heterotrophic algae type 29                             
       using default value:-1.000000                                                                
       [XPCRAlg30           ] P:C ratio for heterotrophic algae type 30                             
       using default value:-1.000000                                                                
       [FNCRFDI_E           ] N:C ratio for nitrogen fixing algae type 01                           
       using default value:-1.000000                                                                
       [FNCRFDI_P           ] N:C ratio for nitrogen fixing algae type 02                           
       using default value:-1.000000                                                                
       [FNCRFFL_E           ] N:C ratio for nitrogen fixing algae type 03                           
       using default value:-1.000000                                                                
       [FNCRGRE_E           ] N:C ratio for nitrogen fixing algae type 04                           
       using default value:-1.000000                                                                
       [FNCRGRE_N           ] N:C ratio for nitrogen fixing algae type 05                           
       using default value:-1.000000                                                                
       [FNCRGRE_P           ] N:C ratio for nitrogen fixing algae type 06                           
       using default value:-1.000000                                                                
       [FNCRAPH_E           ] N:C ratio for nitrogen fixing algae type 07                           
       using default value:-1.000000                                                                
       [FNCRAPH_N           ] N:C ratio for nitrogen fixing algae type 08                           
       using default value:-1.000000                                                                
       [FNCRAPH_P           ] N:C ratio for nitrogen fixing algae type 09                           
       using default value:-1.000000                                                                
       [FNCROSC_E           ] N:C ratio for nitrogen fixing algae type 10                           
       using default value:-1.000000                                                                
       [FNCROSC_N           ] N:C ratio for nitrogen fixing algae type 11                           
       using default value:-1.000000                                                                
       [FNCROSC_P           ] N:C ratio for nitrogen fixing algae type 12                           
       using default value:-1.000000                                                                
       [FNCRAlg13           ] N:C ratio for nitrogen fixing algae type 13                           
       using default value:-1.000000                                                                
       [FNCRAlg14           ] N:C ratio for nitrogen fixing algae type 14                           
       using default value:-1.000000                                                                
       [FNCRAlg15           ] N:C ratio for nitrogen fixing algae type 15                           
       using default value:-1.000000                                                                
       [FNCRAlg16           ] N:C ratio for nitrogen fixing algae type 16                           
       using default value:-1.000000                                                                
       [FNCRAlg17           ] N:C ratio for nitrogen fixing algae type 17                           
       using default value:-1.000000                                                                
       [FNCRAlg18           ] N:C ratio for nitrogen fixing algae type 18                           
       using default value:-1.000000                                                                
       [FNCRAlg19           ] N:C ratio for nitrogen fixing algae type 19                           
       using default value:-1.000000                                                                
       [FNCRAlg20           ] N:C ratio for nitrogen fixing algae type 20                           
       using default value:-1.000000                                                                
       [FNCRAlg21           ] N:C ratio for nitrogen fixing algae type 21                           
       using default value:-1.000000                                                                
       [FNCRAlg22           ] N:C ratio for nitrogen fixing algae type 22                           
       using default value:-1.000000                                                                
       [FNCRAlg23           ] N:C ratio for nitrogen fixing algae type 23                           
       using default value:-1.000000                                                                
       [FNCRAlg24           ] N:C ratio for nitrogen fixing algae type 24                           
       using default value:-1.000000                                                                
       [FNCRAlg25           ] N:C ratio for nitrogen fixing algae type 25                           
       using default value:-1.000000                                                                
       [FNCRAlg26           ] N:C ratio for nitrogen fixing algae type 26                           
       using default value:-1.000000                                                                
       [FNCRAlg27           ] N:C ratio for nitrogen fixing algae type 27                           
       using default value:-1.000000                                                                
       [FNCRAlg28           ] N:C ratio for nitrogen fixing algae type 28                           
       using default value:-1.000000                                                                
       [FNCRAlg29           ] N:C ratio for nitrogen fixing algae type 29                           
       using default value:-1.000000                                                                
       [FNCRAlg30           ] N:C ratio for nitrogen fixing algae type 30                           
       using default value:-1.000000                                                                
       [FixFDI_E            ] algae type 01 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixFDI_P            ] algae type 02 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixFFL_E            ] algae type 03 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixGRE_E            ] algae type 04 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixGRE_N            ] algae type 05 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixGRE_P            ] algae type 06 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAPH_E            ] algae type 07 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAPH_N            ] algae type 08 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAPH_P            ] algae type 09 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixOSC_E            ] algae type 10 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixOSC_N            ] algae type 11 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixOSC_P            ] algae type 12 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg13            ] algae type 13 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg14            ] algae type 14 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg15            ] algae type 15 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg16            ] algae type 16 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg17            ] algae type 17 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg18            ] algae type 18 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg19            ] algae type 19 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg20            ] algae type 20 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg21            ] algae type 21 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg22            ] algae type 22 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg23            ] algae type 23 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg24            ] algae type 24 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg25            ] algae type 25 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg26            ] algae type 26 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg27            ] algae type 27 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg28            ] algae type 28 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg29            ] algae type 29 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg30            ] algae type 30 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
                                                                                                    
 Input for [BLOOM               ] BLOOM II algae module                                             
       [TimMultBl           ] ratio bloom/delwaq time step                                          
       Using constant nr 14 with value: 1.000000                                                    
       [ExtVl               ] total extinction coefficient visible light                            
       Using output from proces [Extinc_VLG          ]                                              
       [ExtVlPhyt           ] VL extinction by phytoplankton                                        
       Using output from proces [EXTINABVL           ]                                              
       [Temp                ] ambient water temperature                                             
       Using constant nr 15 with value:  15.0000                                                    
       [Rad                 ] irradiation at the segment upper-boundary                             
       Using output from proces [CalcRad             ]                                              
       [Depth               ] depth of segment                                                      
       Using output from proces [DynDepth            ]                                              
       [BloomDepth          ] average depth over Bloom time step                                    
       Using output from proces [DepAve              ]                                              
       [DayL                ] daylength <0-1>                                                       
       Using output from proces [Daylength           ]                                              
       [NH4                 ] Ammonium (NH4)                                                        
       Using substance nr   2                                                                       
       [NO3                 ] Nitrate (NO3)                                                         
       Using substance nr   3                                                                       
       [PO4                 ] Ortho-Phosphate (PO4)                                                 
       Using substance nr   4                                                                       
       [Si                  ] dissolved Silica (Si)                                                 
       Using substance nr   5                                                                       
       [ThrAlgNH4           ] threshold concentration uptake ammonium                               
       using default value:  0.00000                                                                
       [ThrAlgNO3           ] threshold concentration uptake nitrate                                
       using default value:  0.00000                                                                
       [ThrAlgPO4           ] threshold concentration uptake phosphate                              
       using default value:  0.00000                                                                
       [ThrAlgSi            ] threshold concentration uptake silicium                               
       using default value:  0.00000                                                                
       [PON1                ] PON1 (fast decomposing fraction)                                      
       Using substance nr   8                                                                       
       [POP1                ] POP1 (fast decomposing fraction)                                      
       Using substance nr   9                                                                       
       [DELT                ] timestep for processes                                                
       Using DELWAQ timestep in days                                                                
       [SWBloomOut          ] switch on BLOOM output (0=no,1=yes)                                   
       Using constant nr 16 with value:  0.00000                                                    
       [SWOxyProd           ] switch on oxygen prod. (0=BLOOM, 1=VAROXY)                            
       using default value:  0.00000                                                                
       [Cl                  ] Chloride                                                              
       Using constant nr 13 with value:  20000.0                                                    
       [Volume              ] volume of computational cell                                          
       Using DELWAQ volume                                                                          
       [SwVTRANS            ] switch effect of vertical mixing on light                             
       using default value:  0.00000                                                                
       [TIC                 ] total inorganic carbonate                                             
       using default value:  0.00000                                                                
       [CO2                 ] CO2                                                                   
       using default value:  0.00000                                                                
       [SWTICdummy          ] dummy option for TIC, do not change value                             
       using default value:  0.00000                                                                
       [KCO2                ] Critical CO2 concentration for limitation                             
       using default value:  0.00000                                                                
       [FDIATOMS_E          ] concentration of algae type 1                                         
       Using substance nr  14                                                                       
       [FDIATOMS_P          ] concentration of algae type 2                                         
       Using substance nr  15                                                                       
       [FFLAGELA            ] concentration of algae type 3                                         
       Using substance nr  16                                                                       
       [GREENS_E            ] concentration of algae type 4                                         
       Using substance nr  17                                                                       
       [GREENS_N            ] concentration of algae type 5                                         
       Using substance nr  18                                                                       
       [GREENS_P            ] concentration of algae type 6                                         
       Using substance nr  19                                                                       
       [APHANIZO_E          ] concentration of algae type 7                                         
       Using substance nr  11                                                                       
       [APHANIZO_N          ] concentration of algae type 8                                         
       Using substance nr  12                                                                       
       [APHANIZO_P          ] concentration of algae type 9                                         
       Using substance nr  13                                                                       
       [OSCILAT_E           ] concentration of algae type 10                                        
       Using substance nr  20                                                                       
       [OSCILAT_N           ] concentration of algae type 11                                        
       Using substance nr  21                                                                       
       [OSCILAT_P           ] concentration of algae type 12                                        
       Using substance nr  22                                                                       
       [BLOOMALG13          ] concentration of algae type 13                                        
       using default value: -101.000                                                                
       [BLOOMALG14          ] concentration of algae type 14                                        
       using default value: -101.000                                                                
       [BLOOMALG15          ] concentration of algae type 15                                        
       using default value: -101.000                                                                
       [BLOOMALG16          ] concentration of algae type 16                                        
       using default value: -101.000                                                                
       [BLOOMALG17          ] concentration of algae type 17                                        
       using default value: -101.000                                                                
       [BLOOMALG18          ] concentration of algae type 18                                        
       using default value: -101.000                                                                
       [BLOOMALG19          ] concentration of algae type 19                                        
       using default value: -101.000                                                                
       [BLOOMALG20          ] concentration of algae type 20                                        
       using default value: -101.000                                                                
       [BLOOMALG21          ] concentration of algae type 21                                        
       using default value: -101.000                                                                
       [BLOOMALG22          ] concentration of algae type 22                                        
       using default value: -101.000                                                                
       [BLOOMALG23          ] concentration of algae type 23                                        
       using default value: -101.000                                                                
       [BLOOMALG24          ] concentration of algae type 24                                        
       using default value: -101.000                                                                
       [BLOOMALG25          ] concentration of algae type 25                                        
       using default value: -101.000                                                                
       [BLOOMALG26          ] concentration of algae type 26                                        
       using default value: -101.000                                                                
       [BLOOMALG27          ] concentration of algae type 27                                        
       using default value: -101.000                                                                
       [BLOOMALG28          ] concentration of algae type 28                                        
       using default value: -101.000                                                                
       [BLOOMALG29          ] concentration of algae type 29                                        
       using default value: -101.000                                                                
       [BLOOMALG30          ] concentration of algae type 30                                        
       using default value: -101.000                                                                
       [SpecAlg01           ] number of the group for algae type 01                                 
       using default value: 1.000000                                                                
       [SpecAlg02           ] number of the group for algae type 02                                 
       using default value: 1.000000                                                                
       [SpecAlg03           ] number of the group for algae type 03                                 
       using default value:  2.00000                                                                
       [SpecAlg04           ] number of the group for algae type 04                                 
       using default value:  3.00000                                                                
       [SpecAlg05           ] number of the group for algae type 05                                 
       using default value:  3.00000                                                                
       [SpecAlg06           ] number of the group for algae type 06                                 
       using default value:  3.00000                                                                
       [SpecAlg07           ] number of the group for algae type 07                                 
       using default value:  4.00000                                                                
       [SpecAlg08           ] number of the group for algae type 08                                 
       using default value:  4.00000                                                                
       [SpecAlg09           ] number of the group for algae type 09                                 
       using default value:  4.00000                                                                
       [SpecAlg10           ] number of the group for algae type 10                                 
       using default value:  5.00000                                                                
       [SpecAlg11           ] number of the group for algae type 11                                 
       using default value:  5.00000                                                                
       [SpecAlg12           ] number of the group for algae type 12                                 
       using default value:  5.00000                                                                
       [SpecAlg13           ] number of the group for algae type 13                                 
       using default value:  0.00000                                                                
       [SpecAlg14           ] number of the group for algae type 14                                 
       using default value:  0.00000                                                                
       [SpecAlg15           ] number of the group for algae type 15                                 
       using default value:  0.00000                                                                
       [SpecAlg16           ] number of the group for algae type 16                                 
       using default value:  0.00000                                                                
       [SpecAlg17           ] number of the group for algae type 17                                 
       using default value:  0.00000                                                                
       [SpecAlg18           ] number of the group for algae type 18                                 
       using default value:  0.00000                                                                
       [SpecAlg19           ] number of the group for algae type 19                                 
       using default value:  0.00000                                                                
       [SpecAlg20           ] number of the group for algae type 20                                 
       using default value:  0.00000                                                                
       [SpecAlg21           ] number of the group for algae type 21                                 
       using default value:  0.00000                                                                
       [SpecAlg22           ] number of the group for algae type 22                                 
       using default value:  0.00000                                                                
       [SpecAlg23           ] number of the group for algae type 23                                 
       using default value:  0.00000                                                                
       [SpecAlg24           ] number of the group for algae type 24                                 
       using default value:  0.00000                                                                
       [SpecAlg25           ] number of the group for algae type 25                                 
       using default value:  0.00000                                                                
       [SpecAlg26           ] number of the group for algae type 26                                 
       using default value:  0.00000                                                                
       [SpecAlg27           ] number of the group for algae type 27                                 
       using default value:  0.00000                                                                
       [SpecAlg28           ] number of the group for algae type 28                                 
       using default value:  0.00000                                                                
       [SpecAlg29           ] number of the group for algae type 29                                 
       using default value:  0.00000                                                                
       [SpecAlg30           ] number of the group for algae type 30                                 
       using default value:  0.00000                                                                
       [FrAutFDI_E          ] fraction autolysis algae type 01                                      
       Using constant nr 17 with value: 0.350000                                                    
       [FrAutFDI_P          ] fraction autolysis algae type 02                                      
       Using constant nr 18 with value: 0.350000                                                    
       [FrAutFFL_E          ] fraction autolysis algae type 03                                      
       Using constant nr 19 with value: 0.350000                                                    
       [FrAutGRE_E          ] fraction autolysis algae type 04                                      
       Using constant nr 20 with value: 0.350000                                                    
       [FrAutGRE_N          ] fraction autolysis algae type 05                                      
       Using constant nr 21 with value: 0.350000                                                    
       [FrAutGRE_P          ] fraction autolysis algae type 06                                      
       Using constant nr 22 with value: 0.350000                                                    
       [FrAutAPH_E          ] fraction autolysis algae type 07                                      
       Using constant nr 23 with value: 0.350000                                                    
       [FrAutAPH_N          ] fraction autolysis algae type 08                                      
       Using constant nr 24 with value: 0.350000                                                    
       [FrAutAPH_P          ] fraction autolysis algae type 09                                      
       Using constant nr 25 with value: 0.350000                                                    
       [FrAutOSC_E          ] fraction autolysis algae type 10                                      
       Using constant nr 26 with value: 0.350000                                                    
       [FrAutOSC_N          ] fraction autolysis algae type 11                                      
       Using constant nr 27 with value: 0.350000                                                    
       [FrAutOSC_P          ] fraction autolysis algae type 12                                      
       Using constant nr 28 with value: 0.350000                                                    
       [FrAutAlg13          ] fraction autolysis algae type 13                                      
       using default value: 0.350000                                                                
       [FrAutAlg14          ] fraction autolysis algae type 14                                      
       using default value: 0.350000                                                                
       [FrAutAlg15          ] fraction autolysis algae type 15                                      
       using default value: 0.350000                                                                
       [FrAutAlg16          ] fraction autolysis algae type 16                                      
       using default value: 0.350000                                                                
       [FrAutAlg17          ] fraction autolysis algae type 17                                      
       using default value: 0.350000                                                                
       [FrAutAlg18          ] fraction autolysis algae type 18                                      
       using default value: 0.350000                                                                
       [FrAutAlg19          ] fraction autolysis algae type 19                                      
       using default value: 0.350000                                                                
       [FrAutAlg20          ] fraction autolysis algae type 20                                      
       using default value: 0.350000                                                                
       [FrAutAlg21          ] fraction autolysis algae type 21                                      
       using default value: 0.350000                                                                
       [FrAutAlg22          ] fraction autolysis algae type 22                                      
       using default value: 0.350000                                                                
       [FrAutAlg23          ] fraction autolysis algae type 23                                      
       using default value: 0.350000                                                                
       [FrAutAlg24          ] fraction autolysis algae type 24                                      
       using default value: 0.350000                                                                
       [FrAutAlg25          ] fraction autolysis algae type 25                                      
       using default value: 0.350000                                                                
       [FrAutAlg26          ] fraction autolysis algae type 26                                      
       using default value: 0.350000                                                                
       [FrAutAlg27          ] fraction autolysis algae type 27                                      
       using default value: 0.350000                                                                
       [FrAutAlg28          ] fraction autolysis algae type 28                                      
       using default value: 0.350000                                                                
       [FrAutAlg29          ] fraction autolysis algae type 29                                      
       using default value: 0.350000                                                                
       [FrAutAlg30          ] fraction autolysis algae type 30                                      
       using default value: 0.350000                                                                
       [FrDetFDI_E          ] fraction detritus by mortality algae type 01                          
       Using constant nr 29 with value: 0.550000                                                    
       [FrDetFDI_P          ] fraction detritus by mortality algae type 02                          
       Using constant nr 30 with value: 0.550000                                                    
       [FrDetFFL_E          ] fraction detritus by mortality algae type 03                          
       Using constant nr 31 with value: 0.550000                                                    
       [FrDetGRE_E          ] fraction detritus by mortality algae type 04                          
       Using constant nr 32 with value: 0.550000                                                    
       [FrDetGRE_N          ] fraction detritus by mortality algae type 05                          
       Using constant nr 33 with value: 0.550000                                                    
       [FrDetGRE_P          ] fraction detritus by mortality algae type 06                          
       Using constant nr 34 with value: 0.550000                                                    
       [FrDetAPH_E          ] fraction detritus by mortality algae type 07                          
       Using constant nr 35 with value: 0.550000                                                    
       [FrDetAPH_N          ] fraction detritus by mortality algae type 08                          
       Using constant nr 36 with value: 0.550000                                                    
       [FrDetAPH_P          ] fraction detritus by mortality algae type 09                          
       Using constant nr 37 with value: 0.550000                                                    
       [FrDetOSC_E          ] fraction detritus by mortality algae type 10                          
       Using constant nr 38 with value: 0.620000                                                    
       [FrDetOSC_N          ] fraction detritus by mortality algae type 11                          
       Using constant nr 39 with value: 0.620000                                                    
       [FrDetOSC_P          ] fraction detritus by mortality algae type 12                          
       Using constant nr 40 with value: 0.620000                                                    
       [FrDetAlg13          ] fraction detritus by mortality algae type 13                          
       using default value: 0.650000                                                                
       [FrDetAlg14          ] fraction detritus by mortality algae type 14                          
       using default value: 0.650000                                                                
       [FrDetAlg15          ] fraction detritus by mortality algae type 15                          
       using default value: 0.650000                                                                
       [FrDetAlg16          ] fraction detritus by mortality algae type 16                          
       using default value: 0.650000                                                                
       [FrDetAlg17          ] fraction detritus by mortality algae type 17                          
       using default value: 0.650000                                                                
       [FrDetAlg18          ] fraction detritus by mortality algae type 18                          
       using default value: 0.650000                                                                
       [FrDetAlg19          ] fraction detritus by mortality algae type 19                          
       using default value: 0.650000                                                                
       [FrDetAlg20          ] fraction detritus by mortality algae type 20                          
       using default value: 0.650000                                                                
       [FrDetAlg21          ] fraction detritus by mortality algae type 21                          
       using default value: 0.650000                                                                
       [FrDetAlg22          ] fraction detritus by mortality algae type 22                          
       using default value: 0.650000                                                                
       [FrDetAlg23          ] fraction detritus by mortality algae type 23                          
       using default value: 0.650000                                                                
       [FrDetAlg24          ] fraction detritus by mortality algae type 24                          
       using default value: 0.650000                                                                
       [FrDetAlg25          ] fraction detritus by mortality algae type 25                          
       using default value: 0.650000                                                                
       [FrDetAlg26          ] fraction detritus by mortality algae type 26                          
       using default value: 0.650000                                                                
       [FrDetAlg27          ] fraction detritus by mortality algae type 27                          
       using default value: 0.650000                                                                
       [FrDetAlg28          ] fraction detritus by mortality algae type 28                          
       using default value: 0.650000                                                                
       [FrDetAlg29          ] fraction detritus by mortality algae type 29                          
       using default value: 0.650000                                                                
       [FrDetAlg30          ] fraction detritus by mortality algae type 30                          
       using default value: 0.650000                                                                
       [EXTVLFDI_E          ] VL specific extinction coefficient algae type 01                      
       Using constant nr 41 with value: 0.270000                                                    
       [EXTVLFDI_P          ] VL specific extinction coefficient algae type 02                      
       Using constant nr 42 with value: 0.187500                                                    
       [EXTVLFFL_E          ] VL specific extinction coefficient algae type 03                      
       Using constant nr 43 with value: 0.225000                                                    
       [EXTVLGRE_E          ] VL specific extinction coefficient algae type 04                      
       Using constant nr 44 with value: 0.225000                                                    
       [EXTVLGRE_N          ] VL specific extinction coefficient algae type 05                      
       Using constant nr 45 with value: 0.187500                                                    
       [EXTVLGRE_P          ] VL specific extinction coefficient algae type 06                      
       Using constant nr 46 with value: 0.187500                                                    
       [EXTVLAPH_E          ] VL specific extinction coefficient algae type 07                      
       Using constant nr 47 with value: 0.450000                                                    
       [EXTVLAPH_N          ] VL specific extinction coefficient algae type 08                      
       Using constant nr 48 with value: 0.400000                                                    
       [EXTVLAPH_P          ] VL specific extinction coefficient algae type 09                      
       Using constant nr 49 with value: 0.400000                                                    
       [EXTVLOSC_E          ] VL specific extinction coefficient algae type 10                      
       Using constant nr 50 with value: 0.400000                                                    
       [EXTVLOSC_N          ] VL specific extinction coefficient algae type 11                      
       Using constant nr 51 with value: 0.287500                                                    
       [EXTVLOSC_P          ] VL specific extinction coefficient algae type 12                      
       Using constant nr 52 with value: 0.287500                                                    
       [ExtVlAlg13          ] VL specific extinction coefficient algae type 13                      
       using default value: 0.200000                                                                
       [ExtVlAlg14          ] VL specific extinction coefficient algae type 14                      
       using default value: 0.200000                                                                
       [ExtVlAlg15          ] VL specific extinction coefficient algae type 15                      
       using default value: 0.200000                                                                
       [ExtVlAlg16          ] VL specific extinction coefficient algae ty                           
       using default value: 0.200000                                                                
       [ExtVlAlg17          ] VL specific extinction coefficient algae ty                           
       using default value: 0.200000                                                                
       [ExtVlAlg18          ] VL specific extinction coefficient algae ty                           
       using default value: 0.200000                                                                
       [ExtVlAlg19          ] VL specific extinction coefficient algae ty                           
       using default value: 0.200000                                                                
       [ExtVlAlg20          ] VL specific extinction coefficient algae ty                           
       using default value: 0.200000                                                                
       [ExtVlAlg21          ] VL specific extinction coefficient algae ty                           
       using default value: 0.200000                                                                
       [ExtVlAlg22          ] VL specific extinction coefficient algae ty                           
       using default value: 0.200000                                                                
       [ExtVlAlg23          ] VL specific extinction coefficient algae ty                           
       using default value: 0.200000                                                                
       [ExtVlAlg24          ] VL specific extinction coefficient algae ty                           
       using default value: 0.200000                                                                
       [ExtVlAlg25          ] VL specific extinction coefficient algae ty                           
       using default value: 0.200000                                                                
       [ExtVlAlg26          ] VL specific extinction coefficient algae ty                           
       using default value: 0.200000                                                                
       [ExtVlAlg27          ] VL specific extinction coefficient algae ty                           
       using default value: 0.200000                                                                
       [ExtVlAlg28          ] VL specific extinction coefficient algae ty                           
       using default value: 0.200000                                                                
       [ExtVlAlg29          ] VL specific extinction coefficient algae ty                           
       using default value: 0.200000                                                                
       [ExtVlAlg30          ] VL specific extinction coefficient algae ty                           
       using default value: 0.200000                                                                
       [DMCFFDI_E           ] DM:C ratio algae type 01                                              
       using default value:  3.00000                                                                
       [DMCFFDI_P           ] DM:C ratio algae type 02                                              
       using default value:  2.50000                                                                
       [DMCFFFL_E           ] DM:C ratio algae type 03                                              
       using default value:  2.50000                                                                
       [DMCFGRE_E           ] DM:C ratio algae type 04                                              
       using default value:  2.50000                                                                
       [DMCFGRE_N           ] DM:C ratio algae type 05                                              
       using default value:  2.50000                                                                
       [DMCFGRE_P           ] DM:C ratio algae type 06                                              
       using default value:  2.50000                                                                
       [DMCFAPH_E           ] DM:C ratio algae type 07                                              
       using default value:  2.50000                                                                
       [DMCFAPH_N           ] DM:C ratio algae type 08                                              
       using default value:  2.50000                                                                
       [DMCFAPH_P           ] DM:C ratio algae type 09                                              
       using default value:  2.50000                                                                
       [DMCFOSC_E           ] DM:C ratio algae type 10                                              
       using default value:  2.50000                                                                
       [DMCFOSC_N           ] DM:C ratio algae type 11                                              
       using default value:  2.50000                                                                
       [DMCFOSC_P           ] DM:C ratio algae type 12                                              
       using default value:  2.50000                                                                
       [DMCFAlg13           ] DM:C ratio algae type 13                                              
       using default value:  2.50000                                                                
       [DMCFAlg14           ] DM:C ratio algae type 14                                              
       using default value:  2.50000                                                                
       [DMCFAlg15           ] DM:C ratio algae type 15                                              
       using default value:  2.50000                                                                
       [DMCFAlg16           ] DM:C ratio algae type 16                                              
       using default value:  2.50000                                                                
       [DMCFAlg17           ] DM:C ratio algae type 17                                              
       using default value:  2.50000                                                                
       [DMCFAlg18           ] DM:C ratio algae type 18                                              
       using default value:  2.50000                                                                
       [DMCFAlg19           ] DM:C ratio algae type 19                                              
       using default value:  2.50000                                                                
       [DMCFAlg20           ] DM:C ratio algae type 20                                              
       using default value:  2.50000                                                                
       [DMCFAlg21           ] DM:C ratio algae type 21                                              
       using default value:  2.50000                                                                
       [DMCFAlg22           ] DM:C ratio algae type 22                                              
       using default value:  2.50000                                                                
       [DMCFAlg23           ] DM:C ratio algae type 23                                              
       using default value:  2.50000                                                                
       [DMCFAlg24           ] DM:C ratio algae type 24                                              
       using default value:  2.50000                                                                
       [DMCFAlg25           ] DM:C ratio algae type 25                                              
       using default value:  2.50000                                                                
       [DMCFAlg26           ] DM:C ratio algae type 26                                              
       using default value:  2.50000                                                                
       [DMCFAlg27           ] DM:C ratio algae type 27                                              
       using default value:  2.50000                                                                
       [DMCFAlg28           ] DM:C ratio algae type 28                                              
       using default value:  2.50000                                                                
       [DMCFAlg29           ] DM:C ratio algae type 29                                              
       using default value:  2.50000                                                                
       [DMCFAlg30           ] DM:C ratio algae type 30                                              
       using default value:  2.50000                                                                
       [NCRFDI_E            ] N:C ratio algae type 01                                               
       using default value: 0.210000                                                                
       [NCRFDI_P            ] N:C ratio algae type 02                                               
       using default value: 0.188000                                                                
       [NCRFFL_E            ] N:C ratio algae type 03                                               
       using default value: 0.275000                                                                
       [NCRGRE_E            ] N:C ratio algae type 04                                               
       using default value: 0.275000                                                                
       [NCRGRE_N            ] N:C ratio algae type 05                                               
       using default value: 0.175000                                                                
       [NCRGRE_P            ] N:C ratio algae type 06                                               
       using default value: 0.200000                                                                
       [NCRAPH_E            ] N:C ratio algae type 07                                               
       using default value: 0.220000                                                                
       [NCRAPH_N            ] N:C ratio algae type 08                                               
       using default value: 0.125000                                                                
       [NCRAPH_P            ] N:C ratio algae type 09                                               
       using default value: 0.170000                                                                
       [NCROSC_E            ] N:C ratio algae type 10                                               
       using default value: 0.225000                                                                
       [NCROSC_N            ] N:C ratio algae type 11                                               
       using default value: 0.125000                                                                
       [NCROSC_P            ] N:C ratio algae type 12                                               
       using default value: 0.150000                                                                
       [NCRAlg13            ] N:C ratio algae type 13                                               
       using default value: 0.200000                                                                
       [NCRAlg14            ] N:C ratio algae type 14                                               
       using default value: 0.200000                                                                
       [NCRAlg15            ] N:C ratio algae type 15                                               
       using default value: 0.200000                                                                
       [NCRAlg16            ] N:C ratio algae type 16                                               
       using default value: 0.200000                                                                
       [NCRAlg17            ] N:C ratio algae type 17                                               
       using default value: 0.200000                                                                
       [NCRAlg18            ] N:C ratio algae type 18                                               
       using default value: 0.200000                                                                
       [NCRAlg19            ] N:C ratio algae type 19                                               
       using default value: 0.200000                                                                
       [NCRAlg20            ] N:C ratio algae type 20                                               
       using default value: 0.200000                                                                
       [NCRAlg21            ] N:C ratio algae type 21                                               
       using default value: 0.200000                                                                
       [NCRAlg22            ] N:C ratio algae type 22                                               
       using default value: 0.200000                                                                
       [NCRAlg23            ] N:C ratio algae type 23                                               
       using default value: 0.200000                                                                
       [NCRAlg24            ] N:C ratio algae type 24                                               
       using default value: 0.200000                                                                
       [NCRAlg25            ] N:C ratio algae type 25                                               
       using default value: 0.200000                                                                
       [NCRAlg26            ] N:C ratio algae type 26                                               
       using default value: 0.200000                                                                
       [NCRAlg27            ] N:C ratio algae type 27                                               
       using default value: 0.200000                                                                
       [NCRAlg28            ] N:C ratio algae type 28                                               
       using default value: 0.200000                                                                
       [NCRAlg29            ] N:C ratio algae type 29                                               
       using default value: 0.200000                                                                
       [NCRAlg30            ] N:C ratio algae type 30                                               
       using default value: 0.200000                                                                
       [PCRFDI_E            ] P:C ratio algae type 01                                               
       using default value: 0.180000E-01                                                            
       [PCRFDI_P            ] P:C ratio algae type 02                                               
       using default value: 0.113000E-01                                                            
       [PCRFFL_E            ] P:C ratio algae type 03                                               
       using default value: 0.180000E-01                                                            
       [PCRGRE_E            ] P:C ratio algae type 04                                               
       using default value: 0.238000E-01                                                            
       [PCRGRE_N            ] P:C ratio algae type 05                                               
       using default value: 0.150000E-01                                                            
       [PCRGRE_P            ] P:C ratio algae type 06                                               
       using default value: 0.125000E-01                                                            
       [PCRAPH_E            ] P:C ratio algae type 07                                               
       using default value: 0.125000E-01                                                            
       [PCRAPH_N            ] P:C ratio algae type 08                                               
       using default value: 0.125000E-01                                                            
       [PCRAPH_P            ] P:C ratio algae type 09                                               
       using default value: 0.880000E-02                                                            
       [PCROSC_E            ] P:C ratio algae type 10                                               
       using default value: 0.188000E-01                                                            
       [PCROSC_N            ] P:C ratio algae type 11                                               
       using default value: 0.138000E-01                                                            
       [PCROSC_P            ] P:C ratio algae type 12                                               
       using default value: 0.113000E-01                                                            
       [PCRAlg13            ] P:C ratio algae type 13                                               
       using default value: 0.200000E-01                                                            
       [PCRAlg14            ] P:C ratio algae type 14                                               
       using default value: 0.200000E-01                                                            
       [PCRAlg15            ] P:C ratio algae type 15                                               
       using default value: 0.200000E-01                                                            
       [PCRAlg16            ] P:C ratio algae type 16                                               
       using default value: 0.200000E-01                                                            
       [PCRAlg17            ] P:C ratio algae type 17                                               
       using default value: 0.200000E-01                                                            
       [PCRAlg18            ] P:C ratio algae type 18                                               
       using default value: 0.200000E-01                                                            
       [PCRAlg19            ] P:C ratio algae type 19                                               
       using default value: 0.200000E-01                                                            
       [PCRAlg20            ] P:C ratio algae type 20                                               
       using default value: 0.200000E-01                                                            
       [PCRAlg21            ] P:C ratio algae type 21                                               
       using default value: 0.200000E-01                                                            
       [PCRAlg22            ] P:C ratio algae type 22                                               
       using default value: 0.200000E-01                                                            
       [PCRAlg23            ] P:C ratio algae type 23                                               
       using default value: 0.200000E-01                                                            
       [PCRAlg24            ] P:C ratio algae type 24                                               
       using default value: 0.200000E-01                                                            
       [PCRAlg25            ] P:C ratio algae type 25                                               
       using default value: 0.200000E-01                                                            
       [PCRAlg26            ] P:C ratio algae type 26                                               
       using default value: 0.200000E-01                                                            
       [PCRAlg27            ] P:C ratio algae type 27                                               
       using default value: 0.200000E-01                                                            
       [PCRAlg28            ] P:C ratio algae type 28                                               
       using default value: 0.200000E-01                                                            
       [PCRAlg29            ] P:C ratio algae type 29                                               
       using default value: 0.200000E-01                                                            
       [PCRAlg30            ] P:C ratio algae type 30                                               
       using default value: 0.200000E-01                                                            
       [SCRFDI_E            ] Si:C ratio algae type 01                                              
       using default value: 0.660000                                                                
       [SCRFDI_P            ] Si:C ratio algae type 02                                              
       using default value: 0.550000                                                                
       [SCRFFL_E            ] Si:C ratio algae type 03                                              
       using default value: 0.180000E-02                                                            
       [SCRGRE_E            ] Si:C ratio algae type 04                                              
       using default value: 0.180000E-02                                                            
       [SCRGRE_N            ] Si:C ratio algae type 05                                              
       using default value: 0.180000E-02                                                            
       [SCRGRE_P            ] Si:C ratio algae type 06                                              
       using default value: 0.180000E-02                                                            
       [SCRAPH_E            ] Si:C ratio algae type 07                                              
       using default value: 0.180000E-02                                                            
       [SCRAPH_N            ] Si:C ratio algae type 08                                              
       using default value: 0.180000E-02                                                            
       [SCRAPH_P            ] Si:C ratio algae type 09                                              
       using default value: 0.180000E-02                                                            
       [SCROSC_E            ] Si:C ratio algae type 10                                              
       using default value: 0.180000E-02                                                            
       [SCROSC_N            ] Si:C ratio algae type 11                                              
       using default value: 0.180000E-02                                                            
       [SCROSC_P            ] Si:C ratio algae type 12                                              
       using default value: 0.180000E-02                                                            
       [SCRAlg13            ] Si:C ratio algae type 13                                              
       using default value: 0.200000E-02                                                            
       [SCRAlg14            ] Si:C ratio algae type 14                                              
       using default value: 0.200000E-02                                                            
       [SCRAlg15            ] Si:C ratio algae type 15                                              
       using default value: 0.200000E-02                                                            
       [SCRAlg16            ] Si:C ratio algae type 16                                              
       using default value: 0.200000E-02                                                            
       [SCRAlg17            ] Si:C ratio algae type 17                                              
       using default value: 0.200000E-02                                                            
       [SCRAlg18            ] Si:C ratio algae type 18                                              
       using default value: 0.200000E-02                                                            
       [SCRAlg19            ] Si:C ratio algae type 19                                              
       using default value: 0.200000E-02                                                            
       [SCRAlg20            ] Si:C ratio algae type 20                                              
       using default value: 0.200000E-02                                                            
       [SCRAlg21            ] Si:C ratio algae type 21                                              
       using default value: 0.200000E-02                                                            
       [SCRAlg22            ] Si:C ratio algae type 22                                              
       using default value: 0.200000E-02                                                            
       [SCRAlg23            ] Si:C ratio algae type 23                                              
       using default value: 0.200000E-02                                                            
       [SCRAlg24            ] Si:C ratio algae type 24                                              
       using default value: 0.200000E-02                                                            
       [SCRAlg25            ] Si:C ratio algae type 25                                              
       using default value: 0.200000E-02                                                            
       [SCRAlg26            ] Si:C ratio algae type 26                                              
       using default value: 0.200000E-02                                                            
       [SCRAlg27            ] Si:C ratio algae type 27                                              
       using default value: 0.200000E-02                                                            
       [SCRAlg28            ] Si:C ratio algae type 28                                              
       using default value: 0.200000E-02                                                            
       [SCRAlg29            ] Si:C ratio algae type 29                                              
       using default value: 0.200000E-02                                                            
       [SCRAlg30            ] Si:C ratio algae type 30                                              
       using default value: 0.200000E-02                                                            
       [XNCRFDI_E           ] N:C ratio for heterotrophic algae type 01                             
       using default value:-1.000000                                                                
       [XNCRFDI_P           ] N:C ratio for heterotrophic algae type 02                             
       using default value:-1.000000                                                                
       [XNCRFFL_E           ] N:C ratio for heterotrophic algae type 03                             
       using default value:-1.000000                                                                
       [XNCRGRE_E           ] N:C ratio for heterotrophic algae type 04                             
       using default value:-1.000000                                                                
       [XNCRGRE_N           ] N:C ratio for heterotrophic algae type 05                             
       using default value:-1.000000                                                                
       [XNCRGRE_P           ] N:C ratio for heterotrophic algae type 06                             
       using default value:-1.000000                                                                
       [XNCRAPH_E           ] N:C ratio for heterotrophic algae type 07                             
       using default value:-1.000000                                                                
       [XNCRAPH_N           ] N:C ratio for heterotrophic algae type 08                             
       using default value:-1.000000                                                                
       [XNCRAPH_P           ] N:C ratio for heterotrophic algae type 09                             
       using default value:-1.000000                                                                
       [XNCROSC_E           ] N:C ratio for heterotrophic algae type 10                             
       using default value:-1.000000                                                                
       [XNCROSC_N           ] N:C ratio for heterotrophic algae type 11                             
       using default value:-1.000000                                                                
       [XNCROSC_P           ] N:C ratio for heterotrophic algae type 12                             
       using default value:-1.000000                                                                
       [XNCRAlg13           ] N:C ratio for heterotrophic algae type 13                             
       using default value:-1.000000                                                                
       [XNCRAlg14           ] N:C ratio for heterotrophic algae type 14                             
       using default value:-1.000000                                                                
       [XNCRAlg15           ] N:C ratio for heterotrophic algae type 15                             
       using default value:-1.000000                                                                
       [XNCRAlg16           ] N:C ratio for heterotrophic algae type 16                             
       using default value:-1.000000                                                                
       [XNCRAlg17           ] N:C ratio for heterotrophic algae type 17                             
       using default value:-1.000000                                                                
       [XNCRAlg18           ] N:C ratio for heterotrophic algae type 18                             
       using default value:-1.000000                                                                
       [XNCRAlg19           ] N:C ratio for heterotrophic algae type 19                             
       using default value:-1.000000                                                                
       [XNCRAlg20           ] N:C ratio for heterotrophic algae type 20                             
       using default value:-1.000000                                                                
       [XNCRAlg21           ] N:C ratio for heterotrophic algae type 21                             
       using default value:-1.000000                                                                
       [XNCRAlg22           ] N:C ratio for heterotrophic algae type 22                             
       using default value:-1.000000                                                                
       [XNCRAlg23           ] N:C ratio for heterotrophic algae type 23                             
       using default value:-1.000000                                                                
       [XNCRAlg24           ] N:C ratio for heterotrophic algae type 24                             
       using default value:-1.000000                                                                
       [XNCRAlg25           ] N:C ratio for heterotrophic algae type 25                             
       using default value:-1.000000                                                                
       [XNCRAlg26           ] N:C ratio for heterotrophic algae type 26                             
       using default value:-1.000000                                                                
       [XNCRAlg27           ] N:C ratio for heterotrophic algae type 27                             
       using default value:-1.000000                                                                
       [XNCRAlg28           ] N:C ratio for heterotrophic algae type 28                             
       using default value:-1.000000                                                                
       [XNCRAlg29           ] N:C ratio for heterotrophic algae type 29                             
       using default value:-1.000000                                                                
       [XNCRAlg30           ] N:C ratio for heterotrophic algae type 30                             
       using default value:-1.000000                                                                
       [XPCRFDI_E           ] P:C ratio for heterotrophic algae type 01                             
       using default value:-1.000000                                                                
       [XPCRFDI_P           ] P:C ratio for heterotrophic algae type 02                             
       using default value:-1.000000                                                                
       [XPCRFFL_E           ] P:C ratio for heterotrophic algae type 03                             
       using default value:-1.000000                                                                
       [XPCRGRE_E           ] P:C ratio for heterotrophic algae type 04                             
       using default value:-1.000000                                                                
       [XPCRGRE_N           ] P:C ratio for heterotrophic algae type 05                             
       using default value:-1.000000                                                                
       [XPCRGRE_P           ] P:C ratio for heterotrophic algae type 06                             
       using default value:-1.000000                                                                
       [XPCRAPH_E           ] P:C ratio for heterotrophic algae type 07                             
       using default value:-1.000000                                                                
       [XPCRAPH_N           ] P:C ratio for heterotrophic algae type 08                             
       using default value:-1.000000                                                                
       [XPCRAPH_P           ] P:C ratio for heterotrophic algae type 09                             
       using default value:-1.000000                                                                
       [XPCROSC_E           ] P:C ratio for heterotrophic algae type 10                             
       using default value:-1.000000                                                                
       [XPCROSC_N           ] P:C ratio for heterotrophic algae type 11                             
       using default value:-1.000000                                                                
       [XPCROSC_P           ] P:C ratio for heterotrophic algae type 12                             
       using default value:-1.000000                                                                
       [XPCRAlg13           ] P:C ratio for heterotrophic algae type 13                             
       using default value:-1.000000                                                                
       [XPCRAlg14           ] P:C ratio for heterotrophic algae type 14                             
       using default value:-1.000000                                                                
       [XPCRAlg15           ] P:C ratio for heterotrophic algae type 15                             
       using default value:-1.000000                                                                
       [XPCRAlg16           ] P:C ratio for heterotrophic algae type 16                             
       using default value:-1.000000                                                                
       [XPCRAlg17           ] P:C ratio for heterotrophic algae type 17                             
       using default value:-1.000000                                                                
       [XPCRAlg18           ] P:C ratio for heterotrophic algae type 18                             
       using default value:-1.000000                                                                
       [XPCRAlg19           ] P:C ratio for heterotrophic algae type 19                             
       using default value:-1.000000                                                                
       [XPCRAlg20           ] P:C ratio for heterotrophic algae type 20                             
       using default value:-1.000000                                                                
       [XPCRAlg21           ] P:C ratio for heterotrophic algae type 21                             
       using default value:-1.000000                                                                
       [XPCRAlg22           ] P:C ratio for heterotrophic algae type 22                             
       using default value:-1.000000                                                                
       [XPCRAlg23           ] P:C ratio for heterotrophic algae type 23                             
       using default value:-1.000000                                                                
       [XPCRAlg24           ] P:C ratio for heterotrophic algae type 24                             
       using default value:-1.000000                                                                
       [XPCRAlg25           ] P:C ratio for heterotrophic algae type 25                             
       using default value:-1.000000                                                                
       [XPCRAlg26           ] P:C ratio for heterotrophic algae type 26                             
       using default value:-1.000000                                                                
       [XPCRAlg27           ] P:C ratio for heterotrophic algae type 27                             
       using default value:-1.000000                                                                
       [XPCRAlg28           ] P:C ratio for heterotrophic algae type 28                             
       using default value:-1.000000                                                                
       [XPCRAlg29           ] P:C ratio for heterotrophic algae type 29                             
       using default value:-1.000000                                                                
       [XPCRAlg30           ] P:C ratio for heterotrophic algae type 30                             
       using default value:-1.000000                                                                
       [FNCRFDI_E           ] N:C ratio for nitrogen fixing algae type 01                           
       using default value:-1.000000                                                                
       [FNCRFDI_P           ] N:C ratio for nitrogen fixing algae type 02                           
       using default value:-1.000000                                                                
       [FNCRFFL_E           ] N:C ratio for nitrogen fixing algae type 03                           
       using default value:-1.000000                                                                
       [FNCRGRE_E           ] N:C ratio for nitrogen fixing algae type 04                           
       using default value:-1.000000                                                                
       [FNCRGRE_N           ] N:C ratio for nitrogen fixing algae type 05                           
       using default value:-1.000000                                                                
       [FNCRGRE_P           ] N:C ratio for nitrogen fixing algae type 06                           
       using default value:-1.000000                                                                
       [FNCRAPH_E           ] N:C ratio for nitrogen fixing algae type 07                           
       using default value:-1.000000                                                                
       [FNCRAPH_N           ] N:C ratio for nitrogen fixing algae type 08                           
       using default value:-1.000000                                                                
       [FNCRAPH_P           ] N:C ratio for nitrogen fixing algae type 09                           
       using default value:-1.000000                                                                
       [FNCROSC_E           ] N:C ratio for nitrogen fixing algae type 10                           
       using default value:-1.000000                                                                
       [FNCROSC_N           ] N:C ratio for nitrogen fixing algae type 11                           
       using default value:-1.000000                                                                
       [FNCROSC_P           ] N:C ratio for nitrogen fixing algae type 12                           
       using default value:-1.000000                                                                
       [FNCRAlg13           ] N:C ratio for nitrogen fixing algae type 13                           
       using default value:-1.000000                                                                
       [FNCRAlg14           ] N:C ratio for nitrogen fixing algae type 14                           
       using default value:-1.000000                                                                
       [FNCRAlg15           ] N:C ratio for nitrogen fixing algae type 15                           
       using default value:-1.000000                                                                
       [FNCRAlg16           ] N:C ratio for nitrogen fixing algae type 16                           
       using default value:-1.000000                                                                
       [FNCRAlg17           ] N:C ratio for nitrogen fixing algae type 17                           
       using default value:-1.000000                                                                
       [FNCRAlg18           ] N:C ratio for nitrogen fixing algae type 18                           
       using default value:-1.000000                                                                
       [FNCRAlg19           ] N:C ratio for nitrogen fixing algae type 19                           
       using default value:-1.000000                                                                
       [FNCRAlg20           ] N:C ratio for nitrogen fixing algae type 20                           
       using default value:-1.000000                                                                
       [FNCRAlg21           ] N:C ratio for nitrogen fixing algae type 21                           
       using default value:-1.000000                                                                
       [FNCRAlg22           ] N:C ratio for nitrogen fixing algae type 22                           
       using default value:-1.000000                                                                
       [FNCRAlg23           ] N:C ratio for nitrogen fixing algae type 23                           
       using default value:-1.000000                                                                
       [FNCRAlg24           ] N:C ratio for nitrogen fixing algae type 24                           
       using default value:-1.000000                                                                
       [FNCRAlg25           ] N:C ratio for nitrogen fixing algae type 25                           
       using default value:-1.000000                                                                
       [FNCRAlg26           ] N:C ratio for nitrogen fixing algae type 26                           
       using default value:-1.000000                                                                
       [FNCRAlg27           ] N:C ratio for nitrogen fixing algae type 27                           
       using default value:-1.000000                                                                
       [FNCRAlg28           ] N:C ratio for nitrogen fixing algae type 28                           
       using default value:-1.000000                                                                
       [FNCRAlg29           ] N:C ratio for nitrogen fixing algae type 29                           
       using default value:-1.000000                                                                
       [FNCRAlg30           ] N:C ratio for nitrogen fixing algae type 30                           
       using default value:-1.000000                                                                
       [CHLACFDI_E          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.400000E-01                                                            
       [CHLACFDI_P          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.250000E-01                                                            
       [CHLACFFL_E          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.290000E-01                                                            
       [CHLACGRE_E          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.330000E-01                                                            
       [CHLACGRE_N          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.250000E-01                                                            
       [CHLACGRE_P          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.250000E-01                                                            
       [CHLACAPH_E          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.330000E-01                                                            
       [CHLACAPH_N          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.250000E-01                                                            
       [CHLACAPH_P          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.250000E-01                                                            
       [CHLACOSC_E          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.330000E-01                                                            
       [CHLACOSC_N          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.200000E-01                                                            
       [CHLACOSC_P          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.200000E-01                                                            
       [ChlaCAlg13          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.300000E-01                                                            
       [ChlaCAlg14          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.300000E-01                                                            
       [ChlaCAlg15          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.300000E-01                                                            
       [ChlaCAlg16          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.300000E-01                                                            
       [ChlaCAlg17          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.300000E-01                                                            
       [ChlaCAlg18          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.300000E-01                                                            
       [ChlaCAlg19          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.300000E-01                                                            
       [ChlaCAlg20          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.300000E-01                                                            
       [ChlaCAlg21          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.300000E-01                                                            
       [ChlaCAlg22          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.300000E-01                                                            
       [ChlaCAlg23          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.300000E-01                                                            
       [ChlaCAlg24          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.300000E-01                                                            
       [ChlaCAlg25          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.300000E-01                                                            
       [ChlaCAlg26          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.300000E-01                                                            
       [ChlaCAlg27          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.300000E-01                                                            
       [ChlaCAlg28          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.300000E-01                                                            
       [ChlaCAlg29          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.300000E-01                                                            
       [ChlaCAlg30          ] Chlorophyll-a:C ratio per algae type                                  
       using default value: 0.300000E-01                                                            
       [PPMAXFDI_E          ] maximum production rate algae type 01                                 
       using default value: 0.350000                                                                
       [PPMAXFDI_P          ] maximum production rate algae type 02                                 
       using default value: 0.350000                                                                
       [PPMAXFFL_E          ] maximum production rate algae type 03                                 
       using default value: 0.350000                                                                
       [PPMAXGRE_E          ] maximum production rate algae type 04                                 
       using default value: 0.680000E-01                                                            
       [PPMAXGRE_N          ] maximum production rate algae type 05                                 
       using default value: 0.680000E-01                                                            
       [PPMAXGRE_P          ] maximum production rate algae type 06                                 
       using default value: 0.680000E-01                                                            
       [PPMAXAPH_E          ] maximum production rate algae type 07                                 
       using default value: 0.190000                                                                
       [PPMAXAPH_N          ] maximum production rate algae type 08                                 
       using default value: 0.150000                                                                
       [PPMAXAPH_P          ] maximum production rate algae type 09                                 
       using default value: 0.150000                                                                
       [PPMAXOSC_E          ] maximum production rate algae type 10                                 
       using default value: 0.450000E-01                                                            
       [PPMAXOSC_N          ] maximum production rate algae type 11                                 
       using default value: 0.340000E-01                                                            
       [PPMAXOSC_P          ] maximum production rate algae type 12                                 
       using default value: 0.340000E-01                                                            
       [PPMaxAlg13          ] maximum production rate algae type 13                                 
       using default value: 0.350000                                                                
       [PPMaxAlg14          ] maximum production rate algae type 14                                 
       using default value: 0.350000                                                                
       [PPMaxAlg15          ] maximum production rate algae type 15                                 
       using default value: 0.350000                                                                
       [PPMaxAlg16          ] maximum production rate algae type 16                                 
       using default value: 0.350000                                                                
       [PPMaxAlg17          ] maximum production rate algae type 17                                 
       using default value: 0.350000                                                                
       [PPMaxAlg18          ] maximum production rate algae type 18                                 
       using default value: 0.350000                                                                
       [PPMaxAlg19          ] maximum production rate algae type 19                                 
       using default value: 0.350000                                                                
       [PPMaxAlg20          ] maximum production rate algae type 20                                 
       using default value: 0.350000                                                                
       [PPMaxAlg21          ] maximum production rate algae type 21                                 
       using default value: 0.350000                                                                
       [PPMaxAlg22          ] maximum production rate algae type 22                                 
       using default value: 0.350000                                                                
       [PPMaxAlg23          ] maximum production rate algae type 23                                 
       using default value: 0.350000                                                                
       [PPMaxAlg24          ] maximum production rate algae type 24                                 
       using default value: 0.350000                                                                
       [PPMaxAlg25          ] maximum production rate algae type 25                                 
       using default value: 0.350000                                                                
       [PPMaxAlg26          ] maximum production rate algae type 26                                 
       using default value: 0.350000                                                                
       [PPMaxAlg27          ] maximum production rate algae type 27                                 
       using default value: 0.350000                                                                
       [PPMaxAlg28          ] maximum production rate algae type 28                                 
       using default value: 0.350000                                                                
       [PPMaxAlg29          ] maximum production rate algae type 29                                 
       using default value: 0.350000                                                                
       [PPMaxAlg30          ] maximum production rate algae type 30                                 
       using default value: 0.350000                                                                
       [TCPMXFDI_E          ] temp. coeff. for growth processes algae type 01                       
       using default value:  1.06000                                                                
       [TCPMXFDI_P          ] temp. coeff. for growth processes algae type 02                       
       using default value:  1.05400                                                                
       [TCPMXFFL_E          ] temp. coeff. for growth processes algae type 03                       
       using default value:  1.05000                                                                
       [TCPMXGRE_E          ] temp. coeff. for growth processes algae type 04                       
       using default value:  0.00000                                                                
       [TCPMXGRE_N          ] temp. coeff. for growth processes algae type 05                       
       using default value:  3.00000                                                                
       [TCPMXGRE_P          ] temp. coeff. for growth processes algae type 06                       
       using default value:  3.00000                                                                
       [TCPMXAPH_E          ] temp. coeff. for growth processes algae type 07                       
       using default value:  1.08300                                                                
       [TCPMXAPH_N          ] temp. coeff. for growth processes algae type 08                       
       using default value:  1.09500                                                                
       [TCPMXAPH_P          ] temp. coeff. for growth processes algae type 09                       
       using default value:  1.09500                                                                
       [TCPMXOSC_E          ] temp. coeff. for growth processes algae type 10                       
       using default value:  0.00000                                                                
       [TCPMXOSC_N          ] temp. coeff. for growth processes algae type 11                       
       using default value:  0.00000                                                                
       [TCPMXOSC_P          ] temp. coeff. for growth processes algae type 12                       
       using default value:  0.00000                                                                
       [TcPMxAlg13          ] temp. coeff. for growth processes algae type 13                       
       using default value:  1.06000                                                                
       [TcPMxAlg14          ] temp. coeff. for growth processes algae type 14                       
       using default value:  1.06000                                                                
       [TcPMxAlg15          ] temp. coeff. for growth processes algae type 15                       
       using default value:  1.06000                                                                
       [TcPMxAlg16          ] temp. coeff. for growth processes algae type 16                       
       using default value:  1.06000                                                                
       [TcPMxAlg17          ] temp. coeff. for growth processes algae type 17                       
       using default value:  1.06000                                                                
       [TcPMxAlg18          ] temp. coeff. for growth processes algae type 18                       
       using default value:  1.06000                                                                
       [TcPMxAlg19          ] temp. coeff. for growth processes algae type 19                       
       using default value:  1.06000                                                                
       [TcPMxAlg20          ] temp. coeff. for growth processes algae type 20                       
       using default value:  1.06000                                                                
       [TcPMxAlg21          ] temp. coeff. for growth processes algae type 21                       
       using default value:  1.06000                                                                
       [TcPMxAlg22          ] temp. coeff. for growth processes algae type 22                       
       using default value:  1.06000                                                                
       [TcPMxAlg23          ] temp. coeff. for growth processes algae type 23                       
       using default value:  1.06000                                                                
       [TcPMxAlg24          ] temp. coeff. for growth processes algae type 24                       
       using default value:  1.06000                                                                
       [TcPMxAlg25          ] temp. coeff. for growth processes algae type 25                       
       using default value:  1.06000                                                                
       [TcPMxAlg26          ] temp. coeff. for growth processes algae type 26                       
       using default value:  1.06000                                                                
       [TcPMxAlg27          ] temp. coeff. for growth processes algae type 27                       
       using default value:  1.06000                                                                
       [TcPMxAlg28          ] temp. coeff. for growth processes algae type 28                       
       using default value:  1.06000                                                                
       [TcPMxAlg29          ] temp. coeff. for growth processes algae type 29                       
       using default value:  1.06000                                                                
       [TcPMxAlg30          ] temp. coeff. for growth processes algae type 30                       
       using default value:  1.06000                                                                
       [TFPMXFDI_E          ] temp. dependency PMAX algae type01 (0=lin,<>0=exp)                    
       using default value: 1.000000                                                                
       [TFPMXFDI_P          ] temp. dependency PMAX algae type02 (0=lin,<>0=exp)                    
       using default value: 1.000000                                                                
       [TFPMXFFL_E          ] temp. dependency PMAX algae type03 (0=lin,<>0=exp)                    
       using default value: 1.000000                                                                
       [TFPMXGRE_E          ] temp. dependency PMAX algae type04 (0=lin,<>0=exp)                    
       using default value:  0.00000                                                                
       [TFPMXGRE_N          ] temp. dependency PMAX algae type05 (0=lin,<>0=exp)                    
       using default value:  0.00000                                                                
       [TFPMXGRE_P          ] temp. dependency PMAX algae type06 (0=lin,<>0=exp)                    
       using default value:  0.00000                                                                
       [TFPMXAPH_E          ] temp. dependency PMAX algae type07 (0=lin,<>0=exp)                    
       using default value: 1.000000                                                                
       [TFPMXAPH_N          ] temp. dependency PMAX algae type08 (0=lin,<>0=exp)                    
       using default value: 1.000000                                                                
       [TFPMXAPH_P          ] temp. dependency PMAX algae type09 (0=lin,<>0=exp)                    
       using default value: 1.000000                                                                
       [TFPMXOSC_E          ] temp. dependency PMAX algae type10 (0=lin,<>0=exp)                    
       using default value:  0.00000                                                                
       [TFPMXOSC_N          ] temp. dependency PMAX algae type11 (0=lin,<>0=exp)                    
       using default value:  0.00000                                                                
       [TFPMXOSC_P          ] temp. dependency PMAX algae type12 (0=lin,<>0=exp)                    
       using default value:  0.00000                                                                
       [TFPMxAlg13          ] temp. dependency PMAX algae type13 (0=lin,<>0=exp)                    
       using default value: 1.000000                                                                
       [TFPMxAlg14          ] temp. dependency PMAX algae type14 (0=lin,<>0=exp)                    
       using default value: 1.000000                                                                
       [TFPMxAlg15          ] temp. dependency PMAX algae type15 (0=lin,<>0=exp)                    
       using default value: 1.000000                                                                
       [TFPMxAlg16          ] temp. dependency PMAX algae type16 (0=lin,<>0=e                       
       using default value: 1.000000                                                                
       [TFPMxAlg17          ] temp. dependency PMAX algae type17 (0=lin,<>0=e                       
       using default value: 1.000000                                                                
       [TFPMxAlg18          ] temp. dependency PMAX algae type18 (0=lin,<>0=e                       
       using default value: 1.000000                                                                
       [TFPMxAlg19          ] temp. dependency PMAX algae type19 (0=lin,<>0=e                       
       using default value: 1.000000                                                                
       [TFPMxAlg20          ] temp. dependency PMAX algae type20 (0=lin,<>0=e                       
       using default value: 1.000000                                                                
       [TFPMxAlg21          ] temp. dependency PMAX algae type21 (0=lin,<>0=e                       
       using default value: 1.000000                                                                
       [TFPMxAlg22          ] temp. dependency PMAX algae type22 (0=lin,<>0=e                       
       using default value: 1.000000                                                                
       [TFPMxAlg23          ] temp. dependency PMAX algae type23 (0=lin,<>0=e                       
       using default value: 1.000000                                                                
       [TFPMxAlg24          ] temp. dependency PMAX algae type24 (0=lin,<>0=e                       
       using default value: 1.000000                                                                
       [TFPMxAlg25          ] temp. dependency PMAX algae type25 (0=lin,<>0=e                       
       using default value: 1.000000                                                                
       [TFPMxAlg26          ] temp. dependency PMAX algae type26 (0=lin,<>0=e                       
       using default value: 1.000000                                                                
       [TFPMxAlg27          ] temp. dependency PMAX algae type27 (0=lin,<>0=e                       
       using default value: 1.000000                                                                
       [TFPMxAlg28          ] temp. dependency PMAX algae type28 (0=lin,<>0=e                       
       using default value: 1.000000                                                                
       [TFPMxAlg29          ] temp. dependency PMAX algae type29 (0=lin,<>0=e                       
       using default value: 1.000000                                                                
       [TFPMxAlg30          ] temp. dependency PMAX algae type30 (0=lin,<>0=e                       
       using default value: 1.000000                                                                
       [MORT0FDI_E          ] mortality rate at 0 oC algae type 01                                  
       using default value: 0.350000E-01                                                            
       [MORT0FDI_P          ] mortality rate at 0 oC algae type 02                                  
       using default value: 0.450000E-01                                                            
       [MORT0FFL_E          ] mortality rate at 0 oC algae type 03                                  
       using default value: 0.350000E-01                                                            
       [MORT0GRE_E          ] mortality rate at 0 oC algae type 04                                  
       using default value: 0.350000E-01                                                            
       [MORT0GRE_N          ] mortality rate at 0 oC algae type 05                                  
       using default value: 0.450000E-01                                                            
       [MORT0GRE_P          ] mortality rate at 0 oC algae type 06                                  
       using default value: 0.450000E-01                                                            
       [MORT0APH_E          ] mortality rate at 0 oC algae type 07                                  
       using default value: 0.350000E-01                                                            
       [MORT0APH_N          ] mortality rate at 0 oC algae type 08                                  
       using default value: 0.450000E-01                                                            
       [MORT0APH_P          ] mortality rate at 0 oC algae type 09                                  
       using default value: 0.450000E-01                                                            
       [MORT0OSC_E          ] mortality rate at 0 oC algae type 10                                  
       using default value: 0.350000E-01                                                            
       [MORT0OSC_N          ] mortality rate at 0 oC algae type 11                                  
       using default value: 0.450000E-01                                                            
       [MORT0OSC_P          ] mortality rate at 0 oC algae type 12                                  
       using default value: 0.450000E-01                                                            
       [Mort0Alg13          ] mortality rate at 0 oC algae type 13                                  
       using default value: 0.450000E-01                                                            
       [Mort0Alg14          ] mortality rate at 0 oC algae type 14                                  
       using default value: 0.450000E-01                                                            
       [Mort0Alg15          ] mortality rate at 0 oC algae type 15                                  
       using default value: 0.450000E-01                                                            
       [Mort0Alg16          ] mortality rate at 0 oC algae type 16                                  
       using default value: 0.450000E-01                                                            
       [Mort0Alg17          ] mortality rate at 0 oC algae type 17                                  
       using default value: 0.450000E-01                                                            
       [Mort0Alg18          ] mortality rate at 0 oC algae type 18                                  
       using default value: 0.450000E-01                                                            
       [Mort0Alg19          ] mortality rate at 0 oC algae type 19                                  
       using default value: 0.450000E-01                                                            
       [Mort0Alg20          ] mortality rate at 0 oC algae type 20                                  
       using default value: 0.450000E-01                                                            
       [Mort0Alg21          ] mortality rate at 0 oC algae type 21                                  
       using default value: 0.450000E-01                                                            
       [Mort0Alg22          ] mortality rate at 0 oC algae type 22                                  
       using default value: 0.450000E-01                                                            
       [Mort0Alg23          ] mortality rate at 0 oC algae type 23                                  
       using default value: 0.450000E-01                                                            
       [Mort0Alg24          ] mortality rate at 0 oC algae type 24                                  
       using default value: 0.450000E-01                                                            
       [Mort0Alg25          ] mortality rate at 0 oC algae type 25                                  
       using default value: 0.450000E-01                                                            
       [Mort0Alg26          ] mortality rate at 0 oC algae type 26                                  
       using default value: 0.450000E-01                                                            
       [Mort0Alg27          ] mortality rate at 0 oC algae type 27                                  
       using default value: 0.450000E-01                                                            
       [Mort0Alg28          ] mortality rate at 0 oC algae type 28                                  
       using default value: 0.450000E-01                                                            
       [Mort0Alg29          ] mortality rate at 0 oC algae type 29                                  
       using default value: 0.450000E-01                                                            
       [Mort0Alg30          ] mortality rate at 0 oC algae type 30                                  
       using default value: 0.450000E-01                                                            
       [TCMRTFDI_E          ] temperature coeff. for mortality algae type 01                        
       using default value:  1.08000                                                                
       [TCMRTFDI_P          ] temperature coeff. for mortality algae type 02                        
       using default value:  1.08500                                                                
       [TCMRTFFL_E          ] temperature coeff. for mortality algae type 03                        
       using default value:  1.08000                                                                
       [TCMRTGRE_E          ] temperature coeff. for mortality algae type 04                        
       using default value:  1.08000                                                                
       [TCMRTGRE_N          ] temperature coeff. for mortality algae type 05                        
       using default value:  1.08500                                                                
       [TCMRTGRE_P          ] temperature coeff. for mortality algae type 06                        
       using default value:  1.08500                                                                
       [TCMRTAPH_E          ] temperature coeff. for mortality algae type 07                        
       using default value:  1.08000                                                                
       [TCMRTAPH_N          ] temperature coeff. for mortality algae type 08                        
       using default value:  1.08500                                                                
       [TCMRTAPH_P          ] temperature coeff. for mortality algae type 09                        
       using default value:  1.08500                                                                
       [TCMRTOSC_E          ] temperature coeff. for mortality algae type 10                        
       using default value:  1.08000                                                                
       [TCMRTOSC_N          ] temperature coeff. for mortality algae type 11                        
       using default value:  1.08500                                                                
       [TCMRTOSC_P          ] temperature coeff. for mortality algae type 12                        
       using default value:  1.08500                                                                
       [TcMrtAlg13          ] temperature coeff. for mortality algae type 13                        
       using default value:  1.08500                                                                
       [TcMrtAlg14          ] temperature coeff. for mortality algae type 14                        
       using default value:  1.08500                                                                
       [TcMrtAlg15          ] temperature coeff. for mortality algae type 15                        
       using default value:  1.08500                                                                
       [TcMrtAlg16          ] temperature coeff. for mortality algae type 16                        
       using default value:  1.08500                                                                
       [TcMrtAlg17          ] temperature coeff. for mortality algae type 17                        
       using default value:  1.08500                                                                
       [TcMrtAlg18          ] temperature coeff. for mortality algae type 18                        
       using default value:  1.08500                                                                
       [TcMrtAlg19          ] temperature coeff. for mortality algae type 19                        
       using default value:  1.08500                                                                
       [TcMrtAlg20          ] temperature coeff. for mortality algae type 20                        
       using default value:  1.08500                                                                
       [TcMrtAlg21          ] temperature coeff. for mortality algae type 21                        
       using default value:  1.08500                                                                
       [TcMrtAlg22          ] temperature coeff. for mortality algae type 22                        
       using default value:  1.08500                                                                
       [TcMrtAlg23          ] temperature coeff. for mortality algae type 23                        
       using default value:  1.08500                                                                
       [TcMrtAlg24          ] temperature coeff. for mortality algae type 24                        
       using default value:  1.08500                                                                
       [TcMrtAlg25          ] temperature coeff. for mortality algae type 25                        
       using default value:  1.08500                                                                
       [TcMrtAlg26          ] temperature coeff. for mortality algae type 26                        
       using default value:  1.08500                                                                
       [TcMrtAlg27          ] temperature coeff. for mortality algae type 27                        
       using default value:  1.08500                                                                
       [TcMrtAlg28          ] temperature coeff. for mortality algae type 28                        
       using default value:  1.08500                                                                
       [TcMrtAlg29          ] temperature coeff. for mortality algae type 29                        
       using default value:  1.08500                                                                
       [TcMrtAlg30          ] temperature coeff. for mortality algae type 30                        
       using default value:  1.08500                                                                
       [MRESPFDI_E          ] maintenance respiration rate algae type 01                            
       using default value: 0.310000E-01                                                            
       [MRESPFDI_P          ] maintenance respiration rate algae type 02                            
       using default value: 0.310000E-01                                                            
       [MRESPFFL_E          ] maintenance respiration rate algae type 03                            
       using default value: 0.310000E-01                                                            
       [MRESPGRE_E          ] maintenance respiration rate algae type 04                            
       using default value: 0.310000E-01                                                            
       [MRESPGRE_N          ] maintenance respiration rate algae type 05                            
       using default value: 0.310000E-01                                                            
       [MRESPGRE_P          ] maintenance respiration rate algae type 06                            
       using default value: 0.310000E-01                                                            
       [MRESPAPH_E          ] maintenance respiration rate algae type 07                            
       using default value: 0.120000E-01                                                            
       [MRESPAPH_N          ] maintenance respiration rate algae type 08                            
       using default value: 0.120000E-01                                                            
       [MRESPAPH_P          ] maintenance respiration rate algae type 09                            
       using default value: 0.120000E-01                                                            
       [MRESPOSC_E          ] maintenance respiration rate algae type 10                            
       using default value: 0.120000E-01                                                            
       [MRESPOSC_N          ] maintenance respiration rate algae type 11                            
       using default value: 0.120000E-01                                                            
       [MRESPOSC_P          ] maintenance respiration rate algae type 12                            
       using default value: 0.120000E-01                                                            
       [MRespAlg13          ] maintenance respiration rate algae type 13                            
       using default value: 0.310000E-01                                                            
       [MRespAlg14          ] maintenance respiration rate algae type 14                            
       using default value: 0.310000E-01                                                            
       [MRespAlg15          ] maintenance respiration rate algae type 15                            
       using default value: 0.310000E-01                                                            
       [MRespAlg16          ] maintenance respiration rate algae type 16                            
       using default value: 0.310000E-01                                                            
       [MRespAlg17          ] maintenance respiration rate algae type 17                            
       using default value: 0.310000E-01                                                            
       [MRespAlg18          ] maintenance respiration rate algae type 18                            
       using default value: 0.310000E-01                                                            
       [MRespAlg19          ] maintenance respiration rate algae type 19                            
       using default value: 0.310000E-01                                                            
       [MRespAlg20          ] maintenance respiration rate algae type 20                            
       using default value: 0.310000E-01                                                            
       [MRespAlg21          ] maintenance respiration rate algae type 21                            
       using default value: 0.310000E-01                                                            
       [MRespAlg22          ] maintenance respiration rate algae type 22                            
       using default value: 0.310000E-01                                                            
       [MRespAlg23          ] maintenance respiration rate algae type 23                            
       using default value: 0.310000E-01                                                            
       [MRespAlg24          ] maintenance respiration rate algae type 24                            
       using default value: 0.310000E-01                                                            
       [MRespAlg25          ] maintenance respiration rate algae type 25                            
       using default value: 0.310000E-01                                                            
       [MRespAlg26          ] maintenance respiration rate algae type 26                            
       using default value: 0.310000E-01                                                            
       [MRespAlg27          ] maintenance respiration rate algae type 27                            
       using default value: 0.310000E-01                                                            
       [MRespAlg28          ] maintenance respiration rate algae type 28                            
       using default value: 0.310000E-01                                                            
       [MRespAlg29          ] maintenance respiration rate algae type 29                            
       using default value: 0.310000E-01                                                            
       [MRespAlg30          ] maintenance respiration rate algae type 30                            
       using default value: 0.310000E-01                                                            
       [TCRSPFDI_E          ] temperature coeff. for respiration algae type 01                      
       using default value:  1.07200                                                                
       [TCRSPFDI_P          ] temperature coeff. for respiration algae type 02                      
       using default value:  1.07200                                                                
       [TCRSPFFL_E          ] temperature coeff. for respiration algae type 03                      
       using default value:  1.07200                                                                
       [TCRSPGRE_E          ] temperature coeff. for respiration algae type 04                      
       using default value:  1.07200                                                                
       [TCRSPGRE_N          ] temperature coeff. for respiration algae type 05                      
       using default value:  1.07200                                                                
       [TCRSPGRE_P          ] temperature coeff. for respiration algae type 06                      
       using default value:  1.07200                                                                
       [TCRSPAPH_E          ] temperature coeff. for respiration algae type 07                      
       using default value:  1.07200                                                                
       [TCRSPAPH_N          ] temperature coeff. for respiration algae type 08                      
       using default value:  1.07200                                                                
       [TCRSPAPH_P          ] temperature coeff. for respiration algae type 09                      
       using default value:  1.07200                                                                
       [TCRSPOSC_E          ] temperature coeff. for respiration algae type 10                      
       using default value:  1.07200                                                                
       [TCRSPOSC_N          ] temperature coeff. for respiration algae type 11                      
       using default value:  1.07200                                                                
       [TCRSPOSC_P          ] temperature coeff. for respiration algae type 12                      
       using default value:  1.07200                                                                
       [TcRspAlg13          ] temperature coeff. for respiration algae type 13                      
       using default value:  1.07200                                                                
       [TcRspAlg14          ] temperature coeff. for respiration algae type 14                      
       using default value:  1.07200                                                                
       [TcRspAlg15          ] temperature coeff. for respiration algae type 15                      
       using default value:  1.07200                                                                
       [TcRspAlg16          ] temperature coeff. for respiration algae type 1                       
       using default value:  1.07200                                                                
       [TcRspAlg17          ] temperature coeff. for respiration algae type 1                       
       using default value:  1.07200                                                                
       [TcRspAlg18          ] temperature coeff. for respiration algae type 1                       
       using default value:  1.07200                                                                
       [TcRspAlg19          ] temperature coeff. for respiration algae type 1                       
       using default value:  1.07200                                                                
       [TcRspAlg20          ] temperature coeff. for respiration algae type 1                       
       using default value:  1.07200                                                                
       [TcRspAlg21          ] temperature coeff. for respiration algae type 1                       
       using default value:  1.07200                                                                
       [TcRspAlg22          ] temperature coeff. for respiration algae type 1                       
       using default value:  1.07200                                                                
       [TcRspAlg23          ] temperature coeff. for respiration algae type 1                       
       using default value:  1.07200                                                                
       [TcRspAlg24          ] temperature coeff. for respiration algae type 1                       
       using default value:  1.07200                                                                
       [TcRspAlg25          ] temperature coeff. for respiration algae type 1                       
       using default value:  1.07200                                                                
       [TcRspAlg26          ] temperature coeff. for respiration algae type 1                       
       using default value:  1.07200                                                                
       [TcRspAlg27          ] temperature coeff. for respiration algae type 1                       
       using default value:  1.07200                                                                
       [TcRspAlg28          ] temperature coeff. for respiration algae type 1                       
       using default value:  1.07200                                                                
       [TcRspAlg29          ] temperature coeff. for respiration algae type 1                       
       using default value:  1.07200                                                                
       [TcRspAlg30          ] temperature coeff. for respiration algae type 1                       
       using default value:  1.07200                                                                
       [SDMIXFDI_E          ] distribution in water column algae type 01                            
       using default value: 1.000000                                                                
       [SDMIXFDI_P          ] distribution in water column algae type 02                            
       using default value: 1.000000                                                                
       [SDMIXFFL_E          ] distribution in water column algae type 03                            
       using default value: 1.000000                                                                
       [SDMIXGRE_E          ] distribution in water column algae type 04                            
       using default value: 1.000000                                                                
       [SDMIXGRE_N          ] distribution in water column algae type 05                            
       using default value: 1.000000                                                                
       [SDMIXGRE_P          ] distribution in water column algae type 06                            
       using default value: 1.000000                                                                
       [SDMIXAPH_E          ] distribution in water column algae type 07                            
       using default value: 1.000000                                                                
       [SDMIXAPH_N          ] distribution in water column algae type 08                            
       using default value: 1.000000                                                                
       [SDMIXAPH_P          ] distribution in water column algae type 09                            
       using default value: 1.000000                                                                
       [SDMIXOSC_E          ] distribution in water column algae type 10                            
       using default value: 1.000000                                                                
       [SDMIXOSC_N          ] distribution in water column algae type 11                            
       using default value: 1.000000                                                                
       [SDMIXOSC_P          ] distribution in water column algae type 12                            
       using default value: 1.000000                                                                
       [SDMixAlg13          ] distribution in water column algae type 13                            
       using default value: 1.000000                                                                
       [SDMixAlg14          ] distribution in water column algae type 14                            
       using default value: 1.000000                                                                
       [SDMixAlg15          ] distribution in water column algae type 15                            
       using default value: 1.000000                                                                
       [SDMixAlg16          ] distribution in water column algae type 16                            
       using default value: 1.000000                                                                
       [SDMixAlg17          ] distribution in water column algae type 17                            
       using default value: 1.000000                                                                
       [SDMixAlg18          ] distribution in water column algae type 18                            
       using default value: 1.000000                                                                
       [SDMixAlg19          ] distribution in water column algae type 19                            
       using default value: 1.000000                                                                
       [SDMixAlg20          ] distribution in water column algae type 20                            
       using default value: 1.000000                                                                
       [SDMixAlg21          ] distribution in water column algae type 21                            
       using default value: 1.000000                                                                
       [SDMixAlg22          ] distribution in water column algae type 22                            
       using default value: 1.000000                                                                
       [SDMixAlg23          ] distribution in water column algae type 23                            
       using default value: 1.000000                                                                
       [SDMixAlg24          ] distribution in water column algae type 24                            
       using default value: 1.000000                                                                
       [SDMixAlg25          ] distribution in water column algae type 25                            
       using default value: 1.000000                                                                
       [SDMixAlg26          ] distribution in water column algae type 26                            
       using default value: 1.000000                                                                
       [SDMixAlg27          ] distribution in water column algae type 27                            
       using default value: 1.000000                                                                
       [SDMixAlg28          ] distribution in water column algae type 28                            
       using default value: 1.000000                                                                
       [SDMixAlg29          ] distribution in water column algae type 29                            
       using default value: 1.000000                                                                
       [SDMixAlg30          ] distribution in water column algae type 30                            
       using default value: 1.000000                                                                
       [MrtExFDI_E          ] coefficient increased mortality rate algae type 01                    
       using default value:  0.00000                                                                
       [MrtExFDI_P          ] coefficient increased mortality rate algae type 02                    
       using default value:  0.00000                                                                
       [MrtExFFL_E          ] coefficient increased mortality rate algae type 03                    
       using default value:  0.00000                                                                
       [MrtExGRE_E          ] coefficient increased mortality rate algae type 04                    
       using default value:  0.00000                                                                
       [MrtExGRE_N          ] coefficient increased mortality rate algae type 05                    
       using default value:  0.00000                                                                
       [MrtExGRE_P          ] coefficient increased mortality rate algae type 06                    
       using default value:  0.00000                                                                
       [MrtExAPH_E          ] coefficient increased mortality rate algae type 07                    
       using default value:  0.00000                                                                
       [MrtExAPH_N          ] coefficient increased mortality rate algae type 08                    
       using default value:  0.00000                                                                
       [MrtExAPH_P          ] coefficient increased mortality rate algae type 09                    
       using default value:  0.00000                                                                
       [MrtExOSC_E          ] coefficient increased mortality rate algae type 10                    
       using default value:  0.00000                                                                
       [MrtExOSC_N          ] coefficient increased mortality rate algae type 11                    
       using default value:  0.00000                                                                
       [MrtExOSC_P          ] coefficient increased mortality rate algae type 12                    
       using default value:  0.00000                                                                
       [MrtExAlg13          ] coefficient increased mortality rate algae type 13                    
       using default value:  0.00000                                                                
       [MrtExAlg14          ] coefficient increased mortality rate algae type 14                    
       using default value:  0.00000                                                                
       [MrtExAlg15          ] coefficient increased mortality rate algae type 15                    
       using default value:  0.00000                                                                
       [MrtExAlg16          ] coefficient increased mortality rate algae                            
       using default value:  0.00000                                                                
       [MrtExAlg17          ] coefficient increased mortality rate algae                            
       using default value:  0.00000                                                                
       [MrtExAlg18          ] coefficient increased mortality rate algae                            
       using default value:  0.00000                                                                
       [MrtExAlg19          ] coefficient increased mortality rate algae                            
       using default value:  0.00000                                                                
       [MrtExAlg20          ] coefficient increased mortality rate algae                            
       using default value:  0.00000                                                                
       [MrtExAlg21          ] coefficient increased mortality rate algae                            
       using default value:  0.00000                                                                
       [MrtExAlg22          ] coefficient increased mortality rate algae                            
       using default value:  0.00000                                                                
       [MrtExAlg23          ] coefficient increased mortality rate algae                            
       using default value:  0.00000                                                                
       [MrtExAlg24          ] coefficient increased mortality rate algae                            
       using default value:  0.00000                                                                
       [MrtExAlg25          ] coefficient increased mortality rate algae                            
       using default value:  0.00000                                                                
       [MrtExAlg26          ] coefficient increased mortality rate algae                            
       using default value:  0.00000                                                                
       [MrtExAlg27          ] coefficient increased mortality rate algae                            
       using default value:  0.00000                                                                
       [MrtExAlg28          ] coefficient increased mortality rate algae                            
       using default value:  0.00000                                                                
       [MrtExAlg29          ] coefficient increased mortality rate algae                            
       using default value:  0.00000                                                                
       [MrtExAlg30          ] coefficient increased mortality rate algae                            
       using default value:  0.00000                                                                
       [Mort2FDI_E          ] salinity dependent mortality rate algae type 01                       
       using default value:  0.00000                                                                
       [Mort2FDI_P          ] salinity dependent mortality rate algae type 02                       
       using default value:  0.00000                                                                
       [Mort2FFL_E          ] salinity dependent mortality rate algae type 03                       
       using default value:  0.00000                                                                
       [Mort2GRE_E          ] salinity dependent mortality rate algae type 04                       
       using default value:  0.00000                                                                
       [Mort2GRE_N          ] salinity dependent mortality rate algae type 05                       
       using default value:  0.00000                                                                
       [Mort2GRE_P          ] salinity dependent mortality rate algae type 06                       
       using default value:  0.00000                                                                
       [Mort2APH_E          ] salinity dependent mortality rate algae type 07                       
       using default value:  0.00000                                                                
       [Mort2APH_N          ] salinity dependent mortality rate algae type 08                       
       using default value:  0.00000                                                                
       [Mort2APH_P          ] salinity dependent mortality rate algae type 09                       
       using default value:  0.00000                                                                
       [Mort2OSC_E          ] salinity dependent mortality rate algae type 10                       
       using default value:  0.00000                                                                
       [Mort2OSC_N          ] salinity dependent mortality rate algae type 11                       
       using default value:  0.00000                                                                
       [Mort2OSC_P          ] salinity dependent mortality rate algae type 12                       
       using default value:  0.00000                                                                
       [Mort2Alg13          ] salinity dependent mortality rate algae type 13                       
       using default value:  0.00000                                                                
       [Mort2Alg14          ] salinity dependent mortality rate algae type 14                       
       using default value:  0.00000                                                                
       [Mort2Alg15          ] salinity dependent mortality rate algae type 15                       
       using default value:  0.00000                                                                
       [Mort2Alg16          ] salinity dependent mortality rate algae type                          
       using default value:  0.00000                                                                
       [Mort2Alg17          ] salinity dependent mortality rate algae type                          
       using default value:  0.00000                                                                
       [Mort2Alg18          ] salinity dependent mortality rate algae type                          
       using default value:  0.00000                                                                
       [Mort2Alg19          ] salinity dependent mortality rate algae type                          
       using default value:  0.00000                                                                
       [Mort2Alg20          ] salinity dependent mortality rate algae type                          
       using default value:  0.00000                                                                
       [Mort2Alg21          ] salinity dependent mortality rate algae type                          
       using default value:  0.00000                                                                
       [Mort2Alg22          ] salinity dependent mortality rate algae type                          
       using default value:  0.00000                                                                
       [Mort2Alg23          ] salinity dependent mortality rate algae type                          
       using default value:  0.00000                                                                
       [Mort2Alg24          ] salinity dependent mortality rate algae type                          
       using default value:  0.00000                                                                
       [Mort2Alg25          ] salinity dependent mortality rate algae type                          
       using default value:  0.00000                                                                
       [Mort2Alg26          ] salinity dependent mortality rate algae type                          
       using default value:  0.00000                                                                
       [Mort2Alg27          ] salinity dependent mortality rate algae type                          
       using default value:  0.00000                                                                
       [Mort2Alg28          ] salinity dependent mortality rate algae type                          
       using default value:  0.00000                                                                
       [Mort2Alg29          ] salinity dependent mortality rate algae type                          
       using default value:  0.00000                                                                
       [Mort2Alg30          ] salinity dependent mortality rate algae type                          
       using default value:  0.00000                                                                
       [MrtB1FDI_E          ] coefficient b1 in salinity stress funct. algae 01                     
       using default value: 0.200000E-02                                                            
       [MrtB1FDI_P          ] coefficient b1 in salinity stress funct. algae 02                     
       using default value: 0.200000E-02                                                            
       [MrtB1FFL_E          ] coefficient b1 in salinity stress funct. algae 03                     
       using default value: 0.200000E-02                                                            
       [MrtB1GRE_E          ] coefficient b1 in salinity stress funct. algae 04                     
       using default value: 0.200000E-02                                                            
       [MrtB1GRE_N          ] coefficient b1 in salinity stress funct. algae 05                     
       using default value: 0.200000E-02                                                            
       [MrtB1GRE_P          ] coefficient b1 in salinity stress funct. algae 06                     
       using default value: 0.200000E-02                                                            
       [MrtB1APH_E          ] coefficient b1 in salinity stress funct. algae 07                     
       using default value: 0.200000E-02                                                            
       [MrtB1APH_N          ] coefficient b1 in salinity stress funct. algae 08                     
       using default value: 0.200000E-02                                                            
       [MrtB1APH_P          ] coefficient b1 in salinity stress funct. algae 09                     
       using default value: 0.200000E-02                                                            
       [MrtB1OSC_E          ] coefficient b1 in salinity stress funct. algae 10                     
       using default value: 0.200000E-02                                                            
       [MrtB1OSC_N          ] coefficient b1 in salinity stress funct. algae 11                     
       using default value: 0.200000E-02                                                            
       [MrtB1OSC_P          ] coefficient b1 in salinity stress funct. algae 12                     
       using default value: 0.200000E-02                                                            
       [MrtB1Alg13          ] coefficient b1 in salinity stress funct. algae 13                     
       using default value: 0.200000E-02                                                            
       [MrtB1Alg14          ] coefficient b1 in salinity stress funct. algae 14                     
       using default value: 0.200000E-02                                                            
       [MrtB1Alg15          ] coefficient b1 in salinity stress funct. algae 15                     
       using default value: 0.200000E-02                                                            
       [MrtB1Alg16          ] coefficient b1 in salinity stress funct. a                            
       using default value: 0.200000E-02                                                            
       [MrtB1Alg17          ] coefficient b1 in salinity stress funct. a                            
       using default value: 0.200000E-02                                                            
       [MrtB1Alg18          ] coefficient b1 in salinity stress funct. a                            
       using default value: 0.200000E-02                                                            
       [MrtB1Alg19          ] coefficient b1 in salinity stress funct. a                            
       using default value: 0.200000E-02                                                            
       [MrtB1Alg20          ] coefficient b1 in salinity stress funct. a                            
       using default value: 0.200000E-02                                                            
       [MrtB1Alg21          ] coefficient b1 in salinity stress funct. a                            
       using default value: 0.200000E-02                                                            
       [MrtB1Alg22          ] coefficient b1 in salinity stress funct. a                            
       using default value: 0.200000E-02                                                            
       [MrtB1Alg23          ] coefficient b1 in salinity stress funct. a                            
       using default value: 0.200000E-02                                                            
       [MrtB1Alg24          ] coefficient b1 in salinity stress funct. a                            
       using default value: 0.200000E-02                                                            
       [MrtB1Alg25          ] coefficient b1 in salinity stress funct. a                            
       using default value: 0.200000E-02                                                            
       [MrtB1Alg26          ] coefficient b1 in salinity stress funct. a                            
       using default value: 0.200000E-02                                                            
       [MrtB1Alg27          ] coefficient b1 in salinity stress funct. a                            
       using default value: 0.200000E-02                                                            
       [MrtB1Alg28          ] coefficient b1 in salinity stress funct. a                            
       using default value: 0.200000E-02                                                            
       [MrtB1Alg29          ] coefficient b1 in salinity stress funct. a                            
       using default value: 0.200000E-02                                                            
       [MrtB1Alg30          ] coefficient b1 in salinity stress funct. a                            
       using default value: 0.200000E-02                                                            
       [MrtB2FDI_E          ] coefficient b2 in salinity stress funct. algae 01                     
       using default value:  8000.00                                                                
       [MrtB2FDI_P          ] coefficient b2 in salinity stress funct. algae 02                     
       using default value:  8000.00                                                                
       [MrtB2FFL_E          ] coefficient b2 in salinity stress funct. algae 03                     
       using default value:  8000.00                                                                
       [MrtB2GRE_E          ] coefficient b2 in salinity stress funct. algae 04                     
       using default value:  8000.00                                                                
       [MrtB2GRE_N          ] coefficient b2 in salinity stress funct. algae 05                     
       using default value:  8000.00                                                                
       [MrtB2GRE_P          ] coefficient b2 in salinity stress funct. algae 06                     
       using default value:  8000.00                                                                
       [MrtB2APH_E          ] coefficient b2 in salinity stress funct. algae 07                     
       using default value:  8000.00                                                                
       [MrtB2APH_N          ] coefficient b2 in salinity stress funct. algae 08                     
       using default value:  8000.00                                                                
       [MrtB2APH_P          ] coefficient b2 in salinity stress funct. algae 09                     
       using default value:  8000.00                                                                
       [MrtB2OSC_E          ] coefficient b2 in salinity stress funct. algae 10                     
       using default value:  8000.00                                                                
       [MrtB2OSC_N          ] coefficient b2 in salinity stress funct. algae 11                     
       using default value:  8000.00                                                                
       [MrtB2OSC_P          ] coefficient b2 in salinity stress funct. algae 12                     
       using default value:  8000.00                                                                
       [MrtB2Alg13          ] coefficient b2 in salinity stress funct. algae 13                     
       using default value:  8000.00                                                                
       [MrtB2Alg14          ] coefficient b2 in salinity stress funct. algae 14                     
       using default value:  8000.00                                                                
       [MrtB2Alg15          ] coefficient b2 in salinity stress funct. algae 15                     
       using default value:  8000.00                                                                
       [MrtB2Alg16          ] coefficient b2 in salinity stress funct. a                            
       using default value:  8000.00                                                                
       [MrtB2Alg17          ] coefficient b2 in salinity stress funct. a                            
       using default value:  8000.00                                                                
       [MrtB2Alg18          ] coefficient b2 in salinity stress funct. a                            
       using default value:  8000.00                                                                
       [MrtB2Alg19          ] coefficient b2 in salinity stress funct. a                            
       using default value:  8000.00                                                                
       [MrtB2Alg20          ] coefficient b2 in salinity stress funct. a                            
       using default value:  8000.00                                                                
       [MrtB2Alg21          ] coefficient b2 in salinity stress funct. a                            
       using default value:  8000.00                                                                
       [MrtB2Alg22          ] coefficient b2 in salinity stress funct. a                            
       using default value:  8000.00                                                                
       [MrtB2Alg23          ] coefficient b2 in salinity stress funct. a                            
       using default value:  8000.00                                                                
       [MrtB2Alg24          ] coefficient b2 in salinity stress funct. a                            
       using default value:  8000.00                                                                
       [MrtB2Alg25          ] coefficient b2 in salinity stress funct. a                            
       using default value:  8000.00                                                                
       [MrtB2Alg26          ] coefficient b2 in salinity stress funct. a                            
       using default value:  8000.00                                                                
       [MrtB2Alg27          ] coefficient b2 in salinity stress funct. a                            
       using default value:  8000.00                                                                
       [MrtB2Alg28          ] coefficient b2 in salinity stress funct. a                            
       using default value:  8000.00                                                                
       [MrtB2Alg29          ] coefficient b2 in salinity stress funct. a                            
       using default value:  8000.00                                                                
       [MrtB2Alg30          ] coefficient b2 in salinity stress funct. a                            
       using default value:  8000.00                                                                
       [FixFDI_E            ] algae type 01 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixFDI_P            ] algae type 02 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixFFL_E            ] algae type 03 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixGRE_E            ] algae type 04 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixGRE_N            ] algae type 05 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixGRE_P            ] algae type 06 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAPH_E            ] algae type 07 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAPH_N            ] algae type 08 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAPH_P            ] algae type 09 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixOSC_E            ] algae type 10 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixOSC_N            ] algae type 11 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixOSC_P            ] algae type 12 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg13            ] algae type 13 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg14            ] algae type 14 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg15            ] algae type 15 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg16            ] algae type 16 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg17            ] algae type 17 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg18            ] algae type 18 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg19            ] algae type 19 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg20            ] algae type 20 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg21            ] algae type 21 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg22            ] algae type 22 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg23            ] algae type 23 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg24            ] algae type 24 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg25            ] algae type 25 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg26            ] algae type 26 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg27            ] algae type 27 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg28            ] algae type 28 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg29            ] algae type 29 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg30            ] algae type 30 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
                                                                                                    
 Input for [DepAve              ] Average depth for Bloom step                                      
       [SWDepAve            ] switch for module DepAve (0=off, 1=on)                                
       using default value:  0.00000                                                                
       [TimMultBl           ] ratio bloom/delwaq time step                                          
       Using constant nr 14 with value: 1.000000                                                    
       [Depth               ] depth of segment                                                      
       Using output from proces [DynDepth            ]                                              
       [BloomDepth          ] average depth over Bloom time step                                    
       Using output from proces [DepAve              ]                                              
                                                                                                    
 Input for [Daylength           ] Daylength calculation                                             
       [ITIME               ] DELWAQ time                                                           
       Using DELWAQ time                                                                            
       [Latitude            ] latitude of study area                                                
       using default value:  52.1000                                                                
       [RefDay              ] daynumber of reference day simulation                                 
       using default value:  0.00000                                                                
       [AuxSys              ] ratio between days and system clock                                   
       using default value:  86400.0                                                                
                                                                                                    
 Input for [CalcRad             ] Radiation at segment upper and lower boundaries                   
       [ExtVl               ] total extinction coefficient visible light                            
       Using output from proces [Extinc_VLG          ]                                              
       [Depth               ] depth of segment                                                      
       Using output from proces [DynDepth            ]                                              
       [RadSurf             ] irradiation at the water surface                                      
       Using function nr  1                                                                         
       [a_enh               ] enhancement factor in radiation calculation                           
       using default value:  1.50000                                                                
       [Surf                ] horizontal surface area of a DELWAQ segment                           
       Using parameter nr  1                                                                        
       [SwEmersion          ] switch indicating submersion(0) or emersion(1)                        
       using default value:  0.00000                                                                
       [RadBot              ] irradiation at the segment lower-boundary                             
       Using output from proces [CalcRad             ]                                              
                                                                                                    
 Input for [Extinc_VLG          ] Extinction of visible-light (370-680nm) DLWQ-G                    
       [ExtVlIM1            ] VL specific extinction coefficient IM1                                
       using default value: 0.100000E-01                                                            
       [ExtVlIM2            ] VL specific extinction coefficient IM2                                
       using default value: 0.100000E-01                                                            
       [ExtVlIM3            ] VL specific extinction coefficient IM3                                
       using default value: 0.100000E-01                                                            
       [ExtVlPOC1           ] VL specific extinction coefficient POC1                               
       using default value: 0.100000E+00                                                            
       [ExtVlBak            ] background extinction visible light                                   
       Using constant nr 60 with value: 0.800000E-01                                                
       [ExtVlPhyt           ] VL extinction by phytoplankton                                        
       Using output from proces [EXTINABVL           ]                                              
       [ExtVlMacro          ] VL extinction by macrophytes                                          
       using default value:  0.00000                                                                
       [IM1                 ] inorganic matter (IM1)                                                
       using default value:  0.00000                                                                
       [IM2                 ] inorganic matter (IM2)                                                
       using default value:  0.00000                                                                
       [IM3                 ] inorganic matter (IM3)                                                
       using default value:  0.00000                                                                
       [POC1                ] POC1 (fast decomposing fraction)                                      
       Using substance nr   7                                                                       
       [POC2                ] POC2 (medium decomposing fraction)                                    
       using default value:  0.00000                                                                
       [SW_Uitz             ] Extinction by Uitzicht On (1) or Off (0)                              
       using default value:  0.00000                                                                
       [DOC                 ] Dissolved Organic Carbon (DOC)                                        
       using default value:  0.00000                                                                
       [ExtVlDOC            ] VL specific extinction coefficient DOC                                
       using default value:  0.00000                                                                
       [UitZDEPT1           ] Z1 (depth)                                                            
       using default value:  1.20000                                                                
       [UitZDEPT2           ] Z2 (depth)                                                            
       using default value: 1.000000                                                                
       [UitZCORCH           ] CORa correction factor                                                
       using default value:  2.50000                                                                
       [UitZC_DET           ] C3 coeff. absorption ash weight & detritus                            
       using default value: 0.260000E-01                                                            
       [UitZC_GL1           ] C1 coeff. absorption ash weight & detritus                            
       using default value: 0.730000                                                                
       [UitZC_GL2           ] C2 coeff. absorption ash weight & detritus                            
       using default value: 1.000000                                                                
       [UitZHELHM           ] Hel_h constant                                                        
       using default value: 0.140000E-01                                                            
       [UitZTAU             ] Tau constant calculation transparency                                 
       using default value:  7.80000                                                                
       [UitZangle           ] Angle of incidence solar radiation                                    
       using default value:  30.0000                                                                
       [DMCFDetC            ] DM:C ratio DetC                                                       
       using default value:  2.50000                                                                
       [ExtVLSal0           ] extra VL extinction at Salinity = 0                                   
       using default value:  0.00000                                                                
       [Salinity            ] Salinity                                                              
       using default value:  35.0000                                                                
       [SalExt0             ] salinity value for extra extinction = 0                               
       using default value:  34.9200                                                                
       [ExtVlPOC2           ] VL specific extinction coefficient POC2                               
       Using constant nr 61 with value: 0.100000E+00                                                
       [ExtVlPOC3           ] VL specific extinction coefficient POC3                               
       using default value: 0.100000E+00                                                            
       [ExtVlPOC4           ] VL specific extinction coefficient POC4                               
       using default value: 0.100000E+00                                                            
       [POC3                ] POC3 (slow decomposing fraction)                                      
       using default value:  0.00000                                                                
       [POC4                ] POC4 (particulate refractory fraction)                                
       using default value:  0.00000                                                                
                                                                                                    
 Input for [EXTINABVL           ] Extinction of light by algae (Bloom)                              
       [NAlgBloom           ] number of algae types in BLOOM                                        
       using default value:  30.0000                                                                
       [SW_fixin_y          ] switch possible scaling of input, DO NOT EDIT                         
       using default value: 1.000000                                                                
       [Depth               ] depth of segment                                                      
       Using output from proces [DynDepth            ]                                              
       [EXTVLFDI_E          ] VL specific extinction coefficient algae type 01                      
       Using constant nr 41 with value: 0.270000                                                    
       [EXTVLFDI_P          ] VL specific extinction coefficient algae type 02                      
       Using constant nr 42 with value: 0.187500                                                    
       [EXTVLFFL_E          ] VL specific extinction coefficient algae type 03                      
       Using constant nr 43 with value: 0.225000                                                    
       [EXTVLGRE_E          ] VL specific extinction coefficient algae type 04                      
       Using constant nr 44 with value: 0.225000                                                    
       [EXTVLGRE_N          ] VL specific extinction coefficient algae type 05                      
       Using constant nr 45 with value: 0.187500                                                    
       [EXTVLGRE_P          ] VL specific extinction coefficient algae type 06                      
       Using constant nr 46 with value: 0.187500                                                    
       [EXTVLAPH_E          ] VL specific extinction coefficient algae type 07                      
       Using constant nr 47 with value: 0.450000                                                    
       [EXTVLAPH_N          ] VL specific extinction coefficient algae type 08                      
       Using constant nr 48 with value: 0.400000                                                    
       [EXTVLAPH_P          ] VL specific extinction coefficient algae type 09                      
       Using constant nr 49 with value: 0.400000                                                    
       [EXTVLOSC_E          ] VL specific extinction coefficient algae type 10                      
       Using constant nr 50 with value: 0.400000                                                    
       [EXTVLOSC_N          ] VL specific extinction coefficient algae type 11                      
       Using constant nr 51 with value: 0.287500                                                    
       [EXTVLOSC_P          ] VL specific extinction coefficient algae type 12                      
       Using constant nr 52 with value: 0.287500                                                    
       [ExtVlAlg13          ] VL specific extinction coefficient algae type 13                      
       using default value: 0.200000                                                                
       [ExtVlAlg14          ] VL specific extinction coefficient algae type 14                      
       using default value: 0.200000                                                                
       [ExtVlAlg15          ] VL specific extinction coefficient algae type 15                      
       using default value: 0.200000                                                                
       [ExtVlAlg16          ] VL specific extinction coefficient algae ty                           
       using default value: 0.200000                                                                
       [ExtVlAlg17          ] VL specific extinction coefficient algae ty                           
       using default value: 0.200000                                                                
       [ExtVlAlg18          ] VL specific extinction coefficient algae ty                           
       using default value: 0.200000                                                                
       [ExtVlAlg19          ] VL specific extinction coefficient algae ty                           
       using default value: 0.200000                                                                
       [ExtVlAlg20          ] VL specific extinction coefficient algae ty                           
       using default value: 0.200000                                                                
       [ExtVlAlg21          ] VL specific extinction coefficient algae ty                           
       using default value: 0.200000                                                                
       [ExtVlAlg22          ] VL specific extinction coefficient algae ty                           
       using default value: 0.200000                                                                
       [ExtVlAlg23          ] VL specific extinction coefficient algae ty                           
       using default value: 0.200000                                                                
       [ExtVlAlg24          ] VL specific extinction coefficient algae ty                           
       using default value: 0.200000                                                                
       [ExtVlAlg25          ] VL specific extinction coefficient algae ty                           
       using default value: 0.200000                                                                
       [ExtVlAlg26          ] VL specific extinction coefficient algae ty                           
       using default value: 0.200000                                                                
       [ExtVlAlg27          ] VL specific extinction coefficient algae ty                           
       using default value: 0.200000                                                                
       [ExtVlAlg28          ] VL specific extinction coefficient algae ty                           
       using default value: 0.200000                                                                
       [ExtVlAlg29          ] VL specific extinction coefficient algae ty                           
       using default value: 0.200000                                                                
       [ExtVlAlg30          ] VL specific extinction coefficient algae ty                           
       using default value: 0.200000                                                                
       [FDIATOMS_E          ] concentration of algae type 1                                         
       Using substance nr  14                                                                       
       [FDIATOMS_P          ] concentration of algae type 2                                         
       Using substance nr  15                                                                       
       [FFLAGELA            ] concentration of algae type 3                                         
       Using substance nr  16                                                                       
       [GREENS_E            ] concentration of algae type 4                                         
       Using substance nr  17                                                                       
       [GREENS_N            ] concentration of algae type 5                                         
       Using substance nr  18                                                                       
       [GREENS_P            ] concentration of algae type 6                                         
       Using substance nr  19                                                                       
       [APHANIZO_E          ] concentration of algae type 7                                         
       Using substance nr  11                                                                       
       [APHANIZO_N          ] concentration of algae type 8                                         
       Using substance nr  12                                                                       
       [APHANIZO_P          ] concentration of algae type 9                                         
       Using substance nr  13                                                                       
       [OSCILAT_E           ] concentration of algae type 10                                        
       Using substance nr  20                                                                       
       [OSCILAT_N           ] concentration of algae type 11                                        
       Using substance nr  21                                                                       
       [OSCILAT_P           ] concentration of algae type 12                                        
       Using substance nr  22                                                                       
       [BLOOMALG13          ] concentration of algae type 13                                        
       using default value: -101.000                                                                
       [BLOOMALG14          ] concentration of algae type 14                                        
       using default value: -101.000                                                                
       [BLOOMALG15          ] concentration of algae type 15                                        
       using default value: -101.000                                                                
       [BLOOMALG16          ] concentration of algae type 16                                        
       using default value: -101.000                                                                
       [BLOOMALG17          ] concentration of algae type 17                                        
       using default value: -101.000                                                                
       [BLOOMALG18          ] concentration of algae type 18                                        
       using default value: -101.000                                                                
       [BLOOMALG19          ] concentration of algae type 19                                        
       using default value: -101.000                                                                
       [BLOOMALG20          ] concentration of algae type 20                                        
       using default value: -101.000                                                                
       [BLOOMALG21          ] concentration of algae type 21                                        
       using default value: -101.000                                                                
       [BLOOMALG22          ] concentration of algae type 22                                        
       using default value: -101.000                                                                
       [BLOOMALG23          ] concentration of algae type 23                                        
       using default value: -101.000                                                                
       [BLOOMALG24          ] concentration of algae type 24                                        
       using default value: -101.000                                                                
       [BLOOMALG25          ] concentration of algae type 25                                        
       using default value: -101.000                                                                
       [BLOOMALG26          ] concentration of algae type 26                                        
       using default value: -101.000                                                                
       [BLOOMALG27          ] concentration of algae type 27                                        
       using default value: -101.000                                                                
       [BLOOMALG28          ] concentration of algae type 28                                        
       using default value: -101.000                                                                
       [BLOOMALG29          ] concentration of algae type 29                                        
       using default value: -101.000                                                                
       [BLOOMALG30          ] concentration of algae type 30                                        
       using default value: -101.000                                                                
       [FixFDI_E            ] algae type 01 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixFDI_P            ] algae type 02 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixFFL_E            ] algae type 03 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixGRE_E            ] algae type 04 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixGRE_N            ] algae type 05 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixGRE_P            ] algae type 06 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAPH_E            ] algae type 07 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAPH_N            ] algae type 08 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAPH_P            ] algae type 09 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixOSC_E            ] algae type 10 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixOSC_N            ] algae type 11 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixOSC_P            ] algae type 12 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg13            ] algae type 13 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg14            ] algae type 14 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg15            ] algae type 15 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg16            ] algae type 16 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg17            ] algae type 17 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg18            ] algae type 18 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg19            ] algae type 19 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg20            ] algae type 20 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg21            ] algae type 21 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg22            ] algae type 22 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg23            ] algae type 23 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg24            ] algae type 24 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg25            ] algae type 25 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg26            ] algae type 26 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg27            ] algae type 27 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg28            ] algae type 28 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg29            ] algae type 29 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [FixAlg30            ] algae type 30 fixed (0=not app,>0=sus,<0=fixed)                       
       using default value:  0.00000                                                                
       [SDMIXFDI_E          ] distribution in water column algae type 01                            
       using default value: 1.000000                                                                
       [SDMIXFDI_P          ] distribution in water column algae type 02                            
       using default value: 1.000000                                                                
       [SDMIXFFL_E          ] distribution in water column algae type 03                            
       using default value: 1.000000                                                                
       [SDMIXGRE_E          ] distribution in water column algae type 04                            
       using default value: 1.000000                                                                
       [SDMIXGRE_N          ] distribution in water column algae type 05                            
       using default value: 1.000000                                                                
       [SDMIXGRE_P          ] distribution in water column algae type 06                            
       using default value: 1.000000                                                                
       [SDMIXAPH_E          ] distribution in water column algae type 07                            
       using default value: 1.000000                                                                
       [SDMIXAPH_N          ] distribution in water column algae type 08                            
       using default value: 1.000000                                                                
       [SDMIXAPH_P          ] distribution in water column algae type 09                            
       using default value: 1.000000                                                                
       [SDMIXOSC_E          ] distribution in water column algae type 10                            
       using default value: 1.000000                                                                
       [SDMIXOSC_N          ] distribution in water column algae type 11                            
       using default value: 1.000000                                                                
       [SDMIXOSC_P          ] distribution in water column algae type 12                            
       using default value: 1.000000                                                                
       [SDMixAlg13          ] distribution in water column algae type 13                            
       using default value: 1.000000                                                                
       [SDMixAlg14          ] distribution in water column algae type 14                            
       using default value: 1.000000                                                                
       [SDMixAlg15          ] distribution in water column algae type 15                            
       using default value: 1.000000                                                                
       [SDMixAlg16          ] distribution in water column algae type 16                            
       using default value: 1.000000                                                                
       [SDMixAlg17          ] distribution in water column algae type 17                            
       using default value: 1.000000                                                                
       [SDMixAlg18          ] distribution in water column algae type 18                            
       using default value: 1.000000                                                                
       [SDMixAlg19          ] distribution in water column algae type 19                            
       using default value: 1.000000                                                                
       [SDMixAlg20          ] distribution in water column algae type 20                            
       using default value: 1.000000                                                                
       [SDMixAlg21          ] distribution in water column algae type 21                            
       using default value: 1.000000                                                                
       [SDMixAlg22          ] distribution in water column algae type 22                            
       using default value: 1.000000                                                                
       [SDMixAlg23          ] distribution in water column algae type 23                            
       using default value: 1.000000                                                                
       [SDMixAlg24          ] distribution in water column algae type 24                            
       using default value: 1.000000                                                                
       [SDMixAlg25          ] distribution in water column algae type 25                            
       using default value: 1.000000                                                                
       [SDMixAlg26          ] distribution in water column algae type 26                            
       using default value: 1.000000                                                                
       [SDMixAlg27          ] distribution in water column algae type 27                            
       using default value: 1.000000                                                                
       [SDMixAlg28          ] distribution in water column algae type 28                            
       using default value: 1.000000                                                                
       [SDMixAlg29          ] distribution in water column algae type 29                            
       using default value: 1.000000                                                                
       [SDMixAlg30          ] distribution in water column algae type 30                            
       using default value: 1.000000                                                                
                                                                                                    
 Input for [DynDepth            ] dynamic calculation of the depth                                  
       [Volume              ] volume of computational cell                                          
       Using DELWAQ volume                                                                          
       [Surf                ] horizontal surface area of a DELWAQ segment                           
       Using parameter nr  1                                                                        
                                                                                                    
# determining the use of the delwaq input                                       
                                                                                
 info: constant [CLOSE_ERR ] is not used by the proces system                   
 info: constant [NOTHREADS ] is not used by the proces system                   
 info: constant [DRY_THRESH] is not used by the proces system                   
 info: constant [maxiter   ] is not used by the proces system                   
 info: constant [tolerance ] is not used by the proces system                   
 info: constant [iteration ] is not used by the proces system                   
                                                                                
# locating requested output from active processes                                                   
                                                                                                    
 output [TIC                 ] default from [BLOOM               ]                                  
 output [ExtVl               ] from proces [Extinc_VLG]                                             
 output [ExtVlPhyt           ] from proces [EXTINABVL ]                                             
 output [Depth               ] from proces [DynDepth  ]                                             
 output [BloomDepth          ] from proces [DepAve    ]                                             
 output [DayL                ] from proces [Daylength ]                                             
 output [Limit Chlo          ] from proces [BLOOM     ]                                             
 output [Limit nit           ] from proces [BLOOM     ]                                             
 output [Limit pho           ] from proces [BLOOM     ]                                             
 output [Limit sil           ] from proces [BLOOM     ]                                             
 output [Limit e             ] from proces [BLOOM     ]                                             
 output [Limit gro           ] from proces [BLOOM     ]                                             
 output [Limit mor           ] from proces [BLOOM     ]                                             
 output [fPPtot              ] from proces [BLOOM     ]                                             
 output [Phyt                ] from proces [Phy_Blo   ]                                             
 output [AlgN                ] from proces [Phy_Blo   ]                                             
 output [AlgP                ] from proces [Phy_Blo   ]                                             
 output [AlgSi               ] from proces [Phy_Blo   ]                                             
 output [TotN                ] from proces [Compos    ]                                             
 output [KjelN               ] from proces [Compos    ]                                             
 output [TotP                ] from proces [Compos    ]                                             
 output [Chlfa               ] from proces [Phy_Blo   ]                                             
 warning: output [BLOOMGRP            ] not located                                                 
 output [APHANIZO            ] from proces [Phy_Blo   ]                                             
 output [FDIATOMS            ] from proces [Phy_Blo   ]                                             
 output [GREENS              ] from proces [Phy_Blo   ]                                             
 output [OSCILAT             ] from proces [Phy_Blo   ]                                             
                                                                                                    
