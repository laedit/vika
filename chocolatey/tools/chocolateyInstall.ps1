$packageName = 'NVika'
$url = 'https://github.com/laedit/Vika/releases/download/v[version]/NVika.win-x86.[version].zip'

Install-ChocolateyZipPackage -PackageName "$packageName" -Url "$url" -UnzipLocation "$(Split-Path -parent $MyInvocation.MyCommand.Definition)" -Checksum '[checksum]' -ChecksumType 'sha256'
