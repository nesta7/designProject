'MAX: Module qui sert à faire fonctionner la fonction messagebox.
Imports System.Windows.Forms
Module Module1
    Sub Main()

        '--------------------------------------------------------------------------------------------------------------
        'Import des zones inondables (identifiant du polygone, altitude de la zone concernée, et infos de connectivité 
        'si on tient compte de la théorie des débordements).
        '--------------------------------------------------------------------------------------------------------------
        Dim path = "C:\Users\max\Documents\cours MA2\design project\dossier_partage\code_vb\polygones.csv"
        Dim lines = IO.File.ReadAllLines(path)
        Dim tbl = New DataTable
        Dim colCount = lines.First.Split(","c).Length
        'MAX: les trois lignes commentées suivantes sont les lignes d'origine du code trouvé sur internet, texte moi 
        'si tu veux que je te donne la source ^^
        'For i As Int32 = 1 To colCount
        'tbl.Columns.Add(New DataColumn("Column_" & i, GetType(Int32)))
        'Next
        tbl.Columns.Add(New DataColumn("Polygone_id", GetType(Int32)))
        tbl.Columns.Add(New DataColumn("Altitude", GetType(Double)))
        For Each line In lines
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
        'passe
        '*****************************
        Dim path2 = "C:\Users\max\Documents\cours MA2\design project\dossier_partage\code_vb\Hauteur_Temp_passe.csv"
        Dim lines2 = IO.File.ReadAllLines(path2)
        Dim tbl2 = New DataTable
        tbl2.Columns.Add(New DataColumn("Date", GetType(String)))
        tbl2.Columns.Add(New DataColumn("Niveau", GetType(Double)))
        tbl2.Columns.Add(New DataColumn("Temperature", GetType(Double)))

        Dim date_vector_past As New List(Of String)
        Dim niveau_vector_past As New List(Of Double)
        Dim temp_vector_past As New List(Of Double)

        'Fractionnement des éléments de date_vector en differentes variables
        Dim jour_past As New List(Of Integer)
        Dim mois_past As New List(Of Integer)
        Dim annee_past As New List(Of Integer)

        i = 0 'MAX: variable servant à ignorer la première ligne
        j = 0

        For Each line In lines2
            If i <> 0 Then
                Dim objFields2 = From field In line.Split(";"c)
                             Select CType(field, Object)
                Dim newRow = tbl2.Rows.Add()
                newRow.ItemArray = objFields2.ToArray()
                date_vector_past.Add(tbl2.Rows(j)(0))
                jma_s = date_vector_past(j).Split(New Char() {"-"c})
                jour_split = jma_s(2).Split(New Char() {" "c}) ' ca sert à enlever 

                'moment des mesures
                annee_past.Add(Integer.Parse(jma_s(0)))
                mois_past.Add(Integer.Parse(jma_s(1)))
                jour_past.Add(Integer.Parse(jour_split(0)))
                'mesures
                niveau_vector_past.Add(tbl2.Rows(j)(1))
                temp_vector_past.Add(tbl2.Rows(j)(2))
                j = j + 1
            End If
            i = 1
        Next

        '*****************************
        'prévisions
        '*****************************
        Dim path3 = "C:\Users\max\Documents\cours MA2\design project\dossier_partage\code_vb\Hauteur_Temp_previ.csv"
        Dim lines3 = IO.File.ReadAllLines(path3)
        Dim tbl3 = New DataTable
        tbl3.Columns.Add(New DataColumn("Date", GetType(String)))
        tbl3.Columns.Add(New DataColumn("Niveau", GetType(Double)))
        tbl3.Columns.Add(New DataColumn("Temperature", GetType(Double)))

        Dim date_vector_previ As New List(Of String)
        Dim niveau_vector_previ As New List(Of Double)
        Dim temp_vector_previ As New List(Of Double)

        'Fractionnement des éléments de date_vector en differentes variables
        Dim jour_previ As New List(Of Integer)
        Dim mois_previ As New List(Of Integer)
        Dim annee_previ As New List(Of Integer)

        i = 0 'MAX: variable servant à ignorer la première ligne
        j = 0
        For Each line In lines3
            If i <> 0 Then
                Dim objFields3 = From field In line.Split(";"c)
                             Select CType(field, Object)
                Dim newRow = tbl3.Rows.Add()
                newRow.ItemArray = objFields3.ToArray()
                date_vector_previ.Add(tbl3.Rows(j)(0))
                jma_s = date_vector_previ(j).Split(New Char() {"-"c})
                jour_split = jma_s(2).Split(New Char() {" "c}) ' ca sert à enlever 

                'moment des mesures
                annee_previ.Add(Integer.Parse(jma_s(0)))
                mois_previ.Add(Integer.Parse(jma_s(1)))
                jour_previ.Add(Integer.Parse(jour_split(0)))
                'mesures
                niveau_vector_previ.Add(tbl2.Rows(j)(1))
                temp_vector_previ.Add(tbl2.Rows(j)(2))
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
        Dim inonde As Integer()
        Dim niveau_jour_t

        inonde = New Integer(lines.Length - 1) {}

        For t = 0 To lines2.Length + lines3.Length - 2 'MAX: modifier si on a plusieurs donnees par jour
            If t < lines2.Length Then
                niveau_jour_t = niveau_vector_past(t)
            ElseIf t >= lines2.Length Then
                niveau_jour_t = niveau_vector_previ(t)
            End If

            For i = 0 To lines.Length - 1
                If niveau_jour_t >= tbl.Rows(i)(1) Then
                    inonde(i) = 1 'MAX: on pourrait peut-etre ajouter une dimension à inonde histoire d'avoir dans un seul array les informations sur les zones inondees pour chaque jour considere
                End If
            Next
            Console.Read()
        Next

    End Sub


End Module
