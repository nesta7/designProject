# -*- coding: utf-8 -*-
"""
Created on Sun May 18 10:44:38 2014

@author: Akio
"""

import sys
import os
from Tkinter import *
import tkMessageBox
import tkFileDialog

def f_rep():

    def choixrep():
    #    os.chdir('/Users/Akio')
        rep = tkFileDialog.askdirectory(initialdir='.')
        if len(rep) > 0:
    #        sys.path.append(rep)
            os.chdir(rep)
            print("vous avez choisi le repertoire ",rep)
            return rep
    def choixrep1():
    #    os.chdir('/Users/Akio')
        rep1 = tkFileDialog.askdirectory(initialdir='.')
        if len(rep1) > 0:
            sys.path.remove(rep1)
            print("vous avez choisi d'enlever le path",rep1)
    def showpaths():
        cwd = os.getcwd()
    #    print sys.path
        print cwd
        
    mGui = Tk()
    mGui.title("Choix des repertoires")
    mGui.geometry('450x450+200+200')
    mlabel = Label(mGui, text='Répertoire des sources').pack()
    mbutton = Button(mGui, text='Add path', command=choixrep).pack()
    mbutton1 = Button(mGui, text='Remove path', command=choixrep1).pack()
    mbutton2 = Button(mGui, text='Show paths', command=showpaths).pack()
    mbutton3 = Button(mGui, text='Quitter', command=mGui.destroy).pack()
    mGui.mainloop()
