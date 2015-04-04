Imports System.Windows.Forms
Module Module1
    Sub Main()
        Dim path As String = "C:\Users\max\Documents\cours MA2\design project\dossier_partage\code_vb\Hauteur_Temp.csv"
        Dim lines = IO.File.ReadAllLines(path)
        Dim tbl = New DataTable
        Dim colCount = lines.First.Split(","c).Length
        For i As Int32 = 1 To colCount
            tbl.Columns.Add(New DataColumn("Column_" & i, GetType(Int32)))
        Next
        For Each line In lines
            Dim objFields = From field In line.Split(","c)
                         Select CType(Int32.Parse(field), Object)
            Dim newRow = tbl.Rows.Add()
            newRow.ItemArray = objFields.ToArray()
        Next
        MessageBox.Show(tbl.Rows(0)(1))

    End Sub

End Module
