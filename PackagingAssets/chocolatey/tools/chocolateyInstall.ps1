$packageName = 'NVika'
$url = 'https://github.com/laedit/vika/releases/download/{{tag}}/NVika.{{version}}.zip'

Install-ChocolateyZipPackage "$packageName" "$url" "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"