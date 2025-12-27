#!/bin/bash
# Generate HTML documentation from XMLDoc comments using DocFX

set -e

echo "Weaviate C# Client - HTML Documentation Generator"
echo "=================================================="
echo ""

# Check if DocFX is installed
if ! command -v docfx &> /dev/null; then
    echo "Error: DocFX is not installed."
    echo ""
    echo "Install DocFX using one of these methods:"
    echo ""
    echo "Option 1: Using .NET tool (recommended)"
    echo "  dotnet tool install -g docfx"
    echo ""
    echo "Option 2: Using Homebrew (macOS)"
    echo "  brew install docfx"
    echo ""
    echo "Option 3: Using Chocolatey (Windows)"
    echo "  choco install docfx"
    echo ""
    exit 1
fi

# Get the script directory and project root
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"

echo "Project root: $PROJECT_ROOT"
echo ""

# Navigate to project root
cd "$PROJECT_ROOT"

# Clean previous documentation
echo "Cleaning previous documentation..."
rm -rf _site api/*.yml api/.manifest

# Build the project to generate XML documentation files
echo ""
echo "Building project to generate XML documentation..."
dotnet build src/Weaviate.Client/Weaviate.Client.csproj --configuration Release

# Check if XML file was generated
XML_FILE="src/Weaviate.Client/bin/Release/net9.0/Weaviate.Client.xml"
if [ ! -f "$XML_FILE" ]; then
    echo "Error: XML documentation file not found at $XML_FILE"
    echo "Make sure GenerateDocumentationFile is set to true in the project file."
    exit 1
fi

echo "XML documentation file generated successfully at $XML_FILE"
echo ""

# Generate metadata (extracts API information from assemblies and XML files)
echo "Generating API metadata..."
docfx metadata docfx.json

# Build the documentation site
echo ""
echo "Building documentation site..."
docfx build docfx.json

echo ""
echo "=================================================="
echo "Documentation generated successfully!"
echo ""
echo "Output location: $PROJECT_ROOT/_site"
echo ""
echo "To view the documentation:"
echo "  1. Serve locally: docfx serve _site"
echo "  2. Open in browser: open _site/index.html (macOS) or start _site/index.html (Windows)"
echo ""
echo "To publish to GitHub Pages or other static hosting:"
echo "  1. Copy contents of _site directory to your web server"
echo "  2. Or use: docfx serve _site (for local preview)"
echo ""
