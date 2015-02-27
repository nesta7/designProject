# -*- coding: utf-8 -*-
"""
Created on Sun May 18 10:44:38 2014

@author: Akio
"""

import sys
import os
from Tkinter import *
import tkFileDialog

def choixrep():
    
    mGui = Tk() # Créer fenêtre
    var = DoubleVar()
    mGui.title("Choix des repertoires") # Titre
    mGui.geometry('450x450+200+200') # Taile + position x + position y
    mbutton1 = Button(mGui, text='Choisir le répertoire', command=choixrep).pack() # Créer bouton pour choisir le répertoire
    mbutton2 = Button(mGui, text='OK', command=mGui.destroy).pack() # Créer bouton pour quitter a fenêtre    
    slider = Scale(mGui,orient=HORIZONTAL, width = 20, length = 200, sliderlength = 10,
              from_=0, to=100, resolution=1,tickinterval=20,label="Surface [ha] pour seuil d'alarme",
              variable=var).pack()  
    rep = tkFileDialog.askdirectory(initialdir='.') # La commande qui créer la recherche de répertoire
    mGui.mainloop() # boucle infini pour garder fenêtre, code marche pas bien sans cette ligne!
    if var.get() > 0: # 0 = oui
#        mGui.destroy() # Détruit fenêtre après avoir choisit le répertoire
        return rep,var.get() # retourne le répertoire choisit   