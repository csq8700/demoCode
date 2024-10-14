$xmlFile = "D:\Downloads\test.xml"
# 生成用户名和密码（初次执行）
$username = "your-username"
$password = "bbbbb"

# 将明文密码转换为加密的 SecureString
$securePassword = ConvertTo-SecureString $password -AsPlainText -Force

# 将 SecureString 密码加密并转换为字符串
$encryptedPassword = $securePassword | ConvertFrom-SecureString

# 创建 XML 文档并保存用户名和加密后的密码
$xml = New-Object System.Xml.XmlDocument
$root = $xml.CreateElement("Credentials")
$xml.AppendChild($root)

$userNode = $xml.CreateElement("Username")
$userNode.InnerText = $username
$root.AppendChild($userNode)

$passNode = $xml.CreateElement("Password")
$passNode.InnerText = $encryptedPassword
$root.AppendChild($passNode)

# 保存到 XML 文件
$xml.Save($xmlFile)

# 从 XML 文件读取用户名和加密的密码
[xml]$xml = Get-Content $xmlFile
$username = $xml.Credentials.Username
$encryptedPassword = $xml.Credentials.Password

# 将加密的密码转换为 SecureString
$securePassword = ConvertTo-SecureString $encryptedPassword

# 将 SecureString 转换回明文密码
$password2 = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto([System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword))
Write-Host $password2
