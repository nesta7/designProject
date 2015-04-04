Imports System.Windows.Forms
Module Module1
    Sub Main()
        Dim path = "C:\Users\max\Documents\cours MA2\design project\dossier_partage\code_vb\polygones.csv"
        Dim lines = IO.File.ReadAllLines(path)
        Dim tbl = New DataTable
        Dim colCount = lines.First.Split(","c).Length
        'MAX: les trois lignes commentées suivantes sont les lignes d'origine du code trouvé sur internet, texte moi si tu veux que je te donne la source ^^
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

    End Sub

End Module
