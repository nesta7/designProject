# -*- coding: utf-8 -*-
"""
Created on Mon May 12 21:28:19 2014

@author: Florian
"""
#Libraires pyplot et matplotlib
import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt
import numpy as np
import matplotlib.cbook as cbook
#Librairies de lecture de répertoire
import os
from os import listdir
from os.path import isfile, join
#Librairie pour la gestion du temps
import time
import datetime
from datetime import date, timedelta as td

#Suppression des figures déjà ouvertes
plt.close('all')

# lecture des fichiers
execfile('f_rep.py')
path1,seuil_aire = choixrep()
onlyfiles = [ f for f in listdir(path1) if isfile(join(path1,f)) ]
inonde_files = [f for f in onlyfiles if (f.startswith('inonde') & f.endswith('.tif'))]
stade_files = [f for f in onlyfiles if (not (f.startswith('inonde')))&(f.endswith('.tif'))]

#variable
aire_pixel = 9 #m^2
oeuf_m2 = 100 #oeufs/metre2
stade_all=('stade L1','stade L2','stade L3','stade L4')
color_value = [255,191,128,64]

start_Date = time.mktime(datetime.datetime.strptime(stade_files[0][0:-4],"%Y-%m-%d").timetuple())
end_Date = time.mktime(datetime.datetime.strptime(stade_files[-1][0:-4],"%Y-%m-%d").timetuple())
start_Date2 = date(int(stade_files[0][0:4]),int(stade_files[0][5:7]),int(stade_files[0][8:10]))
end_Date2 = date(int(stade_files[-1][0:4]),int(stade_files[-1][5:7]),int(stade_files[-1][8:10]))
delta_temps = (end_Date2-start_Date2)
intervall_temps = (end_Date-start_Date)/3600/24

#introduce empty list
allData = []
aire_inondeeAll = [0]*int(intervall_temps+1)
dateAll = []

#vecteurs de date
for i in range(delta_temps.days + 1):
     dateAll.append((start_Date2 + td(days=i)).strftime("%Y-%m-%d"))
L1 = [0]*int(intervall_temps+1)
L2 = [0]*int(intervall_temps+1)
L3 = [0]*int(intervall_temps+1)
L4 = [0]*int(intervall_temps+1)
#Création séquence vidéo
for i in stade_files:
    date_inonde = i[0:-4] #date
    k = dateAll.index(date_inonde)
    #figure zones inondées        
    path2 = os.path.join(path1,i)
    image_file = cbook.get_sample_data(path2)
    image1 = plt.imread(image_file)
    # Histogram 
    develop_all = [0,0,0,0]
    develop_all = [np.sum(image1==jj)*aire_pixel/10000 for jj in color_value]
    L1[k] = develop_all[0]
    L2[k] = develop_all[1]
    L3[k] = develop_all[2]
    L4[k] = develop_all[3]
    print develop_all   
    #figure stade de développement
    path3 = 'inonde_' + i
    path4 = os.path.join(path1,path3)
    if os.path.exists(path4):
        image_file2 = cbook.get_sample_data(path4)
        image2 = plt.imread(image_file2)
        aire_inondeeAll[k] = np.sum(image2)/250*aire_pixel/10000 #arbitrairement la couleur du pixel est 250

allData = np.transpose([dateAll,L1, L2, L3, L4, aire_inondeeAll])
pathsave = os.path.join(os.path.abspath(path1),'resultats.csv')
np.savetxt(pathsave,allData, delimiter=',',fmt="%s",header="Date,Aire L1 [ha],Aire L2 [ha],Aire L3 [ha],Aire L4 [ha],aire_inondeAll [ha]",newline='\n',comments='')    
