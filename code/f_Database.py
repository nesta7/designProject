# -*- coding: utf-8 -*-
"""
Created on Fri May 16 16:10:58 2014

@author: Akio
"""

import numpy as np
import datetime

execfile('f_fetchData.py') # Appeler fonction f_fetchData

def f_Database():
    '''Fonction d'entrée dans les fichiers csv. Permet d'ajouter les nouvelles hauteurs d'eau et températures (eau et air) dans les fichers csv'''
    ################################# CHERCHER NOUVELLES DONNEES #################################
    New_Date = fetchDate() # Date de la donnée
    New_Date1 = datetime.datetime.strptime(New_Date, '%d.%m.%Y') # Convertir en format datetime
    New_Date2 = New_Date1.strftime('%Y-%m-%d 00:00:00') # Convertir en format YYYY-MM-DD 00:00:00
    New_Level = fetchLevel() # Niveau de l'eau (Groupe E)
    New_Temp = fetchTemp() # Temperature de l'air (Suisse meteo)
    
    ################################# AJOUTER NIVEAUX DU LAC #################################    
    data = np.loadtxt('Hauteur_Temp.csv',dtype=np.str,delimiter=',',skiprows=0) # ATTENTION: SI FICHIER .CSV MANUELEMENT MODIFIE, CHANGER à delimiter=";" if .csv ! Puis remettre delimiter="," après un lancement
    
    Time=[]; Level=[]; Temp=[]
    for i in range(len(data)-1): # Permet de prendre tous les données du fichier .csv précédent
        Time = np.append(Time,data[i+1][0])
        Level = np.append(Level,data[i+1][1])
        Temp = np.append(Temp,data[i+1][2])
        
    if New_Date2 != Time[-1]: # Donnée pas encore ajoutée -> Ajouter sinon rien
        print "*** Nouvelle entrée: Hauteur de l'eau - Hauteur_Temp.csv modifié ***"
        Time = np.append(Time,New_Date2)
        Level = np.append(Level,New_Level)
        Temp = np.append(Temp,'')
        All = np.transpose([Time,Level,Temp])
        np.savetxt('Hauteur_Temp.csv',All, delimiter=',',fmt="%s",header="Date,Level [m],Temperature of water [degc]",newline='\n',comments='') # or delimiter=";" if .csv file is saved manually!
    else:
        print "*** Même date - Pas de nouvelle entrée pour la hauteur d'eau ***"
    
    ################################# AJOUTER TEMPERATURE DE L'AIR #################################    
    data2 = np.loadtxt('Temp_air.csv',dtype=np.str,delimiter=',',skiprows=0) # or delimiter=";" if .csv file is saved manually!
       
    Time1=[];Temp_max=[];Temp_mean=[];Temp_min=[]
    
    for i in range(len(data2)-1):
        Time1 = np.append(Time1,data2[i+1][0])
        Temp_max = np.append(Temp_max,data2[i+1][1])
        Temp_mean = np.append(Temp_mean,data2[i+1][2])
        Temp_min = np.append(Temp_min,data2[i+1][3])
        
    if New_Date2 != Time1[-1]:
        print "*** Nouvelle entrée: Température de l'air - Temp_air.csv file modifié ***"
        Time1 = np.append(Time1,New_Date2)
        Temp_max = np.append(Temp_max,New_Temp[0])
        Temp_mean = np.append(Temp_mean,New_Temp[1])
        Temp_min = np.append(Temp_min, mean(New_Temp))
        All1 = np.transpose([Time1,Temp_max,Temp_mean,Temp_min])
        np.savetxt('Temp_air.csv',All1, delimiter=',',fmt="%s",header="Date,Temp air minimum [degC],Temp air maximum [degC],Temp air mean [degC]",
                   newline='\n',comments='') # or delimiter=";" if .csv file is saved manually!
    else:
        print "*** Même date - Pas de nouvelle entrée pour la température de l'air ***"