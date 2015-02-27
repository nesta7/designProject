# -*- coding: utf-8 -*-
"""
Created on Mon Mar 10 22:16:42 2014

@author: F&A
"""

import numpy as np
import matplotlib.pyplot as plt
#from xlrd import open_workbook
import pandas as pd
#import time
import datetime
#import csv
#import sys
        
execfile('f_fetchData.py')
execfile('f_Database.py')
print '---------------------------NUMPY----------------------------'
f_Database()
print '---------------------------PANDA----------------------------'
plt.close('all')
plotfig = 1 # Plot all figures -> 1
plotsub = 0 # Plot subplots of temperature/level -> 1

# Données initiales
niveau_critique = 674.50 #[m]
temperature_critique = 10 #[degC]

########## Lecture du 2eme Excel pour les autres temperatures ##########
#te = open_workbook('Temp_Eau.xlsx')  
#tempAnnees = te.sheet_names()
#temp_table = pd.read_excel('Temp_Eau.xlsx',tempAnnees[0])

########## LECTURE DU FICHIER DONNEES 2004-2014 (CSV) + ajouts ##########
New_Date = fetchDate()
New_Date = datetime.datetime.strptime(New_Date, '%d.%m.%Y') #To convert to datetime format
New_Date = New_Date.strftime('%d/%m/%Y')
New_Date1 = pd.to_datetime(New_Date, dayfirst = True, infer_datetime_format=True)
New_Level = fetchLevel()
New_Temp = fetchTemp() #Temperature de l'air - Suisse meteo

# LIRE ET ECRASER CSV - HAUTEURS ET TEMPERATURES DE l'EAU
df = pd.read_csv("Level_Temp_all.csv",sep=",") # or sep=";" if .csv file is saved manually!
time3 = df.values[:,0]
level3 = df.values[:,1]
temp3 = df.values[:,2]

if New_Date[:2] != time3[-1][-11:10]: # Check if entry already added (so no duplicates)
    print "**** NEW ENTRY: LEVEL - Level_Temp_all.csv file changes size ****"
    TIME_string = np.append(time3,New_Date)
    TIME_date = pd.to_datetime(TIME_string, dayfirst = True, infer_datetime_format=True)
    LEVEL = np.append(level3,New_Level)
    TEMP = np.append(temp3,NaN)  
    df2 = pd.DataFrame({'Date': TIME_date, 'Level [m]': LEVEL, 'Temperature of water [degC]' : TEMP})
    df2.to_csv("Level_Temp_all.csv",index=False)
else:
    print "**** SAME DATE - No addition of level ****"
    TIME_string = time3
    TIME_date = pd.to_datetime(time3, dayfirst = True, infer_datetime_format=True)
    LEVEL = level3
    TEMP = temp3
    

# LIRE ET ECRASER CSV - TEMPERATURES DE L'AIR    
dfair = pd.read_csv("New_Air.csv",sep=",") # or sep=";" if .csv file is saved manually!
time_air = dfair.values[:,0]
temp_air_max = dfair.values[:,1]
temp_air_mean = dfair.values[:,2]
temp_air_min = dfair.values[:,3]
if New_Date[:2] != time_air[-1][-11:10]:
    print "**** NEW ENTRY: TEMPERATURE OF AIR - New_Air.csv file changes size\n****"
    time_air1 = np.append(time_air,New_Date)
    time_air2 = pd.to_datetime(time_air1, dayfirst = True, infer_datetime_format=True)
    temp_air_min1 = np.append(temp_air_min,New_Temp[0])
    temp_air_max1 = np.append(temp_air_max,New_Temp[1])
    temp_air_mean1 = np.append(temp_air_mean, mean(New_Temp))
    dfair = pd.DataFrame({'Date': time_air2, 'Temp Air max [degC]': temp_air_max1, 'Temp Air mean [degC]': temp_air_mean1, 'Temp Air min [degC]': temp_air_min1})
    dfair.to_csv("New_Air.csv",index=False)
else:
    print "**** SAME DATE - No addition of air temperature ****\n"
    time_air2 = pd.to_datetime(time_air, dayfirst = True, infer_datetime_format=True)
    temp_air_mean1 = mean(New_Temp)
#
#
hcrit = [niveau_critique]*len(TIME_string)
tcrit = [temperature_critique]*len(TIME_string)

