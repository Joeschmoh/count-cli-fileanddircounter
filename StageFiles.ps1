
$StagingFolder = "Staging"

$BinaryFolder = "bin"
$SourceFolder = "src"

$BinaryReleaseFolder = "Count\bin\Release"
$BinaryFileList = "Count.exe","Count.exe.config","Count.exe.manifest"

$DocReleaseFolder = "."
$DocFileList = "README.TXT","LICENSE.TXT","ChangeLog.txt"

$SolutionFolder = "."
$SolutionFileList = "Count.sln","Count.suo","StageFiles.ps1","BUILD-howto.txt"

$SourceDestFolder = "Count"
$SourceFileList = "app.config","Count.csproj*","*.cs"

$PropertiesFolder = "Properties"
$PropertiesFileList = "app.manifest","*.cs"

$ArchiveProg = "c:\program files\WinRAR\WinRAR.exe"
$ArchiveCommand = "a"
$ArchiveSwitches = "-afzip","-r","-y","-t","-ep1"

$BinaryArchiveName = ".\$StagingFolder\Count2.3.0.1.zip" 
$SourceArchiveName = ".\$StagingFolder\Count2.3.0.1_Source.zip"

function CreateDirIfNeeded
{
   param ($RootFolder, $FolderName)
   
   $Exists = Test-Path $RootFolder\$FolderName
   if ($Exists -ne $true)
   {
      New-Item -name $FolderName -type directory -path $RootFolder
   }   
}

function DeleteItemIfExists
{
   # Remove file if it already exists 
   
   param($ItemToDelete)
   
   $DestFileExists = Test-Path $ItemToDelete
   if ($DestFileExists -eq $true)
   {
      Remove-Item $ItemToDelete
   }
}

echo "Create base directories..."
CreateDirIfNeeded "." $StagingFolder
CreateDirIfNeeded ".\$StagingFolder" $BinaryFolder
CreateDirIfNeeded ".\$StagingFolder" $SourceFolder

echo "Staging binaries..."
foreach ($File in $BinaryFileList)
{
   Copy-Item ".\$BinaryReleaseFolder\$File" -destination ".\$StagingFolder\$BinaryFolder\"
}

foreach ($File in $DocFileList)
{
   Copy-Item ".\$DocReleaseFolder\$File" -destination ".\$StagingFolder\$BinaryFolder\"
}

echo "Staging source code..."
# This is done more as a clean, you could just zip up the entire folder.

foreach ($File in $SolutionFileList)
{
   Copy-Item ".\$SolutionFolder\$File" -destination ".\$StagingFolder\$SourceFolder\"
}

foreach ($File in $DocFileList)
{
   Copy-Item ".\$DocReleaseFolder\$File" -destination ".\$StagingFolder\$SourceFolder\"
}

CreateDirIfNeeded ".\$StagingFolder\$SourceFolder" $SourceDestFolder
foreach ($File in $SourceFileList)
{
   Copy-Item ".\$SolutionFolder\$SourceDestFolder\$File" -destination ".\$StagingFolder\$SourceFolder\$SourceDestFolder"
}

CreateDirIfNeeded ".\$StagingFolder\$SourceFolder\$SourceDestFolder" $PropertiesFolder
foreach ($File in $PropertiesFileList)
{
   Copy-Item ".\$SolutionFolder\$SourceDestFolder\$PropertiesFolder\$File" -destination ".\$StagingFolder\$SourceFolder\$SourceDestFolder\$PropertiesFolder"
}

echo "Creating Binary archive - $BinaryArchiveName..."
DeleteItemIfExists $BinaryArchiveName
& $ArchiveProg $ArchiveCommand $ArchiveSwitches $BinaryArchiveName ".\$StagingFolder\$BinaryFolder\*.*"

echo "Creating Source archive - $SourceArchiveName..."
DeleteItemIfExists $SourceArchiveName
& $ArchiveProg $ArchiveCommand $ArchiveSwitches $SourceArchiveName ".\$StagingFolder\$SourceFolder\*.*"

echo "Done."