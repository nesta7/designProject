# -*- coding: utf-8 -*-
from qgis.core import *
import qgis.utils

from PyQt4.QtCore import QFileSystemWatcher
from PyQt4.QtCore import QFileInfo,QSettings

import os, osr

from qgis.analysis import QgsRasterCalculator, QgsRasterCalculatorEntry
import gdal_calc

from osgeo import gdal
from osgeo.gdalnumeric import *
from osgeo.gdalconst import *
##libraries pour numpy
import numpy as np
## libraries pour lire le niveau d'eau sur le site du groupe e
import re
from urllib import urlopen
import string
import time
import datetime
import csv
#Pour tester le type d'OS : peut-etre inutile
import platform
typeOS=platform.system()
#definition des differents chemins

pathProject=QgsProject.instance().readPath(".")
pathCode=os.path.join(pathProject,'code')
pathDonnee=os.path.join(pathProject,'donnees')
pathGeneration=os.path.join(pathDonnee,'generees')
#Fonctions

os.chdir(pathCode)
#os.chdir('/Users/Akio/Desktop/Dropbox/Design2014/code')

#ExÃÂ©cute les diffÃÂ©rents fichiers liÃÂ©s (dÃÂ©finitions de fonctions)
execfile('f_fetchData.py')
execfile('f_array2raster.py')
execfile('f_ModeleLarve.py')

def situationDate(startDate, endDate = fetchDate(),timestep = 1, niveau_critique=674.5, aire_pixel = 9):
    ''' Fonction narrant la situation d'inondation, Enregistrement des zones de cohortes
            Entrer les dates en 'jj/mm/aaaa' '''
    #Couleur des zones L1, L2, L3, L4
    col_L1 = 255
    col_L2 = 190
    col_L3 = 127
    col_L4 = 63
    '''
    #crÃÂ©ation de dossier
    project_dir = QgsProject.instance().readPath("./")
    #Pour windows
    #dossier_dir = os.path.join(project_dir, '.\donnees\generees')
    #UNIX
    dossier_dir = os.path.join(project_dir, './donnees/generees')

    os.chdir(QgsProject.instance().readPath("./"))
    dir_name ='.'
    #    dir_name = '/Users/Akio/Desktop/Dropbox/Design2014/donnees/generees'

    #Windows
    #os.chdir( '.\donnees\generees')
    #Unix
    os.chdir('./donnees/generees')
    '''
    os.chdir(pathGeneration)
    dossier = fetchDate()
    dossier2 = 'situation_' +dossier[-4:]+dossier[3:5]+dossier[:2]
    dir_name = os.path.join(dir_name,dossier2)
    dir_name_ori = dir_name #WTF?
    k = 0
    while os.path.isdir(dir_name):
        k +=1
        l = str(k)
        dir_name = dir_name_ori + '(' + l + ')'
    dir_name2=os.path.relpath(dir_name)
    os.makedirs(dir_name2)
    os.chdir(dir_name2)

    '''
    ## *************************************************************
    ## Initialisation des donnees de NIVEAU et de TEMPERATURE du lac
    ## *************************************************************

    Lecture du fichier CSV contenant les mesures de hauteur 
    et de tempÃÂ©rature (Rossens) du lac
    '''
    
    csvPath = os.path.join(os.path.join(QgsProject.instance().readPath("./"),'code'),"Hauteur_Temp.csv")
    csvContent = csv.reader(open(csvPath, 'rb'))
    a=False
    b=True
    list_all = []
    list_date = []
    list_level = []
    list_temp = []
    startDate2 = startDate[-4:]+'-'+startDate[3:5]+'-'+startDate[:2]
    endDate2 = endDate[-4:]+'-'+endDate[3:5]+'-'+endDate[:2]
    for row in csvContent:
        if row[0][:10] == startDate2:
            a=True
        if a & b:
            list_all.append(row)
            list_date.append(row[0])
            list_level.append(row[1])
            list_temp.append(row[2])
        if row[0][:10] == endDate2:
            b = False

    # Creation des images 
    niveau_hier = 0
    os.chdir(QgsProject.instance().readPath("./"))
    rasterfn = os.path.relpath("./donnees/MNT/MNT_broc.tif")
