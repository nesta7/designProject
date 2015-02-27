# -*- coding: utf-8 -*-
"""
Created on Tue May 27 00:30:54 2014

@author: Akio
"""

import numpy as np
import datetime
import matplotlib.pyplot as plt
import pandas as pd

plt.close('all')

data = pd.read_csv("2010_moy.csv",sep=",")
time = data.values[:,0]
temp_moy = data.values[:,2]
temp_reel = data.values[:,3]

time1 = pd.to_datetime(time, dayfirst = True, infer_datetime_format=True)

plt.figure()
plt.plot(time1,temp_reel,'-b',time1,temp_moy,'-r')
plt.xlabel("Date")
plt.ylabel("Temperature du lac [degC]")
plt.title("Comparaison entre donnees reelles et donnees simulees pour 2010")
plt.legend(('Donnees reelles 2010','Moyenne 2007-2014'))