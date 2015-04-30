﻿Imports System.Windows.Forms 'MAX: Module qui sert à faire fonctionner la fonction messagebox.
'les 2 prochains modules servent à écrire des résultats dans un csv...
Imports System.IO
'Imports System.Text

Module Module1
    Sub Main()
        'Dim path_init = "C:\Users\max\Documents\cours MA2\design project\dossier_partage\code_vb\" 'Max
        Dim path_init = "E:\code_vb\" 'Mo
        '**************************************************************************************************************
        'Import des zones inondables (identifiant du polygone, altitude de la zone concernée, evt. connectivité si
        'on tient compte de la migration des larves vers zones plus basses
        '**************************************************************************************************************
        Dim path_polygon = String.Concat(path_init, "polygones.csv")
        Dim lines_polygon = IO.File.ReadAllLines(path_polygon)
        Dim tbl = New DataTable
        Dim colCount = lines_polygon.First.Split(","c).Length
        tbl.Columns.Add(New DataColumn("Polygone_id", GetType(Int32)))
        tbl.Columns.Add(New DataColumn("Altitude", GetType(Double)))
        For Each line In lines_polygon
            Dim objFields = From field In line.Split(","c)
                         Select CType(field, Object)
            Dim newRow = tbl.Rows.Add()
            newRow.ItemArray = objFields.ToArray()
        Next

        '**************************************************************************************************************
        'Import des infos de hauteur d'eau et de température du passé et de prévision.  attention! il faut modifier le 
        'code de telle sorte qu'il y ait une mesure par heure pour le passé, et 2 par jour pour les prévisions!! Ensuite,
        'agréger le tout pour avoir des moyennes journalières
        '**************************************************************************************************************
        '-----------------------------
        'Initialisation des variables
        '-----------------------------
        Dim jma As String() 'MAX: jma pour jour mois annee
        Dim jour_split As String() 'MAX: jour_split permet de dissocier le jour et l'heure de la mesure

        Dim i
        Dim j

        '-----------------------------
        'passé
        '-----------------------------
        'Lecture du csv
        Dim path_past = String.Concat(path_init, "Hauteur_Temp_passe.csv")

        Dim lines_past = IO.File.ReadAllLines(path_past)
        Dim tbl2 = New DataTable
        tbl2.Columns.Add(New DataColumn("Date", GetType(String)))
        tbl2.Columns.Add(New DataColumn("Niveau", GetType(Double)))
        tbl2.Columns.Add(New DataColumn("Temperature", GetType(Double)))

        'declaration des variables de date, de niveau de l'eau et de temperature de l'eau
        Dim date_vector_past(lines_past.Length - 2) As String
        Dim niveau_vector_past(lines_past.Length - 2) As Double
        Dim temp_vector_past(lines_past.Length - 2) As Double

        'Fractionnement des éléments de date_vector en differentes variables
        Dim jour_past(lines_past.Length - 2) As Integer
        Dim mois_past(lines_past.Length - 2) As Integer
        Dim annee_past(lines_past.Length - 2) As Integer

        i = 0 'MAX: variable servant à ignorer la première ligne
        j = 0 'MAX: variable permettant de passer à travers chaque ligne

        'importation des infos du csv dans les variables définies ci-dessus
        For Each line In lines_past
            If i <> 0 Then
                Dim objFields2 = From field In line.Split(";"c)
                             Select CType(field, Object)
                Dim newRow = tbl2.Rows.Add()
                newRow.ItemArray = objFields2.ToArray()

                'moment des mesures
                date_vector_past(j) = tbl2.Rows(j)(0)
                jma = date_vector_past(j).Split(New Char() {"-"c})
                jour_split = jma(2).Split(New Char() {" "c}) 'MAX: cette opération permet de dissocier l'heure et le jour
                annee_past(j) = Integer.Parse(jma(0))
                mois_past(j) = Integer.Parse(jma(1))
                jour_past(j) = Integer.Parse(jour_split(0))

                'mesures
                niveau_vector_past(j) = tbl2.Rows(j)(1)
                temp_vector_past(j) = tbl2.Rows(j)(2)

                j = j + 1
            End If
            i = 1
        Next

        '-----------------------------
        'prévisions
        '-----------------------------
        'Lecture du csv
        Dim path_previ = String.Concat(path_init, "Hauteur_Temp_previ.csv")
        Dim lines_previ = IO.File.ReadAllLines(path_previ)
        Dim tbl3 = New DataTable
        tbl3.Columns.Add(New DataColumn("Date", GetType(String)))
        tbl3.Columns.Add(New DataColumn("Niveau", GetType(Double)))
        tbl3.Columns.Add(New DataColumn("Temperature", GetType(Double)))

        'Declaration des variables de date, de niveau de l'eau et de temperature de l'eau
        Dim date_vector_previ(lines_previ.Length - 2) As String
        Dim niveau_vector_previ(lines_previ.Length - 2) As Double
        Dim temp_vector_previ(lines_previ.Length - 2) As Double

        'Fractionnement des éléments de date_vector en differentes variables
        Dim jour_previ(lines_previ.Length - 2) As Integer
        Dim mois_previ(lines_previ.Length - 2) As Integer
        Dim annee_previ(lines_previ.Length - 2) As Integer

        i = 0 'MAX: variable servant à ignorer la première ligne
        j = 0 'MAX: variable permettant de passer à travers chaque ligne

        'importation des infos du csv dans les variables définies ci-dessus
        For Each line In lines_previ
            If i <> 0 Then
                Dim objFields3 = From field In line.Split(";"c)
                             Select CType(field, Object)
                Dim newRow = tbl3.Rows.Add()
                newRow.ItemArray = objFields3.ToArray()

                'moment des mesures
                date_vector_previ(j) = tbl3.Rows(j)(0)
                jma = date_vector_previ(j).Split(New Char() {"-"c})
                jour_split = jma(2).Split(New Char() {" "c}) 'MAX: cette opération permet de dissocier l'heure et le jour
                annee_previ(j) = Integer.Parse(jma(0))
                mois_previ(j) = Integer.Parse(jma(1))
                jour_previ(j) = Integer.Parse(jour_split(0))

                'mesures
                niveau_vector_previ(j) = tbl3.Rows(j)(1)
                temp_vector_previ(j) = tbl3.Rows(j)(2)

                j = j + 1
            End If
            i = 1
        Next

        '**************************************************************************************************************
        'simulation
        '**************************************************************************************************************
        '-----------------------------
        'simulation du niveau de l'eau
        '-----------------------------
        '..........
        'passé
        '..........
        'MAX: deux facons d'approcher le probleme sont présentées. La première est une approche preliminaire fidèle au code d'origine de florian et akkio.
        'La deuxième est l'approche que l'on devrait utiliser une fois le modèle intégré dans le système d'e-dric.

        ''MAX: approche 1) toutes les mesures a disposition sont calculées à chaque run du programme
        'Dim inonde_past(lines_past.Length - 2, lines_polygon.Length - 1) As Integer 'MAX: plus tard on aura pas besoin de garder cette information. Juste les quelques dernières lignes. Celles d'avant auront déjà été prises en compte.
        ''MAX: pour la prochaine variable: 1ere dimension = temps. 2eme dimension = polygone, 3eme dimension = stade en cours et pourcentage d'avancement du stade
        'Dim state_time(lines_past.Length - 2, lines_polygon.Length - 1, 2) As Double
        'Dim state_output(2) As Double

        ''initialiser l'etat du premier jour à 0
        ''pas besoin, c'est fait automatiquement.

        'For t = 0 To lines_past.Length - 2 'MAX: on fait -2 car la première ligne ne doit pas etre considérée puisque c'est les titres de colonnes et parce que l'on part de 0
        '    For i = 0 To lines_polygon.Length - 1
        '        If niveau_vector_past(t) >= tbl.Rows(i)(1) Then
        '            inonde_past(t, i) = 1
        '        End If
        '        If inonde_past(t, i) = 1 Then
        '            If t <> 0 Then
        '                'MAX:appliquer la fonction de developpement larvaire 
        '                state_output = function_state(state_time(t - 1, i, 0), state_time(t - 1, i, 1), temp_vector_past(t - 1))
        '                state_time(t, i, 0) = state_output(0)
        '                state_time(t, i, 1) = state_output(1)
        '            Else
        '                'state_time(t,i,0)=valeur_arbitraire ??
        '                'state_time(t,i,0)=valeur_arbitraire ??
        '            End If
        '        Else
        '            'state_time(t,i,0)=0
        '            'state_time(t,i,1)=0
        '        End If
        '    Next
        'Next

        'MAX: approche 2) Seules les mesures de la veille sont calculées, les mesures précédentes étant prises en compte par le fichier sauvegardé (voir les lignes suivantes)
        Dim inonde_hier(lines_polygon.Length - 1) As Integer
        'MAX: pour les 2 prochaines variables: 1ere dimension = polygone, 2eme dimension = stade en cours et pourcentage d'avancement du stade
        Dim state_yesterday(lines_polygon.Length - 1, 2) As Double 'MAX: variable a importer d'un fichier qui aura ete sauvegardé la veille

        Dim path_state = String.Concat(path_init, "etat_veille.csv")
        Dim lines_state = IO.File.ReadAllLines(path_state)
        Dim tbl4 = New DataTable
        colCount = lines_state.First.Split(","c).Length
        tbl4.Columns.Add(New DataColumn("Polygone_id", GetType(Int32)))
        tbl4.Columns.Add(New DataColumn("Etat", GetType(Double)))
        tbl4.Columns.Add(New DataColumn("Evolution_etat", GetType(Double)))
        For Each line In lines_state
            Dim objFields = From field In line.Split(","c)
                         Select CType(field, Object)
            Dim newRow = tbl4.Rows.Add()
            newRow.ItemArray = objFields.ToArray()
        Next

        j = 0 'MAX: variable permettant de passer à travers chaque ligne
        'importation des infos de l'etat de développement de la veille
        For Each line In lines_state
            'polygon_id(j) = tbl4.Rows(j)(0)
            state_yesterday(j, 0) = tbl4.Rows(j)(1)
            state_yesterday(j, 1) = tbl4.Rows(j)(2)
            j = j + 1
        Next

        Dim current_state(lines_polygon.Length - 1, 2) As Integer 'MAX: etat d'aujourdhui, défini a l'aide des nouvelles mesures effectuées entre hier et aujourdhui
        Dim current_state_output(2) As Double

        For i = 0 To lines_polygon.Length - 1
            If niveau_vector_past(lines_past.Length - 2) >= tbl.Rows(i)(1) Then
                inonde_hier(i) = 1
            End If
            If inonde_hier(i) = 1 Then
                'MAX:appliquer la fonction de developpement larvaire 
                current_state_output = function_state(state_yesterday(i, 0), state_yesterday(i, 1), temp_vector_past(lines_past.Length - 2))
                current_state(i, 0) = current_state_output(0)
                current_state(i, 1) = current_state_output(1)
            Else
                current_state(i, 0) = 0
                current_state(i, 1) = 0
            End If
        Next

        'écriture de la table enregistrant le stade actuel
        Dim objwri As New StreamWriter(path_state, True) '"C:\Users\max\Documents\cours MA2\design project\dossier_partage\code_vb\etat_veille.csv", True)
        Dim current_state_string_1 As String
        Dim current_state_string_2 As String
        Dim polygon_id As String

        For i = 0 To lines_polygon.Length - 1
            current_state_string_1 = System.Convert.ToString(current_state(i, 0))
            current_state_string_2 = System.Convert.ToString(current_state(i, 1))
            polygon_id = System.Convert.ToString(i + 1)
            objwri.WriteLine(polygon_id + ", " + current_state_string_1 + ", " + current_state_string_2)
        Next
        objwri.Close()
        Console.WriteLine(current_state(1, 0))
        '..........
        'prévisions
        '..........
        Dim inonde_previ(lines_previ.Length, lines_polygon.Length) As Integer 'MAX: chaque ligne indique les polygones inondés pour un jour donné

        For t = 0 To lines_previ.Length - 2
            For i = 0 To lines_polygon.Length - 1
                If niveau_vector_previ(t) >= tbl.Rows(i)(1) Then
                    inonde_previ(t, i) = 1
                End If
            Next
        Next
        Console.Read()
    End Sub

    'MAX: T représente la température de la veille
    Function function_state(ByVal state_yesterday As Double, ByVal perc_yesterday As Double, ByVal T As Double) As Double()

        'Declaration des variables de sortie
        Dim state_perc_today(2) As Double
        Dim state_today As Double
        Dim perc_today As Double

        'categories de temperature
        Dim temp() As Double = {15, 20, 25, 30, 35}

        'Durée stades vexans (établi à l'aide du code matlab "adaptation_albopictus_vexans.m")
        Dim L1() As Double = {3.4, 2.4, 1.5, 0.9, 0.7}
        Dim L2() As Double = {2, 1.1, 0.9, 0.9, 0.5}
        Dim L3() As Double = {2.8, 1.7, 0.9, 0.9, 1}
        Dim L4() As Double = {8.1, 3.3, 2.4, 2, 3}

        'Durée stades albopictus
        'Dim L1() As Double = {5.6, 3.0, 2.1, 1.4, 1.7}
        'Dim L2() As Double = {3.3, 1.4, 1.2, 1.3, 1.2}
        'Dim L3() As Double = {4.6, 2.1, 1.2, 1.4, 2.4}
        'Dim L4() As Double = {13.4, 4.1, 3.3, 3.0, 6.8}


        Dim i As Integer
        Dim noCol As Integer = temp.Length - 1
        Dim Model(4, noCol) As Double
        Dim perc_incr As Double 'MAX: cette variable montre l'augmentation du pourcentage de la maturation de l'état qui avait lieu entre hier et aujourd'hui
        Dim time_new_state As Double 'MAX: dans le cas où un changement d'état a lieu entre hier et aujourd'hui, cette variable indique le temps qui s'est écoulé depuis que ce nouvel état a lieu.

        For i = 0 To noCol
            Model(0, i) = temp(i)
            Model(1, i) = L1(i)
            Model(2, i) = L2(i)
            Model(3, i) = L3(i)
            Model(4, i) = L4(i)
        Next
        If T < Model(0, 0) Or T > Model(0, noCol) Then
            Console.WriteLine("Temperature " & T & "°C out of range [ " & Model(0, 0) & ":" & Model(0, noCol) & " ]")
        Else
            For i = 0 To noCol - 1
                If T >= temp(i) And T < temp(i + 1) Then
                    'Compute a linear estimation of the require number of days to grow up
                    'then compute the actual proportion of growth since the simulation is at a daily scale
                    perc_incr = 1 / (Model(state_yesterday, i) + (T - Model(0, i)) / (Model(0, i + 1) - Model(0, i)) * (Model(state_yesterday, i + 1) - Model(state_yesterday, i)))
                    If perc_incr + perc_yesterday >= 1 Then
                        If state_yesterday = 4 Then
                            'disable_polygon = 1
                        Else
                            time_new_state = (perc_incr + perc_yesterday - 1) * (1 / perc_incr)
                            state_today = state_yesterday + 1 ' this one gets out of range
                            perc_today = time_new_state / (Model(state_today, i) + (T - Model(0, i)) / (Model(0, i + 1) - Model(0, i)) * (Model(state_today, i + 1) - Model(state_today, i)))
                            If perc_today >= 1 Then
                                Dim break As Integer = 0
                                While break = 0
                                    state_today = state_today + 1
                                    time_new_state = (perc_today - 1) * (1 / perc_today)
                                    perc_today = time_new_state / (Model(state_today, i) + (T - Model(0, i)) / (Model(0, i + 1) - Model(0, i)) * (Model(state_today, i + 1) - Model(state_today, i)))
                                    If perc_today < 1 Then
                                        break = 1
                                    End If
                                End While
                            End If
                        End If
                    Else
                        time_new_state = 0
                        state_today = state_yesterday
                        perc_today = perc_incr + perc_yesterday
                    End If
                End If
            Next
        End If
        state_perc_today(0) = state_today
        state_perc_today(1) = perc_today
        Return state_perc_today
    End Function
End Module




