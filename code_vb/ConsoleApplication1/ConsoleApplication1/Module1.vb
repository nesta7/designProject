'MAX: Module qui sert à faire fonctionner la fonction messagebox.
Imports System.Windows.Forms
Module Module1
    Sub Main()

        '--------------------------------------------------------------------------------------------------------------
        'Import des zones inondables (identifiant du polygone, altitude de la zone concernée, et infos de connectivité 
        'si on tient compte de la théorie des débordements)
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
        MessageBox.Show(tbl.Rows(0)(1))

        '--------------------------------------------------------------------------------------------------------------
        'import des infos de hauteur d'eau et de température du passé
        '--------------------------------------------------------------------------------------------------------------
        Dim path2 = "C:\Users\max\Documents\cours MA2\design project\dossier_partage\code_vb\Hauteur_Temp.csv"
        Dim lines2 = IO.File.ReadAllLines(path2)
        Dim tbl2 = New DataTable
        tbl2.Columns.Add(New DataColumn("Date", GetType(String)))
        tbl2.Columns.Add(New DataColumn("Niveau", GetType(Double)))
        tbl2.Columns.Add(New DataColumn("Temperature", GetType(Double)))

        Dim i = 0
        For Each line In lines2
            If i <> 0 Then
                Dim objFields2 = From field In line.Split(";"c)
                             Select CType(field, Object)
                Dim newRow = tbl2.Rows.Add()
                newRow.ItemArray = objFields2.ToArray()
            End If
            i = 1
        Next
        MessageBox.Show(tbl2.Rows(0)(2))

        'verification que l'on peut utiliser les tbl2.Rows(...)(...) comme n'importe quelle variable (c'est vérifié normalement)
        If tbl2.Rows(0)(2) = 99.99 Then
            i = 5
        End If
        Console.WriteLine("i est egal a" & i)
        Console.Read()
    End Sub


End Module
