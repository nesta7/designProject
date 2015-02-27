# -*- coding: utf-8 -*-
from qgis.core import *
import qgis.utils

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
os.chdir(QgsProject.instance().readPath("./"))
os.chdir('.\code')
execfile('f_fetchData.py')
execfile('f_array2raster.py')

# Script
#Variable
niveau_critique = 674.5
#niveau_gruyere = fetchLevel('n')
niveau_gruyere = 676#test
aire_pixel = 9 ##m^2
oeuf_m2 = 100 ## on peut appele une fonction qui modélise les oeufs cf. Shaeffner
pourcent_eclo = 1/3 ## Pourcentage des oeufs qui éclosent pour une parcelle : Luthy + gjullin
print "Simulation des niveaux pour le %s" %fetchDate('n')

#Lecture MNT
fileName = os.path.relpath("../donnees/MNT/MNT_broc.tif")
baseName = os.path.basename(fileName)
rlayer = QgsRasterLayer(fileName, baseName) # EPSG: 21781
#rlayer.setCrs( QgsCoordinateReferenceSystem(21781, QgsCoordinateReferenceSystem.EpsgCrsId) )
if not rlayer.isValid():
  print "Layer failed to load!"
newRasterfn = os.path.relpath("../donnees/MNT/inonde.tif")
newRasterfn2 = os.path.relpath("../donnees/MNT/inonde_oeufs.tif")
rasterfn = os.path.abspath("../donnees/MNT/MNT_broc.tif")
ds1 = gdal.Open(fileName)
band1 = ds1.GetRasterBand(1)
nodata = band1.GetNoDataValue()
array1 = band1.ReadAsArray()

#Lecture Habitat
fileName2 = os.path.relpath("../donnees/habitat/habitat_raster3.tif")
baseName2 = os.path.basename(fileName2)
rlayer_vege = QgsRasterLayer(fileName2, baseName2) # EPSG: 21781
if not rlayer_vege.isValid():
    print "Layer failed to load!"
ds1_vege = gdal.Open(fileName2)
band1_vege = ds1_vege.GetRasterBand(1)
nodata_vege = 0
array1_vege = band1_vege.ReadAsArray()

array2 = 1*((array1 < niveau_gruyere) & (array1 != nodata) & (array1_vege != nodata_vege))
array3 = 1*((array1 > niveau_critique) & (array1 != nodata) &  (array1 < niveau_gruyere) & (array1_vege != nodata_vege))
array2raster(newRasterfn,rasterfn,array2)
array2raster(newRasterfn2,rasterfn,array3)

#Aire inondée en plus par rapport au jour d'avant ou le niveau critique
## Entree du modèle temporel
niveau_hier = fetchLevel('y')
niveau_aujourdhui = niveau_gruyere
## fin entree modèle temporel
if niveau_hier < niveau_critique :
    niveau_hier = niveau_critique
if niveau_hier < niveau_aujourdhui:
    array4 = 1*((array1 > niveau_hier) & (array1 != nodata) &  (array1 < niveau_aujourdhui))
    aire_inondee = array4.sum() * aire_pixel
    print "Nouvelles zones inondées!"
if niveau_aujourdhui < niveau_critique:
    print "Niveau critique non depasse"
#if niveau_aujourdhui < niveau_hier:
#    print "niveau en baisse par rapport a hier"




