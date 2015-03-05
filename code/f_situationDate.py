# -*- coding: utf-8 -*-
'''
## ***********************************************
## Importation des modules
## ***********************************************
'''
#MAX: fonctions SIG de base
from qgis.core import *
#MAX: peut etre un outil pour accéder à l'aide...
import qgis.utils

#MAX: interface pour contrôler des fichiers et dossiers (en ce qui concerne leurs modifications)
from PyQt4.QtCore import QFileSystemWatcher
#MAX: QFileInfo provides information about a file's name and position (path) in the file system,its access rights and whether
#it is a directory or symbolic link, etc. The file's size and last modified/read times are also available.
#The QSettings class provides persistent platform-independent application settings.
from PyQt4.QtCore import QFileInfo,QSettings

#MAX: The OS module in Python provides a way of using operating system dependent    functionality
#MAX: le module osr semble être lié à la projection
import os, osr

#MAX: permet la conversion de rasters
from qgis.analysis import QgsRasterCalculator, QgsRasterCalculatorEntry

#MAX: Command line raster calculator with numpy syntax. Use any basic arithmetic supported
#by numpy arrays such as +-*\ along with logical operators such as >. Note that all files must
#have the same dimensions, but no projection checking is performed
import gdal_calc

#MAX: GDAL is a translator library for raster and vector geospatial data formats that is released under an X/MIT style
#Open Source license by the Open Source Geospatial Foundation. As a library, it presents a single raster abstract data model
#and vector abstract data model to the calling application for all supported formats. It also comes with a variety of useful
#commandline utilities for data translation and processing.
from osgeo import gdal
from osgeo.gdalnumeric import *
from osgeo.gdalconst import *

#MAX: NumPy is the fundamental package for scientific computing with Python. It contains among other things:
#a powerful N-dimensional array object
#sophisticated (broadcasting) functions
#tools for integrating C/C++ and Fortran code
#useful linear algebra, Fourier transform, and random number capabilities
import numpy as np

## libraries pour lire le niveau d'eau sur le site du groupe e

#MAX: The re module was added in Python 1.5, and provides Perl-style regular expression patterns.
#Regular expressions (called REs, or regexes, or regex patterns) are essentially a tiny, highly specialized programming
#language embedded inside Python and made available through the re module. Using this little language, you specify the
#rules for the set of possible strings that you want to match; this set might contain English sentences, or e-mail addresses,
#or TeX commands, or anything you like. You can then ask questions such as “Does this string match the pattern?”, or “Is there
#a match for the pattern anywhere in this string?”. You can also use REs to modify a string or to split it apart in various ways.
import re
#MAX: Open a network object denoted by a URL for reading.
from urllib import urlopen
#MAX: The string module contains a number of useful constants and classes, as well as some deprecated legacy functions that
#are also available as methods on strings.
import string
#MAX: The time module exposes C library functions for manipulating dates and times. Since it is tied to the underlying C
#implementation, some details (such as the start of the epoch and maximum date value supported) are platform-specific.
import time
#MAX: The datetime module supplies classes for manipulating dates and times in both simple and complex ways. While date
#and time arithmetic is supported, the focus of the implementation is on efficient attribute extraction for output formatting and
#manipulation.
import datetime
#MAX: The csv module implements classes to read and write tabular data in CSV format. It allows programmers to say, “write
#this data in the format preferred by Excel,” or “read data from this file which was generated by Excel,” without knowing the precise
#details of the CSV format used by Excel.
import csv

##Pour tester le type d'OS : peut-etre inutile
import platform
typeOS=platform.system()

'''
## ***********************************************
## Definiton des variables globales
## ***********************************************
'''

