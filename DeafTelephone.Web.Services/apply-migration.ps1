Write-Host "Stary applying migration"

dotnet ef database update

Write-Host "Press enter to exit..."
$null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown');