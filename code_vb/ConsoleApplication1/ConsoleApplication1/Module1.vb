Imports System.Windows.Forms 'MAX: Module qui sert à faire fonctionner la fonction messagebox.
Module Module1
    Sub Main()

        '**************************************************************************************************************
        'Import des zones inondables (identifiant du polygone, altitude de la zone concernée, et infos de connectivité 
        'si on tient compte de la théorie des débordements).
        '**************************************************************************************************************
        Dim path_polygon = "C:\Users\max\Documents\cours MA2\design project\dossier_partage\code_vb\polygones.csv"
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
        'code de telle sorte qu'il y ait une mesure par heure pour le passé, et 2 par jour pour les prévisions!!
        '**************************************************************************************************************

        '-----------------------------
        'Initialisation des variables
        '-----------------------------
        'MAX: jma pour jour mois annee
        Dim jma As String()
        'MAX: jour_split permet de dissocier le jour et l'heure de la mesure
        Dim jour_split As String()

        Dim i
        Dim j

        '-----------------------------
        'passé
        '-----------------------------
        'Lecture du csv
        Dim path_past = "C:\Users\max\Documents\cours MA2\design project\dossier_partage\code_vb\Hauteur_Temp_passe.csv"
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
        j = 0

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
        Dim path_previ = "C:\Users\max\Documents\cours MA2\design project\dossier_partage\code_vb\Hauteur_Temp_previ.csv"
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
        j = 0

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

        'MAX: approche 1) toutes les mesures a disposition sont calculées à chaque run du programme
        Dim inonde_past(lines_past.Length - 2, lines_polygon.Length - 1) As Integer 'MAX: plus tard on aura pas besoin de garder cette information. Juste les quelques dernières lignes. Celles d'avant auront déjà été prises en compte.
        'MAX: pour la prochaine variable: 1ere dimension = temps. 2eme dimension = polygone, 3eme dimension = stade en cours et pourcentage d'avancement du stade
        Dim state_time(lines_past.Length - 2, lines_polygon.Length - 1, 2) As Double
        'Dim state_output(2) As integer

        'initialiser l'etat du premier jour à 0
        'pas besoin, c'est fait automatiquement.

        For t = 0 To lines_past.Length - 2 'MAX: on fait -2 car la première ligne ne doit pas etre considérée puisque c'est les titres de colonnes et parce que l'on part de 0
            For i = 0 To lines_polygon.Length - 1
                If niveau_vector_past(t) >= tbl.Rows(i)(1) Then
                    inonde_past(t, i) = 1
                End If
                If inonde_past(t, i) = 1 Then
                    If t <> 0 Then
                        'MAX:appliquer la fonction de developpement larvaire 
                        'state_output=function_state(state_time(t-1,i,0), state_time(t-1,i,1), temp_vector_past(t-1))
                        'state_time(t,i,0)=state_output(0)
                        'state_time(t,i,1)=state_output(1)
                    Else
                        'state_time(t,i,0)=valeur_arbitraire ??
                        'state_time(t,i,0)=valeur_arbitraire ??
                    End If
                Else
                    'state_time(t,i,0)=0
                    'state_time(t,i,1)=0
                End If
            Next
        Next

        ''MAX: approche 2) Seules les mesures de la veille sont calculées, les mesures précédentes étant prises en compte par le fichier sauvegardé (voir les lignes suivantes)

        'Dim inonde_hier(lines_polygon.Length - 1) As Integer
        ''MAX: pour les 2 prochaines variables: 1ere dimension = polygone, 2eme dimension = stade en cours et pourcentage d'avancement du stade
        'Dim state_yesterday(lines_polygon.Length - 1, 2) As Integer 'MAX: variable a importer d'un fichier qui aura ete sauvegardé la veille
        ''state_yesterday=... (import de la variable sauvegardée la veille)
        'Dim current_state(lines_polygon.Length - 1, 2) As Integer 'MAX: etat d'aujourdhui, défini a l'aide des nouvelles mesures effectuées entre hier et aujourdhui
        'Dim current_state_output(2) As Integer

        'For i = 0 To lines_polygon.Length - 1
        '    If niveau_vector_past(lines_past.Length - 2) >= tbl.Rows(i)(1) Then
        '        inonde_hier(i) = 1
        '    End If
        '    If inonde_hier(i) = 1 Then
        '        'MAX:appliquer la fonction de developpement larvaire 
        '        'current_state_output=function_state(state_yesterday(i,0), state_yesterday(i,1), temp_vector_past(lines_past.Length - 2))
        '        'current_state(i,0)=current_state_output(0)
        '        'current_state(i,1)=current_state_output(1)
        '    Else
        '        'current_state(i, 0) = 0
        '        'current_state(i, 0) = 0
        '    End If
        'Next

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
        Dim temp() As Double = {15, 20, 25, 30, 35}
        Console.WriteLine(temp.Length)
        Console.Read()
    End Sub

    'MAX: T représente la température de la veille
    Function function_state(ByVal state_yesterday As Double, ByVal perc_yesterday As Double, ByVal T As Double) As Double()

        'Declaration des variables de sortie
        Dim state_perc_today As Double()
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
                        time_new_state = (perc_incr + perc_yesterday - 1) * (1 / perc_incr)
                        state_today = state_yesterday + 1
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
    '---------------------------------------------------------

    Function LarvaModel(ByVal cs As Integer, ByVal T As Double) As Double

        Dim Prop As Double = 0.0
        Dim i As Integer
        Dim j As Integer
        Dim noCol As Integer = 4
        Dim Model(4, noCol) As Double

        'Reference at the bottom
        Dim temp() As Double = {15, 20, 25, 30, 35}

        'Durée stades vexans (voir code matlab "adaptation_albopictus_vexans.m")
        Dim L1() As Double = {3.4, 2.4, 1.5, 0.9, 0.7}
        Dim L2() As Double = {2, 1.1, 0.9, 0.9, 0.5}
        Dim L3() As Double = {2.8, 1.7, 0.9, 0.9, 1}
        Dim L4() As Double = {8.1, 3.3, 2.4, 2, 3}

        'Durée stades albopictus
        'Dim L1() As Double = {5.6, 3.0, 2.1, 1.4, 1.7}
        'Dim L2() As Double = {3.3, 1.4, 1.2, 1.3, 1.2}
        'Dim L3() As Double = {4.6, 2.1, 1.2, 1.4, 2.4}
        'Dim L4() As Double = {13.4, 4.1, 3.3, 3.0, 6.8}

        'Could probably be improved
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
                    'then compute the actual proportion of growing since the simulation is at a daily scale
                    Prop = 1 / (Model(cs, i) + (T - Model(0, i)) / (Model(0, i + 1) - Model(0, i)) * (Model(cs, i + 1) - Model(cs, i)))
                End If
            Next
        End If
        'Display test
        'For i = 0 To 4
        '   For j= 0 To noCol
        '      Console.WriteLine("Model "& Model(i,j))
        ' Next
        'Next

        Return Prop
    End Function

    'Function that modify the Percentage in the LarvaStates and return the new one after applying the LarvalModel to each require state
    'By Morgan Bruhin
    Function ChangeState(ByVal PercLarvaState() As Double, ByVal T As Double) As Double()
        Dim i As Integer
        Dim index As Integer = -1
        For i = 0 To 3
            'if the larva aren't present at the current state i then the percentage is -1
            If PercLarvaState(i) <> -1 Then
                'Increase the Larva growth percentage
                PercLarvaState(i) = PercLarvaState(i) + LarvaModel(i, T)
                'if the percentage is up to 1 the state is complete
                If PercLarvaState(i) >= 1.0 Then
                    'if the state isn't the last one activate the next
                    If i < 3 Then
                        If PercLarvaState(i + 1) = -1 Then
                            index = (i + 1)
                        Else
                            'TODO : Decide what to do if a younger population arrive befor the previous fully grows
                        End If
                    Else
                        'TODO : decide what to do exactly for T4 fully grow
                    End If
                    'At the end : desactivate the current state
                    PercLarvaState(i) = -1
                End If
                'Activate the next state without computing a new day
            ElseIf i = index Then
                PercLarvaState(i) = 0 ' TODO : Compute the difference with the previous state and compute a proportional increase
            End If
        Next

        Return PercLarvaState
    End Function

    ''VERSION-DE-BASE-DE-MORGAN-VERSION-DE-BASE-DE-MORGAN-VERSION-DE-BASE-DE-MORGAN-VERSION-DE-BASE-DE-MORGAN-VERSION-DE-BASE-DE-MORGAN-VERSION-DE-BASE-DE-MORGAN
    ''Function updating the lavaire state
    ''By Morgan Bruhin
    ''PS didn't use "degre-jour" didn't understand well enought it
    ''Using percentage of grow per state

    ''Advantage : would allow to have more than one state per polygone (more than only one larval type)

    ''Function that compute the Increase percentage for state cs at temperature T for 1 day
    'Function LarvaModel(ByVal cs As Integer, ByVal T As Double) As Double

    '    Dim Prop As Double = 0.0
    '    Dim i As Integer
    '    Dim j As Integer
    '    Dim noCol As Integer = 4
    '    Dim Model(4, noCol) As Double

    '    'Reference at the bottom
    '    Dim temp() As Double = {15, 20, 25, 30, 35}
    '    Dim L1() As Double = {5.6, 3.0, 2.1, 1.4, 1.7}
    '    Dim L2() As Double = {3.3, 1.4, 1.2, 1.3, 1.2}
    '    Dim L3() As Double = {4.6, 2.1, 1.2, 1.4, 2.4}
    '    Dim L4() As Double = {13.4, 4.1, 3.3, 3.0, 6.8}

    '    'Could probably be inproved
    '    For i = 0 To noCol
    '        Model(0, i) = temp(i)
    '        Model(1, i) = L1(i)
    '        Model(2, i) = L2(i)
    '        Model(3, i) = L3(i)
    '        Model(4, i) = L4(i)
    '    Next
    '    If T < Model(0, 0) Or T > Model(0, noCol) Then
    '        Console.WriteLine("Temperature " & T & "°C out of range [ " & Model(0, 0) & ":" & Model(0, noCol) & " ]")
    '    Else
    '        For i = 0 To noCol - 1
    '            If T >= temp(i) And T < temp(i + 1) Then
    '                'Compute a linear estimation of the require number of days to grow up
    '                'then compute the actual proportion of growing since the simulation is at a daily scale
    '                Prop = 1 / (Model(cs, i) + (T - Model(0, i)) / (Model(0, i + 1) - Model(0, i)) * (Model(cs, i + 1) - Model(cs, i)))
    '            End If
    '        Next
    '    End If
    '    'Display test
    '    'For i = 0 To 4
    '    '   For j= 0 To noCol
    '    '      Console.WriteLine("Model "& Model(i,j))
    '    ' Next
    '    'Next

    '    Return Prop
    'End Function

    ''Should be a method but I'mnot good at it
    ''Function that modify the Percentage in the LarvaStates and return the new one after applying the LarvalModel to each require state
    ''By Morgan Bruhin
    'Function ChangeState(ByVal PercLarvaState() As Double, ByVal T As Double) As Double()
    '    Dim i As Integer
    '    Dim index As Integer = -1
    '    For i = 0 To 3
    '        'if the larva aren't present at the current state i then the percentage is -1
    '        If PercLarvaState(i) <> -1 Then
    '            'Increase the Larva growth percentage
    '            PercLarvaState(i) = PercLarvaState(i) + LarvaModel(i, T)
    '            'if the percentage is up to 1 the state is complete
    '            If PercLarvaState(i) >= 1.0 Then
    '                'if the state isn't the last one activate the next
    '                If i < 3 Then
    '                    If PercLarvaState(i + 1) = -1 Then
    '                        index = (i + 1)
    '                    Else
    '                        'TODO : Decide what to do if a younger population arrive befor the previous fully grows
    '                    End If
    '                Else
    '                    'TODO : decide what to do exactly for T4 fully grow
    '                End If
    '                'At the end : desactivate the current state
    '                PercLarvaState(i) = -1
    '            End If
    '            'Activate the next state without computing a new day
    '        ElseIf i = index Then
    '            PercLarvaState(i) = 0 ' TODO : Compute the difference with the previous state and compute a proportional increase
    '        End If
    '    Next

    '    Return PercLarvaState
    'End Function
    ''VERSION-DE-BASE-DE-MORGAN-VERSION-DE-BASE-DE-MORGAN-VERSION-DE-BASE-DE-MORGAN-VERSION-DE-BASE-DE-MORGAN-VERSION-DE-BASE-DE-MORGAN-VERSION-DE-BASE-DE-MORGAN


End Module




