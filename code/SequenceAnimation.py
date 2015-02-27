# -*- coding: utf-8 -*-
"""
Created on Mon May 12 21:28:19 2014

@author: Florian
"""
## Prerequis Installer FFMPEG
#Libraires pyplot et matplotlib
import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt
import numpy as np
import matplotlib.animation as manimation
import matplotlib.cbook as cbook
from matplotlib.colors import from_levels_and_colors
#Librairies de lecture de répertoire
import os
from os import listdir
from os.path import isfile, join
execfile('f_rep.py')
#Librairie pour la gestion du temps
import time
import datetime
from datetime import date, timedelta as td

#Suppression des figures déjà ouvertes
plt.close('all')

# lecture des fichiers
path1,seuil_aire = choixrep()
onlyfiles = [ f for f in listdir(path1) if isfile(join(path1,f)) ]
inonde_files = [f for f in onlyfiles if (f.startswith('inonde') & f.endswith('.tif'))]
stade_files = [f for f in onlyfiles if (not (f.startswith('inonde')))&(f.endswith('.tif'))]

#variable
aire_pixel = 9 #m^2
ha_specifie = seuil_aire #ha
oeuf_m2 = 100 #oeufs/metre2
stade_all=('stade L1','stade L2','stade L3','stade L4')
color_value = [255,191,128,64]

start_Date = time.mktime(datetime.datetime.strptime(stade_files[0][0:-4],"%Y-%m-%d").timetuple())
end_Date = time.mktime(datetime.datetime.strptime(stade_files[-1][0:-4],"%Y-%m-%d").timetuple())
start_Date2 = date(int(stade_files[0][0:4]),int(stade_files[0][5:7]),int(stade_files[0][8:10]))
end_Date2 = date(int(stade_files[-1][0:4]),int(stade_files[-1][5:7]),int(stade_files[-1][8:10]))
delta_temps = (end_Date2-start_Date2)
intervall_temps = (end_Date-start_Date)/3600/24
#anomation file encoding
FFMpegWriter = manimation.writers['ffmpeg']
metadata = dict(title='Animation', artist='',
        comment="Animation d'aide à la décision pour enclenchement de la mission de démoustication")
writer = FFMpegWriter(fps=1.5, metadata=metadata)

#create subplot
fig, ((ax1, ax2), (ax3, ax4)) = plt.subplots(2,2)
fig.suptitle('Aide a la decision', fontsize=20)
ax1.axis('off')
ax2.axis('off')
ax4.set_ylabel('Hectares inondes')

cmap, norm = from_levels_and_colors([1,64,128,191,255], ['black','red','yellow','green','blue'], extend='max')
#introduce empty list
aire_inondeeAll = [0]*int(intervall_temps+1)
dateAll = []

#vecteurs de date
for i in range(delta_temps.days + 1):
     dateAll.append((start_Date2 + td(days=i)).strftime("%Y-%m-%d"))

if len(dateAll)>=20:
    ax4.set_xticks(np.arange(0,len(dateAll),int(len(dateAll)/20)))
    ax4.set_xticklabels(dateAll[0::int(len(dateAll)/20)],rotation=70,fontsize = 6)  
else:
    ax4.set_xticks(np.arange(len(dateAll)))
    ax4.set_xticklabels(dateAll, rotation=70, fontsize=6)  
#Création séquence vidéo
path_video = os.path.join(path1,"Sequence_images.mp4")
with writer.saving(fig,path_video, 400):
    for i in stade_files:
        date_inonde = i[0:-4] #date de 
        #figure zones inondées        
        path2 = os.path.join(path1,i)
        image_file = cbook.get_sample_data(path2)
        image1 = plt.imread(image_file)
        ax1.set_title('Zones de stade de developpement le '+ date_inonde, fontsize=11)
        image1plot = ax1.imshow(image1, cmap=cmap, norm=norm)
        # Histogram 
        ax3.set_title('Repartition des differents stade de developpement', fontsize = 11)
        develop_all = [0,0,0,0]
        ax3.cla()
        ax3.set_xlim([-1,4.0])
        ax3.set_ylim([0,100])
        ax3.set_ylabel('Hectares concernes')
        develop_all = [np.sum(image1==jj)*aire_pixel/10000 for jj in color_value]
        print develop_all
        barlist = ax3.bar(np.arange(len(stade_all)),develop_all, align = 'center')   
        ax3.set_xticks(np.arange(len(stade_all)))
        ax3.set_xticklabels(stade_all,fontsize = 8)
        barlist[0].set_color('b')
        barlist[1].set_color('g')
        barlist[2].set_color('yellow')
        barlist[3].set_color('r')    
        if develop_all[2] > ha_specifie:
            ax3.set_title('Alarme 3: traitement imminent',fontsize=9,color='red')
        elif develop_all[1] > ha_specifie:
            ax3.set_title('Alarme 2: reservation helicoptere pour semaine a venir',fontsize=9,color='green')
        elif develop_all[0] > ha_specifie:
            ax3.set_title('Alarme 1: echantillonage sur le terrain',fontsize=9, color='blue')
        
         
        #figure stade de développement
        path3 = 'inonde_' + i
        path4 = os.path.join(path1,path3)
        if os.path.exists(path4):
            image_file2 = cbook.get_sample_data(path4)
            image2 = plt.imread(image_file2)
            ax2.set_title('Zones inondees le '+ date_inonde, fontsize=11)
            image2plot=ax2.imshow(image2)
            image2plot.set_cmap('spectral')
            k = dateAll.index(date_inonde)
            aire_inondeeAll[k] = np.sum(image2)/250*aire_pixel/10000 #arbitrairement la couleur du pixel est 250
            ax4.plot(aire_inondeeAll, '-b')
            ax4.set_title('Aire inondee: ' + str(aire_inondeeAll[k]))
        #write frame
        writer.grab_frame()
        

