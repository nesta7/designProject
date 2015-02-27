# -*- coding: utf-8 -*-
"""
Created on Sat Mar 22 18:20:28 2014

@author: Florian & Akio
"""

import re
import time
import datetime
from urllib import urlopen

urlLac="http://www.groupe-e.ch/node/683"
urlMeteo="http://www.meteoswiss.admin.ch/web/en/weather/detailed_forecast/local_forecasts.html?language=en&plz=broc&x=0&y=0"

def fetchLevel(whichDay = 'n'):
    ''' Recherche dans le site du groupe e la hauteur du lac
    -whichDay: jour d'interet, 'n' = aujourd'hui, 'y' = hier'''
    #url = "http://www.groupe-e.ch/niveau-des-lacs"
    html = urlopen(urlLac).read()    
    result1 = re.search('La Gruy\xc3\xa8re</td><td>677.00&nbsp;msm</td><td><p>(.*)Schiffenen', html)
    #result1 = re.search('La \xc3\xa8re</td><td>677.00&nbsp;msm</td><td><p>(.*)&nbsp;msm</p></td><td>', html)
    #result2 = re.search('Niveau max.</strong></td><td><strong>(.*)</strong></td><td><p><strong>(.*)</strong></p>', html)
    result2 = re.search('Niveau max.</strong></td><td><p><strong>(.*)</strong></p></td><td><p><strong>(.*)</strong></p>', html)
    y_priori = time.mktime(datetime.datetime.strptime(result2.group(1), "%d.%m.%Y").timetuple())
    n_priori = time.mktime(datetime.datetime.strptime(result2.group(2), "%d.%m.%Y").timetuple())    
    if y_priori > n_priori : 
        if whichDay == 'n':
            return float(result1.group(1)[0:6])
        if whichDay == 'y':
            return float(result1.group(1)[28:34])
    else : 
        if whichDay == 'y':
            return float(result1.group(1)[0:6])
        if whichDay == 'n':
            return float(result1.group(1)[28:34])

def fetchDate(whichDay = 'n'):
    ''' Recherche dans le site du groupe e le jour de mesure
        -whichDay: jour d'interet, 'n' = aujourd'hui, 'y' = hier'''
    html = urlopen(urlLac).read()    
    #result = re.search('Niveau max.</strong></td><td><strong>(.*)</strong></td><td><p><strong>(.*)</strong></p>', html)
    result = re.search('Niveau max.</strong></td><td><p><strong>(.*)</strong></p></td><td><p><strong>(.*)</strong></p>', html)
    y_priori = time.mktime(datetime.datetime.strptime(result.group(1), "%d.%m.%Y").timetuple())
    n_priori = time.mktime(datetime.datetime.strptime(result.group(2), "%d.%m.%Y").timetuple())  
    if y_priori > n_priori : 
        if whichDay == 'n':
            return result.group(1)        
        if whichDay == 'y':
            return result.group(2)

    else : 
        if whichDay == 'y':
            return result.group(1)
        if whichDay == 'n':
            return result.group(2) 
        
def fetchTemp(whichDay = 'n'):
    print(typeOS)
    ''' Recherche dans le site de meteoswiss les temperatures matin et soir
        -whichDay: jour d'interet, 'n' = aujourd'hui, 't1' = demain, 't2' = après-demain, ..., 't5' = dans 5 jours'''
    html = urlopen(urlMeteo).read()    
    result = re.search('" style="text-align: center;">(.*) &deg;C', html) #normalement 6 groupes qui sont les prévisions pour le jour même et les 5 jours à venir
    a = result.group(1).split("|") 
    if whichDay == 'n':
        return float(a[0]), float(a[1][:3]) #temps en temps change de 3 vers 4
    if whichDay == "t5":
        return float(a[-2][-2:]), float(a[-1])
    else:
        b = int(whichDay[-1])
        return float(a[b][-2:]), float(a[b+1][:3])

