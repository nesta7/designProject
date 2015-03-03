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

#Fonctions
os.chdir('C:\Users\Florian\Dropbox\Design2014\donnees')
os.chdir('..\code')
execfile('0test_fetch.py')

def array2raster(newRasterfn,rasterfn,array):
    raster = gdal.Open(rasterfn)
    geotransform = raster.GetGeoTransform()
    originX = geotransform[0]
    originY = geotransform[3]
    pixelWidth = geotransform[1]
    pixelHeight = geotransform[5]
    cols = array.shape[1]
    rows = array.shape[0]
    driver = gdal.GetDriverByName('GTiff')
    outRaster = driver.Create(newRasterfn, cols, rows, 1, gdal.GDT_Byte)
    outRaster.SetGeoTransform((originX, pixelWidth, 0, originY, 0, pixelHeight))
    outband = outRaster.GetRasterBand(1)
    outband.SetNoDataValue(0)
    outband.WriteArray(array)
    outRasterSRS = osr.SpatialReference()
    outRasterSRS.ImportFromWkt(raster.GetProjectionRef())
    outRaster.SetProjection(outRasterSRS.ExportToWkt())
    outband.FlushCache()

def situationDate(startDate, endDate = fetchDate(),timestep = 1):
    ''' Fonction narrant la situation d'inondation, Enregistrement des zones de cohortes
            Entrer les dates en 'jj/mm/aaaa' '''
    #Couleur des zones L1, L2, L3, L4
    col_L1 = 255
    col_L2 = 190
    col_L3 = 127
    col_L4 = 63
    #création de dossier
    dir_name = 'C:\Users\Florian\Dropbox\Design2014\donnees\generees'
    os.chdir(dir_name)
    dossier = fetchDate()
    dossier2 = 'situation' +dossier[-4:]+dossier[3:5]+dossier[:2]
    dir_name = os.path.join(dir_name,dossier2)
    dir_name_ori = dir_name
    k = 0
    while os.path.isdir(dir_name):
        k +=1
        l = str(k)
        dir_name = dir_name_ori + '(' + l + ')'
    dir_name2=os.path.abspath(dir_name)
    os.makedirs(dir_name2)
    os.chdir(dir_name2)
    
    #lire CSV
    csv_path = os.path.abspath('C:\Users\Florian\Dropbox\Design2014\code')
    name_csv = "Hauteur_Temp_All.csv"
    file_csv = os.path.join(csv_path, name_csv)
    csv_file = csv.reader(open(file_csv, 'rb'))
    a=False
    b=True
    list_all = []
    list_date = []
    list_level = []
    list_temp = []
    startDate2 = startDate[-4:]+'-'+startDate[3:5]+'-'+startDate[:2]
    endDate2 = endDate[-4:]+'-'+endDate[3:5]+'-'+endDate[:2]
    for row in csv_file:
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
    rasterfn = os.path.abspath("C:/Users/Florian/Dropbox/Design2014/donnees/SIG Broc/MNT/MNT_broc.tif")
    ds1 = gdal.Open(rasterfn)
    band1 = ds1.GetRasterBand(1)
    nodata = band1.GetNoDataValue()
    array1 = band1.ReadAsArray()
    size_array1 = np.shape(array1)
    ## Liste de hauteur
    my_timestamp = 0
    haut_list = []
    array_all = []
    timestamp_all = []
    array_L11=np.array(np.zeros(size_array1), dtype = 'float32')
    array_L22=np.array(np.zeros(size_array1), dtype = 'float32')
    array_L33=np.array(np.zeros(size_array1), dtype = 'float32')
    array_L44=np.array(np.zeros(size_array1), dtype = 'float32')
    array_AA=np.array(np.ones(size_array1), dtype = 'float32')
    for i in range(0, len(list_date), timestep):
        flagDraw = False
        #Condition avril à Septembre
        if (float(list_date[i][5:7]) < 9) & (float(list_date[i][5:7]) >4):
            haut_list.append(float(list_level[i]))
            niveau_aujourdhui = float(list_level[i])
            #pose datum à 674.5
            if niveau_hier < niveau_critique :
                niveau_hier = niveau_critique
            #verification nouvelles zones inondees
            if (niveau_hier < niveau_aujourdhui) & (niveau_aujourdhui == max(haut_list[-15:])): 
                array = 1*((array1 > niveau_hier) & (array1 != nodata) &  (array1 < niveau_aujourdhui))
                array_all.append(array)
                timestamp_all.append(my_timestamp)
#                print aire_inondee
                #Calcul aire inondée
                aire_inondee = array4.sum() * aire_pixel
            for k in range(len(timestamp_all)):
                if (my_timestamp-timestamp_all[k]<4): ##Condition L1
                    array_L11 += array_all[k]
                    flagDraw = True
                if ((my_timestamp-timestamp_all[k]<7) & (my_timestamp-timestamp_all[k]>3)):##Condition L2
                    array_L22 += array_all[k]
                    flagDraw = True
                if (my_timestamp-timestamp_all[k]<10) & (my_timestamp-timestamp_all[k]>6):##Condition L3
                    array_L33 += array_all[k]
                    flagDraw = True
                if (my_timestamp-timestamp_all[k]<13)&(my_timestamp-timestamp_all[k]>9):##Condition L4
                    array_L44 += array_all[k]
                    flagDraw = True
                if (my_timestamp-timestamp_all[k]>12):##Condition aerien
                    array_AA += array_all[k]
                    
            array_L1 = col_L1 * array_L11
            array_L2 = col_L2 * array_L22
            array_L3 = col_L3 * array_L33
            array_L4 = col_L4 * array_L44
            array_A = 0 * array_AA
            array4 = array_L1 + array_L2 + array_L3 + array_L4 + array_AA
            niveau_hier = niveau_aujourdhui
            #nom du raster
            tail1 = list_date[i][:10] + '.tif'
            newRasterfn4_mid = os.path.join(dir_name2,tail1)
            newRasterfn4=os.path.abspath(newRasterfn4_mid)
            #ecriture raster
            if flagDraw:
                array2raster(newRasterfn4,rasterfn,array4)
            flagDraw = False
            #reinitialisation des zones L1-L2-L3-L4
            array_L11=np.array(np.zeros(size_array1), dtype = 'float32')
            array_L22=np.array(np.zeros(size_array1), dtype = 'float32')
            array_L33=np.array(np.zeros(size_array1), dtype = 'float32')
            array_L44=np.array(np.zeros(size_array1), dtype = 'float32')
            array_AA=np.array(np.ones(size_array1), dtype = 'float32')
        else:
            if len(haut_list) > 0:
                #reinitialisation
                haut_list = []
                array_all = []
                timestamp_all = []
        my_timestamp += 1