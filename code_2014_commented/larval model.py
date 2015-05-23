# -*- coding: utf-8 -*-
"""
Created on Thu May 01 17:40:31 2014

@author: Florian
"""
import numpy as np
import matplotlib.pyplot as plt
import pandas as pd
from operator import add

plt.close('all')

## Load temperature files: only values from 2007 to 2014
df = pd.read_csv("Level_Temp_all.csv",sep=",") # or sep=";" if .csv file is saved manually!
time3 = df.values[:,0]
level3 = df.values[:,1]
temp3 = df.values[:,2]

## larve model fit function
species = {'Vexans': 0, 'Aegypti' : 1, 'Albopictus' : 2}
temp = []
stades = []
L1 = []
L2 = []
L3 = []
L4 = []
#Aedes Vexans
temp0 = []
L10 = []
L20 = []
L30 = []
L40 = []
#Aegypti
temp1 = [15,20,25,27,30,34]
L11 = [86.85,37,34.75,26.19,29.7,28.56] 
L21 = [68.4,32.8,36,19.17,26.7,26.86]
L31 = [97.95,39.6,41.25,27.81,37.2,37.4]
L41 = [128.55,81.2,86.75,72.09,68.7,83.3]

#Albopictus
temp2 = [15,20,25,30,35]
L12 = [84,60,52.5,42,59.5]
L22 = [49.5,28,30,39,42]
L32 = [69,42,30,42,84]
L42 = [201,82,82.5,90,238]
L1 = L12
L2 = map(add,L12,L22)	
L3 = map(add,L2,L32)	
L4 = map(add,L3,L42)	

##All
#temp = [temp0,temp1,temp2]
#L1 = [L10,L11,L12]
#L2 = [L20,L21,L22]
#L3 = [L30,L31,L32]
#L4 = [L40,L41,L42]
#stades = np.array([L1,L2,L3,L4])
stades_albo = np.array([L12,L22,L32,L42])
stades_aegy = np.array([L11,L21,L31,L41])

#coef = []
#for k in range(1,len(temp)):
#    for i in range(4):
#        coef = np.append(coef,np.polyfit(temp[k],stades[i,k],2))
        
coef_albo = []
coef_aegy = []
n1 = np.array([1, 5, 5, 5, 5, 1])
n2 = np.array([1, 5, 5, 5, 1])

for i in range(4):
    coef_aegy = np.append(coef_aegy,np.polyfit(temp1,stades_aegy[i],2,w=np.sqrt(n1)))
for i in range(4):
    coef_albo = np.append(coef_albo,np.polyfit(temp2,stades_albo[i],2,w=np.sqrt(n2)))

TEMP1=np.array(temp1)
TEMP2=np.array(temp2)
yL12 = lambda x: coef_albo[0]*x**2+coef_albo[1]*x+coef_albo[2]
yL22 = lambda x: coef_albo[3]*x**2+coef_albo[4]*x+coef_albo[5]
yL32 = lambda x: coef_albo[6]*x**2+coef_albo[7]*x+coef_albo[8]
yL42 = lambda x: coef_albo[9]*x**2+coef_albo[10]*x+coef_albo[11]
y= lambda x: yL12(x)+yL22(x)+yL32(x)+yL42(x)

yL11 = lambda x: coef_aegy[0]*x**2+coef_aegy[1]*x+coef_aegy[2]
yL21 = lambda x: coef_aegy[3]*x**2+coef_aegy[4]*x+coef_aegy[5]
yL31 = lambda x: coef_aegy[6]*x**2+coef_aegy[7]*x+coef_aegy[8]
yL41 = lambda x: coef_aegy[9]*x**2+coef_aegy[10]*x+coef_aegy[11]
yAlbo= lambda x: yL11(x)+yL21(x)+yL31(x)+yL41(x)

x = np.arange(15,35,0.1)

#plt.figure()
#plt.plot(x,yAegy(x))
#
#plt.figure()
#plt.plot(x,yAlbo(x))

plt.figure() # ALBOPICTUS
plt.plot(temp2,L1,'b--',temp2,L2,'g--',temp2,L3,'y--',temp2,L4,'r--')
plt.hold('on')
plt.plot(x,yL12(x),'b',x,yL12(x)+yL22(x),'g',x,yL12(x)+yL22(x)+yL32(x),'y',x,yL12(x)+yL22(x)+yL32(x)+yL42(x),'r')     
plt.hold('off')
plt.legend(('Seuil L2','Seuil L3','Seuil L4','Seuil Pupe','Approximation seuil L2','Approximation seuil L3','Approximation seuil L4','Approximation seuil Pupe'),loc='best')
#plt.legend(('L1','L2','L3','L4'),loc='best')
plt.title("Temperatures cumulees necessaires pour l'evolution de larves",fontsize = 18)
plt.xlabel("Temperature de l'eau [degC]",fontsize = 14)
plt.ylabel('Degres C * Jours', fontsize= 14)

#plt.figure() # AEGYPTI
#plt.plot(temp1,L11,temp1,L21,temp1,L31,temp1,L41)
#plt.hold('on')
#plt.plot(x,yL11(x),'--',x,yL21(x),'--',x,yL31(x),'--',x,yL41(x),'--')     
#plt.hold('off')
#plt.legend(('L1','L2','L3','L4','Polyfit L1','Polyfit L2','Polyfit L3','Polyfit L4'),loc='best')
##plt.legend(('L1','L2','L3','L4'),loc='best')
#plt.title('Aegypti')

#plt.figure()
#plt.plot(x,yL12(x),'--',TEMP2,L12)     
#plt.title('Polyfit test L1 Albopictus')
#
#plt.figure()
#plt.plot(x,yL22(x),'--',TEMP2,L22)    
#plt.title('Polyfit test L2 Albopictus')  
#
#plt.figure()
#plt.plot(x,yL32(x),'--',TEMP2,L32)     
#plt.title('Polyfit test L3 Albopictus')  
#
#plt.figure()
#plt.plot(x,yL42(x),'--',TEMP2,L42)     
#plt.title('Polyfit test L4 Albopictus')    
    


        
    