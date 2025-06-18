Sub MergeCsvFiles_NoODBC()
    Dim folderPath As String
    Dim fileName As String
    Dim line As String
    Dim header As String
    Dim outputFile As String
    Dim fileNum As Integer
    Dim fso As Object
    Dim uniqueDict As Object ' Scripting.Dictionary
    Dim keyFieldIndex As Integer
    Dim columns() As String
    Dim key As String

    folderPath = "C:\CSVFiles\" ' CSV 文件夹路径
    outputFile = "C:\CSVFiles\Merged_Unique.csv" ' 输出文件路径
    keyFieldIndex = 0 ' 假设第1列为去重的关键字段（从0开始）

    Set uniqueDict = CreateObject("Scripting.Dictionary")
    Set fso = CreateObject("Scripting.FileSystemObject")
    fileName = Dir(folderPath & "*.csv")
    
    ' 删除旧输出文件
    If fso.FileExists(outputFile) Then Kill outputFile

    Do While fileName <> ""
        fileNum = FreeFile
        Open folderPath & fileName For Input As #fileNum
        Line Input #fileNum, header ' 读取第一行标题

        Do While Not EOF(fileNum)
            Line Input #fileNum, line
            If Trim(line) = "" Then GoTo SkipLine

            columns = ParseCsvLine(line)
            If UBound(columns) < keyFieldIndex Then GoTo SkipLine

            key = Trim(columns(keyFieldIndex))

            ' 若未出现过该 key，则添加
            If Not uniqueDict.Exists(key) Then
                uniqueDict.Add key, line
            End If

SkipLine:
        Loop

        Close #fileNum
        fileName = Dir
    Loop

    ' 输出合并数据
    fileNum = FreeFile
    Open outputFile For Output As #fileNum
    Print #fileNum, header

    For Each key In uniqueDict.Keys
        Print #fileNum, uniqueDict(key)
    Next

    Close #fileNum
    MsgBox "CSV 合并完成，输出文件：" & outputFile
End Sub

Function ParseCsvLine(line As String) As Variant
    Dim inQuotes As Boolean
    Dim c As String, i As Long
    Dim result As Collection
    Dim token As String
    Set result = New Collection
    
    inQuotes = False
    token = ""

    For i = 1 To Len(line)
        c = Mid(line, i, 1)
        Select Case c
            Case """"
                inQuotes = Not inQuotes
                token = token & c
            Case ","
                If inQuotes Then
                    token = token & c
                Else
                    result.Add Trim(StripQuotes(token))
                    token = ""
                End If
            Case Else
                token = token & c
        End Select
    Next

    result.Add Trim(StripQuotes(token)) ' 最后一个字段
    ParseCsvLine = CollectionToArray(result)
End Function

Function StripQuotes(s As String) As String
    If Left(s, 1) = """" And Right(s, 1) = """" Then
        StripQuotes = Mid(s, 2, Len(s) - 2)
    Else
        StripQuotes = s
    End If
End Function

Function CollectionToArray(col As Collection) As Variant
    Dim arr() As String
    Dim i As Long
    ReDim arr(0 To col.Count - 1)
    For i = 1 To col.Count
        arr(i - 1) = col(i)
    Next
    CollectionToArray = arr
End Function