# TRAITEMENT PAR LUTHY (cf. RAPMOUST 2002-2012)
#Traitement = ['09-06-2000','15-06-2000','15-07-2000','18-06-2001','23-07-2001','08-06-2004','22-06-2004','10-05-2005','05-08-2005','04-05-2006','15-05-2006',
#              '23-05-2006','14-08-2006','06-07-2007','23-06-2008','21-07-2008','19-06-2009','07-06-2010',
#              '25-06-2010','09-08-2010','24-06-2011','29-07-2011','20-06-2012'] #Pas de traitemnent en 2002,2003
Traitement = ['08-06-2004','22-06-2004','10-05-2005','05-08-2005','04-05-2006','15-05-2006',
              '23-05-2006','14-08-2006','06-07-2007','23-06-2008','21-07-2008','19-06-2009','07-06-2010',
              '25-06-2010','09-08-2010','24-06-2011','29-07-2011','20-06-2012','07-06-2013','12-06-2013'] #Pas de traitemnent en 2002,2003

Traitement_string= [i.replace('-','/') for i in Traitement] 
Traitement_date = pd.to_datetime(Traitement, format='%d-%m-%Y')              
Level_Traitement=[]

#for i in range(len(TIME_string)):
#    for j in range(len(Traitement_string)):
#        if TIME_string[i] == Traitement_string[j]:
#            Level_Traitement.append(LEVEL[i])
for i in range(len(TIME_string)):
    for j in range(len(Traitement_string)):
        if TIME_date[i] == Traitement_date[j]:
            Level_Traitement.append(LEVEL[i])

# PLOTS OF DATA
if plotfig == 1:  
    
    #PLOT OF ALL DATA in subplots
    print "**** Plotting subplot of all data ****"
    plt.subplot(2, 1, 1)
    plt.plot(TIME_date, LEVEL, '-b',TIME_date, hcrit,'--r')
    plt.title('Evolution du niveau du lac')
    #plt.xlabel('Jours')
    plt.ylabel('Niveau du lac [m]')
    plt.subplot(2, 1, 2)
    plt.plot(TIME_date,TEMP,'-g',TIME_date,tcrit,'--r')
    plt.title("Evolution de la temperature du lac")
    #plt.xlabel("Jours")
    plt.ylabel("Temperature du lac [degC]")
    plt.hold(True)
    
    startY =0
    a=-1
    t1=[]
    l1=[]
    
    #INDIVIDUAL PLOTS PER YEAR with treatment
    print "**** Plotting individual graphs ****"
    for i in range(len(TIME_string)-1):
        for j in range(len(Traitement_string)):
            if TIME_date[i] == Traitement_date[j]:
                a+=1
                t1.append(Traitement_date[a])
                l1.append(Level_Traitement[a])
        if TIME_string[i][0:4] != TIME_string[i+1][0:4]:
            plt.figure()
            plt.plot(TIME_date[startY:i],LEVEL[startY:i],'-',TIME_date[startY:i],hcrit[startY:i],'--')
            plt.xlabel("Jours annee")
            plt.ylabel("Niveau du lac [m]")
            plt.title("Evolution du niveau du Lac")
            plt.ylim(673,678)
            ax = plt.axes()
            for k in range(len(t1)):
                ax.arrow(t1[k], l1[k]+0.75, 0, -0.5, head_width=3, head_length=0.25, fc='k', ec='r')
                
            if plotsub == 1:
                plt.figure()
                plt.subplot(2, 1, 1)
                plt.plot(TIME_date[startY:i],LEVEL[startY:i],'-b',TIME_date[startY:i],hcrit[startY:i],'--r')
                plt.title('Evolution du niveau du lac')
                #plt.xlabel('Jours')
                plt.ylabel('Niveau du lac [m]')
                plt.ylim(673,678)
                plt.subplot(2, 1, 2)
                plt.plot(TIME_date[startY:i],TEMP[startY:i],'-g',TIME_date[startY:i],tcrit[startY:i],'--r')
                plt.title("Evolution de la temperature du lac")
                #plt.xlabel("Jours")
                plt.ylabel("Temperature du lac [degC]")
            startY = i+1
            t1 =[]
            l1 =[]
            
        #PLOT of 2014 DATA
        if i == len(TIME_string)-2:
            plt.figure()
            plt.plot(TIME_date[3653:i+2],LEVEL[3653:i+2],'-',TIME_date[3653:i+2],hcrit[3653:i+2],'--')
            plt.xlabel("Jours annee")
            plt.ylabel("Niveau du lac [m]")
            plt.title("Evolution du niveau du Lac en 2014")
            plt.ylim(660,678)

# ANALYSE DES PICS
#for e in range(len(level2)-1):
#    if level2[e]<level2[e+1] and level2[e] >= niveau_critique:
#        print time2[e], level2[e]
#        hauteur_max += [level2[e]]

# DEVELOPPEMENT DES LARVES?
# Paramètre Aire inondee, Date, Zone ensoleillee (0 ou 1)
# Array qui sauvegarde l'aire et sa date
# Variable qui compte le stade de dvlpt de chaque aire
#Time_L = 2 # 2 à 3 jours dépendant du