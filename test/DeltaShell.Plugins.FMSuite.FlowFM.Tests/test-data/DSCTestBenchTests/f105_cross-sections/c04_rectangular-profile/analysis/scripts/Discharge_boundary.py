# -*- coding: utf-8 -*-
"""
Created on Fri May 31 14:26:03 2019

@author: Eskedar Gebremedhin 
"""
import numpy as np 
import pandas as pd 

# A function to calculate discharge of each branch with various roughness type 

def Q_T(w,h,roughness,roughness_type,S):
    
    A = w*h
    P = w + 2*h
        
#    import pdb; pdb.set_trace()     
    R = A/P         
    if roughness_type == 'C': # chezy 
        C = roughness
    elif roughness_type == 'n': # manning
        C = (R**(1/6))/roughness
    elif roughness_type == 'ks': # Strickler
        C = (R**(1/6))*roughness
    elif roughness_type == 'kn_delft3d':# White-Colebrook Delft3D style (not implemented yet)
        g = 9.81
        e = 2.7183
        kappa = 0.41
        Z0 = min(roughness/30,0.3*R)
        C = np.sqrt(g)*(np.log(R/(e*Z0)))/kappa
    elif roughness_type == 'kn_WAQUA': # White-Colebrook WAQUA style 
        C = 18*np.log10(12*max(0.5,(R/roughness)))
    elif roughness_type == 'kn_ks': # White-Colebrook Strickler style 
        C = 25*((R/roughness)**(1/6))
    elif roughness_type == 'γ': # Bos-Bijkerk gamma
        # 1.5m is the maximum water depth on the channel 
        C = roughness*1.5**(1/3)*R**(1/6)
  
    K = A*C*np.sqrt(R)  # hydraulic conveyance 
    Q = K*np.sqrt(S)   # discharge 
    return Q
 
# model variables 
# Model variables    
Z_up = 5.0
Z_dw = 0     
x_dw = 0
x_up = 20000
dx = 50
H = 6.5
Bedlevel = 5

# slope of the channel 
S = (Z_dw-Z_up)/(x_dw-x_up) 

# water level boundary condition
corr_sg = dx/2*S # D-Flow FM 
DS_WL=H-Bedlevel-corr_sg
# Discharge calculation per segment (Sum will be the upsteram boundary condition )

h = 1.5
w = 4

roughness = [45,0.05, 40, 0.4, 0.3, 0.1, 30, 0.025, 36]
roughness_type = ['C','n','ks','kn_WAQUA','kn_WAQUA','kn_ks','γ', 'n', 'γ']


All_Q = []

for i in range(len(roughness)):
    Q = (Q_T(w,h, roughness[i],roughness_type[i], S))
    All_Q.append(Q)

    
#Store all branchs' discharge in a dataframe
cols = ['T1', 'T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'T8', 'T9']
df_Q = pd.DataFrame([All_Q], columns=cols)     

#df_Q .to_excel("boundary.xlsx")


