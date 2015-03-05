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

def array2raster(newRasterfn,rasterfn,array):
    raster = gdal.Open(rasterfn)
    #MAX: la fonction getgeotransform permet d'obtenir différentes informations sur les dimensions, l'orientation et la résolution de l'image:
    #adfGeoTransform[0] /* top left x */
    #adfGeoTransform[1] /* w-e pixel resolution */
    #adfGeoTransform[2] /* rotation, 0 if image is "north up" */
    #adfGeoTransform[3] /* top left y */
    #adfGeoTransform[4] /* rotation, 0 if image is "north up" */
    #adfGeoTransform[5] /* n-s pixel resolution */
    geotransform = raster.GetGeoTransform()
    originX = geotransform[0]
    originY = geotransform[3]
    pixelWidth = geotransform[1]
    pixelHeight = geotransform[5]
    #MAX: determination du nombre de colonnes
    cols = array.shape[1]
    #MAX: determination du nombre de lignes
    rows = array.shape[0]
    driver = gdal.GetDriverByName('GTiff')
    #MAX: les arguments de Create() sont: le nom du fichier, le nombre de colonnes, de lignes, le nombre de bandes et finalement ??
    outRaster = driver.Create(newRasterfn, cols, rows, 1, gdal.GDT_Byte)
    outRaster.SetGeoTransform((originX, pixelWidth, 0, originY, 0, pixelHeight))
    outband = outRaster.GetRasterBand(1)
    outband.SetNoDataValue(0)
    outband.WriteArray(array)
    outRasterSRS = osr.SpatialReference()
    outRasterSRS.ImportFromWkt(raster.GetProjectionRef())
    outRaster.SetProjection(outRasterSRS.ExportToWkt())
    #delete something to make some memory available??
    outband.FlushCache()
