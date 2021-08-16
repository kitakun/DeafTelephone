$MigrationName = Read-Host "Please enter new migration name"

dotnet ef migrations add $MigrationName

Write-Host "Press enter to exit..."
$null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown');