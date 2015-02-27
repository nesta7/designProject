# -*- coding: utf-8 -*-
"""
Created on Tue May 20 17:25:45 2014

@author: Akio
"""

import Tkinter
from Tkinter import *
import time

path='/home/nesta/myCourses/designProject/Outil_Demoustication_2014/code/'

execfile('f_Database.py') # Met à jour la base de donnée Hauteur_Temp.csv
execfile('f_ModeleLarve.py') # Modèle de développement larvaire
execfile('f_rep.py') # Fonction pour choisir le repertoire

def MettreAjour():
    f_Database()

def LancerSeqAnimation():
    root.destroy()
    execfile('SequenceAnimation.py') # fonction pour choisir le répertoire
    
def ExporterResultats():
    root.destroy()
    execfile('GenererResultats.py')

# Création de l'interface graphique avec la librairie Tkinter    
root = Tkinter.Tk()
root.title('Outil d''aide à la décision')
root.geometry('350x350+400+150') # Taile + position x + position y

root.configure(bg='#255') 
MiseAjour = Tkinter.Button(root, text = "Mettre à jour la base de données",command=MettreAjour).place(relx=0.5,rely=0.2,anchor=CENTER)
LancerSeq = Tkinter.Button(root, text = "Créer la séquence d'animation",command=LancerSeqAnimation).place(relx=0.5,rely=0.35,anchor=CENTER)
GenResultats = Tkinter.Button(root, text = "Exporter les résultats en format csv",command=ExporterResultats).place(relx=0.5,rely=0.5,anchor=CENTER)
Quitter = Tkinter.Button(root, text = "Quitter",comman=root.destroy).place(relx=0.5,rely=0.7, anchor=CENTER)
mlabel = Label(root,text='Florian Gandor & Akio Schoorl - 06/06/14').place(relx=0.5,rely=0.97,anchor=CENTER)

menubar = Menu(root)
filemenu = Menu(menubar,tearoff=0)
menubar.add_cascade(label="File",menu=filemenu)
filemenu.add_command(label="Close",command = root.destroy)
filemenu = Menu(menubar,tearoff=0)
menubar.add_cascade(label="Help",menu=filemenu)
filemenu.add_command(label="Mode d'emploi")
root.config(menu=menubar)

root.mainloop()