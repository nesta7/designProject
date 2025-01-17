﻿Imports System.Windows.Forms 'MAX: Module qui sert à faire fonctionner la fonction messagebox.
Imports System.IO 'MAX: module servant à écrire des résultats dans un csv...

Module Module1
    Sub Main()
        Dim path_init = "C:\Users\max\Documents\cours MA2\design project\dossier_partage\code_vb\" 'Max
        'Dim path_init = "E:\code_vb\" 'Mo

        '**************************************************************************************************************
        'Import des zones inondables (identifiant du polygone et altitude de la zone concernée)
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
        'Import des infos de hauteur d'eau et de température du passé et de prévision. (1 mesure par jour)
        '**************************************************************************************************************
        '----------------------------
        'Initialisation des variables
        '----------------------------
        Dim jma As String() 'MAX: jma pour jours mois annee
        Dim jour_split As String() 'MAX: jour_split permet de dissocier le jour et l'heure de la mesure (de les stocker dans 2 différentes variables)

        Dim i
        Dim j

        '-----------------------------
        'mesures de la veille
        '-----------------------------
        'Lecture du csv
        Dim path_past = String.Concat(path_init, "Hauteur_Temp_veille.csv")

        Dim lines_past = IO.File.ReadAllLines(path_past)
        Dim tbl2 = New DataTable
        tbl2.Columns.Add(New DataColumn("Date", GetType(String)))
        tbl2.Columns.Add(New DataColumn("Niveau", GetType(Double)))
        tbl2.Columns.Add(New DataColumn("Temperature", GetType(Double)))

        'declaration des variables de date, de niveau de l'eau et de temperature de l'eau
        Dim niveau_vector_past As Double
        Dim temp_vector_past As Double

        i = 0 'MAX: variable servant à ignorer la première ligne

        'importation des infos du csv dans les variables définies ci-dessus
        For Each line In lines_past
            If i <> 0 Then
                Dim objFields2 = From field In line.Split(";"c)
                             Select CType(field, Object)
                Dim newRow = tbl2.Rows.Add()
                newRow.ItemArray = objFields2.ToArray()

                'mesures
                niveau_vector_past = tbl2.Rows(0)(1)
                temp_vector_past = tbl2.Rows(0)(2)
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
        Dim inonde_hier(lines_polygon.Length - 1) As Integer
        Dim state_yesterday(lines_polygon.Length - 1) As Double
        Dim perc_yesterday(lines_polygon.Length - 1) As Double
        Dim time_since_last_eclosion(lines_polygon.Length - 1) As Integer

        'Lecture du fichier etat_veille.csv
        Dim path_state = String.Concat(path_init, "etat_veille.csv")
        Dim lines_state = IO.File.ReadAllLines(path_state)
        Dim tbl4 = New DataTable
        colCount = lines_state.First.Split(","c).Length
        tbl4.Columns.Add(New DataColumn("Date", GetType(String)))
        tbl4.Columns.Add(New DataColumn("Polygone_id", GetType(Int32)))
        tbl4.Columns.Add(New DataColumn("Etat", GetType(Double)))
        tbl4.Columns.Add(New DataColumn("Evolution_etat", GetType(Double)))
        tbl4.Columns.Add(New DataColumn("Temps_depuis_debut_inondation", GetType(Int32)))
        For Each line In lines_state
            Dim objFields = From field In line.Split(","c)
                         Select CType(field, Object)
            Dim newRow = tbl4.Rows.Add()
            newRow.ItemArray = objFields.ToArray()
        Next

        j = 0 'MAX: variable permettant de passer à travers chaque ligne

        'importation des infos de l'etat de développement de la veille
        For Each line In lines_state
            state_yesterday(j) = tbl4.Rows(j)(2)
            perc_yesterday(j) = tbl4.Rows(j)(3)
            time_since_last_eclosion(j) = tbl4.Rows(j)(4)
            j = j + 1
        Next

        'Suppression de la sauvegarde de la veille
        If System.IO.File.Exists(String.Concat(path_init, "etat_veille.csv")) = True Then
            System.IO.File.Delete(String.Concat(path_init, "etat_veille.csv"))
        End If

        'Création de la nouvelle sauvegarde
        Dim current_state(lines_polygon.Length - 1) As Double 'MAX: etat d'aujourdhui, défini a l'aide des nouvelles mesures effectuées entre hier et aujourdhui
        Dim current_perc(lines_polygon.Length - 1) As Double
        Dim current_state_output(2) As Double

        For i = 0 To lines_polygon.Length - 1
            time_since_last_eclosion(i) = time_since_last_eclosion(i) + 1
            If niveau_vector_past >= tbl.Rows(i)(1) Then
                inonde_hier(i) = 1
            End If
            If inonde_hier(i) = 1 Then
                'MAX:appliquer la fonction de développement larvaire
                If state_yesterday(i) <> 5 Then
                    current_state_output = function_state(state_yesterday(i), perc_yesterday(i), temp_vector_past, time_since_last_eclosion(i))
                    current_state(i) = current_state_output(0)
                    current_perc(i) = current_state_output(1)
                Else
                    If time_since_last_eclosion(i) < 15 Then '14 ou 15 ici????
                        current_state(i) = 5
                        current_perc(i) = 0
                    Else
                        current_state(i) = 1
                        current_perc(i) = 0
                        time_since_last_eclosion(i) = 1 '1 ou 0 ici?
                    End If
                End If
            Else
                current_state(i) = 0
                current_perc(i) = 0
            End If
        Next

        'écriture de la table enregistrant le stade actuel
        Dim objwri As New StreamWriter(path_state, True)
        Dim current_state_string_1 As String
        Dim current_state_string_2 As String
        Dim polygon_id As String
        Dim tsle_string As String
        For i = 0 To lines_polygon.Length - 1
            current_state_string_1 = System.Convert.ToString(current_state(i))
            current_state_string_2 = System.Convert.ToString(current_perc(i))
            polygon_id = System.Convert.ToString(i + 1)
            tsle_string = System.Convert.ToString(time_since_last_eclosion(i))
            objwri.WriteLine(Now.ToShortDateString + ", " + polygon_id + ", " + current_state_string_1 + ", " + current_state_string_2 + ", " + tsle_string)
        Next
        objwri.Close()

        ''..........
        ''prévision
        ''..........
        'Dim inonde_prev As Integer
        'Dim new_state(lines_polygon.Length - 1) As Double 'MAX: etat d'aujourdhui, défini a l'aide des nouvelles mesures effectuées entre hier et aujourdhui
        'Dim new_perc(lines_polygon.Length - 1) As Double
        'Dim new_state_output(2) As Double

        'Dim path_state_previ = String.Concat(path_init, "etat_previ.csv")
        'Dim objwri2 As New StreamWriter(path_state_previ, True)

        'Dim t As Integer
        'Dim numdayprev = lines_previ.Length - 1 'moins 2?
        'For t = 0 To numdayprev
        '    For i = 0 To lines_polygon.Length - 1
        '        time_since_last_eclosion(i) = time_since_last_eclosion(i) + 1
        '        If niveau_vector_previ(t) >= tbl.Rows(i)(1) Then
        '            inonde_prev = 1
        '        Else
        '            inonde_prev = 0
        '        End If
        '        If inonde_prev = 1 Then
        '            'MAX:appliquer la fonction de développement larvaire
        '            If current_state(i) <> 5 Then
        '                new_state_output = function_state(current_state(i), current_perc(i), temp_vector_previ(t), time_since_last_eclosion(i))
        '                new_state(i) = new_state_output(0)
        '                new_perc(i) = new_state_output(1)
        '            Else
        '                If time_since_last_eclosion(i) < 15 Then '14 ou 15 ici????
        '                    new_state(i) = 5
        '                    new_perc(i) = 0
        '                Else
        '                    new_state(i) = 1
        '                    new_perc(i) = 0
        '                    time_since_last_eclosion(i) = 1 '1 ou 0 ici?
        '                End If
        '            End If
        '        Else
        '            new_state(i) = 0
        '            new_perc(i) = 0
        '        End If
        '    Next
        '    Dim new_state_string_1 As String
        '    Dim new_state_string_2 As String
        '    'Dim polygon_id As String
        '    'Dim tsle_string As String
        '    For i = 0 To lines_polygon.Length - 1
        '        new_state_string_1 = System.Convert.ToString(new_state(i))
        '        new_state_string_2 = System.Convert.ToString(new_perc(i))
        '        polygon_id = System.Convert.ToString(i + 1)
        '        tsle_string = System.Convert.ToString(time_since_last_eclosion(i))
        '        objwri2.WriteLine(Now.ToShortDateString + ", " + polygon_id + ", " + new_state_string_1 + ", " + new_state_string_2 + ", " + tsle_string)
        '    Next
        '    current_state = new_state
        '    current_perc = new_perc
        'Next
        'objwri2.Close()

    End Sub

    'MAX: T représente la température de la veille
    Function function_state(ByVal state_yesterday As Double, ByVal perc_yesterday As Double, ByVal T As Double, ByVal time_since_last_eclosion As Integer) As Double()

        'Declaration des variables de sortie
        Dim state_perc_today(2) As Double
        Dim state_today As Double
        Dim perc_today As Double

        'catégories de temperature
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
                            time_new_state = 0
                            state_today = 5
                            perc_today = 0
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