#    rasterfn = os.path.abspath("Users/Akio/Desktop/Dropbox/Design2014/donnees/SIG-Broc/MNT/MNT_broc.tif")
    '''
    ## *********************************
    ## Initialisation des donnees du MNT
    ## *********************************
    '''
    ds1 = gdal.Open(rasterfn)
    band1 = ds1.GetRasterBand(1)
    nodata = band1.GetNoDataValue()
    array1 = band1.ReadAsArray()
    #INUTILE => array1.size EQUIVALENT et MEME TAILLE!!
    size_array1 = np.shape(array1)
    
    '''
    ## ***********************************************
    ## Initialisation des donnÃÂ©es raster de VEGETATION
    ## ***********************************************
    '''
    
    fileName2 = os.path.relpath("./donnees/habitat/habitat_raster3.tif")
#    fileName2 = os.path.abspath("Users/Akio/Desktop/Dropbox/Design2014/donnees/habitat/habitat_raster3.tif")
    baseName2 = os.path.basename(fileName2)
    rlayer_vege = QgsRasterLayer(fileName2, baseName2) # EPSG: 21781
    if not rlayer_vege.isValid():
        print "Layer vegetation failed to load!"
    ds1_vege = gdal.Open(fileName2)
    band1_vege = ds1_vege.GetRasterBand(1)
    nodata_vege = 0
    array1_vege = band1_vege.ReadAsArray()
    
    ## *************************************
    ## Initialisation des listes et vecteurs
    ## *************************************
    
    my_timestamp = 0
    ##enregistre les cohortes devant ÃÂªtre supprimÃÂ©es lorsquelles sont passees au stade adulte
    ##Pas tout compris ?
    to_del = [] 
    temp_list = []
    haut_list = []
    array_all = []
    timestamp_all = []
    deg_cumul_all = []
    moy_temp_all = []
    array_L11=np.array(np.zeros(size_array1), dtype = 'float32')
    array_L22=np.array(np.zeros(size_array1), dtype = 'float32')
    array_L33=np.array(np.zeros(size_array1), dtype = 'float32')
    array_L44=np.array(np.zeros(size_array1), dtype = 'float32')
    array_AA=np.array(np.ones(size_array1), dtype = 'float32')
    array_inonde=np.array(np.ones(size_array1), dtype = 'float32')
    
    #*******************************
    #Boucle Principale de simulation
    #*******************************
    
    for i in range(0, len(list_date), timestep):
        flagDraw = False
        #Condition début avril à fin septembre
        if (float(list_date[i][5:7]) < 10) & (float(list_date[i][5:7]) >3):
            try:
                float(list_temp[i])
            except ValueError:
                list_temp[i] = list_temp[i-1]
                print "Pas de valeur de température pour le jour: " + list_date[i][:9]
            temp_list.append(float(list_temp[i])) ## ajout des temperatures du csv dans la liste de temperature
            haut_list.append(float(list_level[i]))## ajout de la hauteur d'eau du csv dans la liste des hauteurs d'eau
            niveau_aujourdhui = float(list_level[i])
            #Mise à  jour des temperatures cumulees pour la journee a venir
            if len(timestamp_all) != 0: ##ajout des données de temps si certains stades de dÃÂ©veloppement existent
                array_inonde = 250*((array1 > niveau_critique) & (array1 != nodata) & (array1_vege != nodata_vege) & (array1 < niveau_aujourdhui)) ## creation des zones inondees si alt(zone)<niveau eau, zone dans le MNT, zone dans la zone d'habitat
                #nom et chemin du raster inondation
                tail2 = 'inonde_'+list_date[i][:10] + '.tif'
                dir_name3 = os.path.join(dossier_dir,dir_name2)
                newRasterfn4_mid2 = os.path.join(dir_name3,tail2)
                newRasterfn42=os.path.abspath(newRasterfn4_mid2)
                #print newRasterfn42 #DBG
                array2raster(newRasterfn42,rasterfn,array_inonde) ##transformation des tableau en raster
                #ajout des degres_cumules et temperatures moyennes
                deg_cumul_all = [x+temp_list[-1] for x in deg_cumul_all] ##actualisation du nombre de degres cumulÃÂ©s
                moy_temp_all = [deg_cumul_all[x]/(my_timestamp-timestamp_all[x]) for x in range(len(deg_cumul_all))] ##actualisation des temperatures moyennes pour une zone
            #Datum à 674.5
            if niveau_hier < niveau_critique :
                niveau_hier = niveau_critique
            #verification nouvelles zones inondees
            if (niveau_hier < niveau_aujourdhui) & (niveau_aujourdhui == max(haut_list[-15:])): 
                array = 1*((array1 > niveau_hier) & (array1 != nodata) &  (array1 < niveau_aujourdhui) & (array1_vege != nodata_vege))
                array_all.append(array)
                timestamp_all.append(my_timestamp)
                if isinstance(temp_list[-1],float):
                    deg_cumul_all.append(temp_list[-1]) ##ajout d'une nouvelle cohorte ayant les mÃÂªmes degres-jours
                    moy_temp_all.append(temp_list[-1])
                else:
                    deg_cumul_all.append(15) ## valeur arbitraire
                    moy_temp_all.append(15)
                #Calcul aire touchee par eclosion
                #aire_eclosion = array4.sum() * aire_pixel
             # Version degres-jours
            for k in range(len(timestamp_all)):
                if fModeleLarve(deg_cumul_all[k], moy_temp_all[k])==1: ##Condition L1
                    array_L11 += array_all[k]
                    flagDraw = True
                if fModeleLarve(deg_cumul_all[k], moy_temp_all[k])==2:##Condition L2
                    array_L22 += array_all[k]
                    flagDraw = True
                if fModeleLarve(deg_cumul_all[k], moy_temp_all[k])==3:##Condition L3
                    array_L33 += array_all[k]
                    flagDraw = True
                if fModeleLarve(deg_cumul_all[k], moy_temp_all[k])==4:##Condition L4
                    array_L44 += array_all[k]
                    flagDraw = True
                if fModeleLarve(deg_cumul_all[k], moy_temp_all[k])>4:##Condition aerien
                    array_AA += array_all[k]
                    to_del.append(k)
                    #print to_del
            
            ## Version simplifiÃÂ©e avec le temps uniquement
            # for k in range(len(timestamp_all)):
                # if (my_timestamp-timestamp_all[k]<4): ##Condition L1
                    # array_L11 += array_all[k]
                    # flagDraw = True
                # if ((my_timestamp-timestamp_all[k]<7) & (my_timestamp-timestamp_all[k]>3)):##Condition L2
                    # array_L22 += array_all[k]
                    # flagDraw = True
                # if (my_timestamp-timestamp_all[k]<10) & (my_timestamp-timestamp_all[k]>6):##Condition L3
                    # array_L33 += array_all[k]
                    # flagDraw = True
                # if (my_timestamp-timestamp_all[k]<13)&(my_timestamp-timestamp_all[k]>9):##Condition L4
                    # array_L44 += array_all[k]
                    # flagDraw = True
                # if (my_timestamp-timestamp_all[k]>12):##Condition aerien
                    # array_AA += array_all[k]
                    
            array_L1 = col_L1 * array_L11
            array_L2 = col_L2 * array_L22
            array_L3 = col_L3 * array_L33
            array_L4 = col_L4 * array_L44
            array_A = 0 * array_AA
            array4 = array_L1 + array_L2 + array_L3 + array_L4 + array_AA
            niveau_hier = niveau_aujourdhui
            
            #nom et chemin du raster Phase
            tail1 = list_date[i][:10] + '.tif'
            dir_name3 = os.path.join(dossier_dir,dir_name2)
            newRasterfn4_mid = os.path.join(dir_name3,tail1)
            newRasterfn4=os.path.abspath(newRasterfn4_mid)
            #print newRasterfn4
            #ecriture raster
            if flagDraw:
                array2raster(newRasterfn4,rasterfn,array4)
            flagDraw = False
            #reinitialisation des zones L1-L2-L3-L4-inonde
            array_L11=np.array(np.zeros(size_array1), dtype = 'float32')
            array_L22=np.array(np.zeros(size_array1), dtype = 'float32')
            array_L33=np.array(np.zeros(size_array1), dtype = 'float32')
            array_L44=np.array(np.zeros(size_array1), dtype = 'float32')
            array_AA=np.array(np.ones(size_array1), dtype = 'float32')
            array_inonde=np.array(np.ones(size_array1), dtype = 'float32')

            #suppression des parametres pour cohorte adulte
            #print timestamp_all
            for kk in reversed(to_del):
                del timestamp_all[kk]
                del moy_temp_all[kk]
                del deg_cumul_all[kk]
            to_del = []
        else:
            if len(haut_list) > 0:
                #reinitialisation pour nouvelle annee
                to_del = []
                haut_list = []
                deg_cumul_all = []
                moy_temp_all = []
                temp_list = []
                array_all = []
                flag_inonde = False
                timestamp_all = []
        my_timestamp += 1
        
        
