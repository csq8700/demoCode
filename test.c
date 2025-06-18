Sub 合并CSV并去重保留指定条件()
    Dim folderPath As String
    Dim fileName As String
    Dim ws As Worksheet
    Dim lastRow As Long
    Dim fso As Object, ts As Object
    Dim line As String, fields As Variant
    Dim dict As Object
    Dim headerWritten As Boolean
    Dim i As Long

    ' 设定CSV文件夹路径
    folderPath = "C:\MyCSVFiles\"  ' << 修改为你的CSV文件夹路径
    If Right(folderPath, 1) <> "\" Then folderPath = folderPath & "\"

    Set ws = ThisWorkbook.Sheets(1)
    ws.Cells.ClearContents

    Set dict = CreateObject("Scripting.Dictionary")
    headerWritten = False

    fileName = Dir(folderPath & "*.csv")
    Do While fileName <> ""
        Set fso = CreateObject("Scripting.FileSystemObject")
        Set ts = fso.OpenTextFile(folderPath & fileName, 1, False, -1) ' -1 表示自动识别编码 (含 UTF-8)

        i = 0
        Do Until ts.AtEndOfStream
            line = ts.ReadLine
            fields = Split(line, ",")

            ' 写入表头
            If i = 0 And Not headerWritten Then
                For j = 0 To UBound(fields)
                    ws.Cells(1, j + 1).Value = fields(j)
                Next
                headerWritten = True
            ElseIf i > 0 Then
                Dim 编号 As String, 状态 As String
                编号 = Trim(fields(0))
                状态 = Trim(fields(2))

                ' 如果未存在，或当前是“最新”状态，则保留
                If Not dict.exists(编号) Then
                    dict(编号) = fields
                ElseIf 状态 = "最新" Then
                    dict(编号) = fields
                End If
            End If
            i = i + 1
        Loop

        ts.Close
        Set ts = Nothing
        Set fso = Nothing
        fileName = Dir
    Loop

    ' 将去重后的内容写入表格
    lastRow = 2
    Dim key As Variant
    For Each key In dict.Keys
        fields = dict(key)
        For j = 0 To UBound(fields)
            ws.Cells(lastRow, j + 1).Value = fields(j)
        Next
        lastRow = lastRow + 1
    Next

    MsgBox "合并完成，共写入 " & dict.Count & " 条记录", vbInformation
End Sub