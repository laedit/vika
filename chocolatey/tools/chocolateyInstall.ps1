$packageName = 'NVika'
$url = 'https://github.com/laedit/Vika/releases/download/v[version]/NVika.win-x86.[version].zip'

Install-ChocolateyZipPackage "$packageName" "$url" "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
