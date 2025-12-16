#!/bin/bash
# Generate Markdown documentation using xmldocmd (.NET tool)

set -e

echo "Weaviate C# Client - Markdown Documentation Generator (xmldocmd)"
echo "================================================================="
echo ""

# Check if xmldocmd is installed
if ! command -v xmldocmd &> /dev/null; then
    echo "xmldocmd is not installed. Installing now..."
    echo ""
    dotnet tool install xmldocmd -g
    echo ""
fi

# Get the script directory and project root
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"

echo "Project root: $PROJECT_ROOT"
echo ""

# Navigate to project root
cd "$PROJECT_ROOT"

# Build the project to generate XML documentation files
echo "Building project to generate XML documentation..."
dotnet build src/Weaviate.Client/Weaviate.Client.csproj --configuration Release

# Check if XML file was generated (using net8.0 for better compatibility)
XML_FILE="src/Weaviate.Client/bin/Release/net8.0/Weaviate.Client.xml"
if [ ! -f "$XML_FILE" ]; then
    echo "Error: XML documentation file not found at $XML_FILE"
    echo "Make sure GenerateDocumentationFile is set to true in the project file."
    exit 1
fi

echo "XML documentation file generated successfully"
echo ""

# Set output directory
OUTPUT_DIR="docs/api-markdown"

# Clean previous documentation
echo "Cleaning previous documentation..."
rm -rf "$OUTPUT_DIR"
mkdir -p "$OUTPUT_DIR"

# Generate markdown documentation
echo "Generating Markdown documentation using xmldocmd..."
echo ""
xmldocmd src/Weaviate.Client/bin/Release/net8.0/Weaviate.Client.dll "$OUTPUT_DIR" \
    --namespace Weaviate.Client \
    --visibility public

echo ""
echo "================================================================="
echo "Markdown documentation generated successfully!"
echo ""
echo "Output location: $PROJECT_ROOT/$OUTPUT_DIR"
echo ""
echo "To view the documentation:"
echo "  1. Open $OUTPUT_DIR/README.md in your favorite markdown viewer"
echo "  2. Or browse individual namespace/type files in $OUTPUT_DIR/"
echo ""
