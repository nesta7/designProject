# -*- coding: utf-8 -*-
'''
## ***********************************************
## Importation des modules
## ***********************************************
'''

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

'''
## ***********************************************
## Definitons des variables globales
## ***********************************************
'''

#definition des differents chemins d'accÃÂÃÂ¨s aux dossiers

pathProject=QgsProject.instance().readPath(".")
pathCode=os.path.join(pathProject,'code')
pathDonnee=os.path.join(pathProject,'donnees')
pathGeneration=os.path.join(pathDonnee,'generees')
pathMNT= os.path.join(pathDonnee,'MNT')
pathHabitat=os.path.join(pathDonnee,'habitat')

#Nom des fichiers des donnees a extraire
nameMNT='MNT_broc.tif'
nameVegetation='habitat_raster3.tif'

'''
## *********************************************************
## Scripts de definitions de fonctions
## *********************************************************

Execution des differents scripts de definition de fonctions
'''

os.chdir(pathCode)
#Fonctions d'importation de données
execfile('f_fetchData.py')
#Fonctions de creations d'images raster
execfile('f_array2raster.py')
#Fonctions du modele de croissance des larves
execfile('f_ModeleLarve.py')

'''
## *******************************
## Fonction de simulation
## *******************************
'''

def situationDate(startDate, endDate = fetchDate(),timestep = 1, nvCritique=674.5, airePixel = 9):
    '''
    Fonction narrant la situation d'inondation, Enregistrement des zones de cohortes
        Entrer les dates en 'jj/mm/aaaa'

        INPUT :     startDate   date de dÃÂÃÂ©but d'analyse
                    endDate     date de fin d'analyse
                    timestep    interval de simulation [m] DEFAUT : 1
                    nvCritique  niveau auquel les premiÃÂÃÂ¨res zones sont inondÃÂÃÂ©es [m.s.m] DEFAUT : 674.5
                    airePixel   surface d'un pixel du MNT [m] DEFAUT : 9

        OUTPUT :
    '''
    #Couleur des zones L1, L2, L3, L4
    col_L1 = 255
    col_L2 = 190
    col_L3 = 127
    col_L4 = 63
    '''
    #creation de dossier
    project_dir = QgsProject.instance().readPath("./")
    #Pour windows
    #pathGeneration = os.path.join(project_dir, '.\donnees\generees')
    #UNIX
    pathGeneration = os.path.join(project_dir, './donnees/generees')

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
    dir_name = os.path.join(pathDonnee,dossier2)
    dir_name_ori = dir_name #WTF?
    k = 0
    while os.path.isdir(dir_name):
        k +=1
        dir_name = dir_name_ori + '(' + str(k) + ')'

    os.makedirs(dir_name)
    os.chdir(dir_name)

    '''
    ## *************************************************************
    ## Initialisation des donnees de NIVEAU et de TEMPERATURE du lac
    ## *************************************************************

    Lecture du fichier CSV contenant les mesures de hauteur
    et de temperature (Rossens) du lac
    '''

    ''' INTEGRATION FUTURE
    cette partie devra etre adaptee avec les previsions hydrologique et Temperature
    '''
    csvPath = os.path.join(os.path.join(QgsProject.instance().readPath("./"),'code'),"Hauteur_Temp.csv")
    csvContent = csv.reader(open(csvPath, 'rb'))
    a=False
    b=True
    listAll = []
    listDate = []
    listLevel = []
    listTemp = []
    #transformation des dates de debut et de fin
    startDate = startDate[-4:]+'-'+startDate[3:5]+'-'+startDate[:2]
    endDate = endDate[-4:]+'-'+endDate[3:5]+'-'+endDate[:2]
    for row in csvContent:
        if row[0][:10] == startDate:
            a=True
        if a & b:
            listAll.append(row)
            listDate.append(row[0])
            listLevel.append(row[1])
            listTemp.append(row[2])
        if row[0][:10] == endDate:
            b = False

    # Creation des images
    niveau_hier = 0
    os.chdir(pathProject)

    '''
    ## ********************************************************
    ## Initialisation des donnees du MNT
    ## ********************************************************
    '''

    fileMNT=os.path.join(pathMNT,nameMNT)

    if not QgsRasterLayer(fileMNT).isValid():
        print('La couche MNT '+ nameMNT +' n''a pas pu etre chargee !')
    ds1 = gdal.Open(fileMNT)
    band1 = ds1.GetRasterBand(1)
    nodata = band1.GetNoDataValue()
    array1 = band1.ReadAsArray()
    #INUTILE => array1.size EQUIVALENT et MEME TAILLE!!
    size_array1 = np.shape(array1)

    '''
    ## *******************************************************
    ## Initialisation des donnees raster de VEGETATION
    ## *******************************************************
    '''

    fileVegetation = os.path.join(pathHabitat,nameVegetation)

    if not QgsRasterLayer(fileVegetation).isValid():
        print 'La couche de Vegetation '+ nameVegetation +' n''a pas pu etre chargee !'

    ds1_vege = gdal.Open(fileVegetation)
    band1_vege = ds1_vege.GetRasterBand(1)
    nodataVegetation = 0
    array1_vege = band1_vege.ReadAsArray()

    '''
    ## *************************************
    ## Initialisation des listes et vecteurs
    ## *************************************
    '''

    my_timestamp = 0
    ##enregistre les cohortes devant etre supprimees lorsquelles sont passees au stade adulte
    ##MO : Pas tout compris ?
    arrayToRemove = []
    arrayTemp = []
    arrayAltitude = []
    arrayAll = []
    arrayTimestampAll = []
    arrayDegCumulAll = []
    arrayMoyTempAll = []
    arrayL11=np.array(np.zeros(size_array1), dtype = 'float32')
    arrayL22=np.array(np.zeros(size_array1), dtype = 'float32')
    arrayL33=np.array(np.zeros(size_array1), dtype = 'float32')
    arrayL44=np.array(np.zeros(size_array1), dtype = 'float32')
    arrayAA=np.array(np.ones(size_array1), dtype = 'float32')
    arrayInnonde=np.array(np.ones(size_array1), dtype = 'float32')

    '''
    ## ********************************************************************
    ## Boucle Principale de simulation
    ## ********************************************************************
    '''

    for i in range(0, len(listDate), timestep):
        flagDraw = False
        #Condition debut avril a fin septembre
        #MO : cette condition ne devrait pas se trouver la a mon avis mais on devrait pretraiter la liste des dates
        if (float(listDate[i][5:7]) < 10) & (float(listDate[i][5:7]) >3):
            try:
                float(listTemp[i])
            except ValueError:
                listTemp[i] = listTemp[i-1]
                print "Pas de valeur de temperature pour le jour : " + listDate[i][:9]
            arrayTemp.append(float(listTemp[i])) ## ajout des temperatures du csv dans la liste de temperature
            arrayAltitude.append(float(listLevel[i]))## ajout de la hauteur d'eau du csv dans la liste des hauteurs d'eau
            niveau_aujourdhui = float(listLevel[i])
            #Mise a jour des temperatures cumulees pour la journee a venir
            if len(arrayTimestampAll) != 0: ##ajout des donnÃÂÃÂ©es de temps si certains stades de dÃÂÃÂ©vellopement existent
                arrayInnonde = 250*((array1 > nvCritique) & (array1 != nodata) & (array1_vege != nodataVegetation) & (array1 < niveau_aujourdhui)) ## creation des zones inondees si alt(zone)<niveau eau, zone dans le MNT, zone dans la zone d'habitat
                #nom et chemin du raster innondation
                #MO : pas compris ce que c'est ils n'apparaissent nul part !
                tail2 = 'inonde_'+listDate[i][:10] + '.tif'
                dir_name3 = os.path.join(pathGeneration,dir_name)
                newRasterfn4_mid2 = os.path.join(dir_name3,tail2)
                newRasterfn42=os.path.abspath(newRasterfn4_mid2)
                #print newRasterfn42 #DBG
                array2raster(newRasterfn42,fileMNT,arrayInnonde) ##transformation des tableau en raster
                #ajout des degres_cumules et temperatures moyennes
                arrayDegCumulAll = [x+arrayTemp[-1] for x in arrayDegCumulAll] ##actualisation du nombre de degres cumulÃÂÃÂÃÂÃÂÃÂÃÂÃÂÃÂ©s
                arrayMoyTempAll = [arrayDegCumulAll[x]/(my_timestamp-arrayTimestampAll[x]) for x in range(len(arrayDegCumulAll))] ##actualisation des temperatures moyennes pour une zone
            #Datum ÃÂÃÂ  674.5
            if niveau_hier < nvCritique :
                niveau_hier = nvCritique
            #verification nouvelles zones inondees
            if (niveau_hier < niveau_aujourdhui) & (niveau_aujourdhui == max(arrayAltitude[-15:])):
                array = 1*((array1 > niveau_hier) & (array1 != nodata) &  (array1 < niveau_aujourdhui) & (array1_vege != nodataVegetation))
                arrayAll.append(array)
                arrayTimestampAll.append(my_timestamp)
                if isinstance(arrayTemp[-1],float):
                    arrayDegCumulAll.append(arrayTemp[-1]) ##ajout d'une nouvelle cohorte ayant les memes degres-jours
                    arrayMoyTempAll.append(arrayTemp[-1])
                else:
                    arrayDegCumulAll.append(15) ## valeur arbitraire
                    arrayMoyTempAll.append(15)
                #Calcul aire touchee par eclosion
                #aire_eclosion = array4.sum() * airePixel
             # Version degres-jours
            for k in range(len(arrayTimestampAll)):
                if fModeleLarve(arrayDegCumulAll[k], arrayMoyTempAll[k])==1: ##Condition L1
                    arrayL11 += arrayAll[k]
                    flagDraw = True
                if fModeleLarve(arrayDegCumulAll[k], arrayMoyTempAll[k])==2:##Condition L2
                    arrayL22 += arrayAll[k]
                    flagDraw = True
                if fModeleLarve(arrayDegCumulAll[k], arrayMoyTempAll[k])==3:##Condition L3
                    arrayL33 += arrayAll[k]
                    flagDraw = True
                if fModeleLarve(arrayDegCumulAll[k], arrayMoyTempAll[k])==4:##Condition L4
                    arrayL44 += arrayAll[k]
                    flagDraw = True
                if fModeleLarve(arrayDegCumulAll[k], arrayMoyTempAll[k])>4:##Condition aerien
                    arrayAA += arrayAll[k]
                    arrayToRemove.append(k)
                    #print arrayToRemove
            #print(arrayAll[k])
            ## Version simplifiee avec le temps uniquement
            # for k in range(len(arrayTimestampAll)):
                # if (my_timestamp-arrayTimestampAll[k]<4): ##Condition L1
                    # arrayL11 += arrayAll[k]
                    # flagDraw = True
                # if ((my_timestamp-arrayTimestampAll[k]<7) & (my_timestamp-arrayTimestampAll[k]>3)):##Condition L2
                    # arrayL22 += arrayAll[k]
                    # flagDraw = True
                # if (my_timestamp-arrayTimestampAll[k]<10) & (my_timestamp-arrayTimestampAll[k]>6):##Condition L3
                    # arrayL33 += arrayAll[k]
                    # flagDraw = True
                # if (my_timestamp-arrayTimestampAll[k]<13)&(my_timestamp-arrayTimestampAll[k]>9):##Condition L4
                    # arrayL44 += arrayAll[k]
                    # flagDraw = True
                # if (my_timestamp-arrayTimestampAll[k]>12):##Condition aerien
                    # arrayAA += arrayAll[k]

            array_L1 = col_L1 * arrayL11
            array_L2 = col_L2 * arrayL22
            array_L3 = col_L3 * arrayL33
            array_L4 = col_L4 * arrayL44
            array_A = 0 * arrayAA
            array4 = array_L1 + array_L2 + array_L3 + array_L4 + arrayAA
            niveau_hier = niveau_aujourdhui

            #nom et chemin du raster Phase
            tail1 = listDate[i][:10] + '.tif'
            dir_name3 = os.path.join(pathGeneration,dir_name)
            newRasterfn4_mid = os.path.join(dir_name3,tail1)
            newRasterfn4=os.path.abspath(newRasterfn4_mid)
            #print newRasterfn4
            #ecriture raster
            if flagDraw:
                array2raster(newRasterfn4,fileMNT,array4)
            flagDraw = False
            #reinitialisation des zones L1-L2-L3-L4-inonde
            arrayL11=np.array(np.zeros(size_array1), dtype = 'float32')
            arrayL22=np.array(np.zeros(size_array1), dtype = 'float32')
            arrayL33=np.array(np.zeros(size_array1), dtype = 'float32')
            arrayL44=np.array(np.zeros(size_array1), dtype = 'float32')
            arrayAA=np.array(np.ones(size_array1), dtype = 'float32')
            arrayInnonde=np.array(np.ones(size_array1), dtype = 'float32')

            #suppression des parametres pour cohorte adulte
            #print arrayTimestampAll
            for kk in reversed(arrayToRemove):
                del arrayTimestampAll[kk]
                del arrayMoyTempAll[kk]
                del arrayDegCumulAll[kk]
            arrayToRemove = []
        else:
            if len(arrayAltitude) > 0:
                #reinitialisation pour nouvelle annee
                arrayToRemove = []
                arrayAltitude = []
                arrayDegCumulAll = []
                arrayMoyTempAll = []
                arrayTemp = []
                arrayAll = []
                flag_inonde = False
                arrayTimestampAll = []
        my_timestamp += 1
