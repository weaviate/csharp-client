# Generate HTML documentation from XMLDoc comments using DocFX
# PowerShell script for Windows

$ErrorActionPreference = "Stop"

Write-Host "Weaviate C# Client - HTML Documentation Generator" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host ""

# Check if DocFX is installed
$docfxExists = Get-Command docfx -ErrorAction SilentlyContinue
if (-not $docfxExists) {
    Write-Host "Error: DocFX is not installed." -ForegroundColor Red
    Write-Host ""
    Write-Host "Install DocFX using one of these methods:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Option 1: Using .NET tool (recommended)" -ForegroundColor Green
    Write-Host "  dotnet tool install -g docfx"
    Write-Host ""
    Write-Host "Option 2: Using Chocolatey" -ForegroundColor Green
    Write-Host "  choco install docfx"
    Write-Host ""
    exit 1
}

# Get project root
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Split-Path -Parent $ScriptDir

Write-Host "Project root: $ProjectRoot" -ForegroundColor Gray
Write-Host ""

# Navigate to project root
Set-Location $ProjectRoot

# Clean previous documentation
Write-Host "Cleaning previous documentation..." -ForegroundColor Yellow
if (Test-Path "_site") { Remove-Item -Recurse -Force "_site" }
if (Test-Path "api") {
    Get-ChildItem -Path "api" -Filter "*.yml" | Remove-Item -Force
    if (Test-Path "api/.manifest") { Remove-Item -Force "api/.manifest" }
}

# Build the project to generate XML documentation files
Write-Host ""
Write-Host "Building project to generate XML documentation..." -ForegroundColor Yellow
dotnet build src\Weaviate.Client\Weaviate.Client.csproj --configuration Release

# Check if XML file was generated
$XmlFile = "src\Weaviate.Client\bin\Release\net9.0\Weaviate.Client.xml"
if (-not (Test-Path $XmlFile)) {
    Write-Host "Error: XML documentation file not found at $XmlFile" -ForegroundColor Red
    Write-Host "Make sure GenerateDocumentationFile is set to true in the project file." -ForegroundColor Yellow
    exit 1
}

Write-Host "XML documentation file generated successfully at $XmlFile" -ForegroundColor Green
Write-Host ""

# Generate metadata (extracts API information from assemblies and XML files)
Write-Host "Generating API metadata..." -ForegroundColor Yellow
docfx metadata docfx.json

# Build the documentation site
Write-Host ""
Write-Host "Building documentation site..." -ForegroundColor Yellow
docfx build docfx.json

Write-Host ""
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "Documentation generated successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Output location: $ProjectRoot\_site" -ForegroundColor Gray
Write-Host ""
Write-Host "To view the documentation:" -ForegroundColor Yellow
Write-Host "  1. Serve locally: docfx serve _site"
Write-Host "  2. Open in browser: start _site\index.html"
Write-Host ""
Write-Host "To publish to GitHub Pages or other static hosting:" -ForegroundColor Yellow
Write-Host "  1. Copy contents of _site directory to your web server"
Write-Host "  2. Or use: docfx serve _site (for local preview)"
Write-Host ""
