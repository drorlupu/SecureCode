# PowerShell script to publish QuizApp for IIS hosting
$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$outputDir = Join-Path $scriptDir "..\publish\quizapp-iis"

Write-Host "====================================================" -ForegroundColor Cyan
Write-Host " Publishing QuizApp for IIS Deployment (.NET 10)    " -ForegroundColor Cyan
Write-Host "====================================================" -ForegroundColor Cyan

Write-Host "Cleaning previous publish output..." -ForegroundColor Yellow
if (Test-Path $outputDir) {
    Remove-Item -Recurse -Force $outputDir
}

Write-Host "Running dotnet publish..." -ForegroundColor Green
dotnet publish "$scriptDir\QuizApp.csproj" -c Release -o "$outputDir"

Write-Host "`nPublish complete!" -ForegroundColor Green
Write-Host "Output directory: $outputDir" -ForegroundColor White
Write-Host "Files generated for IIS:" -ForegroundColor Gray
Get-ChildItem $outputDir | Select-Object Name, Length | Format-Table -AutoSize

Write-Host @"
Next Steps for IIS:
1. Ensure the .NET 10 Hosting Bundle is installed on the server.
2. Ensure IIS WebSocket Protocol feature is enabled.
3. In IIS Manager, create an App Pool with .NET CLR Version = 'No Managed Code'.
4. Point your IIS Site or Virtual Application physical path to:
   $outputDir
5. Ensure IIS_IUSRS has Read & Execute permissions on this folder.
"@ -ForegroundColor Cyan
