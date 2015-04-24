'MAX: Module qui sert à faire fonctionner la fonction messagebox.
Imports System.Windows.Forms
Module Module1
    'Main fait par MAX MENTHA BITCHES
    Sub Main()

        '--------------------------------------------------------------------------------------------------------------
        'Import des zones inondables (identifiant du polygone, altitude de la zone concernée, et infos de connectivité 
        'si on tient compte de la théorie des débordements).
        '--------------------------------------------------------------------------------------------------------------
        Dim path_polygon = "C:\Users\max\Documents\cours MA2\design project\dossier_partage\code_vb\polygones.csv"
        Dim lines_polygon = IO.File.ReadAllLines(path_polygon)
        Dim tbl = New DataTable
        Dim colCount = lines_polygon.First.Split(","c).Length
        'MAX: les trois lignes commentées suivantes sont les lignes d'origine du code trouvé sur internet, texte moi 
        'si tu veux que je te donne la source ^^
        'For i As Int32 = 1 To colCount
        'tbl.Columns.Add(New DataColumn("Column_" & i, GetType(Int32)))
        'Next
        tbl.Columns.Add(New DataColumn("Polygone_id", GetType(Int32)))
        tbl.Columns.Add(New DataColumn("Altitude", GetType(Double)))
        For Each line In lines_polygon
            Dim objFields = From field In line.Split(","c)
                         Select CType(field, Object)
            'Select CType(Int32.Parse(field), Object)
            Dim newRow = tbl.Rows.Add()
            newRow.ItemArray = objFields.ToArray()
        Next

        '--------------------------------------------------------------------------------------------------------------
        'import des infos de hauteur d'eau et de température du passé et de prévision.  attention! il faut modifier le 
        'code de telle sorte qu'il y ait une mesure par heure pour le passé, et 2 par jour pour les prévisions!!
        '--------------------------------------------------------------------------------------------------------------

        '*****************************
        'Initialisation des variables
        '*****************************
        Dim jma_s As String()
        Dim jour_split As String()

        Dim i
        Dim j

        '*****************************
        'passé
        '*****************************
        Dim path_past = "C:\Users\max\Documents\cours MA2\design project\dossier_partage\code_vb\Hauteur_Temp_passe.csv"
        Dim lines_past = IO.File.ReadAllLines(path_past)
        Dim tbl2 = New DataTable
        tbl2.Columns.Add(New DataColumn("Date", GetType(String)))
        tbl2.Columns.Add(New DataColumn("Niveau", GetType(Double)))
        tbl2.Columns.Add(New DataColumn("Temperature", GetType(Double)))

        Dim date_vector_past(lines_past.Length) As String
        Dim niveau_vector_past(lines_past.Length) As Double
        Dim temp_vector_past(lines_past.Length) As Double

        'Fractionnement des éléments de date_vector en differentes variables
        Dim jour_past(lines_past.Length) As Integer
        Dim mois_past(lines_past.Length) As Integer
        Dim annee_past(lines_past.Length) As Integer

        i = 0 'MAX: variable servant à ignorer la première ligne
        j = 0

        For Each line In lines_past
            If i <> 0 Then
                Dim objFields2 = From field In line.Split(";"c)
                             Select CType(field, Object)
                Dim newRow = tbl2.Rows.Add()
                newRow.ItemArray = objFields2.ToArray()
                date_vector_past(j) = tbl2.Rows(j)(0)
                'date_vector_past.Add(tbl2.Rows(j)(0))
                jma_s = date_vector_past(j).Split(New Char() {"-"c})
                jour_split = jma_s(2).Split(New Char() {" "c}) ' ca sert à enlever les elements inutiles

                'moment des mesures
                annee_past(j) = Integer.Parse(jma_s(0))
                'annee_past.Add(Integer.Parse(jma_s(0)))
                mois_past(j) = Integer.Parse(jma_s(1))
                jour_past(j) = Integer.Parse(jour_split(0))
                'mesures
                niveau_vector_past(j) = tbl2.Rows(j)(1)
                temp_vector_past(j) = tbl2.Rows(j)(2)
                j = j + 1
            End If
            i = 1
        Next

        '*****************************
        'prévisions
        '*****************************
        Dim path_previ = "C:\Users\max\Documents\cours MA2\design project\dossier_partage\code_vb\Hauteur_Temp_previ.csv"
        Dim lines_previ = IO.File.ReadAllLines(path_previ)
        Dim tbl3 = New DataTable
        tbl3.Columns.Add(New DataColumn("Date", GetType(String)))
        tbl3.Columns.Add(New DataColumn("Niveau", GetType(Double)))
        tbl3.Columns.Add(New DataColumn("Temperature", GetType(Double)))

        Dim date_vector_previ(lines_previ.Length) As String
        Dim niveau_vector_previ(lines_previ.Length) As Double
        Dim temp_vector_previ(lines_previ.Length) As Double

        'Fractionnement des éléments de date_vector en differentes variables
        Dim jour_previ(lines_previ.Length) As Integer
        Dim mois_previ(lines_previ.Length) As Integer
        Dim annee_previ(lines_previ.Length) As Integer

        i = 0 'MAX: variable servant à ignorer la première ligne
        j = 0
        For Each line In lines_previ
            If i <> 0 Then
                Dim objFields3 = From field In line.Split(";"c)
                             Select CType(field, Object)
                Dim newRow = tbl3.Rows.Add()
                newRow.ItemArray = objFields3.ToArray()
                date_vector_previ(j) = tbl3.Rows(j)(0)
                jma_s = date_vector_previ(j).Split(New Char() {"-"c})
                jour_split = jma_s(2).Split(New Char() {" "c}) ' ca sert à enlever les elements inutiles

                'moment des mesures
                annee_previ(j) = Integer.Parse(jma_s(0))
                mois_previ(j) = Integer.Parse(jma_s(1))
                jour_previ(j) = Integer.Parse(jour_split(0))
                'mesures
                niveau_vector_previ(j) = tbl2.Rows(j)(1)
                temp_vector_previ(j) = tbl2.Rows(j)(2)
                j = j + 1
            End If
            i = 1
        Next
        '--------------------------------------------------------------------------------------------------------------
        'boucle principale de simulation
        '--------------------------------------------------------------------------------------------------------------

        '*****************************
        'simulation du niveau de l'eau
        '*****************************
        Dim inonde_past(lines_past.Length, lines_polygon.Length) As Integer
        Dim inonde_previ(lines_previ.Length, lines_polygon.Length) As Integer
        Dim niveau_t

        For t = 0 To lines_past.Length - 2 'MAX: on fait -2 car la première ligne ne doit pas etre considérée puisque c'est les titres de colonnes et parce que l'on part de 0
            niveau_t = niveau_vector_past(t)
            For i = 0 To lines_polygon.Length - 1
                If niveau_t >= tbl.Rows(i)(1) Then
                    inonde_past(t, i) = 1
                End If
            Next
        Next

        For t = 0 To lines_previ.Length - 2
            niveau_t = niveau_vector_previ(t)

            For i = 0 To lines_polygon.Length - 1
                If niveau_t >= tbl.Rows(i)(1) Then
                    inonde_previ(t, i) = 1
                End If
            Next
        Next
        Console.WriteLine(inonde_past)
        Console.WriteLine(mois_past(3))
        Console.Read()
    End Sub


    'Function updating the lavaire state
    'By Morgan Bruhin
    'PS didn't use "degre-jour" didn't understand well enought it
    'Using percentage of grow per state

    'Advantage : would allow to have more than one state per polygone (more than only one larval type)

    'Function that compute the Increase percentage for state cs at temperature T for 1 day
    Function LarvaModel(ByVal cs As Integer, ByVal T As Double) As Double

        Dim Prop As Double = 0.0
        Dim i As Integer
        Dim j As Integer
        Dim noCol As Integer = 4
        Dim Model(4, noCol) As Double

        'Reference at the bottom
        Dim temp() As Double = {15, 20, 25, 30, 35}
        Dim L1() As Double = {5.6, 3.0, 2.1, 1.4, 1.7}
        Dim L2() As Double = {3.3, 1.4, 1.2, 1.3, 1.2}
        Dim L3() As Double = {4.6, 2.1, 1.2, 1.4, 2.4}
        Dim L4() As Double = {13.4, 4.1, 3.3, 3.0, 6.8}

        'Could probably be inproved
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

    'Should be a method but I'mnot good at it
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

End Module




