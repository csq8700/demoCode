
param (
    [string]${SubjectName} ,
    [string]${Password},
    [string]${Path} 
)

${SubjectName} = "RSATESTCert"           #证书名
${Password} = "kpassw0rd"                #导出pfx证书密码
${Path} = "D:\${SubjectName}"
#设置pfx文件导出密码
${certPassword} = ConvertTo-SecureString -String "YourPassword" -Force -AsPlainText
#<#
#LocalMachine需要管理员权限

#创建证书
${Cert} = New-SelfSignedCertificate -CertStoreLocation Cert:\CurrentUser\My -Subject "CN=${SubjectName}" -KeyAlgorithm RSA -KeyLength 2048 -Provider "Microsoft Enhanced RSA and AES Cryptographic Provider" -NotBefore (Get-Date).AddYears(10)



#导出pfx证书（包含密钥）
Export-PfxCertificate  -Cert $cert -FilePath "${Path}.pfx" -Password $certPassword
#导出cer证书（仅含有公钥）
Export-Certificate -Cert $cert -FilePath "${Path}.cer" 
#>
# 导入pfx证书
Import-PfxCertificate -FilePath "${Path}.pfx" -CertStoreLocation Cert:\CurrentUser\My -Password $certPassword

#导入cer证书
# 导入 .cer 证书到当前用户的受信任根证书存储
Import-Certificate -FilePath "${Path}.cer"  -CertStoreLocation Cert:\CurrentUser\Root

#删除证书（cer，pfx一样）
Get-ChildItem -Path Cert:\CurrentUser\My | Where-Object { $_.Subject -eq "CN=${SubjectName}" } | Remove-Item

# 查看当前用户的个人证书存储
Get-ChildItem -Path Cert:\CurrentUser\My

# 查看当前用户的受信任根证书存储
Get-ChildItem -Path Cert:\CurrentUser\Root

#查询指定名称证书
#Get-ChildItem -Path Cert:\CurrentUser\My | Where-Object { $_.Subject -eq "CN=${SubjectName}" }

#系统支持加密提供程序查询
#Get-ChildItem -Path "HKLM:\SOFTWARE\Microsoft\Cryptography\Defaults\Provider"
