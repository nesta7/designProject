# -*- coding: utf-8 -*-
"""
Created on Sun May 11 15:35:02 2014

@author: Akio
"""


import numpy as np

def fModeleLarve(deg_cumul, moy_temp,model=1): # VALID FOR TEMPERATURE BETWEEN 15 and 35 deg !
    if model == 1:     
######### ALBOPICTUS - Model = 1 #########
        temp = [15,20,25,30,35]
        L1 = [84,60,52.5,42,59.5]
        L2 = [49.5,28,30,39,42]
        L3 = [69,42,30,42,84]
        L4 = [201,82,82.5,90,238]
        stades = np.array([L1,L2,L3,L4])
        coef = []
        n = np.array([1, 5, 5, 5, 1])
        
        for i in range(4): # 4 stades: L1 L2 L3 L4
#            coef = np.append(coef,np.polyfit(temp,stades[i],2,w=np.sqrt(n)))
            coef = np.append(coef,np.polyfit(temp,stades[i],2))
    
######### AEGYPTI - model = 2 #########
    if model == 2:
        temp = [15,20,25,27,30,34]
        L1 = [86.85,37,34.75,26.19,29.7,28.56] 
        L2 = [68.4,32.8,36,19.17,26.7,26.86]
        L3 = [97.95,39.6,41.25,27.81,37.2,37.4]
        L4 = [128.55,81.2,86.75,72.09,68.7,83.3]		
        stades = np.array([L1,L2,L3,L4])
        coef = []
        n = np.array([1, 5, 5, 5, 5, 1])
    
        for i in range(4):
#            coef = np.append(coef,np.polyfit(temp,stades[i],2,w=np.sqrt(n)))
            coef = np.append(coef,np.polyfit(temp,stades[i],2))
    
######### RESULTS #########
    yL11 = lambda x: coef[0]*x**2+coef[1]*x+coef[2]
    yL21 = lambda x: coef[3]*x**2+coef[4]*x+coef[5]
    yL31 = lambda x: coef[6]*x**2+coef[7]*x+coef[8]
    yL41 = lambda x: coef[9]*x**2+coef[10]*x+coef[11]
    
    yL1= lambda x: yL11(x)
    yL2= lambda x: yL11(x)+yL21(x)
    yL3= lambda x: yL11(x)+yL21(x)+yL31(x)
    yL4= lambda x: yL11(x)+yL21(x)+yL31(x)+yL41(x)    
    
    if deg_cumul >= yL4(moy_temp):
        return 5
    elif deg_cumul >= yL3(moy_temp):
        return 4
    elif deg_cumul >= yL2(moy_temp):
        return 3
    elif deg_cumul >= yL1(moy_temp):
        return 2
    else:
        return 1
