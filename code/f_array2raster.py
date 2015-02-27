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