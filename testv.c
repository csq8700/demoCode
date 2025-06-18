Function SplitWithQuotes(line As String) As Variant
    Dim result() As String
    Dim inQuotes As Boolean
    Dim i As Long, char As String, currentField As String

    ReDim result(0)
    currentField = ""
    inQuotes = False

    For i = 1 To Len(line)
        char = Mid(line, i, 1)

        If char = """" Then
            inQuotes = Not inQuotes
        ElseIf char = "," And Not inQuotes Then
            result(UBound(result)) = currentField
            ReDim Preserve result(UBound(result) + 1)
            currentField = ""
        Else
            currentField = currentField & char
        End If
    Next i

    result(UBound(result)) = currentField ' 最后一列
    SplitWithQuotes = result
End Function

Sub MergeCsvFilesWithoutODBC()
    Dim folderPath As String
    folderPath = "C:\YourCSVFolder\" ' ← 修改为你的文件夹路径

    Dim fso As Object, folder As Object, file As Object
    Dim dict As Object: Set dict = CreateObject("Scripting.Dictionary")
    
    Set fso = CreateObject("Scripting.FileSystemObject")
    Set folder = fso.GetFolder(folderPath)

    Dim line As String, fields() As String
    Dim headerParsed As Boolean: headerParsed = False
    Dim headers() As String
    Dim fileNum As Integer
    Dim email As String, key As String
    Dim existingFields() As String
    
    For Each file In folder.Files
        If LCase(fso.GetExtensionName(file.Name)) = "csv" Then
            fileNum = FreeFile
            Open file.Path For Input As #fileNum

            Do While Not EOF(fileNum)
                Line Input #fileNum, line
                If Trim(line) = "" Then GoTo NextLine

                ' 如果是第一行（表头）
                If Not headerParsed Then
                    headers = Split(line, ",")
                    headerParsed = True
                    GoTo NextLine
                End If

                fields = SplitWithQuotes(line) ' 自定义函数，见下方说明
                email = Trim(fields(0)) ' 假设 Email 在第一个字段

                ' 字典中未存在该 Email
                If Not dict.Exists(email) Then
                    dict.Add email, fields
                Else
                    existingFields = dict(email)
                    ' 保留 IsPreferred=True 或 ImportDate 最新的记录
                    If fields(2) = "True" And existingFields(2) = "False" Then
                        dict(email) = fields
                    ElseIf CDate(fields(3)) > CDate(existingFields(3)) Then
                        dict(email) = fields
                    End If
                End If

NextLine:
            Loop
            Close #fileNum
        End If
    Next

    ' 输出到当前工作簿 Sheet1
    Dim ws As Worksheet
    Set ws = ThisWorkbook.Sheets("Sheet1")
    ws.Cells.Clear
    ws.Range("A1").Resize(1, UBound(headers) + 1).Value = headers

    Dim row As Long: row = 2
    Dim item As Variant
    For Each item In dict.Items
        ws.Cells(row, 1).Resize(1, UBound(item) + 1).Value = item
        row = row + 1
    Next
End Sub

Sub MergeCsvAndFilterToSheets()
    Dim fso As Object, folder As Object, file As Object
    Dim conn As Object, rs As Object
    Dim filePath As String
    Dim folderPath As String: folderPath = "C:\YourCSVFolder\"  ' ← 替换成你CSV目录
    Dim dict As Object: Set dict = CreateObject("Scripting.Dictionary")

    ' 遍历文件夹内所有 CSV 文件
    Set fso = CreateObject("Scripting.FileSystemObject")
    Set folder = fso.GetFolder(folderPath)

    For Each file In folder.Files
        If LCase(fso.GetExtensionName(file.Name)) = "csv" Then
            filePath = folderPath & file.Name

            ' 建立 CSV ODBC 连接
            Set conn = CreateObject("ADODB.Connection")
            conn.Open "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" & folderPath & ";Extended Properties='text;HDR=YES;FMT=Delimited'"
            Set rs = CreateObject("ADODB.Recordset")
            rs.Open "SELECT * FROM [" & file.Name & "]", conn, 1, 1

            Do Until rs.EOF
                Dim email As String: email = Trim(rs.Fields("Email").Value)
                Dim isPreferred As Boolean: isPreferred = rs.Fields("IsPreferred").Value
                Dim importDate As Date: importDate = rs.Fields("ImportDate").Value
                Dim currentRecord As Variant: currentRecord = rs.GetRows(1) ' 一行

                ' 如果字典中没有此 Email，直接添加
                If Not dict.exists(email) Then
                    dict.Add email, currentRecord
                Else
                    ' 已存在：根据条件决定是否替换
                    Dim existingData As Variant
                    existingData = dict(email)

                    ' 只替换满足“更优”的数据：IsPreferred 或 ImportDate 更新
                    If isPreferred = True And existingData(2, 0) = False Then
                        dict(email) = currentRecord
                    ElseIf importDate > existingData(3, 0) Then
                        dict(email) = currentRecord
                    End If
                End If

                rs.MoveNext
            Loop

            rs.Close: conn.Close
        End If
    Next

    ' 结果输出到 Excel
    Dim ws As Worksheet
    Set ws = ThisWorkbook.Sheets("Sheet1")
    ws.Cells.Clear
    ws.Range("A1:D1").Value = Array("Email", "Name", "IsPreferred", "ImportDate")

    Dim row As Long: row = 2
    Dim item As Variant
    For Each item In dict.Items
        ws.Cells(row, 1).Value = item(0, 0) ' Email
        ws.Cells(row, 2).Value = item(1, 0) ' Name
        ws.Cells(row, 3).Value = item(2, 0) ' IsPreferred
        ws.Cells(row, 4).Value = item(3, 0) ' ImportDate
        row = row + 1
    Next
End Sub
