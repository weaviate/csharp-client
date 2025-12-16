# Generate Markdown documentation using xmldocmd (.NET tool)
# PowerShell script for Windows

$ErrorActionPreference = "Stop"

Write-Host "Weaviate C# Client - Markdown Documentation Generator (xmldocmd)" -ForegroundColor Cyan
Write-Host "=================================================================" -ForegroundColor Cyan
Write-Host ""

# Check if xmldocmd is installed
$xmldocmdExists = Get-Command xmldocmd -ErrorAction SilentlyContinue
if (-not $xmldocmdExists) {
    Write-Host "xmldocmd is not installed. Installing now..." -ForegroundColor Yellow
    Write-Host ""
    dotnet tool install xmldocmd -g
    Write-Host ""
}

# Get project root
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Split-Path -Parent $ScriptDir

Write-Host "Project root: $ProjectRoot" -ForegroundColor Gray
Write-Host ""

# Navigate to project root
Set-Location $ProjectRoot

# Build the project to generate XML documentation files
Write-Host "Building project to generate XML documentation..." -ForegroundColor Yellow
dotnet build src\Weaviate.Client\Weaviate.Client.csproj --configuration Release

# Check if XML file was generated (using net8.0 for better compatibility)
$XmlFile = "src\Weaviate.Client\bin\Release\net8.0\Weaviate.Client.xml"
if (-not (Test-Path $XmlFile)) {
    Write-Host "Error: XML documentation file not found at $XmlFile" -ForegroundColor Red
    Write-Host "Make sure GenerateDocumentationFile is set to true in the project file." -ForegroundColor Yellow
    exit 1
}

Write-Host "XML documentation file generated successfully" -ForegroundColor Green
Write-Host ""

# Set output directory
$OutputDir = "docs\api-markdown"

# Clean previous documentation
Write-Host "Cleaning previous documentation..." -ForegroundColor Yellow
if (Test-Path $OutputDir) { Remove-Item -Recurse -Force $OutputDir }
New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null

# Generate markdown documentation
Write-Host "Generating Markdown documentation using xmldocmd..." -ForegroundColor Yellow
Write-Host ""
xmldocmd src\Weaviate.Client\bin\Release\net8.0\Weaviate.Client.dll $OutputDir `
    --namespace Weaviate.Client `
    --visibility public

Write-Host ""
Write-Host "=================================================================" -ForegroundColor Cyan
Write-Host "Markdown documentation generated successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Output location: $ProjectRoot\$OutputDir" -ForegroundColor Gray
Write-Host ""
Write-Host "To view the documentation:" -ForegroundColor Yellow
Write-Host "  1. Open $OutputDir\README.md in your favorite markdown viewer"
Write-Host "  2. Or browse individual namespace/type files in $OutputDir\"
Write-Host ""
