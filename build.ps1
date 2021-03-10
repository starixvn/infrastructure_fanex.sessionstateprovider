[CmdletBinding(PositionalBinding=$false)]
param(
    [bool] $CreatePackages = $true,
    [bool] $RunTests = $true
)

Write-Host "Run Parameters:" -ForegroundColor Cyan
Write-Host "CreatePackages: $CreatePackages"
Write-Host "RunTests: $RunTests"

$packageOutputFolder = "$PSScriptRoot\.nupkgs"
$projectsToBuild = Get-ChildItem "$PSScriptRoot\Sources" -Include *.csproj -Recurse
$testsToRun = Get-ChildItem "$PSScriptRoot\Tests" -Include *.csproj -Recurse

if ($RunTests) {
    dotnet restore /ConsoleLoggerParameters:Verbosity=Quiet
    foreach ($project in $testsToRun) {
        $projectName = $project.Name
        $projectFolder = Split-Path $project.FullName -parent

        Write-Host "Running tests: $projectName (all frameworks)" -ForegroundColor "Magenta"
        Push-Location $projectFolder

        dotnet test /ConsoleLoggerParameters:Verbosity=Quiet
        if ($LastExitCode -ne 0) {
            Write-Host "Error with tests, aborting build." -Foreground "Red"
            Exit 1
        }

        Write-Host "Tests passed!" -ForegroundColor "Green"
    }
}

if ($CreatePackages) {
    mkdir -Force $packageOutputFolder | Out-Null
    Write-Host "Clearing existing $packageOutputFolder..." -NoNewline
    Get-ChildItem $packageOutputFolder | Remove-Item
    Write-Host "Done." -ForegroundColor "Green"

    Write-Host "Building all packages" -ForegroundColor "Green"

    foreach ($project in $projectsToBuild) {
        $projectName = $project.Name
        $projectFolder = Split-Path $project.FullName -parent

        Write-Host "Working on $projectName`:" -ForegroundColor "Magenta"
        Write-Host "  Restoring and packing $project..." -ForegroundColor "Magenta"

        Push-Location $projectFolder

        dotnet msbuild "/t:Restore;Pack" "/p:Configuration=Release" "/p:PackageOutputPath=$packageOutputFolder" /ConsoleLoggerParameters:Verbosity=Quiet

        Write-Host "Done." -ForegroundColor "Green"
        Write-Host ""
    }

    Write-Host "Build Complete." -ForegroundColor "Green"
}

Push-Location $PSScriptRoot