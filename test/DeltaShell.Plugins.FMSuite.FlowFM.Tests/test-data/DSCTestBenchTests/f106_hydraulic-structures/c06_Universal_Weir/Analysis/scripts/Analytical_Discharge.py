# -*- coding: utf-8 -*-
"""
Created on Fri Aug  9 13:10:38 2019

@author: Eskedar Gebremedhin 
"""
import pandas as pd 
import numpy as np


Path = r'd:\05_Projects\05_27_FM_testing\testbench\cases\e02_dflowfm\f106_hydraulic-structures\c06_Universal_Weir\Analysis\results'
fn = 'Boundary'

hus = pd.read_excel(Path + '\\' + fn +'.xlsx', sheet_name = 'upstream')
hds  = pd.read_excel(Path + '\\' + fn +'.xlsx', sheet_name = 'downstream')


Ce = 1  # discharge coefficient 
g = 9.81 # Gravity
switch_factor = 0.667
# sections 
section = ['rectangle', 'traingle', 'rectangle', 'traingle', 'rectangle']
W = [2.49, 0.01, 5.0, 0.01, 2.49]
z = [2.5, 2.5, 2.0, 2.0, 2.9, 2.9]

FlowState = []
Flow = []
        
for i in range(0,len(hus)):
        
    h1 = hus.iloc[i,0]
    h2 = hds.iloc[i,0]
    
    if h1>=h2:
        hup = h1 
        hdown = h2
    else:
        hup = h2 
        hdown = h1
        
    Q_total = 0
    for j in range(0,len(section)):
        z_left = z[j] 
        z_right = z[j+1]
        Zs = min(z_left,z_right) # crestlevel 
        
        if section[j] == 'rectangle':
            ml_rec = 2.0/3.0       
           
            if hup < Zs:
                state = 'No Flow'
                Q = 0
            elif (hdown-Zs)/(hup-Zs) <= ml_rec:
                state = 'Free Weir'
                Q = Ce*2.0/3.0*np.sqrt(2.0/3.0*g)*W[j]*(hup-Zs)**(3.0/2.0)  
            else:
                state = 'Submerged Weir'
                Q = Ce*W[j]*(hdown-Zs)*np.sqrt(2*g*(hup-hdown))
                
        elif section[j] == 'traingle':   
            dz = abs(z_left-z_right)
            
            if (hup-Zs) <= 1.25*dz:
                ml_tri = 4.0/5.0
            else:
                ml_tri = (2.0/3.0 + 1.0/6.0*(dz/(hup-Zs)))
                
            if hup < Zs:
                state = 'No Flow'
                Q = 0
            elif (hdown-Zs)/(hup-Zs) <= ml_tri:
                state = 'Free Weir'
                
                u = Ce*np.sqrt(2*g*(1-ml_tri)*(hup-Zs))
                
                if (hup-Zs)/(1/ml_tri) <= dz:
                    A = W[j]*(((ml_tri*(hup-Zs))**2)/(2*dz))
                else:
                    A = W[j]*(((hup-Zs)/(1/ml_tri))-(dz/2))                
                Q = u*A
            else:
                state = 'Submerged Weir'
                
                u = Ce*np.sqrt(2*g*(hup-hdown))
                
                if hdown-Zs <= dz:
                    A = W[j]*(((hdown-Zs)**2)/(2*dz))
                else:
                    A = W[j]*(hdown-Zs - dz/2)                                
                
                Q = u*A
        Q_total += Q    

    if h1 >= h2:
        Q_total = Q_total
    else:
        Q_total = -1*Q_total
                 
    FlowState.append(state)
    Flow.append(float(Q_total))
        

d = {'FlowState': FlowState, 'Flow': Flow}
df = pd.DataFrame(data=d)