#definition des differents chemins d'acces aux dossiers
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
## Execution des Scripts de definitions de fonctions
## *********************************************************
'''

os.chdir(pathCode)
#Fonctions d'importation de données
execfile('f_fetchData.py')
#Fonctions de creations d'images raster
execfile('f_array2raster.py')
#Fonctions du modele de croissance des larves
execfile('f_ModeleLarve.py')

'''
## ******************************************************
## Fonction de simulation
## ******************************************************
'''
#MAX: Est-ce que ça fait vraiment sens d'avoir timestep, nvCritique et airePixel dans les arguments de la fonction?
def situationDate(startDate, endDate = fetchDate(),timestep = 1, nvCritique=674.5, airePixel = 9):
    '''
    Fonction narrant la situation d'inondation, Enregistrement des zones de cohortes
        Entrer les dates précisées en 'jj/mm/aaaa'

        INPUT :     startDate   date de debut d'analyse
                    endDate     date de fin d'analyse
                    timestep    interval de simulation [m] DEFAUT : 1
                    nvCritique  niveau auquel les premieres zones sont inondees [m.s.m] DEFAUT : 674.5
                    airePixel   surface d'un pixel du MNT [m] DEFAUT : 9

        OUTPUT :
    '''
    #Couleur des zones L1, L2, L3, L4
    col_L1 = 255
    col_L2 = 190
    col_L3 = 127
    col_L4 = 63

    #MAX: Est-ce que le bloc de commentaire suivant est à enlever?
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
    #MAX: changement d'emplacement => dossier de donnees generees.
    os.chdir(pathGeneration)
    #MAX: ecriture de l'emplacement des donnees generees d'aujourd'hui
    dossier = fetchDate()
    dossier2 = 'situation_' +dossier[-4:]+dossier[3:5]+dossier[:2]
    dir_name = os.path.join(pathGeneration,dossier2)
    dir_name_ori = dir_name
    k = 0

    #MAX: si un dossier de donnees generees a deja ete cree aujourdhui, il faut en creer un nouveau avec un (1) a la
    #fin de son nom. S'il en existe deja un avec un 1 a la fin, il faut en creer un nouveau avec un (2) a la fin , etc
    while os.path.isdir(dir_name):
        k +=1
        dir_name = dir_name_ori + '(' + str(k) + ')'
    #MAX: creation du dossier dans lequel se trouveront les donnees generees par ce programme aujourdhui
    os.makedirs(dir_name)
    #MAX: changement d'emplacement => dossier de donnees generees d'ajourd'hui
    os.chdir(dir_name)

    '''
    ## -------------------------------------------------------------------------------------------------
    ## Initialisation des donnees de NIVEAU et de TEMPERATURE du lac
    ## -------------------------------------------------------------------------------------------------

    Lecture du fichier CSV contenant les mesures de hauteur
    et de temperature (Rossens) du lac
    '''

    ''' INTEGRATION FUTURE
    cette partie devra etre adaptee avec les previsions hydrologique et Temperature
    '''
    #MAX: definition du chemin pour acceder au fichier des hauteurs du lac et des temperatures
    csvPath = os.path.join(os.path.join(QgsProject.instance().readPath("./"),'code'),"Hauteur_Temp.csv")

    #MAX: dans 'rb', 'r' signifie "reading", ce qui veut dire que l'utilisateur peut lire le fichier, et 'b' signifie qu'on ouvre un fichier binaire.
    #MAX: Il permet que le fichier soit lu en tant que tel.

    #MAX: liaison entre le contenu du fichier et la variable csvContent (je crois qu'il s'agit d'un pointeur
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
    #MAX: Ajout du contenu de Hauteurs_Temp.csv etant compris entre les dates specifiees par l'utilisateur aux differentes listes
    #definies ci dessus
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

    # Creation des images      MAX: ah oui?
    #MAX: initialisation de la variable niveau_hier pour la première itération de la boucle principale
    niveau_hier = 0
    #MAX: et ca? Pourquoi aller dans le dossier du projet???
    os.chdir(pathProject)

    '''
    ## -------------------------------------------------------------------------------------------------
    ## Initialisation des donnees du MNT
    ## -------------------------------------------------------------------------------------------------
    '''
    #MAX: Definition de l'emplacement du fichier designe par nameMNT
    fileMNT=os.path.join(pathMNT,nameMNT)

    #Definition du message d'erreur a generer si le fichier designe par fileMNT n'existe pas
    if not QgsRasterLayer(fileMNT).isValid():
        print('La couche MNT '+ nameMNT +' n''a pas pu etre chargee !')
    #MAX: ouverture du fichier designe par nameMNT
    ds1 = gdal.Open(fileMNT)
    #MAX: definition of the 1st band object
    band1 = ds1.GetRasterBand(1)
    #MAX: identification of the no data value for this band. The no data value for a band is generally a special marker value used
    #MAX: to mark pixels that are not valid data. Such pixels should generally not be displayed, nor contribute to analysis operations.
    nodata = band1.GetNoDataValue()
    #MAX: Read the data into a 2D Numeric array
    array1 = band1.ReadAsArray()

    #INUTILE => array1.size EQUIVALENT et MEME TAILLE!!
    size_array1 = np.shape(array1)

    '''
    ## -------------------------------------------------------------------------------------------------
    ## Initialisation des donnees raster de VEGETATION
    ## -------------------------------------------------------------------------------------------------
    '''
    #MAX: Definition de l'emplacement du fichier designe par nameVegetation
    fileVegetation = os.path.join(pathHabitat,nameVegetation)

    #Definition du message d'erreur a generer si le fichier designe par fileVegetation n'existe pas
    if not QgsRasterLayer(fileVegetation).isValid():
        print 'La couche de Vegetation '+ nameVegetation +' n''a pas pu etre chargee !'
    #MAX: ouverture du fichier designe par nameVegetation
    ds1_vege = gdal.Open(fileVegetation)
    #MAX: definition of the 1st band object
    band1_vege = ds1_vege.GetRasterBand(1)
    #MAX: identification of the no data value for this band. Dans le cas de la vegetation, tous les pixels ayant la valeur 0 sont
    #des pixels ou il n'y a pas de donnees
    nodataVegetation = 0
    #MAX: Read the data into a 2D Numeric array
    array1_vege = band1_vege.ReadAsArray()

    '''
    ## -------------------------------------------------------------------------------------------------
    ## Initialisation des listes et vecteurs
    ## -------------------------------------------------------------------------------------------------
    '''

    my_timestamp = 0
    ##enregistre les cohortes devant etre supprimees lorsquelles sont passees au stade adulte
    ##MO : Pas tout compris ? MAX: Moi non plus...
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
    ## -------------------------------------------------------------------------------------------------
    ## Boucle Principale de simulation
    ## -------------------------------------------------------------------------------------------------
    '''
    #MAX: La boucle suivante parcourt la periode selectionnee par l'utilisateur, avec un pas de temps de timestep (jours)
    for i in range(0, len(listDate), timestep):
        #MAX: Aucune idee de ce a quoi ca sert!
        flagDraw = False
        #Condition debut avril a fin septembre
        #MO : cette condition ne devrait pas se trouver la a mon avis mais on devrait pretraiter la liste des dates
        if (float(listDate[i][5:7]) < 10) & (float(listDate[i][5:7]) >3):
            try:
                float(listTemp[i])
            except ValueError:
                #MAX: cette condition n'est jamais remplie, puisque lorsque listTemp[i] n'existe pas, ce n'est pas une erreur qui est renvoyée...
                listTemp[i] = listTemp[i-1]
                print "Pas de valeur de temperature pour le jour : " + listDate[i][:9]
            arrayTemp.append(float(listTemp[i])) ## ajout des temperatures du csv dans la liste de temperature
            arrayAltitude.append(float(listLevel[i]))## ajout de la hauteur d'eau du csv dans la liste des hauteurs d'eau
            niveau_aujourdhui = float(listLevel[i])
            #Mise a jour des temperatures cumulees pour la journee a venir
            if len(arrayTimestampAll) != 0: ##ajout des donnÃÂÃÂ©es de temps si certains stades de dÃÂÃÂ©vellopement existent
                #MAX: coloration des pixels représentant le sol (situé dans la zone d'habitat, dans le MNT et en dessus du niveau critique) inonde.
                arrayInnonde = 250*((array1 > nvCritique) & (array1 != nodata) & (array1_vege != nodataVegetation) & (array1 < niveau_aujourdhui))
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
            #MAX: la deuxième partie de la condition permet probablement de déterminer si le niveau d'aujourdhui est plus élevé que le niveau des 15 jours précédents
            if (niveau_hier < niveau_aujourdhui) & (niveau_aujourdhui == max(arrayAltitude[-15:])):
                #MAX: definition de la zone nouvellement inondee (entre hier et ajourd'hui)
                array = 1*((array1 > niveau_hier) & (array1 != nodata) &  (array1 < niveau_aujourdhui) & (array1_vege != nodataVegetation))
                arrayAll.append(array)
                arrayTimestampAll.append(my_timestamp)
                #MAX: cette condition permet de voir s'il y a une donnee de temperature le jour meme (le jour meme?...)
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

            #MAX: assignation de la couleur correspondant au stade larvaire ayant actuellement lieu à la surface actuellement inondee...?
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
