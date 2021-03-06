function ZipFiles( $zipfilename, $sourcedir )
{
   Add-Type -Assembly System.IO.Compression.FileSystem
   Write-Output($zipfilename)
   [System.IO.Compression.ZipFile]::CreateFromDirectory($sourcedir, $zipfilename)
}

ZipFiles "ComputerInterface.zip" "ReleaseZip